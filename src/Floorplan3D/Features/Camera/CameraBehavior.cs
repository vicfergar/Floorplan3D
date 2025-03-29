// Forked from Wave Engine's develop (ce51ee3f557fbf2ebbba2c27ea5282e73e39ed2a):
// src/Tools/Editor/Evergine.Runner/Viewers/Common/CameraBehavior.cs
// https://dev.azure.com/Evergineteam/Wave.Engine/_git/Evergine?path=%2Fsrc%2FTools%2FEditor%2FEvergine.Runner%2FViewers%2FCommon%2FCameraBehavior.cs

// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Linq;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;

namespace Floorplan3D
{
    public abstract class CameraBehavior : Behavior
    {
        private const float OrbitFactor = 0.0025f;

        private const float PanFactor = 0.0025f;

        private const float ZoomFactor = 0.5f;

        private const int OrbitSmoothTimeMilliseconds = 50;

        private const int PanAndZoomSmoothTimeMilliseconds = 200;

        [Flags]
        public enum Modes
        {
            Orbit = 0x1,
            Zoom = 0x2,
            Pan = 0x04,
        }

        /// <summary>
        /// The camera to move.
        /// </summary>
        [BindComponent(false)]
        public Transform3D Transform = null;

        protected Vector2 currentPointerPosition;

        protected Vector2 lastPointerPosition;

        protected bool isOrbiting;

        protected bool isPanning;

        private bool isZooming;

        private readonly Modes mode;

        private Transform3D targetTransform;

        private Transform3D cameraTransform;

        private Vector3 cameraInitialPosition;

        private Vector3 cameraTargetVector;

        private float cameraTargetLength;

        private float cameraInitialAngleRadians;

        private float theta;

        private float phi;

        private Vector3 panIncrement;

        private Vector3 zoomIncrement;

        private Quaternion cameraOrbitSmoothDampDeriv;

        private Quaternion targetOrbitSmoothDampDeriv;

        private Vector3 panSmoothDampVelocity;

        private Vector3 zoomSmoothDampVelocity;

        /// <summary>
        /// IMPORTANT: The actual <see cref="Camera3D"/> *must* be placed with position X = 0 in order to orbit as 
        /// expected; you can rotate the target instead.
        /// </summary>
        public CameraBehavior(Modes mode = Modes.Orbit | Modes.Zoom)
        {
            this.mode = mode;
        }

        /// <summary>
        /// Reset camera position.
        /// </summary>
        public void Reset()
        {
            this.cameraTransform.LocalPosition = this.cameraInitialPosition;
            this.cameraTransform.LocalLookAt(Vector3.Zero, Vector3.Up);
            this.targetTransform.LocalPosition = Vector3.Zero;
            this.targetTransform.LocalRotation = Vector3.Zero;
            this.Transform.LocalPosition = Vector3.Zero;
            this.Transform.LocalRotation = Vector3.Zero;

            this.theta = 0;
            this.phi = 0;

            this.isOrbiting = false;
            this.isPanning = false;
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            var child = this.Owner.ChildEntities.FirstOrDefault();
            this.targetTransform = child?.FindComponent<Transform3D>();
            var grandChild = child?.ChildEntities.FirstOrDefault();
            this.cameraTransform = grandChild?.FindComponent<Transform3D>();

            if (this.targetTransform == null ||
                this.cameraTransform == null)
            {
                return false;
            }

            this.cameraInitialPosition = this.cameraTransform.LocalPosition;

            var targetCameraVector = this.cameraTransform.Position - this.targetTransform.Position;
            this.cameraInitialAngleRadians = Vector3.Angle(targetCameraVector, Vector3.UnitZ);

            return true;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            if (this.IsResetRequested())
            {
                this.Reset();
                return;
            }

            if (this.TryGetPointerPosition(out Vector2 position))
            {
                this.lastPointerPosition = this.currentPointerPosition;
                this.currentPointerPosition = position;
            }
            else
            {
                return;
            }

            this.cameraTargetVector = this.targetTransform.LocalPosition - this.cameraTransform.LocalPosition;
            this.cameraTargetLength = this.cameraTargetVector.Length();

            this.HandleOrbit();
            this.HandlePan();
            this.HandleZoom();

            float elapsedMilliseconds = (float)gameTime.TotalMilliseconds;

            if (this.isOrbiting)
            {
                var orientation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -this.theta);
                this.Transform.LocalOrientation = Quaternion.SmoothDamp(
                    this.Transform.LocalOrientation,
                    orientation,
                    ref this.cameraOrbitSmoothDampDeriv,
                    OrbitSmoothTimeMilliseconds,
                    elapsedMilliseconds);

                var targetOrientation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, this.phi);
                this.targetTransform.LocalOrientation = Quaternion.SmoothDamp(
                    this.targetTransform.LocalOrientation,
                    targetOrientation,
                    ref this.targetOrbitSmoothDampDeriv,
                    OrbitSmoothTimeMilliseconds,
                    elapsedMilliseconds);

                if (this.Transform.LocalOrientation == orientation &&
                    this.targetTransform.LocalOrientation == targetOrientation)
                {
                    this.isOrbiting = false;
                }
            }

            if (this.isPanning)
            {
                var newPosition = this.Transform.LocalPosition + this.panIncrement;
                this.Transform.LocalPosition = Vector3.SmoothDamp(
                    this.Transform.LocalPosition,
                    newPosition,
                    ref this.panSmoothDampVelocity,
                    PanAndZoomSmoothTimeMilliseconds,
                    elapsedMilliseconds);

                if (this.Transform.LocalPosition == newPosition)
                {
                    this.isPanning = false;
                }
            }

            if (this.isZooming)
            {
                var newPosition = this.cameraTransform.LocalPosition + this.zoomIncrement;
                this.cameraTransform.LocalPosition = Vector3.SmoothDamp(
                    this.cameraTransform.LocalPosition,
                    newPosition,
                    ref this.zoomSmoothDampVelocity,
                    PanAndZoomSmoothTimeMilliseconds,
                    elapsedMilliseconds);

                if (this.cameraTransform.LocalPosition == newPosition)
                {
                    this.isZooming = false;
                }
            }
        }

        protected abstract bool TryGetPointerPosition(out Vector2 position);

        protected virtual bool IsResetRequested() => false;

        protected virtual bool IsOrbitRequested() => false;

        protected virtual bool IsPanRequested() => false;

        protected virtual Vector2 GetPanDelta() => Vector2.Zero;

        /// <returns>A value < 0 for getting closer to the target, > 0 for further or 0 to hold on.</returns>
        protected virtual float GetZoomDelta() => 0;

        protected Vector2 CalcDelta(Vector2 current, Vector2 last)
        {
            Vector2 delta;
            delta.X = -current.X + last.X;
            delta.Y = current.Y - last.Y;

            return delta;
        }

        private void HandleOrbit()
        {
            if (!this.mode.HasFlag(Modes.Orbit))
            {
                return;
            }

            if (!this.IsOrbitRequested())
            {
                return;
            }

            Vector2 delta = -this.CalcDelta(this.currentPointerPosition, this.lastPointerPosition);
            delta *= OrbitFactor;

            this.theta += delta.X;
            this.phi += delta.Y;

            if (this.phi > (MathHelper.PiOver2 + this.cameraInitialAngleRadians))
            {
                this.phi = MathHelper.PiOver2 + this.cameraInitialAngleRadians;
            }
            else if (this.phi < -(MathHelper.PiOver2 - this.cameraInitialAngleRadians))
            {
                this.phi = -(MathHelper.PiOver2 - this.cameraInitialAngleRadians);
            }

            this.isOrbiting = true;
        }

        private void HandlePan()
        {
            if (!this.mode.HasFlag(Modes.Pan))
            {
                return;
            }

            if (!this.IsPanRequested())
            {
                this.panIncrement = Vector3.Zero;
                return;
            }

            Vector2 delta = this.GetPanDelta();
            delta *= this.cameraTargetLength * PanFactor;

            this.panIncrement = (this.cameraTransform.WorldTransform.Right * delta.X) +
                (this.cameraTransform.WorldTransform.Up * delta.Y);

            this.isPanning = true;
        }

        private void HandleZoom()
        {
            if (!this.mode.HasFlag(Modes.Zoom))
            {
                return;
            }

            var delta = this.GetZoomDelta();

            if (delta == 0)
            {
                this.zoomIncrement = Vector3.Zero;
                return;
            }

            var deltaSign = Math.Sign(delta);
            var increment = deltaSign * this.cameraTargetVector * ZoomFactor;

            var isZoomingIn = delta > 0;
            var distanceAfterZoom = this.cameraTargetLength + (deltaSign * increment.Length());

            const float minCameraDistanceMeters = 0.1f;

            if (isZoomingIn && distanceAfterZoom <= minCameraDistanceMeters)
            {
                this.zoomIncrement = Vector3.Zero;
                return;
            }

            this.zoomIncrement = increment;
            this.isZooming = true;
        }
    }
}

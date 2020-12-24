using System;
using System.Linq;
using WaveEngine.Common.Input;
using WaveEngine.Common.Input.Pointer;
using WaveEngine.Mathematics;

namespace Floorplan3D

{
    public class TouchCameraBehavior : CameraBehavior
    {
        private const float ZoomFactor = 0.1f;

        private const int MinPixelsBetweenFingersForZooming = 10;

        private static readonly TimeSpan maxTimeBetweenResetPoints = TimeSpan.FromMilliseconds(150);

        private PointerDispatcher touchDispatcher;

        private Point firstPointToResetPosition;

        private DateTime firstPointToResetTime;

        private float lastTouchesDistance;

        private Vector2 lastTouchTwoPointsCenter;

        public TouchCameraBehavior() :
            base()
        {
        }

        public TouchCameraBehavior(Modes mode)
            : base(mode)
        {
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            var display = this.Owner.Scene.Managers.RenderManager.ActiveCamera3D?.Display;

            if (display != null)
            {
                this.touchDispatcher = display.TouchDispatcher;
            }
        }

        protected override bool IsResetRequested()
        {
            if (this.touchDispatcher == null || this.touchDispatcher.Points.Count != 1)
            {
                return false;
            }

            var point = this.touchDispatcher.Points[0];

            if (this.firstPointToResetPosition == default)
            {
                if (point.State == ButtonState.Releasing)
                {
                    this.firstPointToResetPosition = point.Position;
                    this.firstPointToResetTime = DateTime.Now;
                }
            }
            else
            {
                var elapsedTime = DateTime.Now - this.firstPointToResetTime;
                var distance = Vector2.Distance(
                    point.Position.ToVector2(),
                    this.firstPointToResetPosition.ToVector2());

                this.firstPointToResetPosition = default;
                this.firstPointToResetTime = default;

                if (point.State == ButtonState.Pressing && elapsedTime < maxTimeBetweenResetPoints &&
                    distance < 125)
                {
                    this.Reset();
                    return true;
                }
            }

            return false;
        }

        protected override bool TryGetPointerPosition(out Vector2 position)
        {
            position = default;

            if (this.touchDispatcher == null)
            {
                return false;
            }

            if (this.touchDispatcher.Points.Count == 1)
            {
                position = this.touchDispatcher.Points[0].Position.ToVector2();
            }
            else if (this.touchDispatcher.Points.Count == 2 &&
                this.touchDispatcher.Points.Any(item => item.State == ButtonState.Releasing) &&
                this.touchDispatcher.Points.Any(item => item.State == ButtonState.Pressed))
            {
                position = this.touchDispatcher.Points
                    .First(item => item.State == ButtonState.Pressed)
                    .Position.ToVector2();
            }
            else
            {
                position = this.lastPointerPosition;
            }

            return true;
        }

        protected override bool IsOrbitRequested()
        {
            var isRequested = this.touchDispatcher.Points.Count == 1;

            if (isRequested && this.touchDispatcher.Points[0].State == ButtonState.Pressing)
            {
                this.lastPointerPosition = this.currentPointerPosition;
            }

            return isRequested;
        }

        protected override bool IsPanRequested()
        {
            var isRequested = this.touchDispatcher.Points.Count == 2;

            if (!isRequested)
            {
                this.lastTouchTwoPointsCenter = default;
                this.lastTouchesDistance = 0;
            }

            return isRequested;
        }

        protected override Vector2 GetPanDelta()
        {
            var currentCenter = (this.touchDispatcher.Points[0].Position.ToVector2() +
                this.touchDispatcher.Points[1].Position.ToVector2()) / 2;

            if (this.lastTouchTwoPointsCenter == default)
            {
                this.lastTouchTwoPointsCenter = currentCenter;
            }

            var delta = this.CalcDelta(currentCenter, this.lastTouchTwoPointsCenter);
            this.lastTouchTwoPointsCenter = currentCenter;

            return delta;
        }

        protected override float GetZoomDelta()
        {
            if (this.touchDispatcher.Points.Count != 2)
            {
                return base.GetZoomDelta();
            }

            var currentDistance = Vector2.Distance(
                this.touchDispatcher.Points[0].Position.ToVector2(),
                this.touchDispatcher.Points[1].Position.ToVector2());

            if (this.lastTouchesDistance == 0)
            {
                this.lastTouchesDistance = currentDistance;
            }

            var difference = currentDistance - this.lastTouchesDistance;

            this.lastTouchesDistance = currentDistance;

            if (Math.Abs(difference) < MinPixelsBetweenFingersForZooming)
            {
                return base.GetZoomDelta();
            }

            var delta = difference * ZoomFactor;

            return delta;
        }
    }
}
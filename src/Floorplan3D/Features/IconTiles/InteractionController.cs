using System;
using System.Collections.Generic;
using System.Diagnostics;
using WaveEngine.Common.Input;
using WaveEngine.Common.Input.Pointer;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;

namespace Floorplan3D
{
    public class InteractionController : Behavior
    {
        [BindComponent]
        protected Camera3D camera;

        private List<BaseInteractableComponent> interactables = new List<BaseInteractableComponent>();

        private BaseInteractableComponent currentClickInteractable;

        internal void RegisterInteractable(BaseInteractableComponent baseInteractableComponent)
        {
            this.interactables.Add(baseInteractableComponent);
        }

        internal void UnregisterInteractable(BaseInteractableComponent baseInteractableComponent)
        {
            this.interactables.Remove(baseInteractableComponent);
        }

        /// <inheritdoc />
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            this.currentClickInteractable = null;
        }

        /// <inheritdoc />
        protected override void Update(TimeSpan gameTime)
        {
            if (this.TryGetPointerPoint(out var pointerPoint))
            {
                if (pointerPoint.State == ButtonState.Pressing)
                {
                    var ray = CalculatePointerRay(pointerPoint);

                    foreach (var item in this.interactables)
                    {
                        if (item.Intersects(ref ray))
                        {
                            this.currentClickInteractable = item;
                            break;
                        }
                    }
                }
                else if (pointerPoint.State == ButtonState.Releasing)
                {
                    var ray = CalculatePointerRay(pointerPoint);
                    if (this.currentClickInteractable?.Intersects(ref ray) == true)
                    {
                        this.currentClickInteractable.OnClick();
                    }

                    this.currentClickInteractable = null;
                }
            }
        }

        private Ray CalculatePointerRay(PointerPoint pointerPoint)
        {
            var pointerPosition = pointerPoint.Position.ToVector2();
            this.camera.CalculateRay(ref pointerPosition, out Ray ray);
            return ray;
        }

        private bool TryGetPointerPoint(out PointerPoint position)
        {
            var display = this.camera.Display;
            var mouseDispatcher = display.MouseDispatcher;
            if (mouseDispatcher.Points.Count > 0)
            {
                position = mouseDispatcher.Points[0];
                return true;
            }

            var touchDispatcher = display.TouchDispatcher;
            if (touchDispatcher.Points.Count > 0)
            {
                position = touchDispatcher.Points[0];
                return true;
            }

            position = default;
            return false;
        }
    }
}

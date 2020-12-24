using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Common.Input.Mouse;
using WaveEngine.Mathematics;

namespace Floorplan3D
{
    public class MouseCameraBehavior : CameraBehavior
    {
        private KeyboardDispatcher keyboardDispatcher;

        private MouseDispatcher mouseDispatcher;

        public MouseCameraBehavior() :
            base()
        {
        }

        public MouseCameraBehavior(Modes mode) 
            : base(mode)
        {
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            var display = this.Owner.Scene.Managers.RenderManager.ActiveCamera3D?.Display;

            if (display != null)
            {
                this.keyboardDispatcher = display.KeyboardDispatcher;
                this.mouseDispatcher = display.MouseDispatcher;
            };
        }

        protected override bool IsResetRequested() =>
            this.keyboardDispatcher != null && this.keyboardDispatcher.IsKeyDown(Keys.Space);

        protected override bool TryGetPointerPosition(out Vector2 position)
        {
            position = default;

            if (this.mouseDispatcher == null)
            {
                return false;
            }

            position = this.mouseDispatcher.Position.ToVector2();
            
            return true;
        }

        protected override bool IsOrbitRequested()
        {
            var isRequested = this.mouseDispatcher.IsButtonDown(MouseButtons.Left);

            return isRequested;
        }

        protected override bool IsPanRequested()
        {
            var isRequested = this.mouseDispatcher.IsButtonDown(MouseButtons.Middle) ||
                this.mouseDispatcher.IsButtonDown(MouseButtons.Right);

            return isRequested;
        }

        protected override Vector2 GetPanDelta() => 
            this.CalcDelta(this.currentPointerPosition, this.lastPointerPosition);

        protected override float GetZoomDelta() => this.mouseDispatcher.ScrollDelta.Y;
    }
}
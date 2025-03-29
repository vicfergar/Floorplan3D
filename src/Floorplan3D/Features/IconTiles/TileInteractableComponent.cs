using System;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;

namespace Floorplan3D
{
    public class TileInteractableComponent : BaseInteractableComponent
    {
        [BindComponent(source: BindComponentSource.Scene)]
        protected Camera3D camera3D;

        [BindComponent]
        protected Transform3D transform;

        public event EventHandler Detected;

        internal override bool Intersects(ref Ray ray)
        {
            var halfSize = IconTileComponent.ICON_SIZE * 0.5f;
            var bounding = new BoundingOrientedBox(this.transform.Position, new Vector3(halfSize, halfSize, 0), this.transform.Orientation);
            var distance = bounding.Intersects(ref ray);
            return distance != null;
        }

        internal override void OnClick()
        {
            base.OnClick();
            this.Detected?.Invoke(this, EventArgs.Empty);
        }
    }
}

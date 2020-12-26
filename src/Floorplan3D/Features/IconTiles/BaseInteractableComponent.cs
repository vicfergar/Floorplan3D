using System.Diagnostics;
using WaveEngine.Framework;
using WaveEngine.Mathematics;

namespace Floorplan3D
{
    public abstract class BaseInteractableComponent : Component
    {
        [BindComponent(source: BindComponentSource.Scene)]
        protected InteractionController interactionController;

        /// <inheritdoc />
        protected override void OnActivated()
        {
            base.OnActivated();
            this.interactionController.RegisterInteractable(this);
        }

        /// <inheritdoc />
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            this.interactionController.UnregisterInteractable(this);
        }

        internal abstract bool Intersects(ref Ray ray);

        internal virtual void OnClick()
        {
        }
    }
}

using Floorplan3D.Features.IconTiles;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.Toolkit;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;

namespace Floorplan3D
{
    public class IconTileComponent : Component
    {
        public const float ICON_SIZE = 0.1f;

        private const string entityName = "tileEntity";

        [BindService(isRequired: false)]
        protected BaseMDITextureLoader iconLoader;

        [BindService]
        protected AssetsService assetsService;

        private Entity tileEntity;

        private Texture iconTexture;

        private StandardMaterial material;

        private PointLight pointLight;

        private bool isOn;

        public string IconId { get; set; }

        public Color OffColor { get; set; } = Color.White;

        public Color OnColor { get; set; } = Color.FromHex("#fdd835");

        public bool IsLight { get; set; }

        public bool IsOn
        {
            get => this.isOn;
            set
            {
                if (this.isOn != value)
                {
                    this.isOn = value;
                    this.UpdateIconColor();
                }
            }
        }

        /// <inheritdoc />
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.tileEntity = this.Owner.FindChild(entityName);

            if (this.tileEntity == null)
            {
                this.tileEntity = new Entity(entityName) { Flags = HideFlags.DontSave | HideFlags.DontShow }
                    .AddComponent(new Transform3D())
                    .AddComponent(new MaterialComponent() { Material = this.assetsService.Load<Material>(WaveContent.Materials.TilesMaterial, true) })
                    .AddComponent(new PlaneMesh() { Width = ICON_SIZE, Height = ICON_SIZE, Normal = Vector3.Forward })
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new TileInteractableComponent())
                    .AddComponent(new LookAtBehavior())
                    ;

                if (this.IsLight)
                {
                    this.pointLight = new PointLight();
                    this.tileEntity.AddComponent(this.pointLight);
                }

                this.Owner.AddChild(this.tileEntity);
            }

            var materialComponent = this.tileEntity.FindComponent<MaterialComponent>();
            this.material = new StandardMaterial(materialComponent.Material);

            var clickDetector = this.tileEntity.FindComponent<TileInteractableComponent>();
            clickDetector.Detected += (s, e) => this.IsOn = !this.IsOn;

            this.UpdateIconColor();

            return true;
        }

        /// <inheritdoc />
        protected override void OnDetach()
        {
            base.OnDetach();
            this.Owner.RemoveChild(this.tileEntity);
            this.tileEntity = null;
        }

        /// <inheritdoc />
        protected override async void OnActivated()
        {
            base.OnActivated();

            if (!string.IsNullOrEmpty(this.IconId) &&
                this.iconLoader != null)
            {
                this.iconTexture = await this.iconLoader.LoadIconTextureAsync(this.IconId);
                if (this.iconTexture != null)
                {
                    this.material.BaseColorTexture = this.iconTexture;
                }
            }
        }

        private void UpdateIconColor()
        {
            this.material.BaseColor = this.isOn ? this.OnColor : this.OffColor;

            if (this.pointLight != null)
            {
                this.pointLight.Color = this.OnColor;
                this.pointLight.IsEnabled = this.isOn;
            }
        }
    }
}

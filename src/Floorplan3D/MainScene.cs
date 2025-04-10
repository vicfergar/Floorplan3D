using Evergine.Common.Graphics;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Graphics.Materials;

namespace Floorplan3D
{
    public class MainScene : Scene
    {
        public override void RegisterManagers()
        {
            base.RegisterManagers();
        }

        protected override void CreateScene()
        {
            var defaultMaterial = Managers.AssetSceneManager.Load<Material>(EvergineContent.Materials.DefaultMaterial);
            var groundMaterial = new StandardMaterial(defaultMaterial.Clone())
            {
                BaseColor = Color.LightGray,
                AlphaCutout = 0,
            };

            var ground = new Entity("ground")
                .AddComponent(new Transform3D())
                .AddComponent(new MaterialComponent()
                { 
                    Material = groundMaterial.Material
                })
                .AddComponent(new PlaneMesh() 
                {
                    Width = 4,
                    Height = 4,
                    TwoSides = true,
                })
                .AddComponent(new MeshRenderer());
            this.Managers.EntityManager.Add(ground);
        }
    }
}



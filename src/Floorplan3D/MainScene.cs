using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Graphics.Batchers;

namespace Floorplan3D
{
    public class MainScene : Scene
    {
        protected override void CreateScene()
        {
            var camera = this.Managers.EntityManager.FindFirstComponentOfType<Camera3D>();
            camera.AutoExposureEnabled = false;

            //if (DeviceInfo.PlatformType == PlatformType.Web)
            {
                var meshRenderFeature = this.Managers.RenderManager.FindRenderFeature<MeshRenderFeature>();
                var dynamicBatchMeshProcessor = meshRenderFeature.FindMeshProcessor<DynamicBatchMeshProcessor>();
                dynamicBatchMeshProcessor.IsActivated = false;
            }
        }
    }
}
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Batchers;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;

namespace Floorplan3D
{
    public class MainScene : Scene
    {
        protected override void CreateScene()
        {
            this.AvoidBrowserFreeze();

        }

        private void AvoidBrowserFreeze()
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
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace Floorplan3D
{
    public class MainScene : Scene
    {
        protected override void CreateScene()
        {
            base.CreateScene();

            var camera = this.Managers.EntityManager.FindFirstComponentOfType<Camera3D>();
            camera.AutoExposureEnabled = false;
        }
    }
}
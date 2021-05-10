using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;

namespace Floorplan3D
{
    public class MainScene : Scene
    {
		public override void RegisterManagers()
        {
        	base.RegisterManagers();
        	this.Managers.AddManager(new WaveEngine.Bullet.BulletPhysicManager3D());        	
        }

        protected override void CreateScene()
        {
            var camera = this.Managers.EntityManager.FindFirstComponentOfType<Camera3D>();
            camera.AutoExposureEnabled = false;
        }
    }
}
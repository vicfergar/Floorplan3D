using Evergine.Framework;

namespace Floorplan3D
{
    public class MainScene : Scene
    {
        public override void RegisterManagers()
        {
            base.RegisterManagers();
            this.Managers.AddManager(new global::Evergine.Bullet.BulletPhysicManager3D());
        }

        protected override void CreateScene()
        {
        }
    }
}
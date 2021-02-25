using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WaveEngine.Platform;

namespace Floorplan3D
{
    public class MainApplication : Application
    {
        public MainApplication()
        {
            this.Container.RegisterType<Clock>();
            this.Container.RegisterType<TimerFactory>();
            this.Container.RegisterType<Random>();
            this.Container.RegisterType<ErrorHandler>();
            this.Container.RegisterType<ScreenContextManager>();
            this.Container.RegisterType<GraphicsPresenter>();
            this.Container.RegisterType<AssetsDirectory>();
            this.Container.RegisterType<AssetsService>();
            this.Container.RegisterType<ForegroundTaskSchedulerService>();
        }

        public override void Initialize()
        {
            base.Initialize();

            // Get ScreenContextManager
            var screenContextManager = this.Container.Resolve<ScreenContextManager>();
            var assetsService = this.Container.Resolve<AssetsService>();

            // Navigate to scene
            var scene = assetsService.Load<MainScene>(WaveContent.Scenes.MainScene_wescene);
            ScreenContext screenContext = new ScreenContext(scene);
            screenContextManager.To(screenContext);
        }

        public void ConnectToHassUsingCredentialsFile()
        {
            const string CredentialsFile = "credentials.json";

            if (File.Exists(CredentialsFile))
            {
                var fileStr = File.ReadAllText(CredentialsFile);
                var typeDefinition = new { instanceBaseUrl = "", accessToken = "" };
                var credentials = JsonConvert.DeserializeAnonymousType(fileStr, typeDefinition);
            }
            else
            {
                Trace.TraceWarning($"Credentials file not found at: {Path.GetFullPath(CredentialsFile)}");
            }
        }
    }
}

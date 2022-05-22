using Evergine.Common.Graphics;
using Evergine.Common.Helpers;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.Platform;
using System;
using System.Diagnostics;

namespace Floorplan3D.Windows
{
    class Program
    {

#if OPENGL
        static string title = "Floorplan3D - OpenGL";
#else
        static string title = "Floorplan3D - DX11";
#endif

        static uint width = 1280;
        static uint height = 720;
        static bool windowed = true;
        static bool vSync = true;

        static void Main(string[] args)
        {
            // Commandline parser
            if (args.Length > 0)
            {
                CmdParser cmd = new CmdParser()
                    .AddOption(new CmdParser.Option("-Width", (string v) => { return uint.TryParse(v, out width); }, "Set width size in pixels."))
                    .AddOption(new CmdParser.Option("-Height", (string v) => { return uint.TryParse(v, out height); }, "Set height size in pixels."))
                    .AddOption(new CmdParser.Option("-VSync", (string _) => { vSync = true; return true; }, "Activate vertical sync."))
                    .AddOption(new CmdParser.Option("-NoVSync", (string _) => { vSync = false; return true; }, "Deactivate vertical sync."))
                    .AddOption(new CmdParser.Option("-Windowed", (string _) => { windowed = true; return true; }, "Set application to run in windowed mode."))
                    .AddOption(new CmdParser.Option("-FullScreen", (string _) => { windowed = false; return true; }, "Set application to run in full screen mode."));

                var success = cmd.Parse(args);

                if (!success)
                {
                    Console.Write(cmd.ErrorMessage);
                    return;
                }
            }
            else
            {
                var handle = Win32API.GetConsoleWindow();
                Win32API.ShowWindow(handle, 0);
            }

            // Create app
            MainApplication application = new MainApplication();

            // Create Services
            WindowsSystem windowsSystem = new Evergine.Forms.FormsWindowsSystem();
            application.Container.RegisterInstance(windowsSystem);
            var window = windowsSystem.CreateWindow(title, width, height);

            ConfigureGraphicsContext(application, window);

            application.Container.RegisterInstance(new DesktopMDITextureLoader());

            // Creates XAudio device
            var xaudio = new global::Evergine.XAudio2.XAudioDevice();
            application.Container.RegisterInstance(xaudio);

            Stopwatch clockTimer = Stopwatch.StartNew();
            windowsSystem.Run(
            () =>
            {
                application.Initialize();
            },
            () =>
            {
                var gameTime = clockTimer.Elapsed;
                clockTimer.Restart();

                application.UpdateFrame(gameTime);
                application.DrawFrame(gameTime);
            });
        }

        private static void ConfigureGraphicsContext(Application application, Window window)
        {
#if OPENGL
            GraphicsContext graphicsContext = new global::Evergine.OpenGL.GLGraphicsContext();
#else
            GraphicsContext graphicsContext = new global::Evergine.DirectX11.DX11GraphicsContext();
#endif
            graphicsContext.CreateDevice();
            SwapChainDescription swapChainDescription = new SwapChainDescription()
            {
                SurfaceInfo = window.SurfaceInfo,
                Width = window.Width,
                Height = window.Height,
                ColorTargetFormat = PixelFormat.R8G8B8A8_UNorm,
                ColorTargetFlags = TextureFlags.RenderTarget | TextureFlags.ShaderResource,
                DepthStencilTargetFormat = PixelFormat.D24_UNorm_S8_UInt,
                DepthStencilTargetFlags = TextureFlags.DepthStencil,
                SampleCount = TextureSampleCount.None,
                IsWindowed = windowed,
                RefreshRate = 60
            };
            var swapChain = graphicsContext.CreateSwapChain(swapChainDescription);
            swapChain.VerticalSync = vSync;

            var graphicsPresenter = application.Container.Resolve<GraphicsPresenter>();
            var firstDisplay = new Display(window, swapChain);
            graphicsPresenter.AddDisplay("DefaultDisplay", firstDisplay);

            application.Container.RegisterInstance(graphicsContext);
        }
    }
}

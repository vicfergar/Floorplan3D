using System.Collections.Generic;
using System.Diagnostics;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.OpenGL;
using WaveEngine.Web;
using WebAssembly;

namespace Floorplan3D.Web
{
    public class Program
    {
        private static readonly Dictionary<string, WebSurface> appCanvas = new Dictionary<string, WebSurface>();

        public static void Main(string canvasId)
        {
            // Hack for AOT dll dependencies
            var cp = new WaveEngine.Components.Graphics3D.Spinner();

            // Create app
            var application = new MainApplication();

            // Create Services
            var windowsSystem = new WebWindowsSystem();
            application.Container.RegisterInstance(windowsSystem);

            var document = (JSObject)Runtime.GetGlobalObject("document");
            var canvas = (JSObject)document.Invoke("getElementById", canvasId);
            var surface = (WebSurface)windowsSystem.CreateSurface(canvas);
            appCanvas[canvasId] = surface;

            var supportWebGL2 = canvas.Invoke("getContext", "webgl2") != null;
            ConfigureGraphicsContext(application, surface, supportWebGL2 ? GraphicsBackend.WebGL2 : GraphicsBackend.WebGL1);

            // Audio is currently unsupported
            //var xaudio = new WaveEngine.XAudio2.XAudioDevice();
            //application.Container.RegisterInstance(xaudio);

            Stopwatch clockTimer = Stopwatch.StartNew();
            windowsSystem.Run(
                () =>
                {
                    application.Initialize();
                    Runtime.InvokeJS("WaveEngine.init();");
                },
                () =>
                {
                    var gameTime = clockTimer.Elapsed;
                    clockTimer.Restart();

                    application.UpdateFrame(gameTime);
                    application.DrawFrame(gameTime);
                });
        }

        public static void UpdateCanvasSize(string canvasId)
        {
            if (appCanvas.TryGetValue(canvasId, out var surface))
            {
                surface.RefreshSize();
            }
        }

        private static void ConfigureGraphicsContext(Application application, Surface surface, GraphicsBackend graphicsBackend)
        {
            System.Diagnostics.Trace.WriteLine($"GraphicsBackend: {graphicsBackend}");
            var graphicsContext = new GLGraphicsContext(graphicsBackend);
            graphicsContext.CreateDevice();

            var swapChainDescription = new SwapChainDescription()
            {
                SurfaceInfo = surface.SurfaceInfo,
                Width = surface.Width,
                Height = surface.Height,
                ColorTargetFormat = PixelFormat.R8G8B8A8_UNorm,
                ColorTargetFlags = TextureFlags.RenderTarget | TextureFlags.ShaderResource,
                DepthStencilTargetFormat = PixelFormat.D24_UNorm_S8_UInt,
                DepthStencilTargetFlags = TextureFlags.DepthStencil,
                SampleCount = TextureSampleCount.None,
                IsWindowed = true,
                RefreshRate = 60
            };
            var swapChain = graphicsContext.CreateSwapChain(swapChainDescription);
            swapChain.VerticalSync = true;

            var graphicsPresenter = application.Container.Resolve<GraphicsPresenter>();
            var firstDisplay = new Display(surface, swapChain);
            graphicsPresenter.AddDisplay("DefaultDisplay", firstDisplay);

            application.Container.RegisterInstance(graphicsContext);
        }
    }
}

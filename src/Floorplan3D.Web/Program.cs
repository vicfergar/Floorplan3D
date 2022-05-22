using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.OpenGL;
using Evergine.Web;
using System.Collections.Generic;
using System.Diagnostics;

namespace Floorplan3D.Web
{
    public class Program
    {
        private static readonly Dictionary<string, WebSurface> appCanvas = new Dictionary<string, WebSurface>();

        private static WindowsSystem windowsSystem;
        private static MainApplication application;
        private static global::Evergine.Web.WebAssembly wasm;

        public static void Main()
        {
            // Hack for AOT dll dependencies
            var cp = new global::Evergine.Components.Graphics3D.Spinner();

            // Create app
            application = new MainApplication();

            // Create Services
            windowsSystem = new WebWindowsSystem();
            application.Container.RegisterInstance(windowsSystem);

            // Wasm instance need to be initialized here for debugger
            wasm = global::Evergine.Web.WebAssembly.GetInstance();
        }

        public static void Run(string canvasId)
        {
            var canvas = wasm.GetElementById(canvasId);
            var surface = (WebSurface)windowsSystem.CreateSurface(canvas);
            appCanvas[canvasId] = surface;

            var supportWebGL2 = canvas.Invoke<JSObject>("getContext", true, "webgl2") != null;
            var webGLBackend = supportWebGL2 ? GraphicsBackend.WebGL2 : GraphicsBackend.WebGL1;
            ConfigureGraphicsContext(application, surface, webGLBackend);
            application.Container.RegisterInstance(new WebMDITextureLoader(canvas, webGLBackend));

            // Audio is currently unsupported
            //var xaudio = new Evergine.XAudio2.XAudioDevice();
            //application.Container.RegisterInstance(xaudio);

            Stopwatch clockTimer = Stopwatch.StartNew();
            windowsSystem.Run(
                () =>
                {
                    application.Initialize();
                    wasm.Invoke("window._evergine_ready");
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
            // Enabled web canvas antialias (MSAA)
            wasm.Invoke("window._evergine_EGL");

            Trace.WriteLine($"GraphicsBackend: {graphicsBackend}");
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

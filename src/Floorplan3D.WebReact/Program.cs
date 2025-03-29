using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Services;
using Evergine.Serialization.Converters;
using Evergine.OpenGL;
using Evergine.Web;
using Floorplan3D;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Floorplan3D.Web;

public class Program
{
    // Wasm instance need to be initialized here for debugger
    public static readonly Evergine.Web.WebAssembly Wasm;
    public static readonly Dictionary<string, WebSurface> AppCanvas = new();

    public static WebWindowsSystem WindowsSystem;
    public static MainApplication Application;

    static Program()
    {
        Evergine.Web.WebAssembly.HostConfiguration = new HostConfiguration();
        Wasm = Evergine.Web.WebAssembly.GetInstance();
    }

    public static void Main()
    {
    }

    public static void Run(string canvasId)
    {
        // Enable Trace
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        Trace.AutoFlush = true;

        // Create app
        Application = new MainApplication();

        // Create Services
        WindowsSystem = new WebWindowsSystem();
        Application.Container.RegisterInstance(WindowsSystem);

        var canvas = Wasm.GetElementById(canvasId);
        var surface = (WebSurface)WindowsSystem.CreateSurface(canvas);
        AppCanvas[canvasId] = surface;
        ConfigureGraphicsContext(Application, surface, canvasId);

        var supportWebGL2 = canvas.Invoke<JSObject>("getContext", true, "webgl2") != null;
        var webGLBackend = supportWebGL2 ? GraphicsBackend.WebGL2 : GraphicsBackend.WebGL1;
        Application.Container.RegisterInstance(new WebMDITextureLoader(canvas, webGLBackend));

        //var audio = new Evergine.OpenAL.ALAudioDevice();
        //Application.Container.RegisterInstance(audio);

        Stopwatch clockTimer = Stopwatch.StartNew();
        WindowsSystem.Run(
            () =>
            {                
                Application.Initialize();

                var screenContextManager = Application.Container.Resolve<ScreenContextManager>();
                screenContextManager.OnActivatingScene += ScreenContextManager_OnActivatingScene;
                screenContextManager.OnDesactivatingScene += ScreenContextManager_OnDesactivatingScene;
            },
            () =>
            {
                var gameTime = clockTimer.Elapsed;
                clockTimer.Restart();
                Application.UpdateFrame(gameTime);
                Application.DrawFrame(gameTime);
            });
    }

    private static void ScreenContextManager_OnActivatingScene(Scene scene)
    {
        Wasm.Invoke("App.appEventsListener.onEvergineReady", false, true);
    }

    private static void ScreenContextManager_OnDesactivatingScene(Scene scene)
    {
        Wasm.Invoke("App.appEventsListener.onEvergineReady", false, false);
    }

    private static void ConfigureGraphicsContext(Application application, Surface surface, string canvasId)
    {
        // Enabled web canvas antialias (MSAA)
        Wasm.Invoke("window._evergine_EGL", false, "webgl2", canvasId);

        GraphicsContext graphicsContext = new GLGraphicsContext(GraphicsBackend.WebGL2);
        graphicsContext.CreateDevice();
        SwapChainDescription swapChainDescription = new SwapChainDescription()
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
        swapChain.FrameBuffer.IntermediateBufferAssociated = false;

        var graphicsPresenter = application.Container.Resolve<GraphicsPresenter>();
        var firstDisplay = new Display(surface, swapChain);
        graphicsPresenter.AddDisplay("DefaultDisplay", firstDisplay);

        application.Container.RegisterInstance(graphicsContext);
    }

    private class HostConfiguration : IWasmHostConfiguration
    {
        public void ConfigureHost(WebAssemblyHostBuilder builder)
        {
        }

        public void RegisterJsonConverters(IList<JsonConverter> converters)
        {
            converters.AddEvergineConverters();
        }
    }
}


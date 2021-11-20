using Evergine.Common.Graphics;
using Evergine.Web;
using Floorplan3D.Features.IconTiles;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WaveEngine.Bindings.OpenGL;

namespace Floorplan3D.Web
{
    public class WebMDITextureLoader : BaseMDITextureLoader
    {
        private readonly SemaphoreSlim imageLoadSemaphore = new SemaphoreSlim(1);

        private Action<JSObject> onImageLoadedAction;

        private Action<JSObject> onImageErrorAction;

        private JSObject gl;

        private JSObject imgElement;

        private TaskCompletionSource<Texture> activeImageLoadTCS;

        private Func<DataBox[], Texture> textureFactory;

        public WebMDITextureLoader(JSObject canvas, GraphicsBackend webGLBackend)
        {
            this.gl = canvas.Invoke<JSObject>("getContext", true, webGLBackend == GraphicsBackend.WebGL1 ? "webgl" : "webgl2");
            Debug.WriteLine($"{webGLBackend} glContext: {gl}");
            this.onImageLoadedAction = new Action<JSObject>(this.OnImageLoaded);
            this.onImageErrorAction = new Action<JSObject>(this.OnImageError);

            var wasm = Evergine.Web.WebAssembly.GetInstance();
            this.imgElement = wasm.Invoke<JSObject>("eval", true, $"(new Image({TEXTURE_SIZE}, {TEXTURE_SIZE}))");
            this.imgElement.SetObjectProperty("crossOrigin", string.Empty);
            this.imgElement.AddEventListener("load", this.onImageLoadedAction);
            this.imgElement.AddEventListener("error", this.onImageErrorAction);
        }

        protected override async Task<Texture> CreateTextureAsync(string svgContent, Func<DataBox[], Texture> textureFactory)
        {
            var encodedContent = Uri.EscapeDataString(svgContent);
            await this.imageLoadSemaphore.WaitAsync();

            try
            {
                this.textureFactory = textureFactory;
                this.activeImageLoadTCS = new TaskCompletionSource<Texture>();
                this.imgElement.SetObjectProperty("src", $"data:image/svg+xml,{encodedContent}");
                var texture = await this.activeImageLoadTCS.Task;
                this.activeImageLoadTCS = null;
                return texture;
            }
            finally
            {
                this.imageLoadSemaphore.Release();
            }
        }

        private void OnImageLoaded(JSObject obj)
        {
            var texture = this.textureFactory(null);
            GL.glBindTexture(TextureTarget.Texture2d, (uint)texture.NativePointer);
            this.gl.Invoke("texSubImage2D",
                    true,
                    (uint)TextureTarget.Texture2d,
                    0,
                    0,
                    0,
                    (uint)WaveEngine.Bindings.OpenGL.PixelFormat.Rgba,
                    (uint)ColorPointerType.UnsignedByte,
                    this.imgElement.Reference);

            this.activeImageLoadTCS.SetResult(texture);
        }

        private void OnImageError(JSObject errorEvent)
        {
            var errorMessage = errorEvent.GetObjectProperty<string>("message");
            this.activeImageLoadTCS.SetException(new Exception($"An error occurred while loading the image: {errorMessage}"));
        }
    }
}

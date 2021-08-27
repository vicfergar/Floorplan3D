using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Floorplan3D.Features.IconTiles;
using WaveEngine.Bindings.OpenGL;
using WaveEngine.Common.Graphics;
using WebAssembly;
using WebAssembly.Host;

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
            this.gl = (JSObject)canvas.Invoke("getContext", webGLBackend == GraphicsBackend.WebGL1 ? "webgl" : "webgl2");
            Debug.WriteLine($"{webGLBackend} glContext: {gl}");
            this.onImageLoadedAction = new Action<JSObject>(this.OnImageLoaded);
            this.onImageErrorAction = new Action<JSObject>(this.OnImageError);

            this.imgElement = new HostObject("Image", TEXTURE_SIZE, TEXTURE_SIZE);
            this.imgElement.SetObjectProperty("crossOrigin", string.Empty);
            this.imgElement.Invoke("addEventListener", "load", this.onImageLoadedAction);
            this.imgElement.Invoke("addEventListener", "error", this.onImageErrorAction);
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
                    (uint)TextureTarget.Texture2d,
                    0,
                    0,
                    0,
                    (uint)WaveEngine.Bindings.OpenGL.PixelFormat.Rgba,
                    (uint)ColorPointerType.UnsignedByte,
                    this.imgElement);

            this.activeImageLoadTCS.SetResult(texture);
        }

        private void OnImageError(JSObject errorEvent)
        {
            var errorMessage = errorEvent.GetObjectProperty("message");
            this.activeImageLoadTCS.SetException(new Exception($"An error occurred while loading the image: {errorMessage}"));
        }
    }
}

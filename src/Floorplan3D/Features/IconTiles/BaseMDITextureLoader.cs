using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Services;

namespace Floorplan3D.Features.IconTiles
{
    public abstract class BaseMDITextureLoader : Service
    {
        private const string baseUrl = "https://raw.githubusercontent.com/Templarian/MaterialDesign-SVG/v5.5.55/svg/";

        protected const uint TEXTURE_SIZE = 100;

        [BindService]
        protected GraphicsContext graphicsContext;

        private HttpClient httpClient = new HttpClient();

        private ConcurrentDictionary<string, Task<Texture>> iconTexturesById = new ConcurrentDictionary<string, Task<Texture>>();

        public Task<Texture> LoadIconTextureAsync(string id)
        {
            var loadingTask = this.iconTexturesById.GetOrAdd(id, (key) => this.CreateNewIconTextureAsync(key));
            return loadingTask;
        }

        private async Task<Texture> CreateNewIconTextureAsync(string iconId)
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif

            Texture result = null;
            try
            {
                result = await this.DownloadIconTextureAsync(iconId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"An error occurred while donwloading icon {iconId}: {ex.Message}");
            }
#if DEBUG
            sw.Stop();
            Debug.WriteLine($"CreateNewIconTextureAsync {iconId} Succeed: {result != null} => {sw.ElapsedMilliseconds}ms");
#endif

            return result;
        }

        private async Task<Texture> DownloadIconTextureAsync(string iconId)
        {
            var request = $"{baseUrl}{iconId}.svg";
            var svgContent = await this.httpClient.GetStringAsync(request);
            this.SetSVGFillColor(ref svgContent, "white");

            return await this.InternalCreateTextureAsync(svgContent);
        }

        private void SetSVGFillColor(ref string svgContent, string color)
        {
            var regex = new Regex("fill=\"(.*?)\"");
            if (regex.IsMatch(svgContent))
            {
                svgContent = regex.Replace(svgContent, $"fill=\"{color}\"");
            }
            else
            {
                svgContent = svgContent.Replace("<svg", $"<svg fill=\"{color}\"");
            }
        }

        private Task<Texture> InternalCreateTextureAsync(string svgContent)
        {
            var textureDescription = new TextureDescription()
            {
                Type = TextureType.Texture2D,
                Usage = ResourceUsage.Default,
                Flags = TextureFlags.RenderTarget | TextureFlags.ShaderResource,
                Format = PixelFormat.R8G8B8A8_UNorm,
                Width = TEXTURE_SIZE,
                Height = TEXTURE_SIZE,
                Depth = 1,
                MipLevels = 1,
                ArraySize = 1,
                Faces = 1,
                CpuAccess = ResourceCpuAccess.None,
                SampleCount = TextureSampleCount.None,
            };

            var samplerState = SamplerStates.LinearClamp;
            return this.CreateTextureAsync(svgContent, (data) => this.graphicsContext.Factory.CreateTexture(data, ref textureDescription, ref samplerState));
        }

        protected abstract Task<Texture> CreateTextureAsync(string svgContent, Func<DataBox[], Texture> textureFactory);
    }
}

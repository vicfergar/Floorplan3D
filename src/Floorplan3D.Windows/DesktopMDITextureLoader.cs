using Svg;
using System.Drawing;
using Imaging = System.Drawing.Imaging;
using System.Runtime.InteropServices;
using WaveEngine.Common.Graphics;
using Floorplan3D.Features.IconTiles;
using System.Threading.Tasks;
using System;

namespace Floorplan3D.Windows
{
    public class DesktopMDITextureLoader : BaseMDITextureLoader
    {
        protected override Task<Texture> CreateTextureAsync(string svgContent, Func<DataBox[], Texture> textureFactory)
        {
            var svgDocument = SvgDocument.FromSvg<SvgDocument>(svgContent);
            var image = svgDocument.Draw((int)TEXTURE_SIZE, (int)TEXTURE_SIZE);

            var bitmapData = image.LockBits(new Rectangle(Point.Empty, image.Size), Imaging.ImageLockMode.ReadOnly, image.PixelFormat);
            var length = bitmapData.Stride * bitmapData.Height;
            var bytes = new byte[length];
            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            image.UnlockBits(bitmapData);

            var databoxes = new DataBox[] { new DataBox(bytes, (uint)image.Width * 4, (uint)image.Width * (uint)image.Height * 4) };
            var texture = textureFactory(databoxes);
            return Task.FromResult(texture);
        }
    }
}

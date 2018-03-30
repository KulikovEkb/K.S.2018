using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    sealed class ClockwiseRotation : Transformation
    {
        public ClockwiseRotation(Bitmap originalImage, RectangleWithLongs requestedArea) : base(originalImage, requestedArea) { }

        public override Bitmap ApplyTransformation()
        {
            ImageToTransform.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return ImageToTransform.Clone(ResultArea, PixelFormat.Format32bppArgb);
        }
    }
}

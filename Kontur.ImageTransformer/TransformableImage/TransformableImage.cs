using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    class TransformableImage : IDisposable
    {
        private bool disposed;

        private Transformation Transformation { get; }
        private RectangleWithLongs RequestedArea { get; }
        private Bitmap ImageToTransform { get; }
        public Bitmap TransformedImage { get; private set; }

        public TransformableImage(Transformation transformation, RectangleWithLongs requestedArea, Bitmap imageToTransform)
        {
            RequestedArea = requestedArea;
            ImageToTransform = imageToTransform;
            Transformation = transformation;
        }

        public void ApplyTransformation()
        {
            TransformedImage = Transformation.ApplyTransformation();
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ImageToTransform?.Dispose();
                    TransformedImage?.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TransformableImage()
        {
            Dispose(false);
        }
    }
}

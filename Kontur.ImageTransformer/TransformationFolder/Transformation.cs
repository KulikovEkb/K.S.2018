using System;
using System.Drawing;

namespace Kontur.ImageTransformer
{
    abstract class Transformation : IDisposable
    {
        private bool disposed;
        protected Rectangle resultArea;

        protected RectangleWithLongs RequestedArea { get; }
        protected Rectangle ResultArea
        {
            get
            {
                resultArea = RectangleWithLongs.Intersect(ImageToTransform.Size, RequestedArea);
                if (resultArea.IsEmpty)
                {
                    throw new Exception("Requested area is out of image bounds.");
                }
                return resultArea;
            }
        }
        protected Bitmap ImageToTransform { get; set; }

        public Transformation(Bitmap originalImage, RectangleWithLongs requestedArea)
        {
            RequestedArea = requestedArea;
            ImageToTransform = originalImage;
        }

        public abstract Bitmap ApplyTransformation();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ImageToTransform?.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Transformation()
        {
            Dispose(false);
        }
    }
}

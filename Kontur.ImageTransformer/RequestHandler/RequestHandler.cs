using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;

namespace Kontur.ImageTransformer
{
    class RequestHandler : IDisposable
    {
        private const long MaxCoordinatesValue = 2147483648; //2^31
        private const int MaxImageWeight = 102400; //100Kb

        private bool disposed;

        public RectangleWithLongs RequestedArea { get; }
        public Bitmap ImageToHandle { get; }

        protected RequestHandler(HttpListenerRequest request)
        {
            ThrowIfRequestIncorrect(request);
            RequestedArea = GetCoordsFromRequest(request.RawUrl);
            ImageToHandle = GetBitmapFromRequestBody(request.InputStream);
        }

        private void ThrowIfRequestIncorrect(HttpListenerRequest request)
        {
            if (request.ContentLength64 <= 0 || request.ContentLength64 > MaxImageWeight)
            {
                throw new Exception("Incorrect request content length.");
            }

            if (request.HttpMethod != "POST")
            {
                throw new Exception("Incorrect request method.");
            }

            if (!request.RawUrl.StartsWith("/process/") || request.RawUrl.CountBackslashesInString() != 3)
            {
                throw new Exception("Incorrect request.");
            }
        }

        private RectangleWithLongs GetCoordsFromRequest(string requestRawUrl)
        {
            long[] coords;
            try
            {
                coords = requestRawUrl
                    .Substring(requestRawUrl.LastIndexOf('/') + 1)
                    .Split(',')
                    .Select(long.Parse)
                    .ToArray();
            }
            catch
            {
                throw new Exception("Incorrect coordinates.");
            }
            if (coords.Length != 4 || coords.Any(x => Math.Abs(x) > MaxCoordinatesValue))
            {
                throw new Exception("Incorrect coordinates.");
            }
            return new RectangleWithLongs(coords[0], coords[1], coords[2], coords[3]);
        }

        private Bitmap GetBitmapFromRequestBody(Stream imageStream)
        {
            Bitmap requestBitmap;
            try
            {
                requestBitmap = new Bitmap(imageStream);
            }
            catch (Exception)
            {
                throw new Exception("Incorrect request body.");
            }

            if (!ImageFormat.Png.Equals(requestBitmap.RawFormat))
            {
                throw new Exception("Incorrect image format.");
            }

            if (requestBitmap.Width > 1000 || requestBitmap.Height > 1000)
            {
                throw new Exception("Incorrect image size.");
            }

            return requestBitmap;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ImageToHandle?.Dispose();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RequestHandler()
        {
            Dispose(false);
        }
    }
}

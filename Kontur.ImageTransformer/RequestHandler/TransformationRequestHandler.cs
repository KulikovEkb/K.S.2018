using System;
using System.Net;

namespace Kontur.ImageTransformer
{
    sealed class TransformationRequestHandler : RequestHandler
    {
        public Transformation Transformation { get; }

        public TransformationRequestHandler(HttpListenerRequest request) : base(request)
        {
            Transformation = GetTransformationFromRequest(request.RawUrl);
        }

        private Transformation GetTransformationFromRequest(string requestRawUrl)
        {
            string transformationName = requestRawUrl
                .Remove(requestRawUrl.LastIndexOf('/'))
                .Substring("/process/".Length);

            switch (transformationName)
            {
                case "rotate-cw":
                    return new ClockwiseRotation(ImageToHandle, RequestedArea);
                case "rotate-ccw":
                    return new CounterClockwiseRotation(ImageToHandle, RequestedArea);
                case "flip-h":
                    return new HorizontalFlip(ImageToHandle, RequestedArea);
                case "flip-v":
                    return new VerticalFlip(ImageToHandle, RequestedArea);
                default:
                    throw new Exception("Incorrect transformation.");
            }
        }

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Transformation?.Dispose();
            }

            _disposed = true;

            base.Dispose(disposing);
        }
    }
}

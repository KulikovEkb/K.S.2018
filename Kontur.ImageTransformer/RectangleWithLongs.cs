using System;
using System.Drawing;

namespace Kontur.ImageTransformer
{
    public struct RectangleWithLongs
    {
        private long X { get; set; }
        private long Y { get; set; }
        private long Width { get; set; }
        private long Height { get; set; }

        public bool IsEmpty
        {
            get
            {
                return Height == 0 && Width == 0 && X == 0 && Y == 0;
            }
        }

        public RectangleWithLongs(long x, long y, long width, long height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public static Rectangle Intersect(Size imageSize, RectangleWithLongs requestedArea)
        {
            if (requestedArea.Width < 0)
            {
                requestedArea.X = requestedArea.Width + requestedArea.X;
                requestedArea.Width *= -1;
            }
            if (requestedArea.Height < 0)
            {
                requestedArea.Y = requestedArea.Height + requestedArea.Y;
                requestedArea.Height *= -1;
            }

            long x1 = Math.Max(0, requestedArea.X);
            long x2 = Math.Min(0 + imageSize.Width, requestedArea.X + requestedArea.Width);
            long y1 = Math.Max(0, requestedArea.Y);
            long y2 = Math.Min(0 + imageSize.Height, requestedArea.Y + requestedArea.Height);

            if (x2 >= x1 && y2 >= y1 && (x2 - x1) != 0 && (y2 - y1) != 0)
            {
                return new Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
            }

            return Rectangle.Empty;
        }

        public static Rectangle Intersect(Rectangle imageArea, RectangleWithLongs requestedArea)
        {
            if (requestedArea.Width < 0)
            {
                requestedArea.X = requestedArea.Width + requestedArea.X;
                requestedArea.Width *= -1;
            }
            if (requestedArea.Height < 0)
            {
                requestedArea.Y = requestedArea.Height + requestedArea.Y;
                requestedArea.Height *= -1;
            }

            long x1 = Math.Max(imageArea.X, requestedArea.X);
            long x2 = Math.Min(imageArea.X + imageArea.Width, requestedArea.X + requestedArea.Width);
            long y1 = Math.Max(imageArea.Y, requestedArea.Y);
            long y2 = Math.Min(imageArea.Y + imageArea.Height, requestedArea.Y + requestedArea.Height);

            if (x2 >= x1 && y2 >= y1 && (x2 - x1) != 0 && (y2 - y1) != 0)
            {
                return new Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
            }

            return Rectangle.Empty;
        }

        public static Rectangle Intersect(RectangleWithLongs imageArea, RectangleWithLongs requestedArea)
        {
            if (requestedArea.Width < 0)
            {
                requestedArea.X = requestedArea.Width + requestedArea.X;
                requestedArea.Width *= -1;
            }
            if (requestedArea.Height < 0)
            {
                requestedArea.Y = requestedArea.Height + requestedArea.Y;
                requestedArea.Height *= -1;
            }

            long x1 = Math.Max(imageArea.X, requestedArea.X);
            long x2 = Math.Min(imageArea.X + imageArea.Width, requestedArea.X + requestedArea.Width);
            long y1 = Math.Max(imageArea.Y, requestedArea.Y);
            long y2 = Math.Min(imageArea.Y + imageArea.Height, requestedArea.Y + requestedArea.Height);

            if (x2 >= x1 && y2 >= y1 && (x2 - x1) != 0 && (y2 - y1) != 0)
            {
                return new Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
            }

            return Rectangle.Empty;
        }
    }
}
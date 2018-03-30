using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Kontur.ImageTransformer
{
    [TestFixture]
    class RequestCoordsTestClass
    {
        AsyncHttpServer server = new AsyncHttpServer();
        HttpWebRequest request;
        Bitmap testBitmap;

        [OneTimeSetUp]
        public void StartServer()
        {
            server.Start("http://+:8080/");
        }

        [OneTimeTearDown]
        public void DisposeServer()
        {
            server.Dispose();
        }

        private void SetRequest(string uri, string method = "POST")
        {
            request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.ContentType = "application/octet-stream";
            testBitmap = new Bitmap(100, 100);
            Graphics flagGraphics = Graphics.FromImage(testBitmap);
            int red = 0;
            int white = 10;
            while (white <= 100)
            {
                flagGraphics.FillRectangle(Brushes.Red, 0, red, 100, 10);
                flagGraphics.FillRectangle(Brushes.White, 0, white, 100, 10);
                red += 20;
                white += 20;
            }
            using (Stream requestStream = request.GetRequestStream())
            {
                testBitmap.Save(requestStream, ImageFormat.Png);
                requestStream.Close();
            }
        }

        private long GetResponseContentLength(Stream responseStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                responseStream.CopyTo(memoryStream);
                return memoryStream.Length;
            }
        }

        [Test]
        public void TestRequestWithoutCoords()
        {
            SetRequest("http://localhost:8080/process/grayscale");

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [TestCase("http://localhost:8080/process/grayscale/-5,-30,200")]
        [TestCase("http://localhost:8080/process/grayscale/-5,-30,200,50,50")]
        [TestCase("http://localhost:8080/process/grayscale/,,,")]
        [TestCase("http://localhost:8080/process/grayscale/-5-3020050")]
        [TestCase("http://localhost:8080/process/grayscale/-5,-30,200,2147483649")]
        [TestCase("http://localhost:8080/process/grayscale/-5,-30,200,-2147483649")]
        public void TestRequestWithWrongCoordsFormat(string requestPath)
        {
            SetRequest(requestPath);

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [TestCase("http://localhost:8080/process/grayscale/0,0,0,0")]
        [TestCase("http://localhost:8080/process/grayscale/0,0,0,100")]
        [TestCase("http://localhost:8080/process/grayscale/0,0,100,0")]
        [TestCase("http://localhost:8080/process/grayscale/0,0,0,-100")]
        [TestCase("http://localhost:8080/process/grayscale/0,0,-100,0")]
        [TestCase("http://localhost:8080/process/grayscale/100,100,0,0")]
        [TestCase("http://localhost:8080/process/grayscale/100,100,100,100")]
        [TestCase("http://localhost:8080/process/grayscale/-5,-30,4,29")]
        [TestCase("http://localhost:8080/process/grayscale/-5,-30,5,30")]
        [TestCase("http://localhost:8080/process/grayscale/-5,-30,-100,-100")]
        public void TestRequestWithEmptyIntersection(string requestPath)
        {
            SetRequest(requestPath);
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [Test]
        public void TestRectangleWithLongsIntersection()
        {
            Size testSize = new Size(100, 100);

            Assert.AreEqual(new Rectangle(0, 0, 100, 100),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, 100, 100)));
            Assert.AreEqual(new Rectangle(0, 0, 100, 100),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, 200, 200)));
            Assert.AreEqual(new Rectangle(0, 0, 100, 100),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, 200, 100)));
            Assert.AreEqual(new Rectangle(0, 0, 100, 100),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, 100, 200)));
            Assert.AreEqual(new Rectangle(0, 0, 30, 40),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, 30, 40)));
            Assert.AreEqual(new Rectangle(30, 40, 10, 10),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(30, 40, 10, 10)));

            Assert.AreEqual(new Rectangle(0, 0, 30, 40),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(-5, 0, 35, 40)));
            Assert.AreEqual(new Rectangle(0, 0, 30, 40),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, -5, 30, 45)));
            Assert.AreEqual(new Rectangle(0, 0, 100, 20),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(-5, -30, 200, 50)));

            Assert.AreEqual(new Rectangle(30, 0, 20, 80),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(50, 80, -20, -150)));
            Assert.AreEqual(new Rectangle(0, 0, 100, 100),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(100, 100, -100, -100)));

            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, 0, 0)));
            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, 0, 100)));
            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, 100, 0)));
            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, 0, -100)));
            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(0, 0, -100, 0)));
            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(100, 100, 0, 0)));
            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(100, 100, 100, 100)));
            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(-5, -30, 4, 29)));
            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(-5, -30, 5, 30)));
            Assert.AreEqual(new Rectangle(0, 0, 0, 0),
                RectangleWithLongs.Intersect(testSize, new RectangleWithLongs(-5, -30, -100, -100)));
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using NUnit.Framework;

namespace Kontur.ImageTransformer
{
    [TestFixture]
    class RequestTestClass
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
        public void TestCorrectRequest()
        {
            SetRequest("http://localhost:8080/process/grayscale/-5,-30,200,50");

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) > 0);
        }

        [Test]
        public void TestRequestWithoutProcessWord()
        {
            SetRequest("http://localhost:8080/grayscale/-5,-30,200,50");

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [Test]
        public void TestRequestWithOnlyProcessWord()
        {
            SetRequest("http://localhost:8080/process/");

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [Test]
        public void TestRequestWithoutSlashes()
        {
            SetRequest("http://localhost:8080/processgrayscale-5,-30,200,50");

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [Test]
        public void TestRequestWithDoubleSlashes()
        {
            SetRequest("http://localhost:8080//process//grayscale//-5,-30,200,50");

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [Test]
        public void TestRequestWithSlashesOnly()
        {
            SetRequest("http://localhost:8080///");

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [Test]
        public void TestRequestWithUpperCase()
        {
            SetRequest("http://localhost:8080/PROCESS/GRAYSCALE/-5,-30,200,50");

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [Test]
        public void TestRequestWithWrongMethod()
        {
            SetRequest("http://localhost:8080/process/grayscale/-5,-30,200,50", "PUT");

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }
    }
}

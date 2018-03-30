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
    class RequestFilterTestClass
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
        public void TestRequestWithoutFilter()
        {
            SetRequest("http://localhost:8080/process/-5,-30,200,50");

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [TestCase("http://localhost:8080/process/grayscale/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/sepia/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/threshold(30)/-5,-30,200,50")]
        public void TestRequestWithCorrectFilters(string requestPath)
        {
            SetRequest(requestPath);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) > 0);
        }

        [TestCase("http://localhost:8080/process/gray/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/sep/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/thres/-5,-30,200,50")]
        public void TestRequestWithWrongFilters(string requestPath)
        {
            SetRequest(requestPath);

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [TestCase("http://localhost:8080/process/threshold(abc)/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/threshold(-1)/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/threshold(101)/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/threshold(5.5)/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/threshold(3147483648)/-5,-30,200,50")]
        public void TestRequestWithWrongThresholdValue(string requestPath)
        {
            SetRequest(requestPath);

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }

        [TestCase("http://localhost:8080/process/threshold/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/threshold()/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/threshold(/-5,-30,200,50")]
        [TestCase("http://localhost:8080/process/threshold)/-5,-30,200,50")]
        public void TestRequestWithEmptyThresholdValue(string requestPath)
        {
            SetRequest(requestPath);

            WebException ex = Assert.Throws<WebException>(() => request.GetResponse());
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.AreEqual(true, GetResponseContentLength(response.GetResponseStream()) <= 0);
        }
    }
}

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

namespace Kontur.ImageTransformer.Tests
{
    [TestFixture]
    class FilterAlgorithmsTestClass
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

        [Test]
        public void TestGrayscaleFilterAlgorithm()
        {
            //intensity = (oldR + oldG + oldB) / 3
            //R = intensity
            //G = intensity
            //B = intensity

            int redIntensity = (255 + 0 + 0) / 3;
            int whiteIntensity = (255 + 255 + 255) / 3;
            SetRequest("http://localhost:8080/process/grayscale/0,0,100,100");
            Bitmap responseBitmap;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream respponseStream = response.GetResponseStream())
            {
                responseBitmap = new Bitmap(respponseStream);
            }
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(Color.FromArgb(255, redIntensity, redIntensity, redIntensity), responseBitmap.GetPixel(0, 0));
            Assert.AreEqual(Color.FromArgb(255, whiteIntensity, whiteIntensity, whiteIntensity), responseBitmap.GetPixel(0, 10));
        }

        [Test]
        public void TestSepiaFilterAlgorithm()
        {
            //R = (oldR * .393) + (oldG * .769) + (oldB * .189)
            //G = (oldR * .349) + (oldG * .686) + (oldB * .168)
            //B = (oldR * .272) + (oldG * .534) + (oldB * .131)
            //При присвоении новых значений компонентам цвета дробная часть отбрасывается, 
            //если число получилось больше 255 — оно заменяется на 255.

            int redRedValue = (int)(255 * .393 + 0 + 0);
            int redGreenValue = (int)(255 * .349 + 0 + 0);
            int redBlueValue = (int)(255 * .272 + 0 + 0);

            int whiteRedValue = (int)(255 * .393 + 255 * .769 + 255 * .189);
            int whiteGreenValue = (int)(255 * .349 + 255 * .686 + 255 * .168);
            int whiteBlueValue = (int)(255 * .272 + 255 * .534 + 255 * .131);
            if (whiteRedValue > 255) whiteRedValue = 255;
            if (whiteGreenValue > 255) whiteGreenValue = 255;
            if (whiteBlueValue > 255) whiteBlueValue = 255;

            SetRequest("http://localhost:8080/process/sepia/0,0,100,100");
            Bitmap responseBitmap;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                responseBitmap = new Bitmap(responseStream);
            }
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(Color.FromArgb(255, redRedValue, redGreenValue, redBlueValue), responseBitmap.GetPixel(0, 0));
            Assert.AreEqual(Color.FromArgb(255, whiteRedValue, whiteGreenValue, whiteBlueValue), responseBitmap.GetPixel(0, 10));
        }

        [Test]
        public void TestThreshold30FilterAlgorithm()
        {
            //intensity = (oldR + oldG + oldB) / 3
            //if intensity >= 255 * x / 100
            //R = 255
            //G = 255
            //B = 255
            //else
            //R = 0
            //G = 0
            //B = 0

            int redIntensity = (255 + 0 + 0) / 3;
            int whiteIntensity = (255 + 255 + 255) / 3;
            if (redIntensity >= 255 * 30 / 100) redIntensity = 255; else redIntensity = 0;
            if (whiteIntensity >= 255 * 30 / 100) whiteIntensity = 255; else whiteIntensity = 0;

            SetRequest("http://localhost:8080/process/threshold(30)/0,0,100,100");
            Bitmap responseBitmap;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                responseBitmap = new Bitmap(responseStream);
            }
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(Color.FromArgb(255, redIntensity, redIntensity, redIntensity), responseBitmap.GetPixel(0, 0));
            Assert.AreEqual(Color.FromArgb(255, whiteIntensity, whiteIntensity, whiteIntensity), responseBitmap.GetPixel(0, 10));
        }

        [Test]
        public void TestThreshold90FilterAlgorithm()
        {
            //intensity = (oldR + oldG + oldB) / 3
            //if intensity >= 255 * x / 100
                //R = 255
                //G = 255
                //B = 255
            //else
                //R = 0
                //G = 0
                //B = 0

            int redIntensity = (255 + 0 + 0) / 3;
            int whiteIntensity = (255 + 255 + 255) / 3;
            if (redIntensity >= 255 * 90 / 100) redIntensity = 255; else redIntensity = 0;
            if (whiteIntensity >= 255 * 90 / 100) whiteIntensity = 255; else whiteIntensity = 0;

            SetRequest("http://localhost:8080/process/threshold(90)/0,0,100,100");
            Bitmap responseBitmap;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                responseBitmap = new Bitmap(responseStream);
            }
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(Color.FromArgb(255, redIntensity, redIntensity, redIntensity), responseBitmap.GetPixel(0, 0));
            Assert.AreEqual(Color.FromArgb(255, whiteIntensity, whiteIntensity, whiteIntensity), responseBitmap.GetPixel(0, 10));
        }
    }
}

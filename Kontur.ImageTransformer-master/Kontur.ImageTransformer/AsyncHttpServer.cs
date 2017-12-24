using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {
        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;

        public AsyncHttpServer()
        {
            listener = new HttpListener();
        }

        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning)
                {
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    listenerThread.Start();

                    isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();

                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }

        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        var context = listener.GetContext();
                        Task.Run(() => HandleContextAsync(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    // TODO: log errors
                }
            }
        }

        private async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            // TODO: implement request handling

            //using (Stream output = File.OpenWrite(@"C:\Users\Mega\Pictures\file.png"))
            //{

            //}
            Stream output = new MemoryStream();
            using (Stream input = listenerContext.Request.InputStream)
            {
                input.CopyTo(output);
            }
            Image img = Image.FromStream(output);
            img.Save(@"file2.png");
            output.Close();

            var uri = listenerContext.Request.RawUrl.Split('/');
            foreach (var param in uri)
            {
                Console.WriteLine(param);
            }

            listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
            using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                writer.WriteLine("Hello, world!");
        }

        private void DrawImagePoint(PaintEventArgs e)
        {
            // Create image.
            Image newImage = Image.FromFile("SampImag.jpg");

            // Create Point for upper-left corner of image.
            Point ulCorner = new Point(100, 100);

            // Draw image to screen.
            e.Graphics.DrawImage(newImage, ulCorner);
        }
    }
}
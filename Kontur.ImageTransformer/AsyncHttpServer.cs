using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {
        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
        private SemaphoreSlim semaphore;

        public AsyncHttpServer()
        {
            int semaphoreValue = Environment.ProcessorCount - 1;
            listener = new HttpListener();
            semaphore = new SemaphoreSlim(semaphoreValue, semaphoreValue);
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
                    Console.WriteLine($"Service is up and running at {listener.Prefixes.Last()}");
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
                        if (semaphore.CurrentCount == 0)
                        {
                            ReturnResponse(context, 429, "Too Many Requests", null);
                            continue;
                        }
                        semaphore.Wait();
                        Task.Run(() => HandleContext(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    EntryPoint.log.Error(error, error.Message);
                }
            }
        }

        private void HandleContext(HttpListenerContext listenerContext)
        {
            try
            {
                using (TransformationRequestHandler transReqHandler = new TransformationRequestHandler(listenerContext.Request))
                {
                    using (TransformableImage transformableImage = new TransformableImage(transReqHandler.Transformation, transReqHandler.RequestedArea, transReqHandler.ImageToHandle))
                    {
                        transformableImage.ApplyTransformation();

                        ReturnResponse(listenerContext, (int)HttpStatusCode.OK, "OK", transformableImage.TransformedImage);
                    }
                }
            }
            catch (Exception exc)
            {
                switch (exc.Message)
                {
                    case "Too Many Requests.":
                        ReturnResponse(listenerContext, 429, "Too Many Requests", null);
                        break;
                    case "Requested area is out of image bounds.":
                        ReturnResponse(listenerContext, (int)HttpStatusCode.NoContent, "NoContent", null);
                        break;
                    default:
                        ReturnResponse(listenerContext, (int)HttpStatusCode.BadRequest, "BadRequest", null);
                        break;
                }
                EntryPoint.log.Error(exc, exc.Message);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static void ReturnResponse(HttpListenerContext listenerContext, int statusCode, string statusDescription, Bitmap resultImage)
        {
            listenerContext.Response.StatusCode = statusCode;
            listenerContext.Response.StatusDescription = statusDescription;
            listenerContext.Response.ContentType = "application/octet-stream";
            resultImage?.Save(listenerContext.Response.OutputStream, ImageFormat.Png);
            listenerContext.Response.OutputStream.Close();
        }
    }
}
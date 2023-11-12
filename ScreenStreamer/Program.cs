using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using Microsoft.Owin.Hosting;
using Owin;

namespace ScreenStreamer
{
    class Program
    {
        private const int ScreenWidth = 1920;
        private const int ScreenHeight = 1080;
        private const int FrameRate = 30;
        private const int Bitrate = 1000000;
        private const string OutputUrl = "http://localhost:8080/live/stream.m3u8";

        static void Main(string[] args)
        {
            FFmpegBinariesHelper.RegisterFFmpegBinaries();

            using (WebApp.Start<Startup>(OutputUrl))
            {
                Console.WriteLine($"Server running at {OutputUrl}");
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Use((context, next) =>
            {
                if (context.Request.Path.Value.EndsWith(".m3u8"))
                {
                    context.Response.ContentType = "application/vnd.apple.mpegurl";
                    using (var writer = new StreamWriter(context.Response.Body))
                    {
                        writer.Write("#EXTM3U\n");
                        writer.Write("#EXTINF:-1,Stream\n");
                        writer.Write("http://localhost:8080/live/stream.ts\n");
                    }
                    return Task.CompletedTask;
                }
                return next.Invoke();
            });

            appBuilder.Run((context) =>
            {
                context.Response.ContentType = "video/mp2t";
                using (var writer = new BinaryWriter(context.Response.Body))
                {
                    var screenCapture = new ScreenCapture();
                    screenCapture.Start(writer);
                }
            });
        }
    }

    public class ScreenCapture
    {
        private readonly Thread captureThread;
        private readonly BinaryWriter writer;

        public ScreenCapture()
        {
            captureThread = new Thread(CaptureScreen);
            writer = null;
        }

        public void Start(BinaryWriter writer)
        {
            this.writer = writer;
            captureThread.Start();
        }

        private void CaptureScreen()
        {
            var frameInterval = TimeSpan.FromSeconds(1.0 / FrameRate);
            var stopwatch = new Stopwatch();

            while (true)
            {
                stopwatch.Restart();

                using (var bitmap = new Bitmap(ScreenWidth, ScreenHeight))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(0, 0, 0, 0, new Size(ScreenWidth, ScreenHeight));
                    WriteFrame(bitmap);
                }

                var elapsed = stopwatch.Elapsed;
                if (elapsed < frameInterval)
                {
                    Thread.Sleep(frameInterval - elapsed);
                }
            }
        }

        private void WriteFrame(Bitmap bitmap)
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                       ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            try
            {
                var inputFrame = FFmpegHelper.CreateFrame(data.Scan0, AVPixelFormat.AV_PIX_FMT_BGR24,
                                                          bitmap.Width, bitmap.Height);
                FFmpegHelper.EncodeFrame(inputFrame, writer);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
    }

    public static class FFmpegHelper
    {
        private static readonly object locker = new object();
        private static bool isInitialized;

        public static void RegisterFFmpegBinaries()
        {
            lock (locker)
            {
                if (!isInitialized)
                {
                    FFmpegBinariesHelper.RegisterFFmpegBinaries();
                    isInitialized = true;
                }
            }
        }

        public static AVFrame CreateFrame(IntPtr data, AVPixelFormat pixelFormat, int width, int height)
        {
            var frame = ffmpeg.av_frame_alloc();
            frame.width = width;
            frame.height = height;
            frame.format = (int)pixelFormat;

            ffmpeg.av_image_fill_arrays(frame.data, frame.linesize, data, pixelFormat, width, height, 1);

            return frame;
        }

        public static void EncodeFrame(AVFrame inputFrame, BinaryWriter writer)
        {
            // Implementation of encoding goes here using FFmpeg.AutoGen
            // You need to use FFmpeg functions to encode the frame and write it to the output stream
            // This will depend on the specific FFmpeg version and API changes
            // Refer to FFmpeg documentation and examples for details
        }
    }
}

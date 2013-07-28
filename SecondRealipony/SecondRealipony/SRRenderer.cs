using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.IO;
using System.Xml;

namespace SecondRealipony
{
    class SRRenderer
    {
        int[] rawData;      //keep in a class variable to save on memory reallocating
        int width;
        int height;
        string VideoPath;
        double VideoFrameRate;

        public SRRenderer(int width, int height, string videoPath, double videoFrameRate)
        {
            rawData = new int[width * height];
            this.width = width;
            this.height = height;
            VideoPath = videoPath;
            VideoFrameRate = videoFrameRate;
        }

        //MICROSOFT SUCKS: Texture2D.SaveAsPng and .SaveAsJpeg have a fatal memory leak!
        //Proven with this test: with the following command, toss in a Thread.Sleep(10000), open task manager, and watch it leak 3.6 MB (1280x720x4) every ten seconds
        //screenshot.SaveAsPng(fs, screenshot.Width, screenshot.Height);
        //So instead we have to dip into System.Drawing for a non-broken PNG renderer.  Yuck.
        public void RenderScreenshot(GraphicsDevice device, int frameNumber)
        {
            device.GetBackBufferData<int>(rawData);

            //Swap red and blue channels, because incredibly annoyingly XNA only supports ABGR textures and Drawing only ARGB
            //And I hate the int casts here, but Marshal.Copy only works with ints not uints or Colors
            for (int i = 0; i < rawData.Length; i++)
            {
                rawData[i] = (int)(rawData[i] & 0xFF00FF00) | (int)((rawData[i] & 0x000000FF) << 16) | (int)((rawData[i] & 0x00FF0000) >> 16);
            }

            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var rect = new System.Drawing.Rectangle(0, 0, width, height);

            var bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var safePtr = bitmapData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(rawData, 0, safePtr, rawData.Length);
            bitmap.UnlockBits(bitmapData);

            string filename = Path.Combine(VideoPath, String.Format("frame{0:D5}.png", frameNumber));

            bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
        }

        public void WriteMusicTimes(SRSegment[] segments, string AudacityProjectFile)
        {
            Dictionary<string, double> segmentStartTimes = CalculateMusicTimes(segments);
            WriteMusicTimes(segmentStartTimes, AudacityProjectFile);
        }

        protected Dictionary<string, double> CalculateMusicTimes(SRSegment[] segments)
        {
            var results = new Dictionary<string, double>(StringComparer.InvariantCultureIgnoreCase);

            TimeSpan elapsedTime = TimeSpan.Zero;
            foreach (SRSegment segment in segments)
            {
                var delayFrameCount = (int)Math.Floor(segment.MusicDelay * segment.BeatLength * VideoFrameRate);
                var delayTimeSpan = SRController.GetTimespan(delayFrameCount, VideoFrameRate);

                results.Add(segment.MusicName.Replace(".wav", ""), (elapsedTime + delayTimeSpan).TotalSeconds);

                int frameCount = (int)Math.Floor((segment.EndBeat + segment.Anacrusis) * segment.BeatLength * VideoFrameRate) + 1;     //add 1 for 0th frame of each segment
                elapsedTime += SRController.GetTimespan(frameCount, VideoFrameRate);
            }
            return results;
        }

        protected void WriteMusicTimes(Dictionary<string, double> segmentStartTimes, string AudacityProjectFile)
        {
            string filename = Path.Combine(VideoPath, AudacityProjectFile);

            var doc = new XmlDocument();
            try
            {
                doc.Load(filename);
            }
            catch (FileNotFoundException)
            {
                return;
            }

            var nodes = doc.GetElementsByTagName("waveclip");
            foreach (XmlNode node in nodes)
            {
                string trackname = node.ParentNode.Attributes["name"].Value;

                if (segmentStartTimes.ContainsKey(trackname))
                    node.Attributes["offset"].Value = segmentStartTimes[trackname].ToString();
            }

            doc.Save(filename);
        }
    }
}

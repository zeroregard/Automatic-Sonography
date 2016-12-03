using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Kinect;
using Microsoft.Kinect.Fusion;
using System.Windows.Media.Imaging;

namespace KinectConnectionTest
{
    public class Vertex
    {
        public ushort X;
        public ushort Y;
        public ushort Z;

        public Vertex(ushort x, ushort y, ushort z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public static class Extensions
    {

        public static ImageSource ToBitmap(this ColorFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;

            System.Windows.Media.PixelFormat format = PixelFormats.Bgr32;


            byte[] pixels = new byte[width * height * ((format.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        private static Vertex ToVertice(ushort x, ushort y, ushort depth)
        {
            
            return new Vertex(x, y, depth);
        } 

        public static List<Vertex> ToVertices(this DepthFrame frame, ushort min = 0, ushort max = ushort.MaxValue)
        {
            List<Vertex> vertices = new List<Vertex>();
            //depth precision is 1mm

            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            ushort[] depthArray = new ushort[width * height];

            frame.CopyFrameDataToArray(depthArray);

            int vertex_x = 0;
            int vertex_y = 0;
            for (int depthIndex = 0; depthIndex < depthArray.Length; ++depthIndex)
            {
                ushort depth = depthArray[depthIndex];
                if (depth >= min && depth <= max)
                {
                    vertices.Add(new Vertex((ushort)vertex_x, (ushort)vertex_y, depth));
                }
                vertex_x++;
                if(vertex_x > width)
                {
                    vertex_y++;
                    vertex_x = 0;
                }
            }

            return vertices;
        }

        public static ImageSource ToBitmap(this DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            //ushort minDepth = frame.DepthMinReliableDistance;
            //ushort maxDepth = frame.DepthMaxReliableDistance;
            ushort minDepth = 0;
            ushort maxDepth = 4096;

            ushort[] pixelData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(pixelData);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex)
            {
                ushort depth = pixelData[depthIndex];
                byte intensity = ConvertDepthToIntensity(minDepth, maxDepth, depth);

                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red

                ++colorIndex;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        private static byte ConvertDepthToIntensity(ushort min, ushort max, ushort depth)
        {
            if (depth > max)
                return 0;
            if (depth < min)
                return 255;
            float percentage = (depth - min) / (1f * (max - min));
            int toInt = 255 - (int)Math.Floor(255 * percentage);
            return (byte)toInt;
        }

        public static ImageSource ToBitmap(this InfraredFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort[] frameData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(frameData);

            int colorIndex = 0;
            for (int infraredIndex = 0; infraredIndex < frameData.Length; infraredIndex++)
            {
                ushort ir = frameData[infraredIndex];

                byte intensity = (byte)(ir >> 7);

                pixels[colorIndex++] = (byte)(intensity / 1); // Blue
                pixels[colorIndex++] = (byte)(intensity / 1); // Green   
                pixels[colorIndex++] = (byte)(intensity / 0.4); // Red

                colorIndex++;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }
    }
}

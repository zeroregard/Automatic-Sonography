using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using Microsoft.Kinect.Fusion;

namespace KinectConnectionTest
{
    public class KinectFusionizer
    {
        public bool CaptureCurrent = false;
        private KinectSensor sensor;

        private object volumeLock = new object();
        private object rawDataLock = new object();

        private float voxelsPerMeter = 384;
        private int voxelsX = 384;
        private int voxelsY = 384;
        private int voxelsZ = 256;

        private float minDepthClip = 0.5f;
        private float maxDepthClip = 1f;

        private int depthWidth = 512;
        private int depthHeight = 424;
        private int downsampledWidth;
        private int downsampledHeight;
        private int depthVisibilityTestMapWidth = 0;
        private int depthVisibilityTestMapHeight = 0;
        private int colorWidth = 1920;
        private int colorHeight = 1080;

        #region Constants
        private const int DownsampleFactor = 2;
        private const int ColorDownsampleFactor = 4;
        private const ushort DepthVisibilityTestThreshold = 50; // 50mm
        #endregion

        #region Matrixes
        private Matrix4 worldToCameraTransform;
        private Matrix4 defaultWorldToVolumeTransform;
        private Matrix4 worldToBGRTransform; //TODO: needed?
        #endregion

        /// <summary>
        /// The coordinate mapper to convert between depth and color frames of reference
        /// </summary>
        private CoordinateMapper mapper;

        #region Arrays
        private int[] resampledColorImagePixelsAlignedToDepth;
        private ushort[] depthImagePixels;
        private float[] downsampledDepthImagePixels;
        private int[] downsampledDeltaFromReferenceColorPixels;
        private int[] deltaFromReferenceFramePixelsArgb;
        private ColorSpacePoint[] colorCoordinates;
        private ushort[] depthVisibilityTestMap;
        private byte[] colorImagePixels;
        #endregion

        #region Frames
        private FusionColorImageFrame resampledColorFrameDepthAligned;
        private FusionFloatImageFrame depthFloatFrame;
        private FusionFloatImageFrame downsampledDepthFloatFrame;
        private FusionFloatImageFrame downsampledSmoothDepthFloatFrame;
        private FusionPointCloudImageFrame downsampledDepthPointCloudFrame;
        private FusionPointCloudImageFrame downsampledRaycastPointCloudFrame;
        private FusionColorImageFrame downsampledDeltaFromReferenceFrameColorFrame;
        #endregion

        ColorReconstruction volume;
        private MultiSourceFrameReader reader;

        public KinectFusionizer()
        {
            SetUpKinectVariables();
            SetupVariables();
        }

        public void SetUpKinectVariables()
        {
            sensor = KinectSensor.GetDefault();
            if (sensor == null)
            {
                throw new Exception("The Kinect could not be found");
            }
            sensor.Open();
            FrameDescription depthFrameDescription = sensor.DepthFrameSource.FrameDescription;
            depthWidth = depthFrameDescription.Width;
            depthHeight = depthFrameDescription.Height;

            FrameDescription colorFrameDescription = sensor.ColorFrameSource.FrameDescription;
            colorWidth = colorFrameDescription.Width;
            colorHeight = colorFrameDescription.Height;

            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color);
            reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
        }

        private void SetupVariables()
        {
            mapper = sensor.CoordinateMapper;

            //Calculate integers used for allocation
            int depthImageSize = depthWidth * depthHeight;
            downsampledWidth = depthWidth / DownsampleFactor;
            downsampledHeight = depthHeight / DownsampleFactor;
            int colorImageByteSize = colorWidth * colorHeight * sizeof(int);
            int downsampledDepthImageSize = downsampledWidth * downsampledHeight;

            //Allocate frames
            resampledColorFrameDepthAligned = new FusionColorImageFrame(depthWidth, depthHeight);
            depthFloatFrame = new FusionFloatImageFrame(depthWidth, depthHeight);
            downsampledDepthFloatFrame = new FusionFloatImageFrame(downsampledWidth, downsampledHeight);
            downsampledSmoothDepthFloatFrame = new FusionFloatImageFrame(downsampledWidth, downsampledHeight);
            downsampledDepthPointCloudFrame = new FusionPointCloudImageFrame(downsampledWidth, downsampledHeight);
            downsampledRaycastPointCloudFrame = new FusionPointCloudImageFrame(downsampledWidth, downsampledHeight);
            downsampledDeltaFromReferenceFrameColorFrame = new FusionColorImageFrame(downsampledWidth, downsampledHeight);

            //Allocate arrays
            depthImagePixels = new ushort[depthImageSize];
            downsampledDepthImagePixels = new float[downsampledDepthImageSize];
            downsampledDeltaFromReferenceColorPixels = new int[downsampledDepthImageSize];
            deltaFromReferenceFramePixelsArgb = new int[depthImageSize];
            colorCoordinates = new ColorSpacePoint[depthImageSize];
            resampledColorImagePixelsAlignedToDepth = new int[depthImageSize];
            depthVisibilityTestMapWidth = colorWidth / ColorDownsampleFactor;
            depthVisibilityTestMapHeight = colorHeight / ColorDownsampleFactor;
            depthVisibilityTestMap = new ushort[depthVisibilityTestMapWidth * depthVisibilityTestMapHeight];
            colorImagePixels = new byte[colorImageByteSize];
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            if (CaptureCurrent)
            {
                ProcessFrameData(e);
                Process();
            }
        }

        private void ProcessFrameData(MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrameReference frameReference = e.FrameReference;
            MultiSourceFrame multiSourceFrame = null;
            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            try
            {
                multiSourceFrame = frameReference.AcquireFrame();
                if (multiSourceFrame != null)
                {
                    lock (rawDataLock)
                    {
                        ColorFrameReference colorFrameReference = multiSourceFrame.ColorFrameReference;
                        DepthFrameReference depthFrameReference = multiSourceFrame.DepthFrameReference;
                        colorFrame = colorFrameReference.AcquireFrame();
                        depthFrame = depthFrameReference.AcquireFrame();

                        if ((depthFrame != null) && (colorFrame != null))
                        {
                            FrameDescription colorFrameDescription = colorFrame.FrameDescription;
                            int colorWidth = colorFrameDescription.Width;
                            int colorHeight = colorFrameDescription.Height;
                            if ((colorWidth * colorHeight * sizeof(int)) == colorImagePixels.Length)
                                colorFrame.CopyConvertedFrameDataToArray(colorImagePixels, ColorImageFormat.Bgra);

                            FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                            int depthWidth = depthFrameDescription.Width;
                            int depthHeight = depthFrameDescription.Height;

                            if ((depthWidth * depthHeight) == depthImagePixels.Length)
                                depthFrame.CopyFrameDataToArray(depthImagePixels);
                        }
                    }
                }
                CaptureCurrent = false; //We got both color and depth, everything went ok, stop trying to capture this image
            }
            catch (Exception)
            {
                // ignore if the frame is no longer available
            }
            finally
            {
                // MultiSourceFrame, DepthFrame, ColorFrame, BodyIndexFrame are IDispoable
                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                    depthFrame = null;
                }
                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                    colorFrame = null;
                }
                if (multiSourceFrame != null)
                {
                    multiSourceFrame = null;
                }
            }
        }

        private void Process()
        {
            lock (volumeLock)
            {
                RecreateReconstruction();
            }
            ResetReconstruction();
            ProcessDepthData();
            TrackCamera();
            IntegrateData();
            RenderReconstruction();
            WriteToMemory();
        }

        /// <summary>
        /// Constructs and prepares the ColorReconstruction for data input
        /// </summary>
        private void RecreateReconstruction()
        {
            ReconstructionParameters volParam = new ReconstructionParameters(voxelsPerMeter, voxelsX, voxelsY, voxelsZ);
            worldToCameraTransform = Matrix4.Identity;
            ReconstructionProcessor ProcessorType = ReconstructionProcessor.Amp;
            volume = ColorReconstruction.FusionCreateReconstruction(volParam, ProcessorType, -1, worldToCameraTransform);
            defaultWorldToVolumeTransform = volume.GetCurrentWorldToVolumeTransform();
            ResetReconstruction();

            worldToBGRTransform = Matrix4.Identity;
            worldToBGRTransform.M11 = voxelsPerMeter / voxelsX;
            worldToBGRTransform.M22 = voxelsPerMeter / voxelsY;
            worldToBGRTransform.M33 = voxelsPerMeter / voxelsZ;
            worldToBGRTransform.M41 = 0.5f;
            worldToBGRTransform.M42 = 0.5f;
            worldToBGRTransform.M44 = 1.0f;
        }

        private void ResetReconstruction()
        {
            worldToCameraTransform = Matrix4.Identity;
            Matrix4 worldToVolumeTransform = defaultWorldToVolumeTransform;
            float minDist = (minDepthClip < maxDepthClip) ? minDepthClip : maxDepthClip;
            worldToVolumeTransform.M43 -= minDist * voxelsPerMeter;
            volume.ResetReconstruction(worldToCameraTransform, worldToVolumeTransform);
            ResetColorImage();
        }

        private void ResetColorImage()
        {
            Array.Clear(resampledColorImagePixelsAlignedToDepth, 0, resampledColorImagePixelsAlignedToDepth.Length);
            resampledColorFrameDepthAligned.CopyPixelDataFrom(resampledColorImagePixelsAlignedToDepth);
        }

        private void ProcessDepthData()
        {
            volume.DepthToDepthFloatFrame(depthImagePixels, depthFloatFrame, minDepthClip, maxDepthClip, false);
        }

        private void TrackCamera()
        {
            Matrix4 calculatedCameraPos = worldToCameraTransform; //copy
            TrackCamera_AlignPointClouds(ref calculatedCameraPos); //manipulate
            worldToCameraTransform = calculatedCameraPos; //set to new
        }

        private void TrackCamera_AlignPointClouds(ref Matrix4 calculatedCameraPose)
        {
            DownsampleDepthFrameNearestNeighbor(downsampledDepthFloatFrame, DownsampleFactor);
            volume.SmoothDepthFloatFrame(downsampledDepthFloatFrame, downsampledSmoothDepthFloatFrame, 1, 0.04f); //TODO: Magic numbers
            FusionDepthProcessor.DepthFloatFrameToPointCloud(downsampledSmoothDepthFloatFrame, downsampledDepthPointCloudFrame);
            volume.CalculatePointCloud(downsampledRaycastPointCloudFrame, calculatedCameraPose);
            Matrix4 initialPose = calculatedCameraPose;
            FusionDepthProcessor.AlignPointClouds(downsampledRaycastPointCloudFrame, downsampledDepthPointCloudFrame, FusionDepthProcessor.DefaultAlignIterationCount, downsampledDeltaFromReferenceFrameColorFrame, ref calculatedCameraPose);
            UpsampleColorDeltasFrameNearestNeighbor(DownsampleFactor);
            //UpdateAlignDeltas(); //TODO: Necessary?
        }

        /// <summary>
        /// Downsample depth pixels with nearest neighbor
        /// </summary>
        /// <param name="dest">The destination depth image.</param>
        /// <param name="factor">The downsample factor (2=x/2,y/2, 4=x/4,y/4, 8=x/8,y/8, 16=x/16,y/16).</param>
        private unsafe void DownsampleDepthFrameNearestNeighbor(FusionFloatImageFrame dest, int factor)
        {
            #region ErrorHandling
            if (null == dest || null == downsampledDepthImagePixels)
                throw new ArgumentException("inputs null");
            if (false == (2 == factor || 4 == factor || 8 == factor || 16 == factor))
                throw new ArgumentException("factor != 2, 4, 8 or 16");
            int downsampleWidth = depthWidth / factor;
            int downsampleHeight = depthHeight / factor;
            if (dest.Width != downsampleWidth || dest.Height != downsampleHeight)
                throw new ArgumentException("dest != downsampled image size");
            #endregion
            fixed (ushort* rawDepthPixelPtr = depthImagePixels)
            {
                ushort* rawDepthPixels = (ushort*)rawDepthPixelPtr;

                // Horizontal flip the color image as the standard depth image is flipped internally in Kinect Fusion
                // to give a viewpoint as though from behind the Kinect looking forward by default.
                Parallel.For(
                    0,
                    downsampleHeight,
                    y =>
                    {
                        int flippedDestIndex = (y * downsampleWidth) + (downsampleWidth - 1);
                        int sourceIndex = y * depthWidth * factor;

                        for (int x = 0; x < downsampleWidth; ++x, --flippedDestIndex, sourceIndex += factor)
                        {
                            // Copy depth value
                            downsampledDepthImagePixels[flippedDestIndex] = (float)rawDepthPixels[sourceIndex] * 0.001f;
                        }
                    });
            }

            dest.CopyPixelDataFrom(downsampledDepthImagePixels);
        }

        /// <summary>
        /// Up sample color delta from reference frame with nearest neighbor - replicates pixels
        /// </summary>
        /// <param name="factor">The up sample factor (2=x*2,y*2, 4=x*4,y*4, 8=x*8,y*8, 16=x*16,y*16).</param>
        private unsafe void UpsampleColorDeltasFrameNearestNeighbor(int factor)
        {
            #region ErrorHandling
            if (null == downsampledDeltaFromReferenceFrameColorFrame || null == downsampledDeltaFromReferenceColorPixels || null == deltaFromReferenceFramePixelsArgb)
                throw new ArgumentException("inputs null");
            if (false == (2 == factor || 4 == factor || 8 == factor || 16 == factor))
                throw new ArgumentException("factor != 2, 4, 8 or 16");
            int upsampleWidth = downsampledWidth * factor;
            int upsampleHeight = downsampledHeight * factor;
            if (depthWidth != upsampleWidth || depthHeight != upsampleHeight)
                throw new ArgumentException("upsampled image size != depth size");
            #endregion
            downsampledDeltaFromReferenceFrameColorFrame.CopyPixelDataTo(downsampledDeltaFromReferenceColorPixels);

            // Here we make use of unsafe code to just copy the whole pixel as an int for performance reasons, as we do
            // not need access to the individual rgba components.
            fixed (int* rawColorPixelPtr = downsampledDeltaFromReferenceColorPixels)
            {
                int* rawColorPixels = (int*)rawColorPixelPtr;

                // Note we run this only for the source image height pixels to sparsely fill the destination with rows
                Parallel.For(
                    0,
                    downsampledHeight,
                    y =>
                    {
                        int destIndex = y * upsampleWidth * factor;
                        int sourceColorIndex = y * downsampledWidth;

                        for (int x = 0; x < downsampledWidth; ++x, ++sourceColorIndex)
                        {
                            int color = rawColorPixels[sourceColorIndex];

                            // Replicate pixels horizontally
                            for (int colFactorIndex = 0; colFactorIndex < factor; ++colFactorIndex, ++destIndex)
                            {
                                // Replicate pixels vertically
                                for (int rowFactorIndex = 0; rowFactorIndex < factor; ++rowFactorIndex)
                                {
                                    // Copy color pixel
                                    deltaFromReferenceFramePixelsArgb[destIndex + (rowFactorIndex * upsampleWidth)] = color;
                                }
                            }
                        }
                    });
            }

            int sizeOfInt = sizeof(int);
            int rowByteSize = downsampledHeight * sizeOfInt;

            // Duplicate the remaining rows with memcpy
            for (int y = 0; y < downsampledHeight; ++y)
            {
                // iterate only for the smaller number of rows
                int srcRowIndex = upsampleWidth * factor * y;

                // Duplicate lines
                for (int r = 1; r < factor; ++r)
                {
                    int index = upsampleWidth * ((y * factor) + r);

                    System.Buffer.BlockCopy(
                        deltaFromReferenceFramePixelsArgb, srcRowIndex * sizeOfInt, deltaFromReferenceFramePixelsArgb, index * sizeOfInt, rowByteSize);
                }
            }
        }

        private void IntegrateData()
        {
            MapColorToDepth();
            volume.IntegrateFrame(depthFloatFrame, resampledColorFrameDepthAligned, FusionDepthProcessor.DefaultIntegrationWeight, FusionDepthProcessor.DefaultColorIntegrationOfAllAngles, worldToCameraTransform);
            RenderReconstruction();
        }

        /// <summary>
        /// Process the color and depth inputs, converting the color into the depth space
        /// </summary>
        private unsafe void MapColorToDepth()
        {
            mapper.MapDepthFrameToColorSpace(depthImagePixels, colorCoordinates);

            lock (rawDataLock)
            {
                // Fill in the visibility depth map.
                Array.Clear(depthVisibilityTestMap, 0, depthVisibilityTestMap.Length);
                fixed (ushort* ptrDepthVisibilityPixels = depthVisibilityTestMap, ptrDepthPixels = depthImagePixels)
                {
                    for (int index = 0; index < depthImagePixels.Length; ++index)
                    {
                        if (!float.IsInfinity(colorCoordinates[index].X) && !float.IsInfinity(colorCoordinates[index].Y))
                        {
                            int x = (int)(Math.Floor(colorCoordinates[index].X + 0.5f) / ColorDownsampleFactor);
                            int y = (int)(Math.Floor(colorCoordinates[index].Y + 0.5f) / ColorDownsampleFactor);

                            if ((x >= 0) && (x < depthVisibilityTestMapWidth) &&
                                (y >= 0) && (y < depthVisibilityTestMapHeight))
                            {
                                int depthVisibilityTestIndex = (y * depthVisibilityTestMapWidth) + x;
                                if ((ptrDepthVisibilityPixels[depthVisibilityTestIndex] == 0) ||
                                    (ptrDepthVisibilityPixels[depthVisibilityTestIndex] > ptrDepthPixels[index]))
                                {
                                    ptrDepthVisibilityPixels[depthVisibilityTestIndex] = ptrDepthPixels[index];
                                }
                            }
                        }
                    }
                }

                // Here we make use of unsafe code to just copy the whole pixel as an int for performance reasons, as we do
                // not need access to the individual rgba components.
                fixed (byte* ptrColorPixels = colorImagePixels)
                {
                    int* rawColorPixels = (int*)ptrColorPixels;

                    // Horizontal flip the color image as the standard depth image is flipped internally in Kinect Fusion
                    // to give a viewpoint as though from behind the Kinect looking forward by default.
                    Parallel.For(
                        0,
                        depthHeight,
                        y =>
                        {
                            int destIndex = y * depthWidth;
                            int flippedDestIndex = destIndex + (depthWidth - 1); // horizontally mirrored

                                for (int x = 0; x < depthWidth; ++x, ++destIndex, --flippedDestIndex)
                            {
                                    // calculate index into depth array
                                    int colorInDepthX = (int)Math.Floor(colorCoordinates[destIndex].X + 0.5);
                                int colorInDepthY = (int)Math.Floor(colorCoordinates[destIndex].Y + 0.5);
                                int depthVisibilityTestX = (int)(colorInDepthX / ColorDownsampleFactor);
                                int depthVisibilityTestY = (int)(colorInDepthY / ColorDownsampleFactor);
                                int depthVisibilityTestIndex = (depthVisibilityTestY * depthVisibilityTestMapWidth) + depthVisibilityTestX;

                                    // make sure the depth pixel maps to a valid point in color space
                                    if (colorInDepthX >= 0 && colorInDepthX < colorWidth && colorInDepthY >= 0
                                    && colorInDepthY < colorHeight && depthImagePixels[destIndex] != 0)
                                {
                                    ushort depthTestValue = depthVisibilityTestMap[depthVisibilityTestIndex];

                                    if ((depthImagePixels[destIndex] - depthTestValue) < DepthVisibilityTestThreshold)
                                    {
                                            // Calculate index into color array- this will perform a horizontal flip as well
                                            int sourceColorIndex = colorInDepthX + (colorInDepthY * colorWidth);

                                            // Copy color pixel
                                            resampledColorImagePixelsAlignedToDepth[flippedDestIndex] = rawColorPixels[sourceColorIndex];
                                    }
                                    else
                                    {
                                        resampledColorImagePixelsAlignedToDepth[flippedDestIndex] = 0;
                                    }
                                }
                                else
                                {
                                    resampledColorImagePixelsAlignedToDepth[flippedDestIndex] = 0;
                                }
                            }
                        });
                }
            }

            resampledColorFrameDepthAligned.CopyPixelDataFrom(resampledColorImagePixelsAlignedToDepth);
        }

        private void RenderReconstruction()
        {

        }

        private void WriteToMemory()
        {
            ColorMesh m = volume.CalculateMesh(1);
            PCDExporter.Output(m); //output your mesh somehow
        }
    }
}

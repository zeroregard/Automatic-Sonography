using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Microsoft.Kinect;
using ComputerVisionLibrary;
using DataStructures;
using Microsoft.Kinect.Fusion;

namespace AutoSonographyWPF
{
    /// <summary>
    /// Interaction logic for _3DScanMenu.xaml
    /// </summary>
    public partial class _3DScanMenu : Window
    {
        ComputerVisionMaster master;
        MainWindow mainWindowInstance;
        CVMesh currentMesh;
        double y_min;
        double y_max;

        #region 3DDrawingVariables
        // The main object model group.
        private Model3DGroup MainModel3Dgroup;

        // The camera.
        private PerspectiveCamera TheCamera;

        // The camera's current location.
        private double CameraPhi = 0;
        private double CameraTheta = 1.5 * Math.PI;
        private double CameraR = 0.5;

        // The change in CameraPhi when you press up/down arrow.
        private const double CameraDPhi = 0.05;

        // The change in CameraTheta when you press left/right arrow.
        private const double CameraDTheta = 0.05;

        // The change in CameraR when you press + or -.
        private const double CameraDR = 0.1;
        private Vector3 MeshMid;
        #endregion

        public _3DScanMenu()
        {
            InitializeComponent();
        }

        //Code from here was ripped and rewritten to suit our needs:
        //http://csharphelper.com/blog/2014/10/draw-a-3d-surface-with-wpf-xaml-and-c/
        #region 3DDrawingMethods
        // Create the scene.
        private void Initialize3DDrawing()
        {
            BoundingBox b = Extensions.FindBoundingBox(currentMesh.Vertices);
            MeshMid = Extensions.MidPoint(b);
            // Give the camera its initial position.
            MainModel3Dgroup = new Model3DGroup();
            TheCamera = new PerspectiveCamera();
            DefineLights();
            TheCamera.FieldOfView = 60;
            MainViewport.Camera = TheCamera;
            PositionCamera();
            // Define lights.


            // Create the model.
            ConvertToGeometry(MainModel3Dgroup);

            // Add the group of models to a ModelVisual3D.
            ModelVisual3D model_visual = new ModelVisual3D();
            model_visual.Content = MainModel3Dgroup;

            // Add the main visual to the viewportt.
            MainViewport.Children.Clear();
            MainViewport.Children.Add(model_visual);
        }

        private void DefineLights()
        {
            AmbientLight ambient_light = new AmbientLight(Colors.Gray);
            DirectionalLight directional_light =
                new DirectionalLight(Colors.Gray,
                    new Vector3D(1.0, 3.0, 2.0));
            MainModel3Dgroup.Children.Add(ambient_light);
            MainModel3Dgroup.Children.Add(directional_light);
        }

        private void ConvertToGeometry(Model3DGroup model_group)
        {
            CVMesh m = currentMesh;
            MeshGeometry3D mg3D = new MeshGeometry3D();
            foreach (var mFace in m.Faces)
            {
                Point3D a = ToPoint3D(m.Vertices[mFace.index1]);
                Point3D b = ToPoint3D(m.Vertices[mFace.index2]);
                Point3D c = ToPoint3D(m.Vertices[mFace.index3]);
                AddTriangle(mg3D, a, b, c);
            }
            DiffuseMaterial surface_material = new DiffuseMaterial(Brushes.LightGray);
            GeometryModel3D surface_model = new GeometryModel3D(mg3D, surface_material);
            surface_model.BackMaterial = surface_material;
            model_group.Children.Add(surface_model);
        }

        private Point3D ToPoint3D(Vector3 v)
        {
            return new Point3D(v.X, v.Y, v.Z);
        }

        // The function that defines the surface we are drawing.
        private double F(double x, double z)
        {
            const double two_pi = 2 * 3.14159265;
            double r2 = x * x + z * z;
            double r = Math.Sqrt(r2);
            double theta = Math.Atan2(z, x);
            return Math.Exp(-r2) * Math.Sin(two_pi * r) *
                Math.Cos(3 * theta);
        }

        // Add a triangle to the indicated mesh.
        private void AddTriangle(MeshGeometry3D mesh,
            Point3D point1, Point3D point2, Point3D point3)
        {
            // Get the points' indices.
            int index1 = AddPoint(mesh.Positions, point1);
            int index2 = AddPoint(mesh.Positions, point2);
            int index3 = AddPoint(mesh.Positions, point3);

            // Create the triangle.
            mesh.TriangleIndices.Add(index1);
            mesh.TriangleIndices.Add(index2);
            mesh.TriangleIndices.Add(index3);
        }

        // Create the point and return its new index.
        private int AddPoint(Point3DCollection points, Point3D point)
        {
            // Create the point and return its index.
            points.Add(point);
            return points.Count - 1;
        }

        // Adjust the camera's position.
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentMesh == null)
                return;
            switch (e.Key)
            {
                case Key.W:
                    CameraPhi += CameraDPhi;
                    if (CameraPhi > Math.PI / 2.0)
                        CameraPhi = Math.PI / 2.0;
                    break;
                case Key.S:
                    CameraPhi -= CameraDPhi;
                    if (CameraPhi < -Math.PI / 2.0)
                        CameraPhi = -Math.PI / 2.0;
                    break;
                case Key.A:
                    CameraTheta += CameraDTheta;
                    break;
                case Key.D:
                    CameraTheta -= CameraDTheta;
                    break;
                case Key.Add:
                case Key.OemPlus:
                    CameraR -= CameraDR;
                    if (CameraR < CameraDR) CameraR = CameraDR;
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    CameraR += CameraDR;
                    break;
            }

            // Update the camera's position.
            PositionCamera();
        }



        // Position the camera.
        private void PositionCamera()
        {
            // Calculate the camera's position in Cartesian coordinates.
            double y = CameraR * Math.Sin(CameraPhi);
            double hyp = CameraR * Math.Cos(CameraPhi);
            double x = hyp * Math.Cos(CameraTheta);
            double z = hyp * Math.Sin(CameraTheta);
            Point3D pos = new Point3D(x + MeshMid.X, y + MeshMid.Y, z + MeshMid.Z);
            TheCamera.Position = pos;

            // Look toward the origin.
            TheCamera.LookDirection = new Vector3D(-x, -y, -z);

            // Set the Up direction.
            TheCamera.UpDirection = new Vector3D(0, -1, 0);
        }
        #endregion 

        public void Initialize(ComputerVisionMaster cvmaster, MainWindow window)
        {
            master = cvmaster;
            mainWindowInstance = window;
            SetUpKinect();
        }

        private bool SubscribedToEvents;

        public void SetUpKinect()
        {
            master.fusionizer = new KinectFusionizer(master);
            if (!SubscribedToEvents)
            {
                master.DepthArrived += ReceiveFrame;
                master.MeshFinished += ReceiveMesh;
                SubscribedToEvents = true;
            }
        }

        public void ReceiveMesh(object sender, EventArgs e)
        {
            var mesh = master.LastMesh;
            currentMesh = mesh;
            currentMesh = master.slicer.Slice(mesh, y_min, y_max);
            btnOK.IsEnabled = true;
            Initialize3DDrawing();
            SetUpKinect();
        }

        public void ReceiveFrame(object sender, EventArgs e)
        {
            if (master != null)
                depthImage.Source = ToBitmap(master.CurrentFrame);

        }

        public ImageSource ToBitmap(DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            ushort[] pixelData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(pixelData);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex)
            {
                ushort depth = pixelData[depthIndex];

                byte intensity = 0;
                intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red

                ++colorIndex;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            master = null;
            mainWindowInstance.Show();
        }

        private void slider_y_min_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = slider_y_min.Value;
            if (txt_y_min != null)
                txt_y_min.Text = Math.Round(value, 2).ToString();
            MoveMin(value);
            y_min = value;
        }

        private void slider_y_max_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = slider_y_max.Value;
            if (txt_y_max != null)
                txt_y_max.Text = Math.Round(value, 2).ToString();
            MoveMax(value);
            y_max = value;
        }

        public void MoveMin(double value)
        {
            var pixels = ConvertSliderValueToPixelPosition(value);
            Thickness margin = border_y_min.Margin;
            margin.Top = -pixels;
            border_y_min.Margin = margin;
        }

        public void MoveMax(double value)
        {
            var pixels = ConvertSliderValueToPixelPosition(value);
            Thickness margin = border_y_max.Margin;
            margin.Top = -pixels;
            border_y_max.Margin = margin;
        }

        public double ConvertSliderValueToPixelPosition(double value)
        {
            return value * 424d; //depth frame height
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            master.RequestCurrentImageAsMesh();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            mainWindowInstance.MeshScanComplete(currentMesh);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            mainWindowInstance.Show();
            this.Close();
        }
    }
}

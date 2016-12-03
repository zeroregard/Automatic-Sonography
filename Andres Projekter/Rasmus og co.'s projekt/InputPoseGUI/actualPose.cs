//using System;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;

//namespace InputPoseGUI
//{
//    public class ActualPose : INotifyPropertyChanged
//    {
//        double x, y, z, Rx, Ry, Rz;
//        public ActualPose()
//        {

//        }

//        public void Update(double x, double y, double z, double Rx, double Ry, double Rz)
//        {
//            Xpose = x;
//            Ypose = y;
//            Zpose = z;
//            RXpose = Rx;
//            RYpose = Ry;
//            RZpose = Rz;
//        }

//        public double Xpose
//        {
//            get { return x; }
//            private set
//            {
//                if (x != value)
//                {
//                    x = value;
//                    NotifyPropertyChanged();
//                }
//            }
//        }

//        public double Ypose
//        {
//            get { return y; }
//            private set
//            {
//                if (y != value)
//                {
//                    y = value;
//                    NotifyPropertyChanged();
//                }
//            }
//        }

//        public double Zpose
//        {
//            get { return z; }
//            private set
//            {
//                if (z != value)
//                {
//                    z = value;
//                    NotifyPropertyChanged();
//                }
//            }
//        }

//        public double RXpose
//        {
//            get { return Rx; }
//            private set
//            {
//                if (Rx != value)
//                {
//                    Rx = value;
//                    NotifyPropertyChanged();
//                }
//            }
//        }

//        public double RYpose
//        {
//            get { return Ry; }
//            private set
//            {
//                if (Ry != value)
//                {
//                    Ry = value;
//                    NotifyPropertyChanged();
//                }
//            }
//        }

//        public double RZpose
//        {
//            get { return Rz; }
//            private set
//            {
//                if (Rz != value)
//                {
//                    Rz = value;
//                    NotifyPropertyChanged();
//                }
//            }
//        }

//        #region INotifyPropertyChanged implementation

//        public event PropertyChangedEventHandler PropertyChanged;

//        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
//        {
//            var handler = PropertyChanged;
//            if (handler != null)
//            {
//                handler(this, new PropertyChangedEventArgs(propertyName));
//            }
//        }
//        #endregion
//    }
//}

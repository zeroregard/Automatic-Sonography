using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using System.Diagnostics.CodeAnalysis;
namespace RoboLibrary.Dummies
{
    [ExcludeFromCodeCoverage]
    public class DummyUR10
    {
        private DummyUR10()
        {
            pose = new URPose();
            pose.Xpose = 0.4d;
            pose.Ypose = 0.4d;
            pose.Zpose = 0.4d;
        }
        public URPose pose;
        private static DummyUR10 _instance;
        public static DummyUR10 Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DummyUR10();
                }
                return _instance;
            }
        }
    }
}

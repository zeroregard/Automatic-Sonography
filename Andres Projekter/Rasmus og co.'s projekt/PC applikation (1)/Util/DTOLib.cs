namespace Util
{
    /// <summary>
    /// <para>URPose is a DTO consisting of the elements of a position for the Universal Robot.</para>
    /// A pose is given as p[x,y,z,ax,ay,az], where x,y,z is the position of the Tool Center Point(TCP),
    /// and Rx,Ry,Rz is the orientation of the TCP, given in axis-angle notation.
    /// </summary>
    public class URPose
    {
        double x, y, z, Rx, Ry, Rz;
        public URPose()
        {

        }

        public URPose(double x, double y, double z, double Rx, double Ry, double Rz)
        {
            Xpose = x;
            Ypose = y;
            Zpose = z;
            RXpose = Rx;
            RYpose = Ry;
            RZpose = Rz;
        }

        public double Xpose
        {
            get { return x; }
            set { x = value; }
        }

        public double Ypose
        {
            get { return y; }
            set { y = value; }
        }

        public double Zpose
        {
            get { return z; }
            set { z = value; }
        }

        public double RXpose
        {
            get { return Rx; }
            set { Rx = value; }
        }

        public double RYpose
        {
            get { return Ry; }
            set { Ry = value; }
        }

        public double RZpose
        {
            get { return Rz; }
            set { Rz = value; }
        }
    }

    /// <summary>
    /// <para>GeoMagicPose is a DTO consisting the elements of a position for the GeoMagicTouch.</para>
    /// A position is given by x,y,z,Rx,Ry,Rz. x,y,z is the position of the GeoMagic Touch tip in a coordinate system.
    /// Rx,Ry,Rz is the rotations of the three joints in the tip in radians.
    /// </summary>
    public class GeoMagicPose
    {
        double x, y, z, Rx, Ry, Rz, btnState, Joint1, Joint2, Joint3;
        public GeoMagicPose()
        {

        }

        public GeoMagicPose(double x, double y, double z, double Rx, double Ry, double Rz)
        {
            Xpose = x;
            Ypose = y;
            Zpose = z;
            RXpose = Rx;
            RYpose = Ry;
            RZpose = Rz;
        }

        public GeoMagicPose(double x, double y, double z, double Rx, double Ry, double Rz, double btnState, double Joint1, double Joint2, double Joint3)
        {
            Xpose = x;
            Ypose = y;
            Zpose = z;
            RXpose = Rx;
            RYpose = Ry;
            RZpose = Rz;
            BtnState = btnState;
            Joint1pose = Joint1;
            Joint2pose = Joint2;
            Joint3pose = Joint3;
        }

        public double Xpose
        {
            get { return x; }
            set { x = value; }
        }

        public double Ypose
        {
            get { return y; }
            set { y = value; }
        }

        public double Zpose
        {
            get { return z; }
            set { z = value; }
        }

        public double RXpose
        {
            get { return Rx; }
            set { Rx = value; }
        }

        public double RYpose
        {
            get { return Ry; }
            set { Ry = value; }
        }

        public double RZpose
        {
            get { return Rz; }
            set { Rz = value; }
        }

        public double BtnState
        {
            get { return btnState; }
            set { btnState = value; }
        }

        public double Joint1pose
        {
            get { return Joint1; }
            set { Joint1 = value; }
        }

        public double Joint2pose
        {
            get { return Joint2; }
            set { Joint2 = value; }
        }

        public double Joint3pose
        {
            get { return Joint3; }
            set { Joint3 = value; }
        }
    }

    /// <summary>
    /// <para>ForceData is a DTO consisting the elements of the force from the Universal Robot.</para>
    /// <para>Force is the force twist at the Tool Center Point.</para>
    /// <para>The force twist is computet based on the error between the joint torques required to stay on the trajectory, and the expected joint torques.</para>
    /// <para>In Newtons and Newtons/rad.</para>
    /// </summary>
    public class ForceData
    {
        double x, y, z, Rx, Ry, Rz;
        public ForceData()
        {

        }

        public ForceData(double x, double y, double z, double Rx, double Ry, double Rz)
        {
            Xforce = x;
            Yforce = y;
            Zforce = z;
            RXforce = Rx;
            RYforce = Ry;
            RZforce = Rz;
        }

        public double Xforce
        {
            get { return x; }
            set { x = value; }
        }

        public double Yforce
        {
            get { return y; }
            set { y = value; }
        }

        public double Zforce
        {
            get { return z; }
            set { z = value; }
        }

        public double RXforce
        {
            get { return Rx; }
            set { Rx = value; }
        }

        public double RYforce
        {
            get { return Ry; }
            set { Ry = value; }
        }

        public double RZforce
        {
            get { return Rz; }
            set { Rz = value; }
        }
    }

    /// <summary>
    /// <para>RobotMessage is a DTO consisting the informations about the Universal Robot.</para>
    /// <para>VersionMessage is received only in the first package on the socket connection from the robot.</para>
    /// <para>The information can be used to verification.</para>
    /// </summary>
    public class RobotMessage
    {
        string projectName;
        int majorVersion;
        int minorVersion;
        int svnRevision;
        string buildDate;

        public RobotMessage() { }

        public RobotMessage(string projectName, int majorVersion, int minorVersion, int svnRevision, string buildDate)
        {
            this.projectName = projectName;
            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
            this.svnRevision = svnRevision;
            this.buildDate = buildDate;
        }

        public override bool Equals(object obj)
        {
            RobotMessage testObj = obj as RobotMessage;
            if (testObj == null)
                return false;
            else if (testObj.BuildDate.Equals(buildDate) &&
                testObj.MajorVersion.Equals(majorVersion) &&
                testObj.MinorVersion.Equals(minorVersion) &&
                testObj.ProjectName.Equals(projectName) &&
                testObj.SvnRevision.Equals(svnRevision))
                return true;
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; }
        }

        public int MajorVersion
        {
            get { return majorVersion; }
            set { majorVersion = value; }
        }

        public int MinorVersion
        {
            get { return minorVersion; }
            set { minorVersion = value; }
        }

        public int SvnRevision
        {
            get { return svnRevision; }
            set { svnRevision = value; }
        }

        public string BuildDate
        {
            get { return buildDate; }
            set { buildDate = value; }
        }
    }

    /// <summary>
    /// <para>ConfigurationData is a DTO consisting the configurations on the Universal Robot.</para>
    /// <para>ConfigurationData can be used to set up configurations on the robot.</para>
    /// </summary>
    public class ConfigurationData
    {
        double offsetX, offsetY, offsetZ, offsetRx, offsetRy, offsetRz, payload, acceleration, speed, blend;

        public ConfigurationData() { }

        public ConfigurationData(double offsetX, double offsetY, double offsetZ, double offsetRx, double offsetRy, double offsetRz, double payload, double acceleration, double speed, double blend)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.offsetZ = offsetZ;
            this.offsetRx = offsetRx;
            this.offsetRy = offsetRy;
            this.offsetRz = offsetRz;
            this.payload = payload;
            this.acceleration = acceleration;
            this.speed = speed;
            this.blend = blend;
        }

        public override bool Equals(object obj)
        {
            ConfigurationData testObj = obj as ConfigurationData;
            if (testObj == null)
                return false;
            else if (testObj.OffsetX.Equals(offsetX) &&
                testObj.OffsetY.Equals(offsetY) &&
                testObj.OffsetZ.Equals(offsetZ) &&
                testObj.OffsetRx.Equals(OffsetRx) &&
                testObj.OffsetRy.Equals(OffsetRy) &&
                testObj.OffsetRz.Equals(OffsetRz) &&
                testObj.Payload.Equals(payload) &&
                testObj.Acceleration.Equals(acceleration) &&
                testObj.Speed.Equals(speed) &&
                testObj.Blend.Equals(blend))
                return true;
            else return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public double OffsetX
        {
            get { return offsetX; }
            set { offsetX = value; }
        }

        public double OffsetY
        {
            get { return offsetY; }
            set { offsetY = value; }
        }

        public double OffsetZ
        {
            get { return offsetZ; }
            set { offsetZ = value; }
        }

        public double OffsetRx
        {
            get { return offsetRx; }
            set { offsetRx = value; }
        }

        public double OffsetRy
        {
            get { return offsetRy; }
            set { offsetRy = value; }
        }

        public double OffsetRz
        {
            get { return offsetRz; }
            set { offsetRz = value; }
        }

        public double Payload
        {
            get { return payload; }
            set { payload = value; }
        }

        public double Acceleration
        {
            get { return acceleration; }
            set { acceleration = value; }
        }

        public double Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public double Blend
        {
            get { return blend; }
            set { blend = value; }
        }
    }
}

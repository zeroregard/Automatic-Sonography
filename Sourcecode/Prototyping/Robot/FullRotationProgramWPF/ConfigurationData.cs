using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullRotationProgramWPF
{
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

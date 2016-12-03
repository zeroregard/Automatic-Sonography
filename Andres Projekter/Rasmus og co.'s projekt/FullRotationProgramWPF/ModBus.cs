using System;
using System.Net;
using Util;

namespace FullRotationProgramWPF
{
    /// <summary>
    /// <para>ModBus is a class to create connection to the ModBus on the Universal Robot</para>
    /// <para>ModBus </para>
    /// </summary>
    public class ModBus
    {
        Master MBmaster;
        //private bool isConnected = false;
        string ip;
        ushort port = 502;

        public delegate void ModBusExceptionEventHandler(string error, string source);
        public static event ModBusExceptionEventHandler OnException;

        public ModBus(String ipAddress)
        {
            ip = ipAddress;
            try
            {
                MBmaster = new Master(ipAddress, port);
                MBmaster.OnException -= new Master.ExceptionData(MBmaster_OnException);
                MBmaster.OnException += new Master.ExceptionData(MBmaster_OnException);
                //isConnected = MBmaster.connected;
            }
            catch (Exception e)
            {
                OnException("Der er mistet forbindelse til mellem RE og SE.", "MODBUS");
                if (MBmaster != null)
                {
                    MBmaster.disconnect();
                }
            }
        }

        private void MBmaster_OnException(ushort id, byte unit, byte function, byte exception)
        {
            string exc = "Modbus says error: ";
            switch (exception)
            {
                case Master.excIllegalFunction: exc += "Illegal function!"; break;
                case Master.excIllegalDataAdr: exc += "Illegal data adress!"; break;
                case Master.excIllegalDataVal: exc += "Illegal data value!"; break;
                case Master.excSlaveDeviceFailure: exc += "Slave device failure!"; break;
                case Master.excAck: exc += "Acknoledge!"; break;
                case Master.excGatePathUnavailable: exc += "Gateway path unavailbale!"; break;
                case Master.excExceptionTimeout: exc += "Slave timed out!"; break;
                case Master.excExceptionConnectionLost:
                    //exc += "Connection is lost!";
                    try
                    {
                        MBmaster.connect(ip, port);
                        break;
                    }
                    catch (Exception e)
                    {
                        OnException("Der er mistet forbindelse til mellem RE og SE.", "MODBUS");
                        if (MBmaster != null)
                        {
                            MBmaster.disconnect();
                        }
                        break;
                    }

                case Master.excExceptionNotConnected: exc += "Not connected!"; break;
            }

            //MessageBox.Show(exc, "Modbus slave exception");
        }

        public void Disconnect()
        {
            MBmaster.disconnect();
            //isConnected = false;
        }

        public bool IsConnected()
        {
            if (MBmaster != null)
                return MBmaster.connected;
            else
                return false;
        }

        public void SendPose(URPose pose)
        {
            byte[] data = new byte[12];

            ushort ID = 2;
            byte unit = 0;
            ushort StartAddress = 135;

            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(pose.Xpose * 1000))), 0, data, 0, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(pose.Ypose * 1000))), 0, data, 2, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(pose.Zpose * 1000))), 0, data, 4, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(pose.RXpose * 1000))), 0, data, 6, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(pose.RYpose * 1000))), 0, data, 8, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(pose.RZpose * 1000))), 0, data, 10, 2);

            byte[] result = new byte[100];
            if (MBmaster != null)
            {
                MBmaster.WriteMultipleRegister(ID, unit, StartAddress, data, ref result);
            }
        }

        public ForceData ReadURForce()
        {
            Util.ForceData forceData = new Util.ForceData();
            if (MBmaster != null)
            {
                ushort ID = 4;
                byte unit = 0;
                ushort StartAddress = 137;
                byte Length = Convert.ToByte("6");

                byte[] respons = new byte[1];

                MBmaster.ReadInputRegister(ID, unit, StartAddress, Length, ref respons);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(respons);

                if (respons.Length == 12)
                {
                    forceData.Xforce = Convert.ToDouble(BitConverter.ToInt16(respons, 10)) / 100;
                    forceData.Yforce = Convert.ToDouble(BitConverter.ToInt16(respons, 8)) / 100;
                    forceData.Zforce = Convert.ToDouble(BitConverter.ToInt16(respons, 6)) / 100;
                    forceData.RXforce = Convert.ToDouble(BitConverter.ToInt16(respons, 4)) / 100;
                    forceData.RYforce = Convert.ToDouble(BitConverter.ToInt16(respons, 2)) / 100;
                    forceData.RZforce = Convert.ToDouble(BitConverter.ToInt16(respons, 0)) / 100;
                }
            }
            return forceData;
        }
    }
}

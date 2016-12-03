using System;
using System.Net;
using Util;

namespace DATA
{
    /// <summary>
    /// <para>ModBus is a class to create connection to the ModBus on the Universal Robot</para>
    /// This class establish a connection to a modbus server on port 502 and
    /// uses the ModbusTCP library to write/read informations to/from registers on the modbus server.
    /// </summary>
    public class ModBus
    {
        Master MBmaster;

        string ip;
        ushort port = 502;

        /// <summary>Modbus exception event. This event is called when an exception occurres.</summary>
        /// <param name="error">Error message</param>
        /// <param name="source">Source of the error</param>
        public delegate void ModBusExceptionEventHandler(string error, string source);
        /// <summary>Modbus exception event. This event is called when an exception occurres.</summary>
        public static event ModBusExceptionEventHandler OnException;

        /// <summary>Constructor that initiates the connection to the ModBus on port 502.</summary>
        /// <param name="ipAddress">Ip address of modbus slave.</param>
        public ModBus(String ipAddress)
        {
            ip = ipAddress;
            try
            {
                MBmaster = new Master(ipAddress, port);
                MBmaster.OnException -= new Master.ExceptionData(MBmaster_OnException);
                MBmaster.OnException += new Master.ExceptionData(MBmaster_OnException);
            }
            catch (Exception e)
            {
                OnException("Der kan ikke forbindes til modbus.", "MODBUS");
                if (MBmaster != null)
                {
                    MBmaster.disconnect();
                }
            }
        }

        /// <summary>Exception handler when an exception occurres on the modbus connection.</summary>
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
                case Master.excExceptionConnectionLost:                                     // Connection lost is the only exception handled in this solution
                    try
                    {
                        MBmaster.connect(ip, port);                                         // Exception is used to try to reconnect to the modbus slave either when the modbus has shut down
                        break;                                                              // or when it has closed the connection due to too long time between calls
                    }
                    catch (Exception e)
                    {
                        OnException("Der er mistet forbindelse til modbus.", "MODBUS");
                        if (MBmaster != null)
                        {
                            MBmaster.disconnect();
                        }
                        break;
                    }

                case Master.excExceptionNotConnected: exc += "Not connected!"; break;
            }
        }

        /// <summary>Disconnects to the modbus slave.</summary>
        public void Disconnect()
        {
            if (MBmaster != null)
            {
                MBmaster.disconnect();
            }
        }

        /// <summary>Shows whether the connection is alive.</summary>
        public bool IsConnected()
        {
            if (MBmaster != null)
                return MBmaster.connected;
            else
                return false;
        }

        /// <summary>Sends pose to the modbus general purpose register.</summary>
        /// <param name="pose">New position for the universal robot.</param>
        public void SendPose(URPose pose)
        {
            byte[] data = new byte[12];

            ushort ID = 2;
            byte unit = 0;
            ushort StartAddress = 128;      // StartAddress in the general purpose register

            // Generates a byte array with data in the specific order based on the documentation, to be send to the modbus register
            // Every position value is multiplied by 1000 to secure data precision and then converted into 16-bit signed integer
            // Byte is reversed to NetwotkOrder and then converted to a word
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

        public void SendJoints(URPose pose)
        {
            byte[] data = new byte[12];

            ushort ID = 2;
            byte unit = 0;
            ushort StartAddress = 134;      // StartAddress in the general purpose register

            // Generates a byte array with data in the specific order based on the documentation, to be send to the modbus register
            // Every position value is multiplied by 1000 to secure data precision and then converted into 16-bit signed integer
            // Byte is reversed to NetwotkOrder and then converted to a word
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(pose.RXpose * 1000))), 0, data, 0, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(pose.RYpose * 1000))), 0, data, 2, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(pose.RZpose * 1000))), 0, data, 4, 2);

            byte[] result = new byte[100];
            if (MBmaster != null)
            {
                MBmaster.WriteMultipleRegister(ID, unit, StartAddress, data, ref result);
            }
        }

        /// <summary>Reads the force data from the modbus general purpose register.</summary>
        public ForceData ReadURForce()
        {
            ForceData forceData = new ForceData();
            if (MBmaster != null)
            {
                ushort ID = 4;
                byte unit = 0;
                ushort StartAddress = 137;                  // StartAddress in the general purpose register
                byte Length = 6;                            // Length of the desired data array. A representation of the number of registers to be read.

                byte[] respons = new byte[1];

                MBmaster.ReadInputRegister(ID, unit, StartAddress, Length, ref respons);

                if (respons != null && respons.Length == 12)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(respons);

                        // When the computers byte order is little endian the respons array is reversed
                        // And therefor the first value of the ForceData is collected from the buttom and up.
                        forceData.Xforce = Convert.ToDouble(BitConverter.ToInt16(respons, 10)) / 100;
                        forceData.Yforce = Convert.ToDouble(BitConverter.ToInt16(respons, 8)) / 100;
                        forceData.Zforce = Convert.ToDouble(BitConverter.ToInt16(respons, 6)) / 100;
                        forceData.RXforce = Convert.ToDouble(BitConverter.ToInt16(respons, 4)) / 100;
                        forceData.RYforce = Convert.ToDouble(BitConverter.ToInt16(respons, 2)) / 100;
                        forceData.RZforce = Convert.ToDouble(BitConverter.ToInt16(respons, 0)) / 100;
                    }
                    else
                    {
                        forceData.Xforce = Convert.ToDouble(BitConverter.ToInt16(respons, 0)) / 100;
                        forceData.Yforce = Convert.ToDouble(BitConverter.ToInt16(respons, 2)) / 100;
                        forceData.Zforce = Convert.ToDouble(BitConverter.ToInt16(respons, 4)) / 100;
                        forceData.RXforce = Convert.ToDouble(BitConverter.ToInt16(respons, 6)) / 100;
                        forceData.RYforce = Convert.ToDouble(BitConverter.ToInt16(respons, 8)) / 100;
                        forceData.RZforce = Convert.ToDouble(BitConverter.ToInt16(respons, 10)) / 100;
                    }
                }
            }
            return forceData;
        }

        /// <summary>Sends new configurations to the modbus general purpose register, which the robot will use.</summary>
        /// <param name="confData">New configurations for the universal robot.</param>
        public void SendConfigurations(ConfigurationData confData)
        {
            byte[] data = new byte[22];

            ushort ID = 6;
            byte unit = 0;
            ushort StartAddress = 143;                  // StartAddress in the general purpose register

            // Generates a byte array with data in the specific order based on the documentation, to be send to the modbus register
            // Every value is multiplied by 1000 to secure data precision and then converted into 16-bit signed integer
            // Byte is reversed to NetwotkOrder and then converted to a word
            // The first word in the data array is a 1 to signal the robots program that a new configuration has been send
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(1))), 0, data, 0, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.OffsetX * 1000))), 0, data, 2, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.OffsetY * 1000))), 0, data, 4, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.OffsetZ * 1000))), 0, data, 6, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.OffsetRx * 1000))), 0, data, 8, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.OffsetRy * 1000))), 0, data, 10, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.OffsetRz * 1000))), 0, data, 12, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.Payload * 1000))), 0, data, 14, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.Acceleration * 1000))), 0, data, 16, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.Speed * 1000))), 0, data, 18, 2);
            Array.Copy(BitConverter.GetBytes((short)IPAddress.HostToNetworkOrder((short)Convert.ToInt16(confData.Blend * 1000))), 0, data, 20, 2);

            byte[] result = new byte[100];
            if (MBmaster != null)
            {
                MBmaster.WriteMultipleRegister(ID, unit, StartAddress, data, ref result);
            }
        }

        /// <summary>Gets the robots current configuration data from the modbus general purpose register.</summary>
        public ConfigurationData GetConfigurations()
        {
            ConfigurationData conf = new ConfigurationData();
            if (MBmaster != null)
            {
                ushort ID = 10;
                byte unit = 0;
                ushort StartAddress = 144;                  // StartAddress in the general purpose register
                byte Length = 10;                           // Length of the desired data array. A representation of the number of registers to be read.

                byte[] respons = new byte[20];

                MBmaster.ReadInputRegister(ID, unit, StartAddress, Length, ref respons);

                if (respons != null && respons.Length == 20)
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(respons);

                        // When the computers byte order is little endian the respons array is reversed
                        // And therefor the first value of the ConfigurationData is collected from the buttom and up.
                        conf.OffsetX = Convert.ToDouble(BitConverter.ToInt16(respons, 18)) / 1000;
                        conf.OffsetY = Convert.ToDouble(BitConverter.ToInt16(respons, 16)) / 1000;
                        conf.OffsetZ = Convert.ToDouble(BitConverter.ToInt16(respons, 14)) / 1000;
                        conf.OffsetRx = Convert.ToDouble(BitConverter.ToInt16(respons, 12)) / 1000;
                        conf.OffsetRy = Convert.ToDouble(BitConverter.ToInt16(respons, 10)) / 1000;
                        conf.OffsetRz = Convert.ToDouble(BitConverter.ToInt16(respons, 8)) / 1000;
                        conf.Payload = Convert.ToDouble(BitConverter.ToInt16(respons, 6)) / 1000;
                        conf.Acceleration = Convert.ToDouble(BitConverter.ToInt16(respons, 4)) / 1000;
                        conf.Speed = Convert.ToDouble(BitConverter.ToInt16(respons, 2)) / 1000;
                        conf.Blend = Convert.ToDouble(BitConverter.ToInt16(respons, 0)) / 1000;
                    }
                    else
                    {
                        conf.OffsetX = Convert.ToDouble(BitConverter.ToInt16(respons, 0)) / 1000;
                        conf.OffsetY = Convert.ToDouble(BitConverter.ToInt16(respons, 2)) / 1000;
                        conf.OffsetZ = Convert.ToDouble(BitConverter.ToInt16(respons, 4)) / 1000;
                        conf.OffsetRx = Convert.ToDouble(BitConverter.ToInt16(respons, 6)) / 1000;
                        conf.OffsetRy = Convert.ToDouble(BitConverter.ToInt16(respons, 8)) / 1000;
                        conf.OffsetRz = Convert.ToDouble(BitConverter.ToInt16(respons, 10)) / 1000;
                        conf.Payload = Convert.ToDouble(BitConverter.ToInt16(respons, 12)) / 1000;
                        conf.Acceleration = Convert.ToDouble(BitConverter.ToInt16(respons, 14)) / 1000;
                        conf.Speed = Convert.ToDouble(BitConverter.ToInt16(respons, 16)) / 1000;
                        conf.Blend = Convert.ToDouble(BitConverter.ToInt16(respons, 18)) / 1000;
                    }
                }
            }
            return conf;
        }
    }
}

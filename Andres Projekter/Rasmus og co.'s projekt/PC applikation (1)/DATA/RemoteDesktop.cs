﻿using System;
using System.IO;
using System.Net.Sockets;

namespace DATA
{
    /// <summary>
    /// <para>Client that connects to a remote desktop server and receives an invitation.</para>
    /// Connects to the remote desktop server on port 8000 and reads the invitation send from the server.
    /// Returns the invitation for an AxRDPVeiwer to connect to.
    /// </summary>
    public class RemoteDesktop
    {
        /// <summary>Remote desktop exception event. This event is called when an exception occurres when trying to connect to the remote desktop server.</summary>
        /// <param name="error">Error message</param>
        /// <param name="source">Source of the error</param>
        public delegate void RemoteDesktopExceptionEventHandler(string error, string source);
        /// <summary>Remote desktop exception event. This event is called when an exception occurres when trying to connect to the remote desktop server.</summary>
        public static event RemoteDesktopExceptionEventHandler OnException;

        /// <summary>Variable to show if the connection is active.</summary>
        bool isConnected = false;

        /// <summary>Connects to the remote desktop server and retreives an invitation generated by the server.</summary>
        /// <param name="ip">IpAddress for the remote desktop server.</param>
        public string GetInvitation(string ip)
        {
            string invitation;

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(ip, 8000);
                Stream stm = client.GetStream();

                byte[] buffer = new byte[200];
                stm.Read(buffer, 0, buffer.Length);
                invitation = System.Text.Encoding.UTF8.GetString(buffer);
                isConnected = true;
                return invitation;
            }
            catch(Exception e)
            {
                if (OnException != null)
                    OnException("Der kunne ikke oprettes forbindelse til fjernskrivebord.", "RMD");
                invitation = null;
                isConnected = false;
                return invitation;
            }
        }

        /// <summary>Shows the state of the connection.</summary>
        public bool IsConnected()
        {
            return isConnected;
        }
    }
}

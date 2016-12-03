using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace URSocketTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// kode fra http://support.universal-robots.com/Technical/EthernetCommunicationFromScript?foswiki_redirect_cache=2545273fc6e0e311f9c9c119c4cd4fcc

			// The IP address of the server (the PC on which this program is running)
			string sHostIpAddress = "127.0.0.1";
			//string sHostIpAddress = "localhost";
			// Standard port number
			int nPort = 30020;

			// The following names are used in the PolyScope script for refencing the
			// three working points:
			// Name of an arbitrary work point 1
			const string csMsgPoint1 = "Point_1";
			// Name of an arbitrary work point 2
			const string csMsgPoint2 = "Point_2";
			// Name of an arbitrary work point 3
			const string csMsgPoint3 = "Point_3";

			Console.WriteLine("Opening IP Address: " + sHostIpAddress);
			IPAddress ipAddress = IPAddress.Parse(sHostIpAddress);        // Create the IP address
			Console.WriteLine("Starting to listen on port: " + nPort);
			TcpListener tcpListener = new TcpListener(ipAddress, nPort);  // Create the tcp Listener
			tcpListener.Start();                                          // Start listening

			// Keep on listening forever
			while (true)
			{
				TcpClient tcpClient = tcpListener.AcceptTcpClient();        // Accept the client
				Console.WriteLine("Accepted new client");
				NetworkStream stream = tcpClient.GetStream();               // Open the network stream
				while (tcpClient.Client.Connected)
				{
					// Create a byte array for the available bytes
					byte[] arrayBytesRequest = new byte[tcpClient.Available];
					// Read the bytes from the stream
					int nRead = stream.Read(arrayBytesRequest, 0, arrayBytesRequest.Length);
					if (nRead > 0)
					{
						// Convert the byte array into a string
						string sMsgRequest = ASCIIEncoding.ASCII.GetString(arrayBytesRequest);
						Console.WriteLine("Received message request: " + sMsgRequest);
						string sMsgAnswer = string.Empty;

						// Check which workpoint is requested
						if (sMsgRequest.Equals(csMsgPoint1))
						{
							// Some point in space for work point 1
							sMsgAnswer = "(0.4, 0, 0.5, 0, -3.14159, 0)";
						}
						else if (sMsgRequest.Equals(csMsgPoint2))
						{
							// Some point in space for work point 2
							sMsgAnswer = "(0.3, 0.5, 0.5, 0, 3.14159, 0)"; ;
						}
						else if (sMsgRequest.Equals(csMsgPoint3))
						{
							// Some point in space for work point 3
							sMsgAnswer = "(0, 0.6, 0.5, 0, 3.14159, 0)";
						}

						if (sMsgAnswer.Length > 0)
						{
							Console.WriteLine("Sending message answer: " + sMsgAnswer);
							// Convert the point into a byte array
							byte[] arrayBytesAnswer = ASCIIEncoding.ASCII.GetBytes(sMsgAnswer);
							// Send the byte array to the client
							stream.Write(arrayBytesAnswer, 0, arrayBytesAnswer.Length);
						}
					}
					else
					{
						if (tcpClient.Available == 0)
						{
							Console.WriteLine("Client closed the connection.");
							// No bytes read, and no bytes available, the client is closed.
							stream.Close();
						}
					}
				}
			}
		}
	}
}

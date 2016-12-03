using System;
using Declarations.Media;
using Declarations.Players;
using Implementation;
using System.Net.Sockets;
using System.IO;

namespace DATA
{
    /// <summary>
    /// <para>Client that connects to a server and plays a videofeed.</para>
    /// The TcpClient connects to the server on port 8000 to confirm connection.
    /// Is the server connection confirmed this class can connect to a Real Time Streaming Protocol (RTSP) on port 580 on the server.
    /// </summary>
    public class VideoFeed
    {
        IMedia media;

        /// <summary>VideoFeed Exception event. This event is called when the connection to the videofeed has ended.</summary>
        /// <param name="error">Error message</param>
        /// <param name="source">Source of the error</param>
        public delegate void VideoFeedExceptionEventHandler(string error, string source);
        /// <summary>VideoFeed Exception event. This event is called when the connection to the videofeed has ended.</summary>
        public static event VideoFeedExceptionEventHandler OnException;

        /// <summary>Variable to show if the connection is active.</summary>
        bool isConnected = false;

        /// <summary>Create instance of VideoFeed.</summary>
        public VideoFeed()
        {

        }

        /// <summary>Client that connects to the server to confirm the connection.</summary>
        /// <param name="ip">Ip address for the videofeed server</param>
        public bool ConfirmConnection(string ip)
        {
            string result = null;
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(ip, 9000);
                Stream stm = client.GetStream();

                byte[] buffer = new byte[4];
                stm.Read(buffer, 0, buffer.Length);
                result = System.Text.Encoding.UTF8.GetString(buffer);               // receives a string from the server indicating whether the RTSP-server is running and the webcam is on
                isConnected = true;
            }
            catch (Exception e)
            {
                if (OnException != null)
                    OnException("Der kunne ikke oprettes forbindelse til videofeed.", "VF");
                isConnected = false;
            }

            if (result == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsConnected()
        {
            return isConnected;
        }

        /// <summary>Opens and plays the videofeed from the server on port 580.</summary>
        /// <param name="ipAddress">Ip address for the videofeed server</param>
        /// <param name="vPlayer">Reference to the videoplayer in the Presentation frame to play the videofeed.</param>
        public void Connect(string ipAddress, ref IVideoPlayer vPlayer)
        {
            string port = "580";
            string rtsp = "rtsp://" + ipAddress + ":" + port;

            media = new MediaPlayerFactory(true).CreateMedia<IMedia>(rtsp);

            vPlayer.Events.MediaEnded += Events_MediaEnded;

            vPlayer.Open(media);
            vPlayer.Play();
        }

        /// <summary>Handler for the event MediaEnded.</summary>
        private void Events_MediaEnded(object sender, EventArgs e)
        {
            IVideoPlayer obj = sender as IVideoPlayer;
            obj.Stop();                                                 // Stops the videoplayer when connection is lost
            if (OnException != null)
                OnException("Der er mistet forbindelse til videofeed", "VF");
        }
    }
}

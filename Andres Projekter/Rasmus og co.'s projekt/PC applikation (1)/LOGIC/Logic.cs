using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATA;
using Util;
using System.Threading;
using System.Diagnostics;
using Declarations.Media;
using Declarations.Players;
using Implementation;

namespace LOGIC
{
    /// <summary>
    /// <para>Logic is a singleton class to secure only one instance of this class.</para>
    /// This class is the main entrance to the business logic and can be passed between the windows in the application.
    /// This class handles all exceptions for the Presentation layer to listen for, and it contains the instances of the
    /// scan units from the local xml-file and the established connection.
    /// </summary>
    public sealed class Logic
    {
        /// <summary>Instance of the class Logic.</summary>
        private static readonly Logic _instance = new Logic();

        /// <summary>Exception event. This event is thrown whenever an exception occurres in runtime.</summary>
        /// <param name="error">Error message</param>
        /// <param name="source">Source of the error</param>
        public delegate void LogicExceptionEventHandler(string error, string source);
        /// <summary>Exception event. This event is thrown whenever an exception occurres in runtime.</summary>
        public static event LogicExceptionEventHandler OnException;

        // Constructor initializes all the exceptions from the different processes and collects them at one place in CallException
        private Logic()
        {
            GeoMagicTouch.OnException += new GeoMagicTouch.GeoMagicExceptionEventHandler(CallExeption);
            ReadWriteUnitData.OnException += new ReadWriteUnitData.UnitDataExceptionEventHandler(CallExeption);
            RemoteDesktop.OnException += new RemoteDesktop.RemoteDesktopExceptionEventHandler(CallExeption);
            URSocket.OnException += new URSocket.URExceptionEventHandler(CallExeption);
            ModBus.OnException += new ModBus.ModBusExceptionEventHandler(CallExeption);
            VideoFeed.OnException += new VideoFeed.VideoFeedExceptionEventHandler(CallExeption);
            Positioning.OnException += new Positioning.PositioningEventHandler(CallExeption);
        }

        /// <summary>Gets the instance of the logic class.</summary>
        public static Logic GetInstance()
        {
            return _instance;
        }

        /// <summary>Throws the OnException event.</summary>
        private void CallExeption(string error, string source)
        {
            if (OnException != null)
                OnException(error, source);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Scanning Units

        /// <summary>Scan units collected from the local xml-file.</summary>
        ScanUnits scanUnits;
        /// <summary>Current selceted unit.</summary>
        public Unit selectedUnit { get; private set; }

        /// <summary>Get the scan units stored in the local xml-file.</summary>
        public ScanUnits GetScanUnits()
        {
            scanUnits = new ReadWriteUnitData().GetScanUnits();
            return scanUnits;
        }

        /// <summary>Save the scan units in the local xml-file.</summary>
        /// <param name="newUnit">Unit containing the new data to be stored.</param>
        public void SaveNewUnit(Unit newUnit)
        {
            scanUnits.Units.Add(newUnit);
            new ReadWriteUnitData().SaveScanUnits(scanUnits);
        }

        /// <summary>Select a new unit to be tested that has not been stored yet.</summary>
        /// <param name="testUnit">Unit that needs to be tested.</param>
        public void SelectUnit(Unit testUnit)
        {
            selectedUnit = testUnit;
        }

        /// <summary>Finds the selected unit in the list of all units.</summary>
        /// <param name="selectedItem">Selected item to be used in a search for the responding unit.</param>
        public Unit FindSelectedUnit(object selectedItem)
        {
            selectedUnit = scanUnits.Units.Find(unit => unit.id == selectedItem.ToString());
            return selectedUnit;
        }

        /// <summary>Gets the selected unit.</summary>
        public Unit GetSelectedUnit()
        {
            return selectedUnit;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        // Connection establishment

        /// <summary>Contains the current connection and gives access to different objects in the specific connection.</summary>
        public Connection connection { get; private set; }

        /// <summary>Initializes the establishment of a new connection. 
        /// Connection result is given in the OnConnectionCompleted event in the Connection class.</summary>
        public void EstablishConnection()
        {
            connection = new Connection();
            connection.EstablishConnection();
        }
    }
}

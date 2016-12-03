using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DATA
{
    /// <summary>Gets and sets ScanUnits in a local xml-file.</summary>
    public class ReadWriteUnitData
    {
        /// <summary>Unit data exception event. This event is called when an exception occurres when reading/writing the xml-file</summary>
        /// <param name="error">Error message</param>
        /// <param name="source">Source of the error</param>
        public delegate void UnitDataExceptionEventHandler(string error, string source);
        /// <summary>Unit data exception event. This event is called when an exception occurres when reading/writing the xml-file</summary>
        public static event UnitDataExceptionEventHandler OnException;

        /// <summary>Gets and sets ScanUnits in a local xml-file.</summary>
        public ReadWriteUnitData()
        { }

        /// <summary>Gets the ScanUnits stored in a local xml-file</summary>
        public ScanUnits GetScanUnits()
        {
            ScanUnits unitList = new ScanUnits();

            try
            {
                FileStream xmlStream = new FileStream("scanUnits.xml", FileMode.Open);

                XmlReader xmlReader = XmlReader.Create(xmlStream);

                XmlSerializer serializer = new XmlSerializer(typeof(ScanUnits));
                unitList = serializer.Deserialize(xmlReader) as ScanUnits;
            }
            catch (Exception e)
            {
                if (OnException != null)
                    OnException(e.Message, this.ToString());
            }
            return unitList;
        }

        /// <summary>Store the ScanUnits in a local xml-file</summary>
        /// <param name="units">Collection of units to be stored</param>
        public void SaveScanUnits(ScanUnits units)
        {
            try
            {
                FileStream xmlStream = new FileStream("scanUnits.xml", FileMode.Create);

                XmlWriter xmlWriter = XmlWriter.Create(xmlStream);

                XmlSerializer serializer = new XmlSerializer(typeof(ScanUnits));
                serializer.Serialize(xmlWriter, units);
            }
            catch (Exception e)
            {
                if (OnException != null)
                    OnException(e.Message, this.ToString());
            }
        }
    }
}

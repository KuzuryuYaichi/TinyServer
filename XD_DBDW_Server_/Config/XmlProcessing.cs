using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XD_DBDW_Server
{
    public class XmlProcessing
    {
        private XmlNode BsePort;
        private XmlNode IP;
        private XmlNode PortRec;
        private XmlNode PortSnd;
        private XmlNode Disk;
        private XmlNode mark;
        private XmlNode YK;

        public XmlProcessing()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Config/NetConfig.xml");
            XmlNode configNode = doc.SelectSingleNode("config");
            BsePort = configNode.SelectSingleNode("BsePort");
            PortRec = configNode.SelectSingleNode("PortRec");
            PortSnd = configNode.SelectSingleNode("PortSnd");
            IP = configNode.SelectSingleNode("IP");
            Disk = configNode.SelectSingleNode("Disk");
            mark = configNode.SelectSingleNode("mark");
            YK = configNode.SelectSingleNode("YK");
        }

        public string Read_LocalIP()
        {
            return IP.InnerText;
        }

        public string Read_LocalPort()
        {
            return PortRec.InnerText;
        }
    }
}

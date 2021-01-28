using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace XD_DBDW_Server
{
    public class XmlProcessing
    {
        private XmlNode LocalIPNode;
        private XmlNode DevIPNode;
        private XmlNode CC_PortNode;
        private XmlNode RC_PortNode;
        private XmlNode Net10G_LocalIPNode;
        private XmlNode Net10G_LocalPortNode;
        private XmlNode Net10G_GroupIPNode;
        private XmlNode DevIDNode;
        private XmlNode RFCtrlVersionNode;
        private XmlNode RFRFCtrlTypeNode;

        public XmlProcessing()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"NetConfig.xml");
            XmlNode configNode = doc.SelectSingleNode("config");
            LocalIPNode = configNode.SelectSingleNode("LocalIP");
            DevIPNode = configNode.SelectSingleNode("DevIP");
            CC_PortNode = configNode.SelectSingleNode("CC_Port");
            RC_PortNode = configNode.SelectSingleNode("RC_Port");
            Net10G_LocalIPNode = configNode.SelectSingleNode("Net10G_LocalIP");
            Net10G_LocalPortNode = configNode.SelectSingleNode("Net10G_LocalPort");
            Net10G_GroupIPNode = configNode.SelectSingleNode("Net10G_GroupIP");
            DevIDNode = configNode.SelectSingleNode("DevID");
            RFCtrlVersionNode = configNode.SelectSingleNode("RFCtrlVersion");
            RFRFCtrlTypeNode = configNode.SelectSingleNode("RFRFCtrlType");
        }

        public string ReadRFCtrlVersion()
        {
            return RFCtrlVersionNode.InnerText;

        }
        public string ReadRFRFCtrlType()
        {
            return RFRFCtrlTypeNode.InnerText;

        }
        public string ReadDevID()
        {
            return DevIDNode.InnerText;
        }

        public string ReadCtrlLocalIP()
        {
            return LocalIPNode.InnerText;

        }
        public void WriteCtrlLocalIP(string xmlText)
        {
            LocalIPNode.InnerText = xmlText;
        }
        public string ReadCtrlDevIP()
        {
            return DevIPNode.InnerText;

        }
        public void WriteCtrlDevIP(string xmlText)
        {
            DevIPNode.InnerText = xmlText;
        }
        public string ReadCC_Port()
        {
            return CC_PortNode.InnerText;

        }
        public void WriteCC_Port(string xmlText)
        {
            CC_PortNode.InnerText = xmlText;
        }

        public string ReadRC_Port()
        {
            return RC_PortNode.InnerText;

        }
        public void WriteRC_Port(string xmlText)
        {
            RC_PortNode.InnerText = xmlText;
        }

        public string ReadNet10G_LocalIP()
        {
            return Net10G_LocalIPNode.InnerText;
        }

        public void WriteNet10G_LocalIP(string xmlText)
        {
            Net10G_LocalIPNode.InnerText = xmlText;
        }
        public string ReadNet10G_LocalPort()
        {
            return Net10G_LocalPortNode.InnerText;
        }

        public void WriteNet10G_LocalPort(string xmlText)
        {
            Net10G_LocalPortNode.InnerText = xmlText;
        }
        public string ReadNet10G_GroupIP()
        {
            return Net10G_GroupIPNode.InnerText;

        }
        public void WriteNet10G_GroupIP(string xmlText)
        {
            Net10G_GroupIPNode.InnerText = xmlText;
        }
    }
}

using System.Net.Http;
using System.Text;
using System.Xml;

namespace LabelZoom.MocaClient
{
    internal class MocaRequestContent : StringContent
    {
        private const string CONTENT_TYPE = "application/moca-xml";

        public MocaRequestContent(string xmlContent) : base(xmlContent, Encoding.UTF8, CONTENT_TYPE) { }

        public MocaRequestContent(XmlDocument xmlDocument) : this(xmlDocument.OuterXml) { }
    }
}

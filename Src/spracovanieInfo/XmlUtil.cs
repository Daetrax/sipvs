using System;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace spracovanieInfo
{
    static class XmlUtil
    {
        
        public static string ConvertObjectToXml(object objectToSerialize)
        {
            // simple conversion of object to xml
            XmlSerializer xmlSerializer = new XmlSerializer(objectToSerialize.GetType());
            StringWriter stringWriter = new StringWriter();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "https://github.com/Daetrax/sipvs/blob/master/Xml_data/");
            xmlSerializer.Serialize(stringWriter, objectToSerialize, ns);

            return stringWriter.ToString();
        }

        

        public static void SaveXML(Request request)
        {
            // if something went wrong, null is returned
            if (request == null)
            {
                return;
            }

            // serialize and save xml
            XmlSerializer xs = new XmlSerializer(typeof(Request));
            TextWriter tw = new StreamWriter($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml");
            xs.Serialize(tw, request);
            tw.Close();
            
        }

        private static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            XmlSeverityType type = XmlSeverityType.Warning;
            if (Enum.TryParse<XmlSeverityType>("Error", out type))
            {

                
                MessageBoxResult result = MessageBox.Show($"Invalid form {e.Message}",
                                         "",
                                         MessageBoxButton.OK,
                                         MessageBoxImage.Warning);


                if (type == XmlSeverityType.Error)
                {

                    //throw new Exception(e.Message);
                }

            }
        }


        public static void ValidateXml(Request request, MainWindow window)
        {
            // get schema
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt", @"../../../../Xml_data/sipvt.xsd");

            // create serializable request
            if (request == null)
            {
                return;
            }

            XDocument doc = XDocument.Parse(XmlUtil.ConvertObjectToXml(request));
            
            // validation handler takes care of errors or warnings during validation
            doc.Validate(schema, ValidationEventHandler);
        }


        public static string TransformXmlToHtml()
        {
            // read xslt
            XmlReader xsltReader = XmlReader.Create(@"../../../../Xml_data/sipvt.xsl");
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(xsltReader);

            XslCompiledTransform docXsl = new XslCompiledTransform();
            docXsl.Load(@"../../../../Xml_data\sipvt.xsl");

            // currently we transform saved request
            XmlDocument doc = new XmlDocument();
            doc.Load($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml");

            StringWriter results = new StringWriter();
            //using (XmlReader reader = XmlReader.Create(new StringReader(doc.ToString())))
            using (XmlReader reader = XmlReader.Create($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml"))
            {
                docXsl.Transform(reader, null, results);
            }
            // show and save result to html
            File.WriteAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/sipvs.html", results.ToString());
            // form window creates a new window with the result string
            return results.ToString();
        }



    }
}

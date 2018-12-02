using Org.BouncyCastle.Tsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace spracovanieInfo
{
    static class Utils
    {
        private static int sequenceCounter = 0;

        public static string generateSequenceName()
        {
            return $"Book{sequenceCounter++}";
        }
        
        public static string getTimestamp(string text)
        {
            var signedDocument = File.ReadAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/signedDocument.xml");
            //signedDocument = text;

            //convert to Base64
            var data = Convert.ToBase64String(Encoding.ASCII.GetBytes(signedDocument));
            
            // call the service
            byte[] soapResponse = soapController.CallWebService(data);
            TimeStampResponse resp = new TimeStampResponse(soapResponse);

            // status 0 means success
            if (resp.Status != 0)
            {
                Console.Write("Err occurred");
            }

            // token contains the info we need
            TimeStampToken token = resp.TimeStampToken;
            var encToken = token.GetEncoded();
            
            // convert token to Base64 and return
            return Convert.ToBase64String(encToken);
                
        }



        public static void stamp(string filepath)
        {
            XmlDocument doc = new XmlDocument();
            // TODO: make loading via file explorer

            //doc.Load($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/signedDocument.xml");
            doc.Load(filepath);

            XmlNode signature = doc.GetElementsByTagName("ds:Signature")[0];
            if (signature == null)
            {
                //displayError(Constants.GENERIC_ERROR, "This document is missing the Signature element.");
                return;
            }

            XmlNode qualifyingProps = doc.GetElementsByTagName("xades:QualifyingProperties")[0];
            if (qualifyingProps == null)
            {
                return;
            }

            XmlNode sigVal = doc.GetElementsByTagName("ds:SignatureValue")[0];
            if (sigVal == null)
            {
                return;
            }

            string token = null;
            try
            {
                token = getTimestamp(sigVal.InnerText);

            }
            catch (Exception exception)
            {
                return;
            }

            if (token == null)
            {
                Console.WriteLine("Err.");
            }

            //TimeStampRequest
            //TimeStampResponse abcc = token;
            string xades = "xades:";
            XmlElement unsignedProps = doc.CreateElement("xades", "UnsignedProperties", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement unsignedSigProps = doc.CreateElement("xades", "UnsignedSignatureProperties", "http://uri.etsi.org/01903/v1.3.2#");
            XmlElement sigTimestamp = doc.CreateElement("xades", "SignatureTimeStamp", "http://uri.etsi.org/01903/v1.3.2#");
            String sigValId = sigVal.Attributes.GetNamedItem("Id").InnerText;
            sigTimestamp.SetAttribute("Target", "#" + sigValId);
            sigTimestamp.SetAttribute("Id", sigValId + "TimeStamp");

            XmlElement encapsedTimestamp = doc.CreateElement("xades", "EncapsulatedTimeStamp", "http://uri.etsi.org/01903/v1.3.2#");
            unsignedProps.AppendChild(unsignedSigProps);
            unsignedSigProps.AppendChild(sigTimestamp);
            sigTimestamp.AppendChild(encapsedTimestamp);
            XmlText sigTimestampTokenNode = doc.CreateTextNode(token);
            encapsedTimestamp.AppendChild(sigTimestampTokenNode);

            qualifyingProps.AppendChild(unsignedProps);

            doc.Save($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/signed_timestamped_document.xml");
            //TransformerFactory transformerFactory = TransformerFactory.newInstance();
            //Transformer transformer = transformerFactory.newTransformer();
            //transformer.setOutputProperty(OutputKeys.STANDALONE, "yes");

            //DOMSource source = new DOMSource(document);
            //String filename = f.getName();
            //File newFile = new File("requests/timestamped/" + filename);
            //StreamResult result = new StreamResult(newFile.getAbsolutePath());
            //transformer.transform(source, result);
        }
    }
}

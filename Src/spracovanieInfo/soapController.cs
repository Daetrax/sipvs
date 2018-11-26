using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using spracovanieInfo.ServiceReference1;

namespace spracovanieInfo
{
    class soapController
    {
        //private static string soapTemplate = @"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/""
        //                                        xmlns:xsi=""http://www.w3.org/1999/XMLSchema-instance""
        //                                        xmlns:xsd=""http://www.w3.org/1999/XMLSchema"">
        //                                        <SOAP-ENV:Body>
        //                                        <HelloWorld xmlns=""http://tempuri.org/"" SOAP-ENV:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
        //                                        <int1 xsi:type=""xsd:integer""> 12 </int1>
        //                                        <int2 xsi:type=""xsd:integer""> 32 </int2>
        //                                        </HelloWorld>
        //                                        </SOAP-ENV:Body></SOAP-ENV:Envelope>";
        //private static string soapTemplate = 
        //    @"<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
        //        <soap12:Body>
        //        <GetTimestamp xmlns = ""http://www.ditec.sk/"">
        //            <dataB64>dataPlaceholder</dataB64>
        //        </GetTimestamp>
        //        </soap12:Body>
        //    </soap12:Envelope>";
        private static string soapTemplate =
            //"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<soap:Body>" +
                "<GetTimestamp xmlns=\"http://www.ditec.sk/\">" +
                    "<dataB64>dataPlaceholder</dataB64>" +
                "</GetTimestamp>" +
                "</soap:Body>" +
            "</soap:Envelope>";

        private static int contentLength;
        public static byte[] CallWebService(string data)
        {
            //var client = new ServiceReference1.TSSoapClient();
            //var result = client.GetTimestamp(data);

            //var decodedResult = Encoding.Default.GetString(Convert.FromBase64String(result));
            //File.WriteAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/response.ts", decodedResult);

            var _url = "http://test.ditec.sk/timestampws/TS.asmx";
            var _action = "http://www.ditec.sk/GetTimestamp";

            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(data);
            HttpWebRequest webRequest = CreateWebRequest(_url, _action);
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
                    
            
            // begin async call to web request.
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
            string soapResult;
            
            using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                }
                Console.Write(soapResult);
            }
            XmlDocument soapResponse = new XmlDocument();
            soapResponse.LoadXml(soapResult);

            string timestamp = soapResponse.GetElementsByTagName("GetTimestampResult")[0].InnerText;

            return Convert.FromBase64String(timestamp);
        }

        private static HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.Host = "test.ditec.sk";
            //webRequest.ContentLength = contentLength;
            webRequest.ContentType = "text/xml; charset=utf-8"; //"text/xml;charset=\"utf-8\"";
            //webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private static XmlDocument CreateSoapEnvelope(string data)
        {
            //XmlDocument soapEnvelopeDocument = new XmlDocument();
            //string soapRequest = soapTemplate.Replace("dataPlaceholder", data);
            //byte[] soapRequestArray = Encoding.ASCII.GetBytes(soapRequest);
            //soapEnvelopeDocument.LoadXml( Convert.ToBase64String(soapRequestArray) );

            //contentLength = soapRequest.Length;

            //return soapEnvelopeDocument;

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            string soapRequest = soapTemplate.Replace("dataPlaceholder", data);
            //byte[] soapRequestArray = Encoding.ASCII.GetBytes(soapRequest);
            soapEnvelopeDocument.LoadXml(soapRequest);
           

            contentLength = soapRequest.Length;

            return soapEnvelopeDocument;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }
    }
}

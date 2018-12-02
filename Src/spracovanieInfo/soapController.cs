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
            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(data);
            HttpWebRequest webRequest = CreateWebRequest("http://test.ditec.sk/timestampws/TS.asmx", "http://www.ditec.sk/GetTimestamp");
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
                    
            
            // begin async call to web request.
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
            string soapResult;
                        
            using (StreamReader rd = new StreamReader(webRequest.EndGetResponse(asyncResult).GetResponseStream()))
            {
                soapResult = rd.ReadToEnd();
            }
            
            XmlDocument soapResponse = new XmlDocument();
            soapResponse.LoadXml(soapResult);

            return Convert.FromBase64String(soapResponse.GetElementsByTagName("GetTimestampResult")[0].InnerText);
            
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

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            // replace placeholder string with real data
            string soapRequest = soapTemplate.Replace("dataPlaceholder", data);
            soapEnvelopeDocument.LoadXml(soapRequest);
           // web service needs content length to be defined
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

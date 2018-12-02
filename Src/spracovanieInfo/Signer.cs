using Ditec.Zep.DSigXades;
using Ditec.Zep.DSigXades.Plugins;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Xml;

namespace spracovanieInfo
{
    class Signer
    {

        public void SignDocument()
        {
            var xsdUri = @"https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt.xsd";
            var xsdNSUri = @"https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt";
            var xslUri = @"https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt.xsl";

            string schemaString = File.ReadAllText(@"../../../../Xml_data/sipvt.xsd");
            string transformString = File.ReadAllText(@"../../../../Xml_data/sipvt.xsl");

            // this implementation uses file explorer to locate xml file to sign
            XmlDocument doc = new XmlDocument();
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "xml files (*.xml)|*.xml";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            // Process open file dialog box results
            if (openFileDialog.ShowDialog() == true)
            {
                // Open document
                doc.Load(openFileDialog.FileName);
            }
            else
            {
                return;
            }

            var signer = new XadesSig();

            // first we need to create object to be signed
            var objectToSign = new XmlPlugin().CreateObject2("objectId", "Objednanie knih", doc.InnerXml, schemaString, xsdNSUri, xsdUri, transformString, xslUri, "HTML");

            // we can add multiple objects to sign
            signer.AddObject(objectToSign);

            // certificates can be changed, check documentation of xades 
            if (signer.Sign("signatureId10", "sha256", "urn:oid:1.3.158.36061701.1.2.2") == 0)
            {
                Console.WriteLine(signer.SignedXmlWithEnvelope);
                File.WriteAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/signedDocument.xml", signer.SignedXmlWithEnvelope);
            }
            else
            {
                Console.WriteLine(signer.ErrorMessage);
                MessageBox.Show($"Error occurred during signing {Environment.NewLine}{signer.ErrorMessage}",
                                         "",
                                         MessageBoxButton.OK,
                                         MessageBoxImage.Warning);
            }

        }
    }
}

using spracovanieInfo.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Ditec.Zep.DSigXades;
using Ditec.Zep.DSigXades.Plugins;
using Ditec.Zep.DSigXades.Plugins.Utils;
using Ditec.Zep.DSigXades.Plugin;
using Ditec.Zep.DSigXades.Forms;
using Org.BouncyCastle.Tsp;
using System.Net;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Math;
using System.Web.Services.Protocols;
using Microsoft.Win32;

namespace spracovanieInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private XadesSig signer;
        private int sequenceCounter = 0;
        public string Name {
            get {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            } }

        public string Surname
        {
            get { return surname; }
            set
            {
                if (value != surname)
                {
                    surname = value;
                    OnPropertyChanged("Surname");
                }
            }
        }

        private string name;
        private string surname;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox bookSampleBox = new TextBox();
            bookSampleBox.Name = "Book";
            bookSampleBox.Width = 100;
            bookSampleBox.Text = "";
            
            ComboBox combo = new ComboBox();
            combo.Items.Add("En");
            combo.Items.Add("Sk");
            combo.SelectedIndex = 0;
            combo.Margin = new Thickness(10, 0, 0, 0);

            Button button = new Button();
            button.HorizontalAlignment = HorizontalAlignment.Right;
            button.Content = "Delete";
            button.Name = "btnClickMe";        
            button.Margin = new Thickness(10, 0, 0, 0);
            button.Click += new RoutedEventHandler(this.RemoveBook);
                        
            
            StackPanel container = new StackPanel();
            StackPanel bookElement = new StackPanel();

            container.Orientation = Orientation.Horizontal;
            container.Name = this.generateSequenceName();
            container.Children.Add(bookSampleBox);
            container.Children.Add(combo);
            container.Children.Add(button);
            container.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.RegisterName(container.Name, container);

            stackPanel.Children.Add(container);
            Console.WriteLine(Name);

            
        }

        private void RemoveBook(object sender, RoutedEventArgs e)
        {
            FrameworkElement parent = (FrameworkElement)((Button)sender).Parent;
            string parent_name = parent.Name;
            StackPanel element = (StackPanel) stackPanel.FindName(parent_name);
            stackPanel.UnregisterName(parent_name);
            stackPanel.Children.Remove(element);
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    var length = stackPanel.Children.Count;
        //    stackPanel.Children.RemoveAt(length - 1);
        //}

        private string generateSequenceName()
        {
            return $"Book{sequenceCounter++}";
        }

        private Request createRequest()
        {
            List<Book> bookList = new List<Book>();
            foreach (StackPanel child in stackPanel.Children)
            {
                var textbox = (TextBox)child.Children[0];
                var combobox = (ComboBox)child.Children[1];

                bookList.Add(new Book(textbox.Text, combobox.SelectedItem.ToString()));

            }
            Request request;
            try
            {
                request = new Request(NameBox.Text, SurnameBox.Text,
                                              StreetBox.Text, Int32.Parse(StreetNumberBox.Text),
                                              CountryBox.Text, CityBox.Text, ZipcodeBox.Text,
                                              Int32.Parse(LoanPeriodBox.Text), bookList.ToArray());
            }
            catch (Exception e)
            {
                MessageBoxResult result = MessageBox.Show($"Please fill all fields",
                                         "",
                                         MessageBoxButton.OK,
                                         MessageBoxImage.Warning);
                return null;
            }

            return request;
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SaveXML(object sender, RoutedEventArgs e)
        {
            var request = createRequest();
            if (request == null)
            {
                return;
            }

            //XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //ns.Add("", "https://github.com/Daetrax/sipvs/blob/master/Xml_data/");
            XmlSerializer xs = new XmlSerializer(typeof(Request));
            TextWriter tw = new StreamWriter($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml");
            xs.Serialize(tw, request);
            tw.Close();

            ViewButton.IsEnabled = true;           
            
        }
        
        private string ConvertObjectToXml(object objectToSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(objectToSerialize.GetType());
            StringWriter stringWriter = new StringWriter();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "https://github.com/Daetrax/sipvs/blob/master/Xml_data/");
            xmlSerializer.Serialize(stringWriter, objectToSerialize, ns);

            return stringWriter.ToString();
        }
        

        private void ValidateXml(object sender, RoutedEventArgs e)
        {

            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt", @"../../../../Xml_data/sipvt.xsd");
            //XmlReader rd = XmlReader.Create($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml");
            //XDocument doc = XDocument.Load(rd);


            var request = createRequest();
            if (request == null)
            {
                return;
            }

            XDocument doc = XDocument.Parse(ConvertObjectToXml(request));

            ValidationButton.Background = new SolidColorBrush(Colors.Green);

            doc.Validate(schema, ValidationEventHandler);

           
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            XmlSeverityType type = XmlSeverityType.Warning;
            if (Enum.TryParse<XmlSeverityType>("Error", out type))
            {

                ValidationButton.Background = new SolidColorBrush(Colors.Red);
                MessageBoxResult result = MessageBox.Show($"Invalid form {e.Message}",
                                         "",
                                         MessageBoxButton.OK,
                                         MessageBoxImage.Warning);


                if (type == XmlSeverityType.Error) {

                    //throw new Exception(e.Message);
                }
                
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void TransformXmlToHtml(object sender, RoutedEventArgs e)
        {
            XmlReader xsltReader = XmlReader.Create(@"../../../../Xml_data/sipvt.xsl");
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(xsltReader);

            XslCompiledTransform docXsl = new XslCompiledTransform();
            docXsl.Load(@"../../../../Xml_data\sipvt.xsl");
            
            XmlDocument doc = new XmlDocument();
            doc.Load($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml");

            

            StringWriter results = new StringWriter();
            //using (XmlReader reader = XmlReader.Create(new StringReader(doc.ToString())))
            using (XmlReader reader = XmlReader.Create($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml"))
            {
                docXsl.Transform(reader, null, results);
            }

            File.WriteAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/sipvs.html", results.ToString());
            FormWindow formWindow = new FormWindow(results.ToString());
            formWindow.Show();
        }

        private void SignDocument(object sender, RoutedEventArgs e)
        {
            var xsdUri = @"https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt.xsd";
            var xsdNSUri = @"https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt";
            var xslUri = @"https://github.com/Daetrax/sipvs/blob/master/Xml_data/sipvt.xsl";
                        
            string schemaString = File.ReadAllText(@"../../../../Xml_data/sipvt.xsd");
            string transformString = File.ReadAllText(@"../../../../Xml_data/sipvt.xsl");
            //string xmlString = File.ReadAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml");

            // this implementation takes all data from the form window
            //var request = createRequest();
            //if (request == null)
            //{
            //    return;
            //}
            //XDocument doc = XDocument.Parse(ConvertObjectToXml(request));

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
                string filename = openFileDialog.FileName;
                doc.Load(filename);
            } else
            {
                return;
            }


            this.signer = new XadesSig();
            
            XmlPlugin xmlPlugin = new XmlPlugin();

                                    
            var objectToSign = xmlPlugin.CreateObject2("objectId", "Objednanie knih", doc.InnerXml, schemaString, xsdNSUri, xsdUri, transformString, xslUri, "HTML");

            signer.AddObject(objectToSign);
                        

            var res = signer.Sign("signatureId10", "sha256", "urn:oid:1.3.158.36061701.1.2.2");

            if (res == 0)
            {
                Console.WriteLine(signer.SignedXmlWithEnvelope);
                File.WriteAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/signedDocument.xml", signer.SignedXmlWithEnvelope);
            }
            else
            {
                Console.WriteLine(signer.ErrorMessage);
                MessageBoxResult result = MessageBox.Show($"Error occurred during signing {Environment.NewLine}{signer.ErrorMessage}",
                                         "",
                                         MessageBoxButton.OK,
                                         MessageBoxImage.Warning);
            }

        }

        private void timestampDocument(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "xml files (*.xml)|*.xml";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            // Process open file dialog box results
            if (openFileDialog.ShowDialog() == true)
            {
                // Open document
                string filename = openFileDialog.FileName;
            }


            var fileStream = openFileDialog.OpenFile();
            string fileContent;

            using (StreamReader reader = new StreamReader(fileStream))
            {
                fileContent = reader.ReadToEnd();
            }

            

            stamp(openFileDialog.FileName);
            //XadesSig s = new XadesSig();
                                    
            
            



            //var _url = "http://test.ditec.sk/timestampws/TS.asmx";
            //var _action = "http://test.ditec.sk/timestampws/TS.asmx?op=GetTimestamp";

            //TSReference.TS abc = new TSReference.TS();
            

            //var abc = s.GetSignatureTimeStampRequest(null, null);
            //soapController.CallWebService(signedDocument);
            //TimeStampRequest
            //TimeStampResponse
        }



        private string getTimestamp(string text)
        {
            
            

            var signedDocument = File.ReadAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/signedDocument.xml");
            //signedDocument = text;
            WebClient wc = new WebClient();

            byte[] soapRequestArray = Encoding.ASCII.GetBytes(signedDocument);

            var abcc = Convert.ToBase64String(soapRequestArray);

            byte[] ret = wc.UploadData(spracovanieInfo.Properties.Settings.Default.AspxReference, "POST", System.Text.Encoding.Default.GetBytes(Convert.ToBase64String(soapRequestArray)));

            byte[] soapResponse = soapController.CallWebService(abcc);
            TimeStampResponse resp = new TimeStampResponse(soapResponse);

            if(resp.Status != 0)
            {
                // TODO: notify via window
                Console.Write("Err occurred");
            }

            TimeStampToken token = resp.TimeStampToken;
            var encToken = token.GetEncoded();

            //return Encoding.UTF8.GetString(encToken);
            var stringTest = Encoding.UTF8.GetString(encToken);
            var tokenString = Convert.ToBase64String(encToken);

            var anotherTest = Convert.ToBase64String(Encoding.UTF8.GetBytes(stringTest));

            //return anotherTest;
            //signer = new XadesSig();

            //var obj = File.ReadAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/signedDocument.xml");
            //XmlDataContainer cont = new XmlDataContainer();
            //cont.Initialize(obj);

            //XmlBpPlugin plg = new XmlBpPlugin();
            

            //var addObjectCode = signer.AddObject(obj);
            //var code = signer.CreateXAdESZepT(resp.GetEncoded(), encToken);

            //var decToken = Convert.FromBase64String()
            //var result = signer.CreateXAdESZepT(resp.GetEncoded(), token.GetEncoded());
            //var errMsg = signer.ErrorMessage;

            return tokenString;
            //var testText = System.Text.Encoding.Default.GetString(ret);
            //File.WriteAllBytes($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/response.ts", Convert.FromBase64String(testText));
            //TimeStampResponse response = new TimeStampResponse(ret);
            

          

            //TimeStampResponse ab;
            //TimeStampRequest req = new TimeStampRequest(Encoding.Default.GetBytes(Convert.ToBase64String(soapRequestArray)));
            //ab = new TimeStampResponse(req.GetMessageImprintDigest());
            //CmsSignedDataGenerator cmsGen = new CmsSignedDataGenerator();

            //var document = File.ReadAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/signedDocument.xml");
            //var sigData = Encoding.Default.GetBytes(document);


            //var msg = new CmsProcessableByteArray(Encoding.Default.GetBytes(document)).GetInputStream();
            //var cms = cmsGen.Generate(msg, );
            //var token = new TimeStampToken(cms);
            //return System.Text.Encoding.Default.GetString(ret);
            //var code = s.CreateXAdESZepT(result, null);

        }

        private string buildSoapRequest(string data)
        {
            string envelope = $"<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                                  "<soap:Body>" +
                                    "<GetTimestamp xmlns=\"http://www.ditec.sk/\">" +
                                      $"<dataB64>{data}</dataB64>" +
                                    "</GetTimestamp>" +
                                  "</soap:Body>" +
                                "</soap:Envelope>";
            return envelope;
        }

        private string getTimestamp2(string text)
        {
            TimeStampRequestGenerator reqGen = new TimeStampRequestGenerator();

            var document = File.ReadAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/signedDocument.xml");
            //var sigData = Encoding.Default.GetBytes(document);
            var byteSigValue = Encoding.ASCII.GetBytes(text);
            var tt = Convert.ToBase64String(byteSigValue);
            var requesData = Encoding.Default.GetBytes(Convert.ToBase64String(byteSigValue));
            // Dummy request
            TimeStampRequest request = reqGen.Generate(
                TspAlgorithms.Sha1, requesData);

            byte[] reqData = request.GetEncoded();

            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create("http://test.ditec.sk/timestampws/TS.aspx");
            httpReq.Headers.Add("SOAPAction", "http://www.ditec.sk/GetTimestamp");
            httpReq.Host = "test.ditec.sk";
            httpReq.Method = "POST";
            httpReq.ContentType = "text/xml; charset=utf-8";
            httpReq.ContentLength = requesData.Length;

            // Write the request content
            Stream reqStream = httpReq.GetRequestStream();
            reqStream.Write(requesData, 0, requesData.Length);
            reqStream.Close();

            HttpWebResponse httpResp = (HttpWebResponse)httpReq.GetResponse();

            // Read the response
            Stream respStream = new BufferedStream(httpResp.GetResponseStream());

            //StreamReader reader1 = new StreamReader(respStream);
            //var ttext = reader1.ReadToEnd();
            //Console.WriteLine(ttext);
            //File.WriteAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/response.txt", ttext);

            TimeStampResponse response = new TimeStampResponse(respStream);
            respStream.Close();

            return response.ToString();
        }

        private void stamp(string filepath)
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

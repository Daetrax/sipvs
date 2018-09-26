using spracovanieInfo.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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

namespace spracovanieInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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
            Button button = new Button();
            button.HorizontalAlignment = HorizontalAlignment.Right;
            button.Content = "Delete";
            button.Name = "btnClickMe";            
            button.Click += new RoutedEventHandler(this.RemoveBook);
            TextBox bookSampleBox = new TextBox();
            bookSampleBox.Name = "Book";
            bookSampleBox.Text = "Book sample";

            ComboBox combo = new ComboBox();
            combo.Items.Add("En");
            combo.Items.Add("Sk");

            StackPanel container = new StackPanel();
            StackPanel bookElement = new StackPanel();

            container.Orientation = Orientation.Horizontal;
            container.Name = this.generateSequenceName();
            container.Children.Add(bookSampleBox);
            container.Children.Add(combo);
            container.Children.Add(button);
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var length = stackPanel.Children.Count;
            stackPanel.Children.RemoveAt(length - 1);
        }

        private string generateSequenceName()
        {
            return $"Book{sequenceCounter++}";
        }

        private void SaveXML(object sender, RoutedEventArgs e)
        {
            List<Book> bookList = new List<Book>();
            foreach (StackPanel child in stackPanel.Children)
            {
                var textbox = (TextBox) child.Children[0];
                var combobox = (ComboBox)child.Children[1];

                bookList.Add(new Book(textbox.Text, combobox.SelectedItem.ToString()));

            }
            Request request = new Request(NameBox.Text, SurnameBox.Text,
                                          StreetBox.Text, Int32.Parse(StreetNumberBox.Text),
                                          CountryBox.Text, CityBox.Text, ZipcodeBox.Text,
                                          Int32.Parse(LoanPeriodBox.Text), bookList.ToArray());
            
            XmlSerializer xs = new XmlSerializer(typeof(Request));
            TextWriter tw = new StreamWriter($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml");
            xs.Serialize(tw, request);
            tw.Close();
            
            
        }

        //private void TransformXmlToHtml(object sender, ValidationEventArgs e)
        //{
        //    XmlReader xsltReader = XmlReader.Create(@"C:\Users\Tomas\source\repos\sipvs\Xml_data\sipvt_custom.xsd");
        //    XslCompiledTransform xslt = new XslCompiledTransform();
        //    xslt.Load(xsltReader);

        //    //XslCompiledTransform transform = new XslCompiledTransform();
        //    //using (XmlReader reader = XmlReader.Create(new StringReader(xsltString)))
        //    //{
        //    //    transform.Load(reader);
        //    //}
        //    XmlDocument doc = new XmlDocument();
        //    doc.Load("c:\\temp.xml");

        //    StringWriter results = new StringWriter();
        //    using (XmlReader reader = XmlReader.Create(new StringReader(doc.ToString())))
        //    {
        //        xslt.Transform(reader, null, results);
        //    }

        //    File.WriteAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/sipvs.html", results.ToString());
        //    //return results.ToString();
            

        //}

        private void ValidateXml(object sender, RoutedEventArgs e)
        {

            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("", @"C:\Users\Tomas\source\repos\sipvs\Xml_data\sipvt_custom.xsd");
            XmlReader rd = XmlReader.Create($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml");
            XDocument doc = XDocument.Load(rd);
            doc.Validate(schema, ValidationEventHandler);

           
        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            XmlSeverityType type = XmlSeverityType.Warning;
            if (Enum.TryParse<XmlSeverityType>("Error", out type))
            {
                MessageBoxResult result = MessageBox.Show("Invalid form",
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
            XmlReader xsltReader = XmlReader.Create(@"C:\Users\Tomas\source\repos\sipvs\Xml_data\sipvt_xslt.xsl");
            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(xsltReader);

            XslCompiledTransform docXsl = new XslCompiledTransform();
            docXsl.Load(@"C:\Users\Tomas\source\repos\sipvs\Xml_data\sipvt_xslt.xsl");
            
            XmlDocument doc = new XmlDocument();
            doc.Load($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml");

            

            StringWriter results = new StringWriter();
            //using (XmlReader reader = XmlReader.Create(new StringReader(doc.ToString())))
            using (XmlReader reader = XmlReader.Create($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SIPVS_SerializedRequest.xml"))
            {
                docXsl.Transform(reader, null, results);
            }

            File.WriteAllText($"{System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/sipvs.html", results.ToString());
        }
    }
}

using spracovanieInfo.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace spracovanieInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new UiBuilder(this).Build_Book();
        }

        private void RemoveBook(object sender, RoutedEventArgs e)
        {
            new UiBuilder(this).RemoveBook(sender, e);
        }

        protected void OnPropertyChanged(string name)
        {
            // handles all properties
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                // this causes the update
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
                
        private void SignDocument(object sender, RoutedEventArgs e)
        {
            new Signer().SignDocument();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SaveXML(object sender, RoutedEventArgs e)
        {
            var builder = new UiBuilder(this);
            var request = builder.createRequest();
            // if something went wrong, null is returned
            if (request == null)
            {
                return;
            }
            // serialize and save xml
            XmlUtil.SaveXML(request);
            ViewButton.IsEnabled = true;    
        }
                

        private void ValidateXml(object sender, RoutedEventArgs e)
        {
            var builder = new UiBuilder(this);
            var request = builder.createRequest();
            // validation handler takes care of errors or warnings during validation
            XmlUtil.ValidateXml(request, this);
            ValidationButton.Background = new SolidColorBrush(Colors.Green);            
        }
        
        
        private void TransformXmlToHtml(object sender, RoutedEventArgs e)
        {
            var results = XmlUtil.TransformXmlToHtml();
            // form window creates a new window with the result string
            FormWindow formWindow = new FormWindow(results);
            formWindow.Show();
        }


        private void timestampDocument(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "xml files (*.xml)|*.xml";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            // Process open file dialog box results
            openFileDialog.ShowDialog();  
            Utils.stamp(openFileDialog.FileName);           
        }


    }
}

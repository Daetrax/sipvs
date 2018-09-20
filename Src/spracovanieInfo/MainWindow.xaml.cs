using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace spracovanieInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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

        private string name = "John Doe";
        private string surname = "Doe";
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

            StackPanel container = new StackPanel();
            container.Name = "abc123";
            container.Children.Add(bookSampleBox);
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
    }
}

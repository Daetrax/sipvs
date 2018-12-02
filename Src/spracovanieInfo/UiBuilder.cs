using spracovanieInfo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace spracovanieInfo
{
    class UiBuilder
    {
        private MainWindow window;
        public UiBuilder(MainWindow window)
        {
            this.window = window;
        }

        public void Build_Book()
        {
            // create container for book
            TextBox bookSampleBox = new TextBox();
            bookSampleBox.Name = "Book";
            bookSampleBox.Width = 100;
            bookSampleBox.Text = "";

            // create language combo box
            ComboBox combo = new ComboBox();
            combo.Items.Add("En");
            combo.Items.Add("Sk");
            combo.SelectedIndex = 0;
            //extra space for better visuals
            combo.Margin = new Thickness(10, 0, 0, 0);

            // delete  button
            Button button = new Button();
            button.HorizontalAlignment = HorizontalAlignment.Right;
            button.Content = "Delete";
            button.Name = "btnClickMe";
            button.Margin = new Thickness(10, 0, 0, 0);
            // add method to remove itself
            button.Click += new RoutedEventHandler(this.RemoveBook);

            // create stack panel containers
            StackPanel container = new StackPanel();
            StackPanel bookElement = new StackPanel();

            // put all elements into the containers
            container.Orientation = Orientation.Horizontal;
            container.Name = Utils.generateSequenceName();
            container.Children.Add(bookSampleBox);
            container.Children.Add(combo);
            container.Children.Add(button);
            container.Margin = new Thickness(0, 0, 0, 5);
            // this is required for it to appear in ui
            this.window.stackPanel.RegisterName(container.Name, container);

            //add containers
            this.window.stackPanel.Children.Add(container);
        }

        public void RemoveBook(object sender, RoutedEventArgs e)
        {
            FrameworkElement parent = (FrameworkElement)((Button)sender).Parent;
            string parent_name = parent.Name;

            StackPanel element = (StackPanel)this.window.stackPanel.FindName(parent_name);
            // unregister allows to create books in the future
            this.window.stackPanel.UnregisterName(parent_name);
            this.window.stackPanel.Children.Remove(element);
        }

        public Request createRequest()
        {
            List<Book> bookList = new List<Book>();
            // add each book to a list
            foreach (StackPanel child in this.window.stackPanel.Children)
            {
                var textbox = (TextBox)child.Children[0];
                var combobox = (ComboBox)child.Children[1];
                // create instance of book that can be serialized
                bookList.Add(new Book(textbox.Text, combobox.SelectedItem.ToString()));
            }
            Request request;
            // TODO: rework to be more safe
            try
            {
                request = new Request(this.window, bookList);
            }
            catch (Exception e)
            {
                // all fields need to be filled
                MessageBoxResult result = MessageBox.Show($"Please fill all fields",
                                         "",
                                         MessageBoxButton.OK,
                                         MessageBoxImage.Warning);
                return null;
            }
            return request;
        }
    }
}

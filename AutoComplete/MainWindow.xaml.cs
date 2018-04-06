using System.Windows;

namespace AutoComplete
{
    public partial class MainWindow : Window
    {
        public class User
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();

            autoTextBox1.ItemsSource = new System.Collections.Generic.List<object>
            {
                "abcd",
                "abcde",
                "abcdef",
                "bcdefg",
                "cdefg"
            };

            autoTextBox1.StringFormat = "{{{0}}}";

            autoTextBox2.ItemsSource = new System.Collections.Generic.List<object>
            {
                new User{ Name = "abcd", Age = 1 },
                new User{ Name =  "abcde", Age = 2 },
                new User{ Name =  "abcdef", Age = 3 },
                new User{ Name =  "bcdefg", Age = 4 },
                new User{ Name =  "cdefg", Age = 5 }
            };

            autoTextBox2.StringFormat = "{{{0}}}";
            autoTextBox2.ItemPath = "Name";
        }
    }
}
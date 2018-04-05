using System.Windows;

namespace AutoComplete
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            autoTextBox.ItemsSource = new System.Collections.Generic.List<string>
            {
                "abcd",
                "abcde",
                "abcdef",
                "bcdefg",
                "cdefg"
            };
        }
    }
}
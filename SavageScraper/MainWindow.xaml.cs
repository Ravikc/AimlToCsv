using System.Windows;
using System.Windows.Controls;

namespace SavageScraper
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string inputPath = textBox1.Text;
            string outputPath = textBox3.Text;

            Parser tool = new Parser(inputPath, outputPath);
            tool.Parse();

            MessageBox.Show("Done!");


        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

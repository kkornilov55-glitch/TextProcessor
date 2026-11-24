using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UI.ViewModels;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            this.viewModel = new MainViewModel();
            this.DataContext = this.viewModel;
        }

        // Левая панель

        private void BtnNormalization_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SelectNormalization();
        }

        private void BtnMasking_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SelectMasking();
        }

        private void BtnFiltering_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SelectFiltering();
        }

        private void BtnLoadFile_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.LoadFile();
        }

        private void BtnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SaveFile();
        }

        // Правая панель

        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.ExecuteOperation();
        }

        private void BtnShowContract_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.ShowContract();
        }
        private void BtnWpCalculator_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.SelectWpCalculator();
        }
        private void BtnWpHelp_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.ShowWpHelp();
        }
    }
}
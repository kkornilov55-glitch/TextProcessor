using System.Windows;
using UI.ViewModels;

namespace UI
{
    public partial class LoopWindow : Window
    {
        private readonly LoopViewModel viewModel;

        public LoopWindow()
        {
            this.InitializeComponent();
            this.viewModel = new LoopViewModel();
            this.DataContext = this.viewModel;
        }

        private void BtnGenerateArray_Click(object sender, RoutedEventArgs e) => this.viewModel.GenerateArray();

        private void BtnStep_Click(object sender, RoutedEventArgs e) => this.viewModel.StepOnce();

        private void BtnRun_Click(object sender, RoutedEventArgs e) => this.viewModel.RunToEnd();

        private void BtnStop_Click(object sender, RoutedEventArgs e) => this.viewModel.StopRun();

        private void BtnReset_Click(object sender, RoutedEventArgs e) => this.viewModel.ResetSession();
    }
}

using RIoT2.Mobile.ViewModels;

namespace RIoT2.Mobile.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        // Replaces the legacy RIoTWebViewClient loading/error handling.
        private void OnNavigating(object? sender, WebNavigatingEventArgs e)
        {
            _viewModel.IsLoading = true;
        }

        private void OnNavigated(object? sender, WebNavigatedEventArgs e)
        {
            _viewModel.IsLoading = false;

            // Dismiss the pull-to-refresh spinner once the page finishes loading.
            _viewModel.IsRefreshing = false;

            if (e.Result != WebNavigationResult.Success)
            {
                // Show a local error page, mirroring the legacy error.html behavior.
                Web.Source = new HtmlWebViewSource
                {
                    Html = "<html><body style='font-family:sans-serif;text-align:center;padding-top:40px'>" +
                           "<h3>Unable to load the RIoT2 dashboard.</h3>" +
                           "<p>Please check the controller URL in Settings and your network connection.</p>" +
                           "</body></html>"
                };
            }
        }
    }
}
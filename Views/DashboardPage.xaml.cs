using RIoT2.Mobile.ViewModels;

namespace RIoT2.Mobile.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _viewModel;
        private CancellationTokenSource? _navigationTimeout;
        private bool _showingError;

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        // Replaces the legacy RIoTWebViewClient loading/error handling.
        private void OnNavigating(object? sender, WebNavigatingEventArgs e)
        {
            _viewModel.IsLoading = true;
            StartNavigationTimeout();
        }

        private void OnNavigated(object? sender, WebNavigatedEventArgs e)
        {
            CancelNavigationTimeout();
            _viewModel.CompleteNavigation();

            if (e.Result != WebNavigationResult.Success && !_showingError)
                ShowError();
        }

        private void OnRefreshRequested(object? sender, EventArgs e)
        {
            try
            {
                _showingError = false;
                StartNavigationTimeout();
                if (Web.Source is UrlWebViewSource current && current.Url == _viewModel.Source)
                    Web.Reload();
                else
                    Web.Source = new UrlWebViewSource { Url = _viewModel.Source };
            }
            catch
            {
                ShowError();
            }
        }

        private void ShowError()
        {
            CancelNavigationTimeout();
            _viewModel.CompleteNavigation();
            _showingError = true;
            try
            {
                Web.Source = new HtmlWebViewSource
                {
                    Html = "<html><body style='font-family:sans-serif;text-align:center;padding-top:40px'>" +
                           "<h3>Unable to load the RIoT2 dashboard.</h3>" +
                           "<p>Please check the controller URL in Settings and your network connection.</p>" +
                           "</body></html>"
                };
            }
            catch { _viewModel.CompleteNavigation(); }
        }

        private void StartNavigationTimeout()
        {
            CancelNavigationTimeout();
            _navigationTimeout = new CancellationTokenSource();
            _ = CompleteAfterTimeoutAsync(_navigationTimeout.Token);
        }

        private async Task CompleteAfterTimeoutAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), token);
                if (!token.IsCancellationRequested)
                    _viewModel.CompleteNavigation();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        }

        private void CancelNavigationTimeout()
        {
            _navigationTimeout?.Cancel();
            _navigationTimeout?.Dispose();
            _navigationTimeout = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.RefreshRequested += OnRefreshRequested;
            _viewModel.Activate();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.RefreshRequested -= OnRefreshRequested;
            _viewModel.Deactivate();
            CancelNavigationTimeout();
        }
    }
}
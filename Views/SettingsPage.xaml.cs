using RIoT2.Mobile.ViewModels;

namespace RIoT2.Mobile.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // LoadAsync reports its own failures through BeaconError.
            _ = _viewModel.LoadAsync();
        }
    }
}
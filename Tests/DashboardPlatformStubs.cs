// Headless stand-ins for the MAUI APIs used by the source-linked view model.
namespace RIoT2.Mobile.ViewModels
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class QueryPropertyAttribute(string name, string query) : Attribute
    {
        public string Name { get; } = name;
        public string Query { get; } = query;
    }

    public enum NetworkAccess { None, Internet }

    public sealed class ConnectivityChangedEventArgs(NetworkAccess networkAccess) : EventArgs
    {
        public NetworkAccess NetworkAccess { get; } = networkAccess;
    }

    public interface IConnectivity
    {
        NetworkAccess NetworkAccess { get; }
        event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;
    }

    internal static class MainThread
    {
        public static void BeginInvokeOnMainThread(Action action) => action();
    }

    internal sealed class Shell
    {
        public static Shell Current { get; } = new();
        public Task GoToAsync(string route) => Task.CompletedTask;
    }
}

namespace RIoT2.Mobile.Views
{
    internal sealed class SettingsPage { }

    public class ContentPage
    {
        public object? BindingContext { get; set; }
        protected virtual void OnAppearing() { }
        protected virtual void OnDisappearing() { }
    }

    public sealed class UrlWebViewSource
    {
        public string? Url { get; set; }
    }

    public sealed class HtmlWebViewSource
    {
        public string? Html { get; set; }
    }

    public enum WebNavigationResult { Success, Failure }
    public sealed class WebNavigatingEventArgs : EventArgs { }
    public sealed class WebNavigatedEventArgs(WebNavigationResult result) : EventArgs
    {
        public WebNavigationResult Result { get; } = result;
    }

    public sealed class FakeWebView
    {
        private object? _source;
        public bool ThrowOnNavigation { get; set; }
        public int Reloads { get; private set; }
        public event EventHandler<WebNavigatingEventArgs>? Navigating;
        public object? Source
        {
            get => _source;
            set
            {
                Navigate();
                _source = value;
            }
        }

        public void Reload()
        {
            Navigate();
            Reloads++;
        }

        private void Navigate()
        {
            if (ThrowOnNavigation) throw new InvalidOperationException("Navigation failed.");
            Navigating?.Invoke(this, new WebNavigatingEventArgs());
        }
    }

    public partial class DashboardPage
    {
        public FakeWebView Web { get; } = new();
        private void InitializeComponent() => Web.Navigating += OnNavigating;
        public void Appear() => OnAppearing();
        public void Disappear() => OnDisappearing();
        public void Finish(WebNavigationResult result) => OnNavigated(Web, new WebNavigatedEventArgs(result));
    }
}

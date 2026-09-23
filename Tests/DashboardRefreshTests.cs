using Microsoft.VisualStudio.TestTools.UnitTesting;
using RIoT2.Mobile.Services;
using RIoT2.Mobile.ViewModels;
using RIoT2.Mobile.Views;

namespace RIoT2.Mobile.Tests;

[TestClass]
public class DashboardRefreshTests
{
    [TestMethod]
    public void PageReloadsSameUrlAndRestoresUrlAfterAnErrorPage()
    {
        var settings = new Settings();
        var connection = new Connectivity();
        var vm = new DashboardViewModel(settings, connection);
        var page = new DashboardPage(vm);
        page.Appear();
        page.Finish(WebNavigationResult.Success);
        vm.RefreshCommand.Execute(null);
        Assert.AreEqual(1, page.Web.Reloads);
        Assert.IsTrue(vm.IsRefreshing);
        page.Finish(WebNavigationResult.Failure);
        Assert.IsInstanceOfType<HtmlWebViewSource>(page.Web.Source);
        page.Finish(WebNavigationResult.Success);
        Assert.IsFalse(vm.IsRefreshing);
        connection.Change(NetworkAccess.None);
        connection.Change(NetworkAccess.Internet);
        Assert.AreEqual(settings.DashboardUrl, ((UrlWebViewSource)page.Web.Source!).Url);
        Assert.AreEqual(1, page.Web.Reloads);
        page.Finish(WebNavigationResult.Success);
        Assert.IsFalse(vm.IsLoading);
        page.Disappear();
    }

    [TestMethod]
    public void PageClearsIndicatorsWhenNavigationThrowsOrPageDisappears()
    {
        var vm = new DashboardViewModel(new Settings(), new Connectivity());
        var page = new DashboardPage(vm);
        page.Appear();
        page.Web.ThrowOnNavigation = true;
        vm.RefreshCommand.Execute(null);
        Assert.IsFalse(vm.IsLoading);
        Assert.IsFalse(vm.IsRefreshing);
        page.Web.ThrowOnNavigation = false;
        vm.RefreshCommand.Execute(null);
        Assert.IsTrue(vm.IsLoading);
        page.Disappear();
        Assert.IsFalse(vm.IsLoading);
        Assert.IsFalse(vm.IsRefreshing);
    }

    [TestMethod]
    public void SameUrlRefreshAlwaysRequestsNavigationAndCompletionClearsSpinners()
    {
        var vm = new DashboardViewModel(new Settings(), new Connectivity());
        int requests = 0;
        vm.RefreshRequested += (_, _) => requests++;
        vm.Activate();
        vm.CompleteNavigation();
        vm.RefreshCommand.Execute(null);
        Assert.AreEqual(2, requests);
        Assert.IsTrue(vm.IsLoading);
        Assert.IsTrue(vm.IsRefreshing);
        vm.CompleteNavigation();
        Assert.IsFalse(vm.IsLoading);
        Assert.IsFalse(vm.IsRefreshing);
        vm.Dispose();
    }

    [TestMethod]
    public void ReappearingRestoresConnectivityRecoveryWithoutDuplicateSubscriptions()
    {
        var connectivity = new Connectivity();
        var vm = new DashboardViewModel(new Settings(), connectivity);
        int requests = 0;
        vm.RefreshRequested += (_, _) => requests++;
        vm.Activate();
        vm.Activate();
        Assert.AreEqual(1, requests);
        vm.Deactivate();
        connectivity.Change(NetworkAccess.None);
        Assert.AreEqual(1, requests);
        vm.Activate();
        Assert.AreEqual(2, requests);
        connectivity.Change(NetworkAccess.Internet);
        Assert.AreEqual(3, requests);
        connectivity.Change(NetworkAccess.None);
        Assert.IsFalse(vm.IsLoading);
        Assert.IsFalse(vm.IsRefreshing);
        vm.Dispose();
        connectivity.Change(NetworkAccess.Internet);
        Assert.AreEqual(3, requests);
    }

    [TestMethod]
    public void NavigationErrorsAndMissingPageCannotLeaveSpinnersActive()
    {
        var vm = new DashboardViewModel(new Settings(), new Connectivity());
        vm.IsRefreshing = true;
        vm.Activate();
        Assert.IsFalse(vm.IsRefreshing);
        vm.RefreshRequested += (_, _) => throw new InvalidOperationException("Navigation failed.");
        Assert.ThrowsException<InvalidOperationException>(() => vm.RefreshCommand.Execute(null));
        Assert.IsFalse(vm.IsLoading);
        Assert.IsFalse(vm.IsRefreshing);
        vm.Dispose();
    }

    [TestMethod]
    public void ReturningFromSettingsUsesNewUrlButUnchangedSettingsPreserveDeepLink()
    {
        var settings = new Settings();
        var vm = new DashboardViewModel(settings, new Connectivity());
        var urls = new List<string>();
        vm.RefreshRequested += (_, _) => urls.Add(vm.Source);
        vm.Source = "https://example.invalid/deep-link";
        vm.Activate();
        Assert.AreEqual("https://example.invalid/deep-link", urls.Single());
        vm.Deactivate();
        settings.DashboardUrl = "https://example.invalid/new";
        vm.Activate();
        Assert.AreEqual(settings.DashboardUrl, urls.Last());
        vm.Source = "https://example.invalid/second-link";
        Assert.AreEqual(vm.Source, urls.Last());
        vm.Dispose();
    }

    private sealed class Connectivity : IConnectivity
    {
        public NetworkAccess NetworkAccess { get; private set; } = NetworkAccess.Internet;
        public event EventHandler<ConnectivityChangedEventArgs>? ConnectivityChanged;
        public void Change(NetworkAccess access)
        {
            NetworkAccess = access;
            ConnectivityChanged?.Invoke(this, new ConnectivityChangedEventArgs(access));
        }
    }

    private sealed class Settings : ISettingsService
    {
        public string DashboardUrl { get; set; } = "https://example.invalid/dashboard";
        public bool AlertsEnabled { get; set; }
        public bool NotificationsEnabled { get; set; }
        public bool BeaconEnabled { get; set; }
        public string BeaconKey { get; set; } = "";
        public string BeaconMessage { get; set; } = "";
        public int BeaconIntervalSeconds { get; set; }
    }
}

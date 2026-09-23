using Microsoft.VisualStudio.TestTools.UnitTesting;
using RIoT2.Mobile.Services;

namespace RIoT2.Mobile.Tests;

[TestClass]
public class BeaconServiceTests
{
    [TestMethod]
    public async Task UnsupportedPlatformNeverAdvertisesAndRejectsStart()
    {
        IBeaconService service = new UnsupportedBeaconService();
        Assert.IsFalse(service.IsSupported);
        Assert.IsFalse(string.IsNullOrWhiteSpace(service.UnavailableReason));
        await Assert.ThrowsExceptionAsync<PlatformNotSupportedException>(() => service.StartAsync());
        await Assert.ThrowsExceptionAsync<PlatformNotSupportedException>(() => service.RestartAsync());
        await service.StopAsync();
        Assert.IsFalse(service.IsAdvertising);
    }

    [TestMethod]
    public async Task ActiveStateWaitsForAdvertisingConfirmation()
    {
        var confirmation = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new FakeBeacon { Advertise = () => confirmation.Task };
        var starting = service.StartAsync();
        Assert.IsFalse(service.IsAdvertising);
        confirmation.SetResult();
        await starting;
        Assert.IsTrue(service.IsAdvertising);
        await service.StopAsync();
        Assert.IsFalse(service.IsAdvertising);
        Assert.AreEqual(1, service.Stops);
    }

    [TestMethod]
    public async Task FailedAdvertisementIsReportedAndCleanedUp()
    {
        var service = new FakeBeacon
        {
            Advertise = () => Task.FromException(new InvalidOperationException("Advertisement rejected."))
        };
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.StartAsync());
        Assert.IsFalse(service.IsAdvertising);
        Assert.AreEqual("Advertisement rejected.", service.LastError);
        Assert.AreEqual(1, service.Stops);
        await service.StopAsync();
        service.Advertise = () => Task.CompletedTask;
        await service.StartAsync();
        Assert.IsTrue(service.IsAdvertising);
        Assert.IsNull(service.LastError);
        await service.StopAsync();
    }

    [TestMethod]
    public async Task DeniedPermissionIsNotSuccessfulStartup()
    {
        var service = new FakeBeacon { PermissionGranted = false };
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.StartAsync());
        Assert.IsFalse(service.IsAdvertising);
        Assert.AreEqual(0, service.Starts);
        Assert.IsNotNull(service.LastError);
    }

    [TestMethod]
    public async Task InvalidPayloadDoesNotRequestPermissionOrStartPlatformAdvertising()
    {
        var service = new FakeBeacon
        {
            Validate = _ => throw new InvalidOperationException("Payload too large.")
        };
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.StartAsync());
        Assert.AreEqual(0, service.PermissionRequests);
        Assert.AreEqual(0, service.Starts);
        Assert.IsFalse(service.IsAdvertising);
        Assert.AreEqual("Payload too large.", service.LastError);
    }

    [TestMethod]
    public async Task PeriodicFailureStopsAdvertisingAndCanBeStoppedAgain()
    {
        var service = new FakeBeacon();
        await service.StartAsync();
        service.Advertise = () => Task.FromException(new InvalidOperationException("Refresh rejected."));
        await service.Stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsFalse(service.IsAdvertising);
        Assert.AreEqual("Refresh rejected.", service.LastError);
        await service.StopAsync();
        Assert.AreEqual(1, service.Stops);
    }

    [TestMethod]
    public async Task ConcurrentStartsDoNotCreateMultipleAdvertisers()
    {
        var confirmation = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = new FakeBeacon { Advertise = () => confirmation.Task };
        var first = service.StartAsync();
        var second = service.StartAsync();
        confirmation.SetResult();
        await Task.WhenAll(first, second);
        Assert.AreEqual(1, service.Starts);
        await service.StopAsync();
    }

    [TestMethod]
    public void CurrentProtocolCannotFitEvenWithEmptyMessage()
    {
        var settings = new FakeSettings();
        var factory = new BeaconPayloadFactory(settings, new FakeIdentity(), new AesCryptoService());
        byte[] payload = factory.BuildManufacturerData();
        Assert.IsTrue(payload.Length >= 57);
        Assert.ThrowsException<InvalidOperationException>(() => LegacyBeaconPayload.Validate(payload));
        LegacyBeaconPayload.Validate(new byte[LegacyBeaconPayload.MaximumManufacturerDataLength]);
        Assert.ThrowsException<InvalidOperationException>(() =>
            LegacyBeaconPayload.Validate(new byte[LegacyBeaconPayload.MaximumManufacturerDataLength + 1]));
    }

    private sealed class FakeBeacon : BeaconServiceBase
    {
        public FakeBeacon() : base(new FakeSettings(), new FakePayload()) { }
        public bool PermissionGranted { get; set; } = true;
        public int PermissionRequests { get; private set; }
        public int Starts { get; private set; }
        public int Stops { get; private set; }
        public Func<Task> Advertise { get; set; } = () => Task.CompletedTask;
        public Action<byte[]> Validate { get; set; } = _ => { };
        public TaskCompletionSource Stopped { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override void ValidatePayload(byte[] data) => Validate(data);
        protected override Task<bool> EnsurePermissionsAsync()
        {
            PermissionRequests++;
            return Task.FromResult(PermissionGranted);
        }
        protected override Task OnStartAdvertisingAsync() { Starts++; return Task.CompletedTask; }
        protected override Task OnAdvertiseAsync(byte[] data) => Advertise();
        protected override Task OnStopAdvertisingAsync()
        {
            Stops++;
            Stopped.TrySetResult();
            return Task.CompletedTask;
        }
    }

    private sealed class FakePayload : IBeaconPayloadFactory
    {
        public byte[] BuildManufacturerData() => [1];
    }

    private sealed class FakeIdentity : IDeviceIdentityService
    {
        public string GetDeviceIdentifier() => "AA:BB:CC:DD:EE:FF";
    }

    private sealed class FakeSettings : ISettingsService
    {
        public string DashboardUrl { get; set; } = "";
        public bool AlertsEnabled { get; set; }
        public bool NotificationsEnabled { get; set; }
        public bool BeaconEnabled { get; set; } = true;
        public string BeaconKey { get; set; } = "test-key";
        public string BeaconMessage { get; set; } = "";
        public int BeaconIntervalSeconds { get; set; } = 1;
    }
}

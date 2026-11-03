using Microsoft.VisualStudio.TestTools.UnitTesting;
using RIoT2.Mobile.Services;

namespace RIoT2.Mobile.Tests;

[TestClass]
public class BeaconKeyStoreTests
{
    private const string Key = "beaconKey";

    [TestMethod]
    public async Task LegacyPreferenceIsMovedToSecretStoreAndDeleted()
    {
        var secrets = new FakeSecrets();
        var prefs = new FakePreferences { [Key] = "legacy-key" };
        var store = new BeaconKeyStore(secrets, prefs);

        Assert.AreEqual("legacy-key", await store.GetAsync());
        Assert.AreEqual("legacy-key", secrets.Values[Key]);
        Assert.IsFalse(prefs.ContainsKey(Key));
    }

    [TestMethod]
    public async Task ExistingSecretWinsAndLegacyCopyIsStillDeleted()
    {
        var secrets = new FakeSecrets { Values = { [Key] = "secure-key" } };
        var prefs = new FakePreferences { [Key] = "stale-key" };
        var store = new BeaconKeyStore(secrets, prefs);

        Assert.AreEqual("secure-key", await store.GetAsync());
        Assert.AreEqual("secure-key", secrets.Values[Key]);
        Assert.IsFalse(prefs.ContainsKey(Key));
    }

    [TestMethod]
    public async Task EmptyLegacyPreferenceIsDeletedWithoutWritingASecret()
    {
        var secrets = new FakeSecrets();
        var prefs = new FakePreferences { [Key] = "" };
        var store = new BeaconKeyStore(secrets, prefs);

        Assert.AreEqual("", await store.GetAsync());
        Assert.IsFalse(secrets.Values.ContainsKey(Key));
        Assert.IsFalse(prefs.ContainsKey(Key));
    }

    [TestMethod]
    public async Task KeyIsCachedAfterTheFirstRead()
    {
        var secrets = new FakeSecrets { Values = { [Key] = "k" } };
        var store = new BeaconKeyStore(secrets, new FakePreferences());

        await store.GetAsync();
        await store.GetAsync();
        Assert.AreEqual(1, secrets.Reads);
    }

    [TestMethod]
    public async Task SetStoresSecretAndEmptyValueDeletesIt()
    {
        var secrets = new FakeSecrets();
        var prefs = new FakePreferences { [Key] = "legacy" };
        var store = new BeaconKeyStore(secrets, prefs);

        await store.SetAsync("new-key");
        Assert.AreEqual("new-key", secrets.Values[Key]);
        Assert.IsFalse(prefs.ContainsKey(Key));
        Assert.AreEqual("new-key", await store.GetAsync());

        await store.SetAsync("");
        Assert.IsFalse(secrets.Values.ContainsKey(Key));
        Assert.AreEqual("", await store.GetAsync());
    }

    [TestMethod]
    public async Task ReadErrorKeepsEverythingAndIsRetried()
    {
        var secrets = new FakeSecrets { Values = { [Key] = "secure-key" }, FailReads = true };
        var prefs = new FakePreferences { [Key] = "legacy-key" };
        var store = new BeaconKeyStore(secrets, prefs);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => store.GetAsync());
        Assert.AreEqual("secure-key", secrets.Values[Key]);
        Assert.IsTrue(prefs.ContainsKey(Key));

        secrets.FailReads = false;
        Assert.AreEqual("secure-key", await store.GetAsync());
        Assert.IsFalse(prefs.ContainsKey(Key));
    }

    private sealed class FakeSecrets : ISecretStore
    {
        public Dictionary<string, string> Values { get; } = [];
        public bool FailReads { get; set; }
        public int Reads { get; private set; }

        public Task<string?> GetAsync(string key)
        {
            Reads++;
            if (FailReads)
                return Task.FromException<string?>(new InvalidOperationException("Keystore key lost."));
            return Task.FromResult(Values.TryGetValue(key, out var value) ? value : null);
        }

        public Task SetAsync(string key, string value)
        {
            Values[key] = value;
            return Task.CompletedTask;
        }

        public bool Remove(string key) => Values.Remove(key);
    }

    private sealed class FakePreferences : Dictionary<string, string>, IPreferenceStore
    {
        public string? GetString(string key) => TryGetValue(key, out var value) ? value : null;
        void IPreferenceStore.Remove(string key) => Remove(key);
    }
}

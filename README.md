# RIoT2.Mobile

Mobile application for the **RIoT2** system, built with **.NET MAUI (.NET 9)**.
It is the cross-platform successor to the legacy Xamarin `RIoT2.Android` app.

## Features

- **Dashboard** — hosts the RIoT2 web dashboard in a `WebView` with a loading
  indicator, pull-to-refresh, connectivity tracking, and a friendly offline/error page.
- **Settings** — configure the controller URL and toggle **Alerts** /
  **Notifications** (persisted via `Preferences`, keys preserved from the legacy app).
- **Push notifications** — Firebase Cloud Messaging (FCM) with `alerts` and
  `notifications` topic subscriptions.
- **BLE beacon** — periodically broadcasts an encrypted manufacturer-data
  message over Bluetooth Low Energy. Configurable shared key, message, interval,
  and on/off toggle (see [BLE Beacon](#ble-beacon)).
- **MVVM** — built with `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`,
  `[RelayCommand]`) and Shell navigation.

## Supported Platforms

Actively built out of the box (see `<TargetFrameworks>` in
[`RIoT2.Mobile.csproj`](RIoT2.Mobile.csproj)):

- **Android** (`net9.0-android`)
- **Windows** (`net9.0-windows10.0.19041.0`)

Standard `dotnet new maui` scaffolding for **iOS**, **Mac Catalyst**, and
**Tizen** is present under [`Platforms/`](Platforms/), but those target
frameworks are commented out in the `.csproj` and not built by default. Add
them back to `<TargetFrameworks>` to enable those platforms.

## Requirements

- Visual Studio 2022 with the **.NET Multi-platform App UI development** workload.
- .NET 9 SDK (see [`global.json`](global.json)).
- A Firebase project (see [Firebase Setup](#firebase-console-setup)).

## Project Structure

```
Platforms/        Platform-specific entry points (Android, iOS, MacCatalyst, Tizen, Windows)
Resources/         Fonts, images, app icon, splash screen, and styles
Services/           IPushNotificationService / PushNotificationService (FCM)
                    ISettingsService / SettingsService (Preferences-backed settings)
ViewModels/        DashboardViewModel, SettingsViewModel (CommunityToolkit.Mvvm)
Views/             DashboardPage, SettingsPage (XAML)
MauiProgram.cs      App startup: DI registration, fonts, Firebase, ViewModel/Page registration
AppShell.xaml       Shell navigation host and route registration
```

## Getting Started

1. Clone the repository.
2. Complete the [Firebase setup](#firebase-console-setup) below.
3. Restore and build:

   ```bash
   dotnet restore
   dotnet build -f net9.0-android
   ```

4. Select a target (Android/Windows) in Visual Studio and run.

## Configuration

The default controller URL and notification toggles can be changed at runtime
in the **Settings** page. Values are stored with these `Preferences` keys
(kept identical to the legacy app):

| Setting | Key |
|---|---|
| Controller URL | `textCtrlUrl` |
| Alerts | `cbAlerts` |
| Notifications | `cbNotifications` |
| Beacon enabled | `beaconEnabled` |
| Beacon shared key | `beaconKey` |
| Beacon message | `beaconMessage` |
| Beacon interval (seconds) | `beaconIntervalSeconds` |

## BLE Beacon

When enabled, the app broadcasts an encrypted BLE advertisement at the
configured interval. All settings are configured at runtime on the **Settings**
page (shared key, message, interval, and on/off toggle) and persisted via
`Preferences`.

### Payload

Before encryption the payload is a UTF-8 string in the form:

```
{unixepoch timestamp}|{mac address}|{message}
```

- `unixepoch timestamp` — `DateTimeOffset.UtcNow.ToUnixTimeSeconds()`.
- `mac address` — modern Android/iOS do not expose the hardware MAC, so a stable
  per-install identifier (MAC-formatted, persisted under the `beaconDeviceId`
  `Preferences` key) is used instead.
- `message` — the free-text message from Settings.

### Receiver Contract

To decrypt the advertised manufacturer data, the receiving device must reverse
the encryption performed by `AesCryptoService`:

1. **Key derivation** — the 256-bit AES key is `SHA-256(sharedKey)`, where
   `sharedKey` is the UTF-8 bytes of the shared key configured in Settings.
2. **Cipher** — AES-GCM with a 12-byte nonce and 16-byte authentication tag.
3. **Byte layout** — the raw manufacturer-data bytes are:

   ```
   [ nonce (12 bytes) ][ tag (16 bytes) ][ ciphertext (variable) ]
   ```

4. **Decrypt** — split the layout above, run AES-GCM decrypt with the derived
   key, nonce, and tag, then UTF-8 decode the plaintext to recover the
   `{timestamp}|{mac}|{message}` string.

> **Platform note:** iOS does not support advertising custom manufacturer data.
> On iOS the encrypted payload is Base64-encoded and carried in the
> advertisement's local-name field instead; the receiver must Base64-decode it
> back to the raw bytes before applying the layout above.

### Background Execution (Android)

On Android the beacon runs inside a **foreground service**
(`BeaconForegroundService`) so the interval timer and BLE advertiser keep
running while the app is backgrounded. Android requires a persistent
notification for foreground services, so a low-importance "RIoT2 beacon active"
notification is shown while advertising. The service is started when advertising
begins and stopped when the beacon is turned off. This requires the
`FOREGROUND_SERVICE` and `FOREGROUND_SERVICE_CONNECTED_DEVICE` permissions
(declared in `AndroidManifest.xml`).

## Firebase Console Setup

Push notifications require a Firebase project and platform config files.

### 1. Create a Firebase Project

1. Go to [console.firebase.google.com](https://console.firebase.google.com).
2. **Add project** → name it (e.g., `RIoT2`) → **Create project**.

### 2. Register the Android App

1. In the project overview, click the **Android** icon (**Add app**).
2. **Android package name** — must match `<ApplicationId>` in [`RIoT2.Mobile.csproj`](RIoT2.Mobile.csproj) (currently `com.riot.jsuutari.riotmessanger`).
   > Change this to a real identifier before publishing to the Play Store — it
   > cannot be changed later. Update both the `.csproj` and Firebase to match.
3. Click **Register app** and **download `google-services.json`**.
4. Place the file at `Platforms/Android/google-services.json`.
5. Confirm its **Build Action** is `GoogleServicesJson` (already wired in the `.csproj`).

### 3. Register the iOS App (optional, requires enabling `net9.0-ios` in the `.csproj`)

1. Click **Add app** → **iOS** icon.
2. **Apple bundle ID** — match the iOS bundle identifier (same `ApplicationId`).
3. **Register app** and **download `GoogleService-Info.plist`**.
4. Place the file at `Platforms/iOS/GoogleService-Info.plist`
   (Build Action `BundleResource`, already wired in the `.csproj`.)
5. **Upload an APNs authentication key** (required for iOS push):
   - Firebase → **Project settings** → **Cloud Messaging** → **Apple app
     configuration** → **APNs Authentication Key**.
   - Upload the `.p8` key (created in the Apple Developer portal with the
     **Apple Push Notifications service** capability), plus the **Key ID** and **Team ID**.
   - Enable the **Push Notifications** capability and add `aps-environment`
     to `Platforms/iOS/Entitlements.plist`.

### 4. Enable Cloud Messaging

Verify **Project settings → Cloud Messaging** shows the
**Cloud Messaging API (V1)** enabled (Plugin.Firebase uses V1).

### 5. Topics

The `alerts` and `notifications` topics are created **automatically** the first
time a device subscribes — no console configuration required.

### 6. Send a Test Message

1. Firebase console → **Messaging** → **Create your first campaign** →
   **Firebase Notification messages**.
2. Enter a title and body.
3. **Target** → **Topic** → select `alerts` or `notifications`.
4. **Review** → **Publish** and confirm the device receives it.

To test a single device, copy the FCM token printed in the debug output
(logged by `PushNotificationService.InitializeAsync`) and use
**Send test message**.

### Verification Checklist

| Item | Done |
|---|---|
| `google-services.json` in `Platforms/Android/` (Build Action `GoogleServicesJson`) | ☐ |
| `GoogleService-Info.plist` in `Platforms/iOS/` (iOS only) | ☐ |
| Package/bundle ID matches `<ApplicationId>` | ☐ |
| APNs key uploaded (iOS only) | ☐ |
| App launches, permission prompt appears, FCM token logged | ☐ |
| Test message to `alerts` topic received | ☐ |

## Migration Notes

This app replaces the legacy Xamarin `RIoT2.Android` project. `Preferences`
keys for settings (see [Configuration](#configuration)) are kept identical to
the legacy app so existing user data carries over on upgrade.
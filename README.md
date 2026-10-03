# RIoT2.Mobile

.NET MAUI client app for the [RIoT2](https://github.com/Revolutionized-IoT2) IoT platform. It hosts
the RIoT2 web dashboard in a native `WebView`, manages Firebase Cloud Messaging topic
subscriptions, and includes a BLE beacon implementation for Android/iOS-oriented scenarios.

- Type: .NET MAUI app
- Target frameworks: `net10.0-android`, `net10.0-windows10.0.19041.0`
- App id: `com.riot.jsuutari.riotmessanger`

How this app fits into the platform: [architecture overview](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/architecture/overview.md).

## Features

- **Dashboard**: displays the configured dashboard URL in a MAUI `WebView` with loading state,
  pull-to-refresh, connectivity recovery and a local error page.
- **Settings**: lets the user edit the dashboard/controller URL and toggle `alerts` and
  `notifications` FCM topic subscriptions.
- **Push notifications**: initializes Firebase Cloud Messaging, subscribes or unsubscribes from
  the `alerts` and `notifications` topics, and supports notification deep links with `route` and
  `url` data payload keys.
- **BLE beacon**: encrypts a timestamp, per-install device id and message, then advertises the
  payload on supported platforms.

## Requirements

- Visual Studio 2022 with the .NET Multi-platform App UI workload.
- .NET 10 SDK (`global.json` requests 10.0.100 with `latestFeature` roll-forward) and the MAUI 10
  workload.
- Android SDK platform API 36 for `net10.0-android` builds.
- A Firebase project and Android `google-services.json` for push notifications.

## Build and test

From the repository root (`C:\Src\RIoT2\RIoT2.Mobile`):

```powershell
dotnet restore .\RIoT2.Mobile.sln
dotnet test .\Tests\RIoT2.Mobile.Tests.csproj
dotnet build .\RIoT2.Mobile.csproj -f net10.0-windows10.0.19041.0
dotnet build .\RIoT2.Mobile.csproj -f net10.0-android
```

The tests are offline client regressions. They source-link the real dashboard view model, dashboard
page and beacon services into `Tests/RIoT2.Mobile.Tests.csproj` with headless MAUI stand-ins, so
they do not require Bluetooth hardware, Firebase or a running dashboard.

`Directory.Build.repo.props`, imported by the shared `Directory.Build.props`, writes intermediates
to `<repo>\obj\<ProjectName>\` and outputs to `<repo>\bin\<ProjectName>\` while preserving excludes
for local `obj`, `bin` and `.vs` folders.

## Run

Open `RIoT2.Mobile.sln` in Visual Studio, select an Android or Windows target, and run. Android
push notifications require `Platforms/Android/google-services.json` with build action
`GoogleServicesJson`.

## Configuration

Settings are stored with `Microsoft.Maui.Storage.Preferences` using keys preserved from the legacy
Xamarin app:

| Setting | Preference key |
|---|---|
| Dashboard URL | `textCtrlUrl` |
| Alerts topic enabled | `cbAlerts` |
| Notifications topic enabled | `cbNotifications` |
| Beacon enabled | `beaconEnabled` |
| Beacon shared key | `beaconKey` |
| Beacon message | `beaconMessage` |
| Beacon interval seconds | `beaconIntervalSeconds` |

The default dashboard URL in `Services/SettingsService.cs` is an HTTP LAN URL for local
development. Change it in the Settings page before using the app outside that environment.

The app does not connect to MQTT directly. The dashboard loaded in the `WebView` is responsible for
its own MQTT and orchestrator API communication. Platform source-of-truth docs:

- [MQTT topics and payloads](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/contracts/mqtt-topics.md)
- [HTTP and gRPC APIs](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/contracts/http-api.md)
- [Environment variables, ports, volumes and images](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/contracts/env-vars.md)

## Firebase setup

1. Create a Firebase project at [console.firebase.google.com](https://console.firebase.google.com).
2. Register an Android app whose package name matches `<ApplicationId>` in `RIoT2.Mobile.csproj`.
3. Download `google-services.json` and place it at `Platforms/Android/google-services.json`.
4. Restrict the Firebase API key by Android package name and signing certificate before publishing.
5. Send test messages to the `alerts` or `notifications` topic.

Firebase client configuration is not a server secret, but production project files are still
environment-specific. Do not commit real production Firebase configuration to a public repository.

## BLE beacon

When enabled, the app periodically builds this plaintext payload:

```text
{unixepoch timestamp}|{device id}|{message}
```

`Services/AesCryptoService.cs` encrypts it with AES-GCM. The AES-256 key is `SHA-256(sharedKey)`,
and the advertised byte layout is:

```text
[ nonce (12 bytes) ][ tag (16 bytes) ][ ciphertext ]
```

Android advertises the encrypted bytes as manufacturer data with manufacturer id `0xFFFF`, starts a
foreground service while advertising, and validates the legacy BLE payload size before reporting
success. iOS base64-encodes the encrypted bytes in the advertisement local-name field because iOS
does not allow arbitrary manufacturer data. Windows uses `UnsupportedBeaconService`, so beacon
settings are disabled and attempts to start advertising fail explicitly.

`Preferences` is not encrypted storage. Move `beaconKey` to `SecureStorage` before treating it as a
production secret.

## Releases

Release notes are in [CHANGELOG.md](CHANGELOG.md). This repository currently has no tag-driven
release workflow.

## Contributing

- Instructions for AI coding agents: [AGENTS.md](AGENTS.md).
- Platform documentation: [.github/docs](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/README.md).

## License

See [LICENSE](LICENSE).

# AGENTS.md — RIoT2.Mobile

Applies to: this repository. Read the platform guide first:
[.github/AGENTS.md](https://github.com/Revolutionized-IoT2/.github/blob/main/AGENTS.md). It covers the
workspace map, platform-wide rules and the documentation rules. In the local workspace, every
`https://github.com/Revolutionized-IoT2/<Repo>/blob/main/<path>` link is the file
`C:\Src\RIoT2\<Repo>\<path>`; read the local file instead of fetching the URL.

## What this is

A .NET 10 MAUI app for Android and Windows. It hosts the RIoT2 UI dashboard in a `WebView`,
manages Firebase Cloud Messaging topic subscriptions, and contains a BLE beacon implementation
that is supported on Android/iOS service classes and explicitly unsupported elsewhere.

## Commands

Run from the repository root (`C:\Src\RIoT2\RIoT2.Mobile`), in PowerShell:

```powershell
dotnet restore .\RIoT2.Mobile.sln
dotnet test .\Tests\RIoT2.Mobile.Tests.csproj
dotnet build .\RIoT2.Mobile.csproj -f net10.0-windows10.0.19041.0
dotnet build .\RIoT2.Mobile.csproj -f net10.0-android
```

- `RIoT2.Mobile.csproj` targets `net10.0-android` and `net10.0-windows10.0.19041.0`.
- `Tests/RIoT2.Mobile.Tests.csproj` targets `net10.0` with `MSTest.Sdk` 4.4.1 and link-compiles
  dashboard and beacon code for offline tests.
- Android builds require Android SDK platform API 36.
- There is no repository-local CI workflow and no tag-driven release workflow today.

`Directory.Build.repo.props`, imported by the shared `Directory.Build.props`, sets intermediates to
`<repo>\obj\<ProjectName>\` and outputs to `<repo>\bin\<ProjectName>\`. Keep the explicit excludes
for local `bin`, `obj` and `.vs` folders.

## Layout

| Path | Contents |
|---|---|
| `RIoT2.Mobile.csproj` | MAUI target frameworks, app id, package references and Firebase config items |
| `MauiProgram.cs` | DI registrations, Firebase lifecycle initialization, platform beacon service selection |
| `App.xaml.cs` | Starts push initialization and resumes the beacon when enabled |
| `AppShell.xaml`, `AppShell.xaml.cs` | Shell host and Settings route registration |
| `ViewModels/DashboardViewModel.cs` | WebView source, refresh, connectivity and lifecycle state |
| `Views/DashboardPage.xaml.cs` | WebView navigation events, reload, local error page and 30-second timeout |
| `ViewModels/SettingsViewModel.cs` | Settings persistence, FCM subscription updates and beacon restart |
| `Services/SettingsService.cs` | `Preferences` keys and defaults |
| `Services/PushNotificationService.cs` | Firebase token logging, topic subscriptions and notification deep links |
| `Services/*Beacon*`, `Platforms/*/Services/BeaconService.cs` | Cross-platform and platform BLE beacon logic |
| `Tests/RIoT2.Mobile.Tests.csproj` | Offline dashboard and beacon regression tests |

## Contracts consumed here

RIoT2.Mobile is not an MQTT participant and does not call the orchestrator HTTP API directly. It
loads a dashboard URL in a `WebView`; the loaded UI handles MQTT and REST. Link to the hub docs
instead of copying their contracts:

- [mqtt-topics.md](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/contracts/mqtt-topics.md):
  states that Mobile does not use MQTT and that push notifications use Firebase topics.
- [http-api.md](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/contracts/http-api.md):
  relevant to whatever RIoT2.UI instance is loaded in the `WebView`.
- [env-vars.md](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/contracts/env-vars.md):
  relevant to the dashboard/orchestrator deployment; this app reads no RIoT2 environment variables.

Firebase topics are implemented in `Services/PushNotificationService.cs`: `alerts` and
`notifications`. Notification tap deep links read optional `route` and `url` keys from the FCM data
payload.

## Rules

- Do not copy platform MQTT, REST or environment-variable tables into this repository. Link to the
  hub contract docs.
- Do not commit production Firebase project files. `Platforms/Android/google-services.json` is
  included by the project when present, but the API key must be restricted by package and signing
  certificate before publishing.
- Keep legacy `Preferences` keys in `Services/SettingsService.cs` unless a migration is written, so
  existing users keep their settings.
- Treat `beaconKey` as sensitive. It currently lives in `Preferences`; move it to `SecureStorage`
  before using it as a production secret.
- Keep push subscription changes in `SettingsViewModel.SaveAsync`: settings are saved, then
  `IPushNotificationService.UpdateChannelSubscriptionsAsync()` updates FCM topics.
- Keep Firebase failures logged, not thrown through app startup. `App.xaml.cs` intentionally
  fire-and-forgets initialization because `PushNotificationService` handles/logs failures.
- Windows and other unsupported beacon platforms must fail explicitly through
  `UnsupportedBeaconService`; don't report successful advertising without platform confirmation.
- Android beacon startup must wait for the platform advertise callback before setting active state.

## Pitfalls

- The WebView accepts any URL the user stores in Settings. The default URL is HTTP, and there is no
  host allowlist (optional hardening item S11).
- The dashboard URL, notification toggles and beacon values are all in `Preferences`, not encrypted
  storage. This preserves the legacy app keys but is not suitable for production secrets.
- `Views/DashboardPage.xaml.cs` stops loading indicators after a 30-second timeout; it does not
  cancel an eventual WebView load.
- Returning from Settings refreshes the dashboard when `Services/SettingsService.cs` has a new URL;
  same-URL refresh uses `Web.Reload()`.
- `Services/PushNotificationService.cs` logs the FCM token and token changes at information level.
- Android BLE legacy advertising is size-limited. `LegacyBeaconPayload.Validate` can reject even an
  empty encrypted message as too large for non-connectable manufacturer-data advertising.
- `Plugin.Firebase.CloudMessaging` has no `net10.0-android` build; the app intentionally consumes
  its `net9.0-android` assets from `net10.0-android`.
- Android `targetSdkVersion` is 36 by default for `net10.0-android`; `minSdk` remains 21.

## Related work

- Optional hardening item [S11](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/backlog/optional-hardening.md):
  WebView any URL, beacon key in `Preferences`, HTTP default.
- Maintainer action [MA1](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/backlog/README.md#ma1-rotate-the-leaked-credentials-and-scrub-them-from-git-history):
  rotate/restrict exposed credentials and keys; Mobile's Firebase client key must be restricted
  before publishing.
- [M8](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/plans/m08-dotnet10-migration.md):
  completed the MAUI/.NET 10 migration and portable build-output paths; nullable and
  threading-analyzer practice steps remain open.

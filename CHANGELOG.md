# Changelog

All notable changes to `RIoT2.Mobile`. A version is released by pushing a git tag; CI then attaches
the Android APK to a GitHub release.

## [Unreleased]

## [1.1.0] - 2026-11-03

Mobile quick hardening ([I44](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/increments/phase-3-4.md#i44-mobile-quick-hardening-s), part of optional hardening item S11).

- Security: the beacon shared key moved from plain `Preferences` to `SecureStorage` (Android
  Keystore, iOS Keychain) behind the new `IBeaconKeyStore`. The first read after the upgrade moves
  the old value and deletes the `Preferences` copy; no user action is needed. A read error is
  reported and retried, never turned into a deleted key. A failed key write on Save shows an error
  on the Settings page instead of crashing the app.
- Security: `PushNotificationService` no longer logs the FCM registration token or its changes;
  it logs only whether the token is ready.
- Changed: `ISettingsService.BeaconKey` is removed, and `IBeaconPayloadFactory.BuildManufacturerData`
  is now `BuildManufacturerDataAsync`.
- Tests: `BeaconKeyStoreTests` cover the migration, caching, deletion and a read error.

- Build: `renovate.json` extends the shared Renovate preset ([I33](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/increments/phase-2.md#i33-supply-chain-s)). Renovate opens grouped
  update pull requests once the app is installed (MA7).
- Release: the APK gets `RIoT2.Mobile_<version>.apk.spdx.json`, an SPDX SBOM of the app's NuGet
  packages (Microsoft sbom-tool), on the GitHub release.
- Build: the shared `Directory.Build.props` sets `Nullable=enable` for every project ([I32](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/increments/phase-2.md#i32-nullable-reference-types-m)).
  The projects here already had it, so nothing changes.
- Build: the threading analyzers ([I31](https://github.com/Revolutionized-IoT2/.github/blob/main/docs/increments/phase-2.md#i31-async-cleanup-part-2-and-threading-analyzers-m)) run in every project from the shared
  `Directory.Build.props` and `.editorconfig`: a blocking wait, `async void` or an unobserved task
  fails the CI build. No change to the shipped binaries.

## [1.0.0] - 2026-10-12

The first tagged release: MAUI 10, Android API 36.

- Added CI and a release workflow: every push runs the unit tests, the Android APK builds weekly,
  and a tag attaches `RIoT2.Mobile_<tag>.apk` and its `.sha256` to a GitHub release.
  `.gitignore` no longer hides `.github/workflows`.
- Changed the app to MAUI 10 with `net10.0-android` and `net10.0-windows10.0.19041.0`; Android
  targets API 36 and keeps `minSdk` 21.
- Updated packages to `Microsoft.Maui.Controls` 10.0.110, `CommunityToolkit.Maui` 15.0.1,
  `Plugin.Firebase.CloudMessaging` 4.0.1 and `Microsoft.Extensions.Logging.Debug` 10.0.12.
- Changed tests to `net10.0` with `MSTest.Sdk` 4.4.1.
- Replaced absolute output roots with repository-local `obj\<ProjectName>\` and
  `bin\<ProjectName>\` paths via `Directory.Build.repo.props`.
- Fixed Android Firebase lifecycle initialization, Android 12+ Bluetooth permission requests and
  foreground-service notification error reporting.
- Added `.gitleaks.toml`, which allowlists the Firebase client API key in
  `Platforms/Android/google-services.json` for secret scans. The key ships in every APK; restrict it
  in Google Cloud to the app's package name and signing SHA.
- Documentation: `AGENTS.md` is the AI instruction file, `CLAUDE.md` imports it, and release notes
  now live in this file.

## Earlier versions

See `git log`; there are no release notes for earlier versions in the existing docs.

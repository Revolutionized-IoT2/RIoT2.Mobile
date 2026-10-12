# Changelog

All notable changes to `RIoT2.Mobile`. A version is released by pushing a git tag; CI then attaches
the Android APK to a GitHub release.

## [Unreleased]

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

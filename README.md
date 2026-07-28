# RIoT2.Mobile

Mobile application for the **RIoT2** system, built with **.NET MAUI (.NET 9)**.
It is the cross-platform successor to the legacy Xamarin `RIoT2.Android` app.

## Features

- **Dashboard** — hosts the RIoT2 web dashboard in a `WebView` with a loading
  indicator and a friendly offline/error page.
- **Settings** — configure the controller URL and toggle **Alerts** /
  **Notifications** (persisted via `Preferences`, keys preserved from the legacy app).
- **Push notifications** — Firebase Cloud Messaging (FCM) with `alerts` and
  `notifications` topic subscriptions.
- **Cross-platform** — Android, iOS, macOS (Mac Catalyst), and Windows.

## Requirements

- Visual Studio 2022 with the **.NET Multi-platform App UI development** workload.
- .NET 9 SDK.
- A Firebase project (see [Firebase Setup](#firebase-console-setup)).

## Project Structure

## Getting Started

1. Clone the repository.
2. Complete the [Firebase setup](#firebase-console-setup) below.
3. Restore packages and build: dotnet restore dotnet build -f net9.0-android
4. Select a target (Android/iOS/Windows) in Visual Studio and run.

## Configuration

The default controller URL and notification toggles can be changed at runtime
inthe **Settings** page. Values are stored with these `Preferences` keys
(kept identical to the legacy app):

| Setting | Key |
|---|---|
| Controller URL | `textCtrlUrl` |
| Alerts | `cbAlerts` |
| Notifications | `cbNotifications` |

## Firebase Console Setup

Push notifications require a Firebase project and platform config files.

### 1. Create a Firebase Project

1. Go to [console.firebase.google.com](https://console.firebase.google.com).
2. **Add project** → name it (e.g., `RIoT2`) → **Create project**.

### 2. Register the Android App

1. In the project overview, click the **Android** icon (**Add app**).
2. **Android package name** —must match `<ApplicationId>` in `RIoT2.Mobile.csproj`: com.companyname.riot2.mobile
> Change this to a real identifier before publishing to the Play Store — it
> cannot be changed later. Update both the `.csproj` and Firebase to match.
3. Click **Register app** and **download `google-services.json`**.
4. Place the file at: Platforms/Android/google-services.json
5. Confirm its **Build Action** is `GoogleServicesJson` (already wired in the `.csproj`).

### 3. Register the iOS App (optional)

1. Click **Add app** → **iOS** icon.
2. **Apple bundle ID** — match the iOS bundle identifier (same `ApplicationId`).
3. **Register app** and **download `GoogleService-Info.plist`**.
4. Place the file at: Platforms/iOS/GoogleService-Info.plist
(Build Action `BundleResource`, already wired in the `.csproj`.)
5. **Upload an APNs authentication key** (required for iOS push):
- Firebase → **Project settings** → **Cloud Messaging** → **Apple app
  configuration** → **APNs Authentication Key**.
- Upload the `.p8` key (created in the Apple Developer portal with the
  **Apple Push Notifications service** capability),plus the **Key ID** and **Team ID**.
- Enable the **Push Notifications** capability and add `aps-environment`
  to `Platforms/iOS/Entitlements.plist`.

### 4. Enable Cloud Messaging

Verify **Project settings →Cloud Messaging** shows the
**Cloud Messaging API (V1)** enabled (Plugin.Firebase uses V1).

### 5. Topics

The `alerts` and `notifications` topics are created **automatically** the first
time a device subscribes — no console configuration required.

### 6. Send a Test Message

1. Firebase console → **Messaging** → **Create your first campaign** →
**Firebase Notification messages**.
2. Enter a title and body.
3. **Target** → **Topic** → select `alerts` or `notifications`.
4. **Review** → **Publish**and confirm the device receives it.

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

This app replaces the legacy Xamarin `RIoT2.Android` project. See
[`docs/PORTING_PLAN.md`](docs/PORTING_PLAN.md) for the full porting plan and the
mapping of legacy components to their MAUI equivalents.
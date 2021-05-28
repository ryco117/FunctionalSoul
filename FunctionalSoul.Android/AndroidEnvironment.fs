module AndroidEnvironment

open Android
open Android.OS

open Types

type AndroidEnvironment (mainActivity: App.Activity) =
    let requiredStoragePermissions = [|Manifest.Permission.WriteExternalStorage|]

    interface IEnvironment with
        member _.getSharedDownloads () =
            let folder = Environment.GetExternalStoragePublicDirectory Environment.DirectoryDownloads
            folder.AbsolutePath

        member _.requestExternalPermissions () =
            // TODO: Use `ShouldShowRequestPermissionRationale Manifest.Permission.WriteExternalStorage` to determine if more context for request is needed
            if mainActivity.CheckSelfPermission Manifest.Permission.WriteExternalStorage <> Content.PM.Permission.Granted then
                mainActivity.RequestPermissions (requiredStoragePermissions, 1000)
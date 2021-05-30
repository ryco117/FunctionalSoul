(*
This file is part of FunctionalSoul

FunctionalSoul is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

FunctionalSoul is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with FunctionalSoul. If not, see <https://www.gnu.org/licenses/>.
*)

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
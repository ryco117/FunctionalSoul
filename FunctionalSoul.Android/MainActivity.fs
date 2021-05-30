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

namespace FunctionalSoul.Android

open Android.App
open Android.Content
open Android.Content.PM
open Android.Runtime
open Android.Views
open Android.Widget
open Android.OS
open Xamarin.Forms.Platform.Android

open AndroidEnvironment
open FunctionalSoul

[<Activity (Label = "FunctionalSoul.Android", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = (ConfigChanges.ScreenSize ||| ConfigChanges.Orientation))>]
type MainActivity () =
    inherit FormsAppCompatActivity ()
    override this.OnCreate (bundle: Bundle) =
        FormsAppCompatActivity.TabLayoutResource <- Resources.Layout.Tabbar
        FormsAppCompatActivity.ToolbarResource <- Resources.Layout.Toolbar

        base.OnCreate (bundle)
        Xamarin.Essentials.Platform.Init (this, bundle)
        Xamarin.Forms.Forms.Init (this, bundle)

        let environment = AndroidEnvironment (this)
        this.LoadApplication (FunctionalSoul (environment))

    override this.OnRequestPermissionsResult (requestCode: int, permissions: string[], [<GeneratedEnum>] grantResults: Android.Content.PM.Permission[]) =
        // TODO: Handle permission result gracefully
        Xamarin.Essentials.Platform.OnRequestPermissionsResult (requestCode, permissions, grantResults)
        base.OnRequestPermissionsResult (requestCode, permissions, grantResults)
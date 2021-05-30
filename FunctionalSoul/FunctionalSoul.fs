(*
FunctionalSoul - A cross-platform Soulseek client app written in F#.
Copyright (C) 2021  Ryan Andersen

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
*)

module FunctionalSoul

open Fabulous.XamarinForms
open Xamarin.Forms

open Types

type FunctionalSoul (environment: IEnvironment) as app = 
    inherit Application ()

    let update msg model =
        match model with
        | Login model -> LoginPage.update environment msg model
        | FileSearch model -> FileSearchPage.update environment msg model

    let view (model: Model) dispatch =
        match model with
        | Login model -> LoginPage.view environment model dispatch
        | FileSearch model -> FileSearchPage.view environment model dispatch

    let program =
        XamarinFormsProgram.mkProgram Helpers.init update view

    let runner = 
        program
        |> XamarinFormsProgram.run app

    override _.OnStart () =
        // TODO: Request permissions at more appropriate time
        environment.requestExternalPermissions ()

    override _.OnSleep () =
        match runner.CurrentModel with
        | FileSearch model ->
            match model.download with
            | Some {progress = Some progress; client = _; peer = _; selectedFile = _} -> progress.cancellationToken.Cancel ()
            | _ -> ()
            model.client.Dispose ()
        | _ -> ()
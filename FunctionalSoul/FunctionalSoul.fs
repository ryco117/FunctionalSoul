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
module DownloadPage

open Fabulous
open Fabulous.XamarinForms
open Soulseek
open System.IO
open Xamarin.Forms

open Types

let download (env: IEnvironment) (model: DownloadModel) (fileSaveName: string) dispatch =
    try
        let localFilePath = 
            let illegal = Path.GetInvalidFileNameChars ()
            let mutable name = fileSaveName
            for c in illegal do
                name <- name.Replace (c.ToString(), "")
            Path.Combine(env.getSharedDownloads (), name)
        let outFile = new FileStream (localFilePath, FileMode.Create)
        let file = model.selectedFile
        let size = file.Size
        let fsize = float file.Size
        async {
            try
                do! model.client.EnsureConnectedAsync ()
                do! model.client.DownloadAsync (
                        username = model.peer,
                        filename = file.Filename,
                        outputStream = outFile,
                        size = System.Nullable size,
                        cancellationToken = System.Nullable model.progress.Value.cancellationToken.Token,
                        options = TransferOptions (
                            progressUpdated = (fun p -> DownloadUpdateStatus (float p.Transfer.BytesTransferred / fsize) |> dispatch),
                            disposeInputStreamOnCompletion = true,
                            disposeOutputStreamOnCompletion = true))
                    |> Async.AwaitTask
                return DownloadFinish (Ok localFilePath)
            with 
            | :? System.Threading.Tasks.TaskCanceledException ->
                // TODO: Delete files on exception
                outFile.Dispose ()
                return DownloadFinish (Error $"Cancelled: %s{fileSaveName}")
            | err ->
                outFile.Dispose ()
                System.Diagnostics.Debug.WriteLine $"Canceled or failed to download: %s{err.Message}"
                return DownloadFinish (Error err.Message)
        } |> Cmd.ofAsyncMsg
    with err ->
        System.Diagnostics.Debug.WriteLine $"Failed to start download: %s{err.Message}"
        DownloadFinish (Error err.Message) |> Cmd.ofMsg

let update environment msg model =
    match msg with
    | DownloadDismissUi ->
        match model.progress with
        | Some progress -> progress.cancellationToken.Cancel ()
        | None -> ()
        None, Cmd.none
    | DownloadStart (fileSaveName, dispatch) ->
        let newModel = {model with progress = Some {downloadedFraction = 0.; cancellationToken = new System.Threading.CancellationTokenSource ()}}
        Some newModel, download environment newModel fileSaveName dispatch
    | DownloadUpdateStatus frac ->
        match model.progress with
        | Some progress -> Some {model with progress = Some {progress with downloadedFraction = frac}}, Cmd.none
        | None -> Some model, Cmd.none
    | DownloadFinish result ->
        let message = 
            match result with
            | Ok localFile -> $"Saved: %s{localFile}"
            | Error err -> err
        let snackbar = {lifetime = new System.Timers.Timer (5000., AutoReset = false, Enabled = false); message = message}
        None, Cmd.ofMsg (CreateSnackbar snackbar)
    | _ -> System.Diagnostics.Debug.WriteLine "Handling unexpected message for Download page"; Some model, Cmd.none

let view model dispatch =
    let file = model.selectedFile
    let filenameMatch = Helpers.filenameRegex.Match file.Filename
    let niceFilename, nicePath =
        let matchName = filenameMatch.Groups.[5].Value
        let matchPath = filenameMatch.Groups.[3].Value
        if System.String.IsNullOrEmpty matchName then
            if System.String.IsNullOrEmpty matchPath then
                None, file.Filename
            else
                Some (matchPath.Replace ("\\", " ")), matchPath
        else
            Some matchName, matchPath
    let bestUnits, bestSize = Helpers.bestUnits file.Size
    let unitString = Helpers.unitsToString bestUnits
    let nameLabel =
        match niceFilename with
        | Some name -> Some (View.Label (text = $"Name: %s{name}"))
        | None -> None
    let pathLabel = View.Label (text = $"Path: %s{nicePath}")
    let sizeLabel = View.Label (text = $"Size: %.1f{bestSize} %s{unitString}")
    let peerLabel = View.Label (text = $"Peer: %s{model.peer}")
    let progressBar =
        match model.progress with
        | Some progress -> Some (View.ProgressBar (progress = progress.downloadedFraction))
        | None -> None
    View.ContentPage (
        content = View.StackLayout (
            padding = Thickness 20.,
            children = [
                if nameLabel.IsSome then
                    nameLabel.Value
                pathLabel
                peerLabel
                sizeLabel
                match progressBar with
                | Some progressBar -> progressBar
                | None ->
                    View.Button (
                        text = "Download",
                        command = (fun () ->
                            let saveFileName =
                                match niceFilename with 
                                | Some name -> name
                                | None -> "download.FunctionalSoul"
                            DownloadStart (saveFileName, dispatch) |> dispatch))]))
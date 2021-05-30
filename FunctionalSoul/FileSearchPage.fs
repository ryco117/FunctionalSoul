module FileSearchPage

open Fabulous
open Fabulous.XamarinForms
open Soulseek
open Xamarin.Forms

open Types

let searchPattern model dispatch =
    async {
        try
            do! model.client.EnsureConnectedAsync ()
            do! model.client.SearchAsync (
                    query = SearchQuery (model.searchPattern),
                    token = Helpers.nextToken (),
                    options =
                        SearchOptions (
                            searchTimeout = 5000,
                            responseLimit = 10,
                            responseReceived = (fun e ->
                                let response = e.Response
                                if response.FileCount > 0 then
                                    System.Diagnostics.Debug.WriteLine $"Results from %s{response.Username} for search '%s{model.searchPattern}'"
                                    let mutable files = []
                                    for file in response.Files do
                                        System.Diagnostics.Debug.WriteLine $"%s{file.Filename}: %u{file.Size}"
                                        files <- file::files
                                    SearchPatternUserFiles {username = response.Username; files = files} |> dispatch)))
                |> Async.AwaitTask
                |> Async.Ignore
            return SearchPatternFinish (Ok ())
        with err ->
            System.Diagnostics.Debug.WriteLine $"Failed to search: %s{err.Message}"
            return SearchPatternFinish (Error "Big boo boo")
    } |> Cmd.ofAsyncMsg

let update environment msg model =
    match model.download with
    | Some downloadModel ->
        let newDownloadModel, cmd = DownloadPage.update environment msg downloadModel
        FileSearch {model with download = newDownloadModel}, cmd
    | None ->
        match msg with
        | CreateSnackbar snackbar -> FileSearch {model with snackbar = Some snackbar}, Cmd.none
        | ClearSnackbar ->
            match model.snackbar with
            | Some snackbar -> snackbar.lifetime.Dispose (); FileSearch {model with snackbar = None}, Cmd.none
            | None -> FileSearch model, Cmd.none
        | CompletedSearchPattern pattern -> FileSearch {model with searchPattern = pattern}, Cmd.none
        | SearchPatternStart dispatch -> FileSearch {model with userFileMatches = []; searching = true}, searchPattern model dispatch
        | SearchPatternUserFiles files -> FileSearch {model with userFileMatches = files::model.userFileMatches}, Cmd.none
        | SearchPatternFinish _result -> FileSearch {model with searching = false}, Cmd.none    // TODO: Handle FileSearch errors
        | SelectedFileIndexOption (Some i) ->
            let user, file = Helpers.userFileByIndex model.userFileMatches i
            FileSearch {model with download = Some {client = model.client; progress = None; peer = user; selectedFile = file}}, Cmd.none
        | SelectedFileIndexOption None -> FileSearch {model with download = None}, Cmd.none
        | DownloadFinish result ->
            let message = 
                match result with
                | Ok localFile -> $"Saved: %s{localFile}"
                | Error err -> err
            FileSearch model, Helpers.createSnackbarInfo 5000. message |> CreateSnackbar |> Cmd.ofMsg
        | _ -> System.Diagnostics.Debug.WriteLine "Handling unexpected message for File Search page"; FileSearch model, Cmd.none

let view _environment model dispatch =
    let userFiles =
        let rec compileUsers acc i = function
        | [] -> acc
        | userFiles::tail ->
            let list =
                let rec compileFiles acc i = function
                | [] -> acc
                | (file: File)::tail ->
                    let fileEntry =
                        let filenameMatch = Helpers.filenameRegex.Match file.Filename
                        let niceFilename =
                            let matchName = filenameMatch.Groups.[5].Value
                            if System.String.IsNullOrEmpty matchName then
                                let matchPath = filenameMatch.Groups.[3].Value
                                if System.String.IsNullOrEmpty matchPath then
                                    file.Filename
                                else
                                    matchPath
                            else
                                matchName
                        let extension =
                            if System.String.IsNullOrEmpty file.Extension then
                                let extStr = filenameMatch.Groups.[8].Value
                                if System.String.IsNullOrEmpty extStr then
                                    None
                                else
                                    Some extStr
                            else
                                Some file.Extension
                        let bestUnits, bestSize = Helpers.bestUnits file.Size
                        let unitString = Helpers.unitsToString bestUnits
                        View.TextCell (
                            text = niceFilename,
                            detail =
                                match extension with
                                | None -> $"%.1f{bestSize} %s{unitString}"
                                | Some extension -> $"%.1f{bestSize} %s{unitString}        %s{extension.ToUpper ()}")
                    compileFiles (fileEntry::acc) (i+1) tail
                compileFiles acc i userFiles.files
            compileUsers list i tail
        compileUsers [] 0 model.userFileMatches
    let fileSearchBar =
        View.Entry (
            placeholder = "term1 orTerm2 -notThis",
            text = model.searchPattern,
            textChanged = (fun args ->
                let t = args.NewTextValue
                CompletedSearchPattern (if isNull t then "" else t) |> dispatch),
            completed = (fun _ -> SearchPatternStart dispatch |> dispatch))
    let fileList =
        View.ListView (
            items = userFiles,
            itemSelected = (fun i -> SelectedFileIndexOption i |> dispatch),
            selectedItem = None)
    let searchButton = View.Button (text = "Search", command = (fun () -> SearchPatternStart dispatch |> dispatch))
    let fileSearchPage =
        let stackedContent =
            View.StackLayout (
                padding = Xamarin.Forms.Thickness 20.,
                children =
                    if model.searching then [
                        fileSearchBar.IsEnabled false
                        View.ActivityIndicator (isRunning = true)
                        (fileList.IsEnabled false).BackgroundColor (Color.FromRgb (0xE8, 0xE8, 0xE8))]
                    else [
                        fileSearchBar
                        fileList
                        searchButton])
        let page = Helpers.createPageWithLayoutSnackbarOption model.snackbar stackedContent dispatch
        page.HasNavigationBar false

    // Allow modal Download page over File Search
    View.NavigationPage (
        pages = [
            fileSearchPage
            match model.download with
            | Some model -> ((DownloadPage.view model dispatch).HasNavigationBar true).HasBackButton true
            | None -> ()],
        popped = (fun _ -> dispatch DownloadDismissUi))
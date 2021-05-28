module LoginPage

open Fabulous
open Fabulous.XamarinForms
open Soulseek
open System.Text.RegularExpressions
open Xamarin.Forms

open Types

let connectOptions =
    let connectOpt = ConnectionOptions (connectTimeout = 2000, inactivityTimeout = 5000)
    SoulseekClientOptions (peerConnectionOptions = connectOpt, transferConnectionOptions = connectOpt)

let portRegex = Regex "(.*)(:([0-9]+))"

// Perform asynchronous work to determine if login information can succeed
let attemptLogin model =
    let address, user, password, port = 
        let __address = model.loginInfo.address
        let portMatch = portRegex.Match __address
        let _address, _port =
            let portStr = portMatch.Groups.[3].Value
            if  portStr.Length = 0 then __address, 2271 else portMatch.Groups.[1].Value, System.Int32.Parse portStr
        _address, model.loginInfo.user, model.loginInfo.password, _port
    async {
        let client = new FunctionalClient (connectOptions, address, port, user, password)
        try
            do! client.ConnectAsync ()
            Helpers.storeLoginInfo model.loginInfo
            System.Diagnostics.Debug.WriteLine $"Login succeeded: %s{client.Address}"
            return LoginAttemptResult (Ok client)
        with err ->
            System.Diagnostics.Debug.WriteLine $"Login failed: %s{err.Message}"
            return LoginAttemptResult (Error err.Message)
    } |> Cmd.ofAsyncMsg

let update _environment msg model =
    match msg with
    | CreateSnackbar snackbar -> Login {model with snackbar = Some snackbar}, Cmd.none
    | ClearSnackbar ->
        match model.snackbar with
        | Some snackbar -> snackbar.lifetime.Dispose (); Login {model with snackbar = None}, Cmd.none
        | None -> Login model, Cmd.none
    | LoginAttemptStart ->
        match model.loginStatus with
        | Pending -> Login model, Cmd.none
        | _ -> Login {model with loginStatus = Pending}, attemptLogin model
    | LoginAttemptResult (Error message) -> Login {model with loginStatus = LoginFail}, Helpers.createSnackbarInfo 5000. message |> CreateSnackbar |> Cmd.ofMsg
    | LoginAttemptResult (Ok client) -> FileSearch {Helpers.initFileSearchModelNullClient with client = client}, Cmd.none
    | CompletedAddress address -> Login {model with loginInfo = {model.loginInfo with address = address}}, Cmd.none
    | CompletedUser user -> Login {model with loginInfo = {model.loginInfo with user = user}}, Cmd.none
    | CompletedPassword password -> Login {model with loginInfo = {model.loginInfo with password = password}}, Cmd.none
    | _ -> System.Diagnostics.Debug.WriteLine "Handling unexpected message for Login page"; Login model, Cmd.none

let view _environment model dispatch =
    let loginStatus = model.loginStatus
    let errorColour = Color (0.8, 0.1, 0.1)
    let defaultColour = Color.Transparent
    let framePadding = Thickness 4.
    let defaultPagePadding = Thickness 20.
    let address =
        let v = View.Entry (placeholder = "vps.slsknet.org", text = model.loginInfo.address, textChanged = (fun addr -> CompletedAddress addr.NewTextValue |> dispatch))
        View.Frame (
            padding = framePadding,
            backgroundColor = Color.Transparent,
            borderColor = (
                match loginStatus with
                | Offline | Pending -> defaultColour
                | LoginFail -> errorColour),
            content = if loginStatus = Pending then v.IsEnabled false else v)
    let user =
        let v = View.Entry (placeholder = "username", text = model.loginInfo.user, textChanged = (fun user -> CompletedUser user.NewTextValue |> dispatch))
        View.Frame (
            padding = framePadding,
            backgroundColor = Color.Transparent,
            borderColor = (
                match loginStatus with
                | Offline | Pending -> defaultColour
                | LoginFail -> errorColour),
            content = if loginStatus = Pending then v.IsEnabled false else v)
    let password =
        let v = View.Entry (placeholder = "password", text = model.loginInfo.password, isPassword = true, textChanged = (fun pswd -> CompletedPassword pswd.NewTextValue |> dispatch))
        View.Frame (
            padding = framePadding,
            backgroundColor = Color.Transparent,
            borderColor = (
                match loginStatus with
                | Offline | Pending -> defaultColour
                | LoginFail -> errorColour),
            content = if loginStatus = Pending then v.IsEnabled false else v)
    let stackedContent =
        View.StackLayout (
            padding = defaultPagePadding,
            children = [
                address
                user
                password
                if loginStatus = Pending then
                    View.ActivityIndicator (isRunning = true)
                else
                    View.Button (text = "Login", command = (fun () -> dispatch LoginAttemptStart))])
    Helpers.createPageWithLayoutSnackbarOption model.snackbar stackedContent dispatch
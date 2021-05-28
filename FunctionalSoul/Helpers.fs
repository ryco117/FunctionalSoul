module Helpers

open Fabulous
open Fabulous.XamarinForms
open System.Linq
open System.Text.RegularExpressions
open Xamarin.Forms

open Types

let initLoginModel = {
    loginInfo = {address = "vps.slsknet.org"; user = ""; password = ""}
    loginStatus = Offline
    snackbar = None}

let initFileSearchModelNullClient = {
    client = Unchecked.defaultof<_>
    download = None
    searching = false
    searchPattern = ""
    snackbar = None
    userFileMatches = []}

let initModel = Login initLoginModel

let retrieveLoginInfo () =
    let address = Xamarin.Essentials.Preferences.Get ("login_address", initLoginModel.loginInfo.address)
    let username = Xamarin.Essentials.Preferences.Get ("login_username", initLoginModel.loginInfo.user)
    let password = Xamarin.Essentials.Preferences.Get ("login_password", initLoginModel.loginInfo.password)
    {address = address; user = username; password = password}

let storeLoginInfo {address = address; user = username; password = password} =
    Xamarin.Essentials.Preferences.Set ("login_address", address)
    Xamarin.Essentials.Preferences.Set ("login_username", username)
    Xamarin.Essentials.Preferences.Set ("login_password", password)

let init () =
    Login {initLoginModel with loginInfo = retrieveLoginInfo ()}, Cmd.none

let random = System.Random ()
let nextToken () = System.Nullable (random.Next ())

let filenameRegex = Regex @"^((@@|[a-zA-Z]:)?[^\\]*\\)(([^\\]*\\)*(([^\\.]*\.)*(([a-zA-Z0-9]{1,6})|[^\\.]+)))$"

// Convert bytes-measure enum to count in bytes
let unitsToBytes = function
| BytesUnits.Bytes -> 1L
| BytesUnits.KiloBytes -> 1L <<< 10
| BytesUnits.MegaBytes -> 1L <<< 20
| BytesUnits.GigaBytes -> 1L <<< 30
| _ -> raise (System.Exception "Unknown units enum")

// Convert bytes-measure enum to short-hand string
let unitsToString = function
| BytesUnits.Bytes -> "B"
| BytesUnits.KiloBytes -> "KB"
| BytesUnits.MegaBytes -> "MB"
| BytesUnits.GigaBytes -> "GB"
| _ -> raise (System.Exception "Unknown units enum")

// Given a byte count, find the largest measure of which there is more than one unit of
let bestUnits n =
    if n = 0L then
        BytesUnits.Bytes, 0.f
    else
        let x = float32 n
        let mutable s = x
        let mutable units = BytesUnits.Bytes
        for u in (System.Enum.GetValues typeof<BytesUnits>).Cast<BytesUnits>() do
            let t = x / (float32 (unitsToBytes u))
            if t >= 1.f then
                units <- u
                s <- t
        units, s

// Given a list a user-files and a UI index, find the file at that index
let userFileByIndex userFilesList index =
    let rec creepUsers i = function
    | [] -> raise (System.Exception "Index not within range")
    | peer::tail ->
        let rec creepFiles i = function
        | [] -> creepUsers i tail
        | _::tail when i < index -> creepFiles (i+1) tail
        | file::_ when i = index -> peer.username, file
        | _ -> raise (System.Exception "Great Scott, we over shot!")
        creepFiles i (List.rev peer.files)
    creepUsers 0 (List.rev userFilesList)

// Create a new snackbar state
let createSnackbarInfo lifetime message =
    {lifetime = new System.Timers.Timer (lifetime, AutoReset = false, Enabled = false); message = message}

// Given a layout with contents, and an snackbar description option, create a snackbar compatible page
let createPageWithLayoutSnackbarOption snackbarOption (contentLayout: ViewElement) dispatch =
    View.ContentPage (
        content = View.AbsoluteLayout(
            children = [
                (contentLayout.LayoutFlags AbsoluteLayoutFlags.All).LayoutBounds (Rectangle (0., 0., 1., 1.))
                match snackbarOption with
                | Some snackbar ->
                    if not snackbar.lifetime.Enabled then
                        snackbar.lifetime.Elapsed.Add (fun _ -> dispatch ClearSnackbar)
                        snackbar.lifetime.Enabled <- true
                    let label =
                        View.Label (
                            text = snackbar.message,
                            padding = Thickness.op_Implicit 4.,
                            fontSize = FontSize.fromNamedSize NamedSize.Small,
                            verticalOptions = LayoutOptions.Fill,
                            verticalTextAlignment = TextAlignment.Center,
                            horizontalOptions = LayoutOptions.Fill,
                            backgroundColor = Color.FromRgb (0x28, 0x28, 0x28),
                            textColor = Color.White)
                    (label.LayoutFlags (AbsoluteLayoutFlags.YProportional + AbsoluteLayoutFlags.WidthProportional)).LayoutBounds (Rectangle (0., 1., 1., 52.))
                | None -> ()]))
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

module Types

open Soulseek

// Interface to store Platform-specific features
type IEnvironment =
    abstract getSharedDownloads: unit -> string
    abstract requestExternalPermissions: unit -> unit

// Create custom SoulseekClient type for easily reconnecting to a server
type FunctionalClient (options, address: string, port: int, username: string, password: string) =
    inherit SoulseekClient (options)
    member self.ConnectAsync () =
        self.ConnectAsync (address, port, username, password) |> Async.AwaitTask
    member self.EnsureConnectedAsync () = async {
        if not (self.State.HasFlag SoulseekClientStates.LoggedIn) then
            do! self.ConnectAsync ()
    }

type LoginStatus =
| Offline
| Pending
| LoginFail

type LoginInfo = {
    address: string
    user: string
    password: string}

type UserFiles = {
    username: string
    files: File list}

type SnackbarInfo = {
    lifetime: System.Timers.Timer
    message: string}

type LoginModel = {
    loginInfo: LoginInfo
    loginStatus: LoginStatus
    snackbar: SnackbarInfo option}

type Progress = {
    downloadedFraction: float
    cancellationToken: System.Threading.CancellationTokenSource}

type DownloadModel = {
    client: FunctionalClient
    progress: Progress option
    peer: string
    selectedFile: File}

type FileSearchModel = {
    client: FunctionalClient
    download: DownloadModel option
    searching: bool
    searchPattern: string
    snackbar: SnackbarInfo option
    userFileMatches: UserFiles list}

type Model =
| Login of LoginModel
| FileSearch of FileSearchModel

type Msg = 
    | LoginAttemptStart
    | LoginAttemptResult of Result<FunctionalClient, string>
    | CompletedAddress of string
    | CompletedUser of string
    | CompletedPassword of string
    | CompletedSearchPattern of string
    | CreateSnackbar of SnackbarInfo
    | ClearSnackbar
    | SearchPatternStart of (Msg -> unit)
    | SearchPatternUserFiles of UserFiles
    | SearchPatternFinish of Result<unit, string>
    | SelectedFileIndexOption of int option
    | DownloadDismissUi
    | DownloadStart of string * (Msg -> unit)
    | DownloadUpdateStatus of float
    | DownloadFinish of Result<string, string>

type BytesUnits =
| Bytes = 1
| KiloBytes = 10
| MegaBytes = 20
| GigaBytes = 30
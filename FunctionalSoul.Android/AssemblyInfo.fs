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

open System.Reflection
open System.Runtime.InteropServices
open Android.App

// the name of the type here needs to match the name inside the ResourceDesigner attribute
type Resources = FunctionalSoul.Android.Resource
[<assembly: Android.Runtime.ResourceDesigner("FunctionalSoul.Android.Resources", IsApplication=true)>]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[<assembly: AssemblyTitle("FunctionalSoul.Android")>]
[<assembly: AssemblyDescription("")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("")>]
[<assembly: AssemblyProduct("FunctionalSoul.Android")>]
[<assembly: AssemblyCopyright("Copyright ©  2014")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]
[<assembly: ComVisible(false)>]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [<assembly: AssemblyVersion("1.0.*")>]
[<assembly: AssemblyVersion("0.0.0.2")>]
[<assembly: AssemblyFileVersion("0.0.0.2")>]

// Add some common permissions, these can be removed if not needed
[<assembly: UsesPermission(Android.Manifest.Permission.Internet)>]
[<assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)>]
do ()

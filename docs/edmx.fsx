(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../bin"

(**
The EdmxFile Type Provider (FSharp.Data.TypeProviders)
========================

Please see [Archived Walkthrough: Generating F# Types from an EDMX Schema File](https://web.archive.org/web/20140704050638/https://msdn.microsoft.com/en-us/library/hh361038.aspx)

> NOTE: Use ``FSharp.Data.TypeProviders`` instead of ``Microsoft.FSharp.Data.TypeProviders`` 

Reference 
---------

Please see the [Archived MSDN Documentation](https://web.archive.org/web/20150702053725/https://msdn.microsoft.com/en-us/library/hh362313.aspx)

Example
---------

See below for a micro use of the type provider:
*)
#r "System.Data.Entity.dll"
#r "nuget: FSharp.Data.TypeProviders"
open FSharp.Data.TypeProviders

type internal Edmx1 = EdmxFile< "SampleModel01.edmx">

let internal container = new Edmx1.SampleModel01.SampleModel01Container()

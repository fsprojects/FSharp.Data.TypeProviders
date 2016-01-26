(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
The EdmxFile Type Provider (FSharp.Data.TypeProviders)
========================

Please see [Walkthrough: Generating F# Types from an EDMX Schema File](https://msdn.microsoft.com/en-us/library/hh361038.aspx)

> NOTE: Use ``FSharp.Data.TypeProviders`` instead of ``Microsoft.FSharp.Data.TypeProviders`` 

Reference 
---------

Please see the [MSDN Documentation](https://msdn.microsoft.com/en-us/library/hh362324.aspx)

Example 
---------

See below for an example use of the type provider:
*)
#r "System.Data.Entity.dll"
#r "FSharp.Data.TypeProviders.dll"
open FSharp.Data.TypeProviders

type internal Edmx1 = EdmxFile< "SampleModel01.edmx">

let internal container = new Edmx1.SampleModel01.SampleModel01Container()

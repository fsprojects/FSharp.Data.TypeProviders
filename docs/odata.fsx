(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../bin"

(**
The ODataService Type Provider (FSharp.Data.TypeProviders)
========================


Please see [Archived Walkthrough: Accessing an OData Service by Using Type Providers](https://web.archive.org/web/20140704061901/https://msdn.microsoft.com/en-us/library/hh156504.aspx)

> NOTE: Use ``FSharp.Data.TypeProviders`` instead of ``Microsoft.FSharp.Data.TypeProviders`` 

Reference 
---------

Please see the [Archived MSDN Documentation](https://web.archive.org/web/20140301090938/https://msdn.microsoft.com/en-us/library/hh362325.aspx)

Example
---------

See below for a micro use of the type provider:

*)
#r "nuget: FSharp.Data.TypeProviders"
open FSharp.Data.TypeProviders

type OData1= ODataService< @"http://services.odata.org/V2/OData/OData.svc/">

type ST = OData1.ServiceTypes
type Address = OData1.ServiceTypes.Address
module M = 
    let ctx = OData1.GetDataContext()

(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
The ODataService Type Provider (FSharp.Data.TypeProviders)
========================


Please see [Walkthrough: Accessing an OData Service by Using Type Providers](https://msdn.microsoft.com/en-us/library/hh156504.aspx)

> NOTE: Use ``FSharp.Data.TypeProviders`` instead of ``Microsoft.FSharp.Data.TypeProviders`` 

Reference 
---------

Please see the [MSDN Documentation](https://msdn.microsoft.com/en-us/library/hh362324.aspx)

Example
-------

Please see [Walkthrough: Accessing an OData Service by Using Type Providers](https://msdn.microsoft.com/en-us/library/hh156504.aspx)

See also below for a micro use of the type provider:

*)
#r "FSharp.Data.TypeProviders.dll"
open FSharp.Data.TypeProviders

type OData1= ODataService< @"http://services.odata.org/V2/OData/OData.svc/">

type ST = OData1.ServiceTypes
type Address = OData1.ServiceTypes.Address
module M = 
    let ctx = OData1.GetDataContext()

// #Conformance #TypeProviders #ODataService

#if COMPILED
module FSharp.Data.TypeProviders.UsageTests.OdataServiceTests
#else
#r "../../bin/FSharp.Data.TypeProviders.dll"
#endif

open FSharp.Data.TypeProviders

type OData1= ODataService< @"http://services.odata.org/V2/OData/OData.svc/">

type ST = OData1.ServiceTypes
type Address = OData1.ServiceTypes.Address
module M = 
    let ctx = OData1.GetDataContext()

(*
type OData2 = Microsoft.FSharp.Data.TypeProviders.ODataService< @"http://services.odata.org/V2/OData/OData.svc/">

module M2 = 
    let ctx = OData2.GetDataContext()
*)


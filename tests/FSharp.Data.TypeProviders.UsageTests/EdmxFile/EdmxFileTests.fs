// #Conformance #TypeProviders #EdmxFile
#if COMPILED
module FSharp.Data.TypeProviders.UsageTests.EdmxFile
#else
#r "../../bin/FSharp.Data.TypeProviders.dll"
#endif


open FSharp.Data.TypeProviders

type internal Edmx1 = EdmxFile< @"EdmxFile\EdmxFiles\SampleModel01.edmx">

type Customers = Edmx1.SampleModel01.Customers
type Container = Edmx1.SampleModel01.SampleModel01Container
module M = 
    let internal c = new Edmx1.SampleModel01.SampleModel01Container()

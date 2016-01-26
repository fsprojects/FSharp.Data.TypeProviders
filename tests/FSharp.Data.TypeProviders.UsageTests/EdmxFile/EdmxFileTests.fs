// #Conformance #TypeProviders #EdmxFile
#if COMPILED
module FSharp.Data.TypeProviders.UsageTests.EdmxFile
#else
#r "../../bin/FSharp.Data.TypeProviders.dll"
#endif


open FSharp.Data.TypeProviders

type internal Edmx1 = EdmxFile< @"EdmxFile\EdmxFiles\SampleModel01.edmx">

let internal container = new Edmx1.SampleModel01.SampleModel01Container()

// #Conformance #TypeProviders #SqlEntityConnection
#if COMPILED
module FSharp.Data.TypeProviders.UsageTests.SqlEntityConnectionTests
#else
#r "FSharp.Data.TypeProviders.dll"
#r "System.Management.dll"
#endif


open FSharp.Data.TypeProviders

//let [<Literal>] SERVER = @".\SQLEXPRESS"
let [<Literal>] SERVER2 = @"(localdb)\MSSQLLocalDB"
let [<Literal>] CONN = "AttachDBFileName = '" + __SOURCE_DIRECTORY__ + @"\DB\NORTHWND.MDF';Server='" + SERVER2 + "'"

type internal SqlEntity1 = SqlEntityConnection< CONN>

let internal ctxt = SqlEntity1.GetDataContext()

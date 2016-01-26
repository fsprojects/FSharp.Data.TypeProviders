// #Conformance #TypeProviders #SqlDataConnection

#if COMPILED
module FSharp.Data.TypeProviders.UsageTests.SqlDataConnectionTests
#else
#r "FSharp.Data.TypeProviders.dll"
#r "System.Management.dll"
#endif


open FSharp.Data.TypeProviders

//let [<Literal>] SERVER = @".\SQLEXPRESS"
let [<Literal>] SERVER2 = @"(localdb)\MSSQLLocalDB"
let [<Literal>] CONN = "AttachDBFileName = '" + __SOURCE_DIRECTORY__ + @"\DB\NORTHWND.MDF';Server='" + SERVER2 + "'"

type SqlData1 = SqlDataConnection< CONN>

let ctxt = SqlData1.GetDataContext()

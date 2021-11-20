(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
The SqlDataConnection Type Provider (FSharp.Data.TypeProviders)
========================

Please see [Walkthrough: Accessing a SQL Database by Using Type Providers](https://msdn.microsoft.com/en-us/library/hh361033.aspx)

> NOTE: Use ``FSharp.Data.TypeProviders`` instead of ``Microsoft.FSharp.Data.TypeProviders`` 

Reference 
---------

Please see the [MSDN Documentation](https://msdn.microsoft.com/en-us/library/hh362324.aspx)

Example
------

Please see [Walkthrough: Accessing a SQL Database by Using Type Providers](https://msdn.microsoft.com/en-us/library/hh361033.aspx)

See also below for a micro example use of the type provider:

*)
#r "System.Data.Linq.dll"
#r "FSharp.Data.TypeProviders.dll"
open FSharp.Data.TypeProviders

// The database connection string
let [<Literal>] CONN = 
    @"AttachDBFileName='C:\DB\NORTHWND.MDF';Server='(localdb)\MSSQLLocalDB'"

// Connect to the database at compile-time
type SqlData1 = SqlDataConnection<CONN>

// Connect to the database at runtime
let ctxt = SqlData1.GetDataContext()

// Execute a query
let categories = 
    query { for d in ctxt.Categories do
            where (d.CategoryID > 10)
            select (d.CategoryName, d.Description) } 

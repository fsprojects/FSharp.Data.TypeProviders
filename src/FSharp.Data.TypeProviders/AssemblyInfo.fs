namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Data.TypeProviders")>]
[<assembly: AssemblyProductAttribute("FSharp.Data.TypeProviders")>]
[<assembly: AssemblyDescriptionAttribute("F# Type Providers using .NET Framework generators, was FSharp.Data.TypeProviders.dll")>]
[<assembly: AssemblyVersionAttribute("5.0.0.0")>]
[<assembly: AssemblyFileVersionAttribute("5.0.0.0")>]
[<assembly:System.Runtime.CompilerServices.InternalsVisibleTo("FSharp.Data.TypeProviders.Tests")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "5.0.0.0"

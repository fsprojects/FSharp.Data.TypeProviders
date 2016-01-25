namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Data.FxTypeProviders")>]
[<assembly: AssemblyProductAttribute("FSharp.Data.FxTypeProviders")>]
[<assembly: AssemblyDescriptionAttribute("F# Type Providers using .NET Framework generators, was FSharp.Data.TypeProviders.dll")>]
[<assembly: AssemblyVersionAttribute("5.0.0.0")>]
[<assembly: AssemblyFileVersionAttribute("5.0.0.0")>]
[<assembly:System.Runtime.CompilerServices.InternalsVisibleTo("FSharp.Data.FxTypeProviders.Tests")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "5.0.0.0"


# Some old F# Type Providers

This repository contains the source for the Old F# type providers called SqlDataConnection, SqlEntityConnection, ODataService, WsdlService and EdmxFile.
Implemented using .NET Framework code generators. These may only be used within .NET Framework projects, and probably require the .NET Framework F# compiler, which is now only available as part of Visual Studio, and is not available in the .NET SDK.


For up-to-date type providers see https://fsharp.org/guides/data-access/ and search on Google.

For WSDL see https://github.com/fsprojects/FSharp.Data.WsdlProvider

For modern WebAPIs see [FSharp.Data](https://fsprojects.github.io/FSharp.Data/) and also [SwaggerProvider](https://fsprojects.github.io/SwaggerProvider/)


# Docs

The doc build is no longer fully working because `fsdocs` processes scripts as .NET Core, yet the scripts must execute using .NET Framework.

## Maintainer(s)

- [@dsyme](https://github.com/dsyme)

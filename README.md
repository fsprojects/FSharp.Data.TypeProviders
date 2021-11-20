
# Some old F# Type Providers

This repository contains the source for the Old F# type providers called SqlDataConnection, SqlEntityConnection, ODataService, WsdlService and EdmxFile.

**These are considered legacy.** They are implemented using old .NET Framework code generators (~2006 technology) which haven't been updated for some time. These may only be used within .NET Framework projects, and require the .NET Framework F# compiler, which is now only available as part of Visual Studio and its Build TOols, and is not available in the .NET SDK.  If using these type providers with F# scripting inside VIsual Studio you should also enable .NET Framework scripting by default in the F# options.

**If you are using these type providers you could consider moving to a different data access technique.**

For up-to-date type providers see https://fsharp.org/guides/data-access/ and search on Google.

For WSDL see https://github.com/fsprojects/FSharp.Data.WsdlProvider

For modern WebAPIs see [FSharp.Data](https://fsprojects.github.io/FSharp.Data/) and also [SwaggerProvider](https://fsprojects.github.io/SwaggerProvider/)


# Docs

The doc build is no longer fully working because `fsdocs` processes scripts as .NET Core, yet the scripts must execute using .NET Framework.

## Maintainer(s)

- [@dsyme](https://github.com/dsyme)

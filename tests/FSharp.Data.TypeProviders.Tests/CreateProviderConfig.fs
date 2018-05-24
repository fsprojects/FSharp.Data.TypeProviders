// --------------------------------------------------------------------------------------
// Helpers for writing type providers
// ----------------------------------------------------------------------------------------------

namespace ProviderImplementation.ProvidedTypesTesting

open System
open System.Collections.Generic
open System.Reflection
open System.IO
open System.Text
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Core.Printf
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Reflection
open ProviderImplementation.ProvidedTypes

[<AutoOpen>]
module Utils = 
    let isNull x = match x with null -> true | _ -> false


/// Simulate a real host of TypeProviderConfig
type internal DllInfo(path: string) =
    member __.FileName = path

/// Simulate a real host of TypeProviderConfig
type internal TcImports(bas: TcImports option, dllInfos: DllInfo list) =
    member __.Base = bas
    member __.DllInfos = dllInfos


type internal Testing() =

    /// Simulates a real instance of TypeProviderConfig
    static member MakeSimulatedTypeProviderConfig (resolutionFolder: string, runtimeAssembly: string, runtimeAssemblyRefs: string list, ?isHostedExecution, ?isInvalidationSupported) =

        let cfg = TypeProviderConfig(fun _ -> false)
        cfg.IsHostedExecution <- defaultArg isHostedExecution false
        cfg.IsInvalidationSupported <- defaultArg isInvalidationSupported true
        let (?<-) cfg prop value =
            let ty = cfg.GetType()
            match ty.GetProperty(prop,BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic) with
            | null -> 
                let fld = ty.GetField(prop,BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic)
                if fld = null then failwith ("expected TypeProviderConfig to have a property or field "+prop)
                fld.SetValue(cfg, value)|> ignore
            | p -> 
                p.GetSetMethod(nonPublic = true).Invoke(cfg, [| box value |]) |> ignore
        cfg?ResolutionFolder <- resolutionFolder
        cfg?RuntimeAssembly <- runtimeAssembly
        cfg?ReferencedAssemblies <- Array.zeroCreate<string> 0

        // Fake an implementation of SystemRuntimeContainsType the shape expected by AssemblyResolver.fs.
        let dllInfos = [yield DllInfo(runtimeAssembly); for r in runtimeAssemblyRefs do yield DllInfo(r)]
        let tcImports = TcImports(Some(TcImports(None,[])),dllInfos)
        let systemRuntimeContainsType = (fun (_s:string) -> if tcImports.DllInfos.Length = 1 then true else true)
        cfg?systemRuntimeContainsType <- systemRuntimeContainsType

        //Diagnostics.Debugger.Launch() |> ignore
        Diagnostics.Debug.Assert(cfg.GetType().GetField("systemRuntimeContainsType",BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Instance) |> isNull |> not)
        Diagnostics.Debug.Assert(systemRuntimeContainsType.GetType().GetField("tcImports",BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Instance) |> isNull |> not)
        Diagnostics.Debug.Assert(typeof<TcImports>.GetField("dllInfos",BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Instance) |> isNull |> not)
        Diagnostics.Debug.Assert(typeof<TcImports>.GetProperty("Base",BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Instance) |> isNull |> not)
        Diagnostics.Debug.Assert(typeof<DllInfo>.GetProperty("FileName",BindingFlags.NonPublic ||| BindingFlags.Public ||| BindingFlags.Instance) |> isNull |> not)

        cfg



module internal Targets =

    let private (++) a b = System.IO.Path.Combine(a,b)

    let runningOnMono = Type.GetType("Mono.Runtime") |> isNull |> not
    let runningOnMac =
        (Environment.OSVersion.Platform = PlatformID.MacOSX)
        || (Environment.OSVersion.Platform = PlatformID.Unix) && Directory.Exists("/Applications") && Directory.Exists("/System") && Directory.Exists("/Users") && Directory.Exists("/Volumes")
    let runningOnLinux =
        (Environment.OSVersion.Platform = PlatformID.Unix) && not runningOnMac

    // Assumes OSX
    let monoRoot =
        Path.GetFullPath(Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),".."))
        //match System.Environment.OSVersion.Platform with
        //| System.PlatformID.MacOSX -> "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono"
        //| System.PlatformID.MacOSX -> "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono"
        //| _ ->

    let referenceAssembliesPath() =
            (if runningOnMono then monoRoot else Environment.GetFolderPath Environment.SpecialFolder.ProgramFilesX86)
            ++ "Reference Assemblies"
            ++ "Microsoft"

    let packagesDirectory() = 
        // this takes into account both linux-on-windows (can't use __SOURCE_DIRECTORY__) and shadow copying (can't use .Location)
        let root = Path.GetDirectoryName(Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath)
        let rec loop dir = 
             //printfn "looking for references in %s" dir
             if Directory.Exists(dir ++ "packages" ) then 
                 dir ++ "packages" 
             else
                 let parent = Path.GetDirectoryName(dir)
                 match parent with
                 | null | "" -> failwith ("couldn't find packages directory anywhere above  " + root)
                 | _ ->  loop parent
                 
        loop  root

    let paketPackagesGroupDirectory paketGroup = 
        let pd = packagesDirectory()
        if Directory.Exists (pd ++ paketGroup) then 
            Some (pd ++ paketGroup)
        else 
            None

    let private fsharpCoreFromInstalledAssemblies fsharp profile =
         match fsharp, profile with
         | "3.1", "net45" -> 
                Some (if runningOnMono then monoRoot ++ "gac" ++ "FSharp.Core" ++ "4.3.1.0__b03f5f7f11d50a3a" ++ "FSharp.Core.dll" 
                      else referenceAssembliesPath() ++ "FSharp" ++ ".NETFramework" ++ "v4.0" ++ "4.3.1.0" ++ "FSharp.Core.dll")
         | "4.0", "net45" -> 
                Some (if runningOnMono then monoRoot ++ "gac" ++ "FSharp.Core" ++ "4.4.0.0__b03f5f7f11d50a3a" ++ "FSharp.Core.dll"
                      else referenceAssembliesPath() ++ "FSharp" ++ ".NETFramework" ++ "v4.0" ++ "4.4.0.0" ++ "FSharp.Core.dll")
         | "4.1", "net45" -> 
                Some (if runningOnMono then monoRoot ++ "gac" ++ "FSharp.Core" ++ "4.4.1.0__b03f5f7f11d50a3a" ++ "FSharp.Core.dll"
                      else referenceAssembliesPath() ++ "FSharp" ++ ".NETFramework" ++ "v4.0" ++ "4.4.0.1" ++ "FSharp.Core.dll")
         | "3.1", "portable47" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETPortable" ++ "2.3.5.1" ++ "FSharp.Core.dll" |> Some
         | "3.1", "portable7" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETCore" ++ "3.3.1.0" ++ "FSharp.Core.dll" |> Some
         | "3.1", "portable78" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETCore" ++ "3.78.3.1" ++ "FSharp.Core.dll" |> Some
         | "3.1", "portable259" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETCore" ++ "3.259.3.1" ++ "FSharp.Core.dll" |> Some
         | "4.0", "portable47" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETPortable" ++ "3.47.4.0" ++ "FSharp.Core.dll" |> Some
         | "4.0", "portable7" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETCore" ++ "3.7.4.0" ++ "FSharp.Core.dll" |> Some
         | "4.0", "portable78" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETCore" ++ "3.78.4.0" ++ "FSharp.Core.dll" |> Some
         | "4.0", "portable259" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETCore" ++ "3.259.4.0" ++ "FSharp.Core.dll" |> Some
         | "4.1", "portable47" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETPortable" ++ "3.47.4.1" ++ "FSharp.Core.dll" |> Some
         | "4.1", "portable7" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETCore" ++ "3.7.4.1" ++ "FSharp.Core.dll" |> Some
         | "4.1", "portable78" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETCore" ++ "3.78.4.1" ++ "FSharp.Core.dll" |> Some
         | "4.1", "portable259" -> referenceAssembliesPath() ++ "FSharp" ++ ".NETCore" ++ "3.259.4.1" ++ "FSharp.Core.dll" |> Some
         | "4.1", "netstandard1.6" -> None
         | "4.1", "netstandard2.0" -> None
         | "4.1", "netcoreapp2.0" -> None
         | _ -> failwith (sprintf "unimplemented  profile, fsharpVersion = %s, profile = %s" fsharp profile)


    let paketPackageFromMainPaketGroup packageName = 
        let pd = packagesDirectory()
        if Directory.Exists (pd ++ packageName) then 
            pd ++ packageName
        else 
            failwithf "couldn't find %s/NETStandard.Library, whcih is needed for testing .NET Standard 2.0 code generation of a type provider using these utilities" pd

    /// Compute a path to an FSharp.Core suitable for the target profile
    let private fsharpRestoredAssembliesPath fsharp profile =
        let paketGroup =
           match fsharp with
           | "3.1" -> "fs31"
           | "4.0" -> "fs40"
           | "4.1" -> "fs41"
           | _ -> failwith ("unimplemented F# version" + fsharp)
        let compatProfiles =
            match profile with
            | "net45"    -> ["net45";"net40" ]
            | "netstandard1.6"    -> [ "netstandard1.6" ]
            | "netstandard2.0"    -> [ "netstandard2.0"; "netstandard1.6" ]
            | "netcoreapp2.0"    -> [ "netcoreapp2.0"; "netstandard2.0"; "netstandard1.6" ]
            | "portable47"    -> ["portable-net45+sl5+netcore45"]
            | "portable7"     -> ["portable-net45+netcore45"]
            | "portable78"    -> ["portable-net45+netcore45+wp8"]
            | "portable259"   -> ["portable-net45+netcore45+wpa81+wp8"]
            | _ -> failwith "unimplemented portable profile"
        let groupDirectory = 
            match paketPackagesGroupDirectory paketGroup with 
            | None -> 
                 printfn "couldn't find paket packages group %s.  Assuming %s/FSharp.Core is for F# version %s" paketGroup (packagesDirectory()) fsharp
                 packagesDirectory()
            | Some dir -> dir
                
        compatProfiles |> List.tryPick (fun profileFolder -> 
            let file = groupDirectory ++ "FSharp.Core" ++ "lib" ++ profileFolder ++ "FSharp.Core.dll"
            if File.Exists file then Some file else None
        ) |> function 
             | None -> groupDirectory ++ "no.compat.FSharp.Core.dll.found.under.here"
             | Some res -> res
        

    let sysInstalledAssembliesPath profile =
        let portableRoot = if runningOnMono then monoRoot ++ "xbuild-frameworks" else referenceAssembliesPath() ++ "Framework"
        match profile with
        | "net45"->
            if runningOnMono then monoRoot ++ "4.5"
            else referenceAssembliesPath() ++ "Framework" ++ ".NETFramework" ++ "v4.5"
        | "netstandard2.0"->
            let packageDir = paketPackageFromMainPaketGroup "NETStandard.Library" 
            packageDir ++ "build" ++ "netstandard2.0" ++ "ref"
        | "netcoreapp2.0"->
            let packageDir = paketPackageFromMainPaketGroup "Microsoft.NETCore.App" 
            packageDir ++ "ref" ++ "netcoreapp2.0"
        | "portable47" -> portableRoot ++ ".NETPortable" ++ "v4.0" ++ "Profile" ++ "Profile47"
        | "portable7" -> portableRoot ++ ".NETPortable" ++ "v4.5" ++ "Profile" ++ "Profile7"
        | "portable78" -> portableRoot ++ ".NETPortable" ++ "v4.5" ++ "Profile" ++ "Profile78"
        | "portable259" -> portableRoot ++ ".NETPortable" ++ "v4.5" ++ "Profile" ++ "Profile259"
        | _ -> failwith (sprintf "unimplemented profile '%s'" profile)

    let FSharpCoreRef fsharp profile = 
        let installedFSharpCore = fsharpCoreFromInstalledAssemblies fsharp profile
        match installedFSharpCore with 
        | Some path when File.Exists path -> path
        | _ -> 
        let restoredFSharpCore  = fsharpRestoredAssembliesPath fsharp profile
        match restoredFSharpCore  with 
        | path when File.Exists(restoredFSharpCore) -> path
        | _ -> 
        match installedFSharpCore with 
        | Some path -> failwith ("couldn't find FSharp.Core.dll at either '" + path + "' or '" + restoredFSharpCore + "'")
        | None  -> failwith ("couldn't find FSharp.Core.dll at '" + restoredFSharpCore + "'")

    let FSharpRefs fsharp profile =
        [ match profile with
          | "portable7" | "portable78" | "portable259" ->
              let sysPath = sysInstalledAssembliesPath profile
              for asm in [ "System.Runtime"; "mscorlib"; "System.Collections"; "System.Core"; "System"; "System.Globalization"; "System.IO"; "System.Linq"; "System.Linq.Expressions";
                           "System.Linq.Queryable"; "System.Net"; "System.Net.NetworkInformation"; "System.Net.Primitives"; "System.Net.Requests"; "System.ObjectModel"; "System.Reflection";
                           "System.Reflection.Extensions"; "System.Reflection.Primitives"; "System.Resources.ResourceManager"; "System.Runtime.Extensions";
                           "System.Runtime.InteropServices.WindowsRuntime"; "System.Runtime.Serialization"; "System.Threading"; "System.Threading.Tasks"; "System.Xml"; "System.Xml.Linq"; "System.Xml.XDocument";
                           "System.Runtime.Serialization.Json"; "System.Runtime.Serialization.Primitives"; "System.Windows" ] do
                  yield sysPath ++ asm + ".dll"
          | "portable47" -> 
              let sysPath = sysInstalledAssembliesPath profile
              yield sysPath ++ "mscorlib.dll"
              yield sysPath ++ "System.Xml.Linq.dll"
          | "net45" ->
              // See typical command line in https://github.com/fsprojects/FSharp.TypeProviders.SDK/issues/190#issuecomment-356564344
              // This is just a subset
              let sysPath = sysInstalledAssembliesPath profile
              yield sysPath ++ "mscorlib.dll"
              yield sysPath ++ "System.Numerics.dll" 
              yield sysPath ++ "System.Xml.dll" 
              yield sysPath ++ "System.Core.dll"
              yield sysPath ++ "System.Xml.Linq.dll"
              yield sysPath ++ "System.dll" 
          | "netstandard2.0" ->
             // See typical command line in https://github.com/fsprojects/FSharp.TypeProviders.SDK/issues/190#issuecomment-356564344
             let sysPath = sysInstalledAssembliesPath profile
             yield sysPath ++ "Microsoft.Win32.Primitives.dll"
             yield sysPath ++ "mscorlib.dll"
             yield sysPath ++ "netstandard.dll"
             yield sysPath ++ "System.AppContext.dll"
             yield sysPath ++ "System.Collections.Concurrent.dll"
             yield sysPath ++ "System.Collections.dll"
             yield sysPath ++ "System.Collections.NonGeneric.dll"
             yield sysPath ++ "System.Collections.Specialized.dll"
             yield sysPath ++ "System.ComponentModel.Composition.dll"
             yield sysPath ++ "System.ComponentModel.dll"
             yield sysPath ++ "System.ComponentModel.EventBasedAsync.dll"
             yield sysPath ++ "System.ComponentModel.Primitives.dll"
             yield sysPath ++ "System.ComponentModel.TypeConverter.dll"
             yield sysPath ++ "System.Console.dll"
             yield sysPath ++ "System.Core.dll"
             yield sysPath ++ "System.Data.Common.dll"
             yield sysPath ++ "System.Data.dll"
             yield sysPath ++ "System.Diagnostics.Contracts.dll"
             yield sysPath ++ "System.Diagnostics.Debug.dll"
             yield sysPath ++ "System.Diagnostics.FileVersionInfo.dll"
             yield sysPath ++ "System.Diagnostics.Process.dll"
             yield sysPath ++ "System.Diagnostics.StackTrace.dll"
             yield sysPath ++ "System.Diagnostics.TextWriterTraceListener.dll"
             yield sysPath ++ "System.Diagnostics.Tools.dll"
             yield sysPath ++ "System.Diagnostics.TraceSource.dll"
             yield sysPath ++ "System.Diagnostics.Tracing.dll"
             yield sysPath ++ "System.dll"
             yield sysPath ++ "System.Drawing.dll"
             yield sysPath ++ "System.Drawing.Primitives.dll"
             yield sysPath ++ "System.Dynamic.Runtime.dll"
             yield sysPath ++ "System.Globalization.Calendars.dll"
             yield sysPath ++ "System.Globalization.dll"
             yield sysPath ++ "System.Globalization.Extensions.dll"
             yield sysPath ++ "System.IO.Compression.dll"
             yield sysPath ++ "System.IO.Compression.FileSystem.dll"
             yield sysPath ++ "System.IO.Compression.ZipFile.dll"
             yield sysPath ++ "System.IO.dll"
             yield sysPath ++ "System.IO.FileSystem.dll"
             yield sysPath ++ "System.IO.FileSystem.DriveInfo.dll"
             yield sysPath ++ "System.IO.FileSystem.Primitives.dll"
             yield sysPath ++ "System.IO.FileSystem.Watcher.dll"
             yield sysPath ++ "System.IO.IsolatedStorage.dll"
             yield sysPath ++ "System.IO.MemoryMappedFiles.dll"
             yield sysPath ++ "System.IO.Pipes.dll"
             yield sysPath ++ "System.IO.UnmanagedMemoryStream.dll"
             yield sysPath ++ "System.Linq.dll"
             yield sysPath ++ "System.Linq.Expressions.dll"
             yield sysPath ++ "System.Linq.Parallel.dll"
             yield sysPath ++ "System.Linq.Queryable.dll"
             yield sysPath ++ "System.Net.dll"
             yield sysPath ++ "System.Net.Http.dll"
             yield sysPath ++ "System.Net.NameResolution.dll"
             yield sysPath ++ "System.Net.NetworkInformation.dll"
             yield sysPath ++ "System.Net.Ping.dll"
             yield sysPath ++ "System.Net.Primitives.dll"
             yield sysPath ++ "System.Net.Requests.dll"
             yield sysPath ++ "System.Net.Security.dll"
             yield sysPath ++ "System.Net.Sockets.dll"
             yield sysPath ++ "System.Net.WebHeaderCollection.dll"
             yield sysPath ++ "System.Net.WebSockets.Client.dll"
             yield sysPath ++ "System.Net.WebSockets.dll"
             yield sysPath ++ "System.Numerics.dll"
             yield sysPath ++ "System.ObjectModel.dll"
             yield sysPath ++ "System.Reflection.dll"
             yield sysPath ++ "System.Reflection.Extensions.dll"
             yield sysPath ++ "System.Reflection.Primitives.dll"
             yield sysPath ++ "System.Resources.Reader.dll"
             yield sysPath ++ "System.Resources.ResourceManager.dll"
             yield sysPath ++ "System.Resources.Writer.dll"
             yield sysPath ++ "System.Runtime.CompilerServices.VisualC.dll"
             yield sysPath ++ "System.Runtime.dll"
             yield sysPath ++ "System.Runtime.Extensions.dll"
             yield sysPath ++ "System.Runtime.Handles.dll"
             yield sysPath ++ "System.Runtime.InteropServices.dll"
             yield sysPath ++ "System.Runtime.InteropServices.RuntimeInformation.dll"
             yield sysPath ++ "System.Runtime.Numerics.dll"
             yield sysPath ++ "System.Runtime.Serialization.dll"
             yield sysPath ++ "System.Runtime.Serialization.Formatters.dll"
             yield sysPath ++ "System.Runtime.Serialization.Json.dll"
             yield sysPath ++ "System.Runtime.Serialization.Primitives.dll"
             yield sysPath ++ "System.Runtime.Serialization.Xml.dll"
             yield sysPath ++ "System.Security.Claims.dll"
             yield sysPath ++ "System.Security.Cryptography.Algorithms.dll"
             yield sysPath ++ "System.Security.Cryptography.Csp.dll"
             yield sysPath ++ "System.Security.Cryptography.Encoding.dll"
             yield sysPath ++ "System.Security.Cryptography.Primitives.dll"
             yield sysPath ++ "System.Security.Cryptography.X509Certificates.dll"
             yield sysPath ++ "System.Security.Principal.dll"
             yield sysPath ++ "System.Security.SecureString.dll"
             yield sysPath ++ "System.ServiceModel.Web.dll"
             yield sysPath ++ "System.Text.Encoding.dll"
             yield sysPath ++ "System.Text.Encoding.Extensions.dll"
             yield sysPath ++ "System.Text.RegularExpressions.dll"
             yield sysPath ++ "System.Threading.dll"
             yield sysPath ++ "System.Threading.Overlapped.dll"
             yield sysPath ++ "System.Threading.Tasks.dll"
             yield sysPath ++ "System.Threading.Tasks.Parallel.dll"
             yield sysPath ++ "System.Threading.Thread.dll"
             yield sysPath ++ "System.Threading.ThreadPool.dll"
             yield sysPath ++ "System.Threading.Timer.dll"
             yield sysPath ++ "System.Transactions.dll"
             yield sysPath ++ "System.ValueTuple.dll"
             yield sysPath ++ "System.Web.dll"
             yield sysPath ++ "System.Windows.dll"
             yield sysPath ++ "System.Xml.dll"
             yield sysPath ++ "System.Xml.Linq.dll"
             yield sysPath ++ "System.Xml.ReaderWriter.dll"
             yield sysPath ++ "System.Xml.Serialization.dll"
             yield sysPath ++ "System.Xml.XDocument.dll"
             yield sysPath ++ "System.Xml.XmlDocument.dll"
             yield sysPath ++ "System.Xml.XmlSerializer.dll"
             yield sysPath ++ "System.Xml.XPath.dll"
             yield sysPath ++ "System.Xml.XPath.XDocument.dll"
          | "netcoreapp2.0" ->
             // See typical command line in https://github.com/fsprojects/FSharp.TypeProviders.SDK/issues/190#issuecomment-356564344
             let sysPath = sysInstalledAssembliesPath profile
             yield sysPath ++ "Microsoft.CSharp.dll"
             yield sysPath ++ "Microsoft.VisualBasic.dll"
             yield sysPath ++ "Microsoft.Win32.Primitives.dll"
             yield sysPath ++ "mscorlib.dll"
             yield sysPath ++ "netstandard.dll"
             yield sysPath ++ "System.AppContext.dll"
             yield sysPath ++ "System.Buffers.dll"
             yield sysPath ++ "System.Collections.Concurrent.dll"
             yield sysPath ++ "System.Collections.dll"
             yield sysPath ++ "System.Collections.Immutable.dll"
             yield sysPath ++ "System.Collections.NonGeneric.dll"
             yield sysPath ++ "System.Collections.Specialized.dll"
             yield sysPath ++ "System.ComponentModel.Annotations.dll"
             yield sysPath ++ "System.ComponentModel.Composition.dll"
             yield sysPath ++ "System.ComponentModel.DataAnnotations.dll"
             yield sysPath ++ "System.ComponentModel.dll"
             yield sysPath ++ "System.ComponentModel.EventBasedAsync.dll"
             yield sysPath ++ "System.ComponentModel.Primitives.dll"
             yield sysPath ++ "System.ComponentModel.TypeConverter.dll"
             yield sysPath ++ "System.Configuration.dll"
             yield sysPath ++ "System.Console.dll"
             yield sysPath ++ "System.Core.dll"
             yield sysPath ++ "System.Data.Common.dll"
             yield sysPath ++ "System.Data.dll"
             yield sysPath ++ "System.Diagnostics.Contracts.dll"
             yield sysPath ++ "System.Diagnostics.Debug.dll"
             yield sysPath ++ "System.Diagnostics.DiagnosticSource.dll"
             yield sysPath ++ "System.Diagnostics.FileVersionInfo.dll"
             yield sysPath ++ "System.Diagnostics.Process.dll"
             yield sysPath ++ "System.Diagnostics.StackTrace.dll"
             yield sysPath ++ "System.Diagnostics.TextWriterTraceListener.dll"
             yield sysPath ++ "System.Diagnostics.Tools.dll"
             yield sysPath ++ "System.Diagnostics.TraceSource.dll"
             yield sysPath ++ "System.Diagnostics.Tracing.dll"
             yield sysPath ++ "System.dll"
             yield sysPath ++ "System.Drawing.dll"
             yield sysPath ++ "System.Drawing.Primitives.dll"
             yield sysPath ++ "System.Dynamic.Runtime.dll"
             yield sysPath ++ "System.Globalization.Calendars.dll"
             yield sysPath ++ "System.Globalization.dll"
             yield sysPath ++ "System.Globalization.Extensions.dll"
             yield sysPath ++ "System.IO.Compression.dll"
             yield sysPath ++ "System.IO.Compression.FileSystem.dll"
             yield sysPath ++ "System.IO.Compression.ZipFile.dll"
             yield sysPath ++ "System.IO.dll"
             yield sysPath ++ "System.IO.FileSystem.dll"
             yield sysPath ++ "System.IO.FileSystem.DriveInfo.dll"
             yield sysPath ++ "System.IO.FileSystem.Primitives.dll"
             yield sysPath ++ "System.IO.FileSystem.Watcher.dll"
             yield sysPath ++ "System.IO.IsolatedStorage.dll"
             yield sysPath ++ "System.IO.MemoryMappedFiles.dll"
             yield sysPath ++ "System.IO.Pipes.dll"
             yield sysPath ++ "System.IO.UnmanagedMemoryStream.dll"
             yield sysPath ++ "System.Linq.dll"
             yield sysPath ++ "System.Linq.Expressions.dll"
             yield sysPath ++ "System.Linq.Parallel.dll"
             yield sysPath ++ "System.Linq.Queryable.dll"
             yield sysPath ++ "System.Net.dll"
             yield sysPath ++ "System.Net.Http.dll"
             yield sysPath ++ "System.Net.HttpListener.dll"
             yield sysPath ++ "System.Net.Mail.dll"
             yield sysPath ++ "System.Net.NameResolution.dll"
             yield sysPath ++ "System.Net.NetworkInformation.dll"
             yield sysPath ++ "System.Net.Ping.dll"
             yield sysPath ++ "System.Net.Primitives.dll"
             yield sysPath ++ "System.Net.Requests.dll"
             yield sysPath ++ "System.Net.Security.dll"
             yield sysPath ++ "System.Net.ServicePoint.dll"
             yield sysPath ++ "System.Net.Sockets.dll"
             yield sysPath ++ "System.Net.WebClient.dll"
             yield sysPath ++ "System.Net.WebHeaderCollection.dll"
             yield sysPath ++ "System.Net.WebProxy.dll"
             yield sysPath ++ "System.Net.WebSockets.Client.dll"
             yield sysPath ++ "System.Net.WebSockets.dll"
             yield sysPath ++ "System.Numerics.dll"
             yield sysPath ++ "System.Numerics.Vectors.dll"
             yield sysPath ++ "System.ObjectModel.dll"
             yield sysPath ++ "System.Reflection.DispatchProxy.dll"
             yield sysPath ++ "System.Reflection.dll"
             yield sysPath ++ "System.Reflection.Emit.dll"
             yield sysPath ++ "System.Reflection.Emit.ILGeneration.dll"
             yield sysPath ++ "System.Reflection.Emit.Lightweight.dll"
             yield sysPath ++ "System.Reflection.Extensions.dll"
             yield sysPath ++ "System.Reflection.Metadata.dll"
             yield sysPath ++ "System.Reflection.Primitives.dll"
             yield sysPath ++ "System.Reflection.TypeExtensions.dll"
             yield sysPath ++ "System.Resources.Reader.dll"
             yield sysPath ++ "System.Resources.ResourceManager.dll"
             yield sysPath ++ "System.Resources.Writer.dll"
             yield sysPath ++ "System.Runtime.CompilerServices.VisualC.dll"
             yield sysPath ++ "System.Runtime.dll"
             yield sysPath ++ "System.Runtime.Extensions.dll"
             yield sysPath ++ "System.Runtime.Handles.dll"
             yield sysPath ++ "System.Runtime.InteropServices.dll"
             yield sysPath ++ "System.Runtime.InteropServices.RuntimeInformation.dll"
             yield sysPath ++ "System.Runtime.InteropServices.WindowsRuntime.dll"
             yield sysPath ++ "System.Runtime.Loader.dll"
             yield sysPath ++ "System.Runtime.Numerics.dll"
             yield sysPath ++ "System.Runtime.Serialization.dll"
             yield sysPath ++ "System.Runtime.Serialization.Formatters.dll"
             yield sysPath ++ "System.Runtime.Serialization.Json.dll"
             yield sysPath ++ "System.Runtime.Serialization.Primitives.dll"
             yield sysPath ++ "System.Runtime.Serialization.Xml.dll"
             yield sysPath ++ "System.Security.Claims.dll"
             yield sysPath ++ "System.Security.Cryptography.Algorithms.dll"
             yield sysPath ++ "System.Security.Cryptography.Csp.dll"
             yield sysPath ++ "System.Security.Cryptography.Encoding.dll"
             yield sysPath ++ "System.Security.Cryptography.Primitives.dll"
             yield sysPath ++ "System.Security.Cryptography.X509Certificates.dll"
             yield sysPath ++ "System.Security.dll"
             yield sysPath ++ "System.Security.Principal.dll"
             yield sysPath ++ "System.Security.SecureString.dll"
             yield sysPath ++ "System.ServiceModel.Web.dll"
             yield sysPath ++ "System.ServiceProcess.dll"
             yield sysPath ++ "System.Text.Encoding.dll"
             yield sysPath ++ "System.Text.Encoding.Extensions.dll"
             yield sysPath ++ "System.Text.RegularExpressions.dll"
             yield sysPath ++ "System.Threading.dll"
             yield sysPath ++ "System.Threading.Overlapped.dll"
             yield sysPath ++ "System.Threading.Tasks.Dataflow.dll"
             yield sysPath ++ "System.Threading.Tasks.dll"
             yield sysPath ++ "System.Threading.Tasks.Extensions.dll"
             yield sysPath ++ "System.Threading.Tasks.Parallel.dll"
             yield sysPath ++ "System.Threading.Thread.dll"
             yield sysPath ++ "System.Threading.ThreadPool.dll"
             yield sysPath ++ "System.Threading.Timer.dll"
             yield sysPath ++ "System.Transactions.dll"
             yield sysPath ++ "System.Transactions.Local.dll"
             yield sysPath ++ "System.ValueTuple.dll"
             yield sysPath ++ "System.Web.dll"
             yield sysPath ++ "System.Web.HttpUtility.dll"
             yield sysPath ++ "System.Windows.dll"
             yield sysPath ++ "System.Xml.dll"
             yield sysPath ++ "System.Xml.Linq.dll"
             yield sysPath ++ "System.Xml.ReaderWriter.dll"
             yield sysPath ++ "System.Xml.Serialization.dll"
             yield sysPath ++ "System.Xml.XDocument.dll"
             yield sysPath ++ "System.Xml.XmlDocument.dll"
             yield sysPath ++ "System.Xml.XmlSerializer.dll"
             yield sysPath ++ "System.Xml.XPath.dll"
             yield sysPath ++ "System.Xml.XPath.XDocument.dll"
             yield sysPath ++ "WindowsBase.dll"
             yield sysPath ++ "Microsoft.Win32.Primitives.dll"
          | _ -> 
             failwith (sprintf "unimplemented profile, fsharpVersion = %s, profile = %s" fsharp profile)

          yield FSharpCoreRef fsharp profile
        ]
    let FSharpCore31Ref() = FSharpCoreRef "3.1" "net45"
    let DotNet45FSharp31Refs() = FSharpRefs "3.1" "net45"
    let Portable47FSharp31Refs() = FSharpRefs "3.1" "portable47"
    let Portable7FSharp31Refs() = FSharpRefs "3.1" "portable7"
    let Portable78FSharp31Refs() = FSharpRefs "3.1" "portable78"
    let Portable259FSharp31Refs() = FSharpRefs "3.1" "portable259"

    let FSharpCore40Ref() = FSharpCoreRef "4.0" "net45"
    let DotNet45FSharp40Refs() = FSharpRefs "4.0" "net45"
    let Portable7FSharp40Refs() = FSharpRefs "4.0" "portable7"
    let Portable78FSharp40Refs() = FSharpRefs "4.0" "portable78"
    let Portable259FSharp40Refs() = FSharpRefs "4.0" "portable259"
    let DotNet45Ref r = sysInstalledAssembliesPath "net45" ++ r

    let FSharpCore41Ref() = FSharpCoreRef "4.1" "net45"
    let DotNet45FSharp41Refs() = FSharpRefs "4.1" "net45"
    let Portable7FSharp41Refs() = FSharpRefs "4.1" "portable7"
    let Portable78FSharp41Refs() = FSharpRefs "4.1" "portable78"
    let Portable259FSharp41Refs() = FSharpRefs "4.1" "portable259"

    let DotNetStandard20FSharp41Refs() = FSharpRefs "4.1" "netstandard2.0"
    let DotNetCoreApp20FSharp41Refs() = FSharpRefs "4.1" "netcoreapp2.0"
    
    let supportsFSharp31() = (try File.Exists (FSharpCore31Ref()) with _ -> false)
    let supportsFSharp40() = (try File.Exists (FSharpCore40Ref()) with _ -> false)
    let supportsFSharp41() = (try File.Exists (FSharpCore41Ref()) with _ -> false)
    let hasPortable47Assemblies() = Directory.Exists (sysInstalledAssembliesPath "portable47")
    let hasPortable7Assemblies() = Directory.Exists (sysInstalledAssembliesPath "portable7")
    let hasPortable78Assemblies() = Directory.Exists (sysInstalledAssembliesPath "portable78")
    let hasPortable259Assemblies() = Directory.Exists (sysInstalledAssembliesPath "portable259")

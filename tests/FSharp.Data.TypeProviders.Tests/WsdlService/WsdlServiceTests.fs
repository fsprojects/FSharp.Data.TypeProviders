// #Conformance #TypeProviders #WsdlService
#if COMPILED
module FSharp.Data.TypeProviders.Tests.WsdlServiceTests
#else
#r "nuget: FSharp.Data.TypeProviders"
#endif

open Microsoft.FSharp.Core.CompilerServices
open System
open System.IO
open NUnit.Framework

[<AutoOpen>]
module Infrastructure = 
    let reportFailure () = stderr.WriteLine " NO"; Assert.Fail("test failed")
    let test s b = stderr.Write(s:string);  if b then stderr.WriteLine " OK" else reportFailure() 
    let check s v1 v2 = stderr.Write(s:string);  if v1 = v2 then stderr.WriteLine " OK" else Assert.Fail(sprintf "... FAILURE: expected %A, got %A  " v2 v1)


[<AbstractClass>]
type WsdlServiceTest(serviceUri, prefix, useMsPrefix, checkHostedType) =
    
    let check caption a b = Infrastructure.check (prefix + caption) a b
    let test caption v = Infrastructure.test (prefix + caption) v

    let InstantiateTypeProvider (useLocalSchemaFile : string option, useForceUpdate : bool option, typeFullPath : string[]) = 
        let assemblyFile = typeof<FSharp.Data.TypeProviders.DesignTime.DataProviders>.Assembly.CodeBase.Replace("file:///","").Replace("/","\\")
        test "cnlkenkewe" (File.Exists assemblyFile) 

        // If/when we care about the "target framework", this mock function will have to be fully implemented
        let systemRuntimeContainsType s = 
            Console.WriteLine (sprintf "Call systemRuntimeContainsType(%s) returning dummy value 'true'" s)
            true

        let tpConfig = new TypeProviderConfig(systemRuntimeContainsType, ResolutionFolder=__SOURCE_DIRECTORY__, RuntimeAssembly=assemblyFile, ReferencedAssemblies=[| |], TemporaryFolder=Path.GetTempPath(), IsInvalidationSupported=true, IsHostedExecution=true)
        use typeProvider1 = (new FSharp.Data.TypeProviders.DesignTime.DataProviders( tpConfig ) :> ITypeProvider)

        let invalidateEventCount = ref 0

        typeProvider1.Invalidate.Add(fun _ -> incr invalidateEventCount)

        // Load a type provider instance for the type and restart
        let hostedNamespace1 = typeProvider1.GetNamespaces() |> Seq.find (fun t -> t.NamespaceName = if useMsPrefix then "Microsoft.FSharp.Data.TypeProviders" else "FSharp.Data.TypeProviders")

        check "eenewioinw" (set [ for i in hostedNamespace1.GetTypes() -> i.Name ]) (set ["DbmlFile"; "EdmxFile"; "ODataService"; "SqlDataConnection";"SqlEntityConnection";"WsdlService"])

        let hostedType1 = hostedNamespace1.ResolveTypeName("WsdlService")
        let hostedType1StaticParameters = typeProvider1.GetStaticParameters(hostedType1)
        check "eenewioinw2" 
            (set [ for i in hostedType1StaticParameters -> i.Name ]) 
            (set ["ServiceUri"; "LocalSchemaFile"; "ResolutionFolder"; "ForceUpdate"; "Serializable"; "MessageContract"; "EnableDataBinding"; "Async"; "CollectionType"; "Wrapped"; "SvcUtilPath"])

        let staticParameterValues = 
            [| for x in hostedType1StaticParameters -> 
                (match x.Name with 
                 | "ServiceUri" -> box serviceUri
                 | "LocalSchemaFile" when useLocalSchemaFile.IsSome -> box useLocalSchemaFile.Value
                 | "ForceUpdate" when useForceUpdate.IsSome -> box useForceUpdate.Value
                 | _ -> box x.RawDefaultValue) |]
        
        for p in Seq.zip hostedType1StaticParameters staticParameterValues do
            Console.WriteLine (sprintf "%A" p)
        Console.WriteLine (sprintf "instantiating service type... may take a while for WSDL service metadata to be downloaded, code generation tool to run and csc.exe to run...")
        typeProvider1.ApplyStaticArguments(hostedType1, typeFullPath, staticParameterValues)

    member this.Run() = 
        InstantiateTypeProvider( None, None, [| "WsdlServiceApplied" |]  ) |> checkHostedType

        let sfile = "sfile.wsdlschema"
        let fullPath s = Path.Combine(__SOURCE_DIRECTORY__, s)
        let schemaFile = fullPath sfile

        (try File.Delete schemaFile with _ -> ())
        InstantiateTypeProvider(Some sfile, Some true, [| "WsdlServiceApplied" |]) |> checkHostedType
        // schemaFile should now exist
        test "eoinew0c9e1" (File.Exists schemaFile)

        let writeTime = File.GetLastWriteTime(schemaFile)
        // Reuse the WsdlSchema just created
        InstantiateTypeProvider(Some sfile, Some false, [| "WsdlServiceApplied" |]) |> checkHostedType
        // schemaFile should still exist
        test "eoinew0c9e" (File.Exists schemaFile)
        check "LastWriteTime_1" (File.GetLastWriteTime(schemaFile)) writeTime

        let sfile2 = "sfile2.wsdlschema"
        let schemaFile2 = fullPath sfile2
        (try File.Delete schemaFile2 with _ -> ())

        let check(prefix2) = 
            // schemaFile2 should now exist
            test (prefix2 + "eoinew0c9e") (File.Exists schemaFile2)

(*
            // rename schema file
            let renamedFile = fullPath "renamed"
            // delete existing file
            try File.Delete renamedFile with _ -> ()
            System.Threading.SpinWait.SpinUntil((fun () -> File.Exists(schemaFile2)), 10000)
            |> ignore
            test (prefix2 + "SchemaFileExists") (File.Exists schemaFile2)
*)

        InstantiateTypeProvider(Some sfile2, Some false, [| "WsdlServiceApplied" |]) |> checkHostedType
        check "noncorrupt-"


(*
        // corrupt source file
        let initial = File.ReadAllText(sfile2)
        let text = "123" + File.ReadAllText(sfile2)
        File.WriteAllText(sfile2, text)
        try
            InstantiateTypeProvider(Some sfile2, Some false, [| "WsdlServiceApplied" |]) |> ignore
            test "Exception_Expected" false
        with
            e -> ()
        // read all text and verify that it was not overwritten
        let newText = File.ReadAllText(sfile2)
        test "FileWasNotChanged" (text = newText)

*)

    
type SimpleWsdlTest(useMsPrefix) = 
        inherit WsdlServiceTest("http://api.microsofttranslator.com/V2/Soap.svc", SimpleWsdlTest.Prefix, useMsPrefix, SimpleWsdlTest.CheckHostedType)

        static let check caption a b = Infrastructure.check (SimpleWsdlTest.Prefix + caption) a b
        static let test caption v = Infrastructure.test (SimpleWsdlTest.Prefix + caption) v

        static member Prefix = "simple-"
        static member CheckHostedType(hostedType) = 
            test "09wlkm1a" (hostedType.Assembly <> typeof<FSharp.Data.TypeProviders.DesignTime.DataProviders>.Assembly)
            test "09wlkm1b" (hostedType.Assembly.FullName.StartsWith "tmp")

            check "09wlkm2" hostedType.DeclaringType null
            check "09wlkm3" hostedType.DeclaringMethod null
            check "09wlkm4" hostedType.FullName "WsdlService1.WsdlServiceApplied"
            check "09wlkm5" (hostedType.GetConstructors()) [| |]
            check "09wlkm6" (hostedType.GetCustomAttributesData().Count) 1
            check "09wlkm6" (hostedType.GetCustomAttributesData().[0].Constructor.DeclaringType.FullName) typeof<TypeProviderXmlDocAttribute>.FullName
            check "09wlkm7" (hostedType.GetEvents()) [| |]
            check "09wlkm8" (hostedType.GetFields()) [| |]
            check "09wlkm9" (hostedType.GetMethods() |> Array.map (fun m -> m.Name)) [| "GetBasicHttpBinding_LanguageService"; "GetBasicHttpBinding_LanguageService"|]   
            check "09wlkm10" (hostedType.GetProperties()) [| |]
            check "09wlkm11" 
                (set [ for x in hostedType.GetNestedTypes() -> x.Name ]) 
                (set ["ServiceTypes"]   )

            let serviceTypes = hostedType.GetNestedTypes().[0]

            check "09wlkm11" (serviceTypes.GetNestedTypes().Length) 5
            check "09wlkm12" 
                (set [ for x in serviceTypes.GetNestedTypes() -> x.Name ]) 
                (set ["LanguageService"; "LanguageServiceChannel"; "LanguageServiceClient"; "Microsoft"; "SimpleDataContextTypes" ]   )

            let languageServiceType = (serviceTypes.GetNestedTypes() |> Seq.find (fun t -> t.Name = "LanguageService"))
            check "09wlkm13"  (languageServiceType.GetProperties().Length) 0
    

type XIgniteWsdlTest(useMsPrefix) = 
        inherit WsdlServiceTest("http://www.xignite.com/xFutures.asmx?WSDL", XIgniteWsdlTest.Prefix, useMsPrefix, XIgniteWsdlTest.CheckHostedType)

        static let check caption a b = Infrastructure.check (XIgniteWsdlTest.Prefix + caption) a b
        static let test caption v = Infrastructure.test (XIgniteWsdlTest.Prefix + caption) v

        static member Prefix = "xignite-"

        static member CheckHostedType (hostedType: System.Type) = 
            test "09wlkm1ad233" (hostedType.Assembly <> typeof<FSharp.Data.TypeProviders.DesignTime.DataProviders>.Assembly)
            test "09wlkm1b2ed1" (hostedType.Assembly.FullName.StartsWith "tmp")

            check "09wlkm2" hostedType.DeclaringType null
            check "09wlkm3" hostedType.DeclaringMethod null
            check "09wlkm4" hostedType.FullName "WsdlService1.WsdlServiceApplied"
            check "09wlkm5" (hostedType.GetConstructors()) [| |]
            check "09wlkm6" (hostedType.GetCustomAttributesData().Count) 1
            check "09wlkm6" (hostedType.GetCustomAttributesData().[0].Constructor.DeclaringType.FullName) typeof<TypeProviderXmlDocAttribute>.FullName
            check "09wlkm7" (hostedType.GetEvents()) [| |]
            check "09wlkm8" (hostedType.GetFields()) [| |]
            check "09wlkm9" (hostedType.GetMethods() |> Array.map (fun m -> m.Name)) [| "GetXigniteFuturesSoap"; "GetXigniteFuturesSoap"; "GetXigniteFuturesSoap12";"GetXigniteFuturesSoap12"|]   
            check "09wlkm10" (hostedType.GetProperties()) [| |]

            let serviceTypes = hostedType.GetNestedTypes().[0]


            check "09wlkm11a" (serviceTypes.GetNestedTypes().Length >= 1) true
            check "09wlkm11b" (serviceTypes.GetNestedType("www") <> null) true
            check "09wlkm11c" (serviceTypes.GetNestedType("www").GetNestedType("xignite") <> null) true
            check "09wlkm11d" (serviceTypes.GetNestedType("www").GetNestedType("xignite").GetNestedType("com") <> null) true
            check "09wlkm11e" (serviceTypes.GetNestedType("www").GetNestedType("xignite").GetNestedType("com").GetNestedType("services") <> null) true
            check "09wlkm11f" (serviceTypes.GetNestedType("www").GetNestedType("xignite").GetNestedType("com").GetNestedType("services").GetNestedTypes().Length >= 1) true
            check "09wlkm11g" [ for x in serviceTypes.GetNestedTypes() do if not x.IsNested && x.Namespace = null then yield x.Name ].Length 175


[<Test; Category("WsdlService")>]
let ``WSDL Tests 1`` () =
    (new SimpleWsdlTest(false)).Run()            // Typeprovider name = FSharp.Data.TypeProviders
    (new SimpleWsdlTest(true)).Run()        // Typeprovider name = Microsoft.FSharp.Data.TypeProviders

[<Test; Category("WsdlService")>]
let ``WSDL Tests 2`` () =
    (new XIgniteWsdlTest(false)).Run()           // Typeprovider name = FSharp.Data.TypeProviders
    (new XIgniteWsdlTest(true)).Run()       // Typeprovider name = Microsoft.FSharp.Data.TypeProviders



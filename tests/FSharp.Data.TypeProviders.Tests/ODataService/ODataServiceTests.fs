// #Conformance #TypeProviders #ODataService

#if COMPILED
module FSharp.Data.TypeProviders.Tests.OdataServiceTests
#else
#r "../../bin/FSharp.Data.TypeProviders.dll"
#endif

open Microsoft.FSharp.Core.CompilerServices
open System
open System.IO
open System.Reflection
open NUnit.Framework

[<AutoOpen>]
module Infrastructure = 
    let reportFailure () = stderr.WriteLine " NO"; Assert.Fail("test failed")
    let test s b = stderr.Write(s:string);  if b then stderr.WriteLine " OK" else reportFailure() 
    let check s v1 v2 = stderr.Write(s:string);  if v1 = v2 then stderr.WriteLine " OK" else Assert.Fail(sprintf "... FAILURE: expected %A, got %A  " v2 v1)

let checkHostedType (hostedType: System.Type) = 
        let bindingAttr = BindingFlags.DeclaredOnly ||| BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.Static
        //let hostedType = hostedAppliedType1
        test "ceklc09wlkm1a" (hostedType.Assembly <> typeof<FSharp.Data.TypeProviders.DesignTime.DataProviders>.Assembly)
        test "ceklc09wlkm1b" (hostedType.Assembly.FullName.StartsWith "tmp")

        check "ceklc09wlkm2" hostedType.DeclaringType null
        check "ceklc09wlkm3" hostedType.DeclaringMethod null
        check "ceklc09wlkm4" hostedType.FullName "FSharp.Data.TypeProviders.ODataServiceApplied"
        check "ceklc09wlkm5" (hostedType.GetConstructors(bindingAttr)) [| |]
        check "ceklc09wlkm6b1" (hostedType.GetCustomAttributesData().Count) 2
        check "ceklc09wlkm6b2" (hostedType.GetCustomAttributesData().[0].Constructor.DeclaringType.Name) typeof<TypeProviderEditorHideMethodsAttribute>.Name
        check "ceklc09wlkm6b3" (hostedType.GetCustomAttributesData().[1].Constructor.DeclaringType.Name) typeof<TypeProviderXmlDocAttribute>.Name
        check "ceklc09wlkm7" (hostedType.GetEvents(bindingAttr)) [| |]
        check "ceklc09wlkm8" (hostedType.GetFields(bindingAttr)) [| |]
        check "ceklc09wlkm9" [ for m in hostedType.GetMethods(bindingAttr) -> m.Name ] [ "GetDataContext" ; "GetDataContext" ]
        let m1 = hostedType.GetMethods(bindingAttr).[0]
        let m2 = hostedType.GetMethods(bindingAttr).[1]
        check "ceklc09wlkm9b" (m1.GetParameters().Length) 0
        check "ceklc09wlkm9b" (m2.GetParameters().Length) 1
        check "ceklc09wlkm9b" (m1.ReturnType.Name) "DemoService"
        check "ceklc09wlkm9c" (m1.ReturnType.FullName) ("FSharp.Data.TypeProviders.ODataServiceApplied+ServiceTypes+SimpleDataContextTypes+DemoService")

        check "ceklc09wlkm9d"  (m1.ReturnType.GetProperties(bindingAttr).Length) 5
        check "ceklc09wlkm9e"  (set [ for p in m1.ReturnType.GetProperties(bindingAttr) -> p.Name ]) (set ["Categories"; "Credentials"; "DataContext"; "Products"; "Suppliers"]) 
        check "ceklc09wlkm9f"  (set [ for p in m1.ReturnType.GetProperties(bindingAttr) -> p.PropertyType.Name ]) (set ["DataServiceQuery`1"; "DataServiceQuery`1";"DataServiceQuery`1";"ICredentials"; "DataServiceContext"])
        
        // We expose some getters and 1 setter on the simpler data context
        check "ceklc09wlkm9g"  (m1.ReturnType.GetMethods(bindingAttr).Length) 6
        check "ceklc09wlkm9h" (set [ for p in m1.ReturnType.GetMethods(bindingAttr) -> p.Name ]) (set ["get_Categories"; "get_Credentials"; "get_DataContext"; "get_Products"; "get_Suppliers"; "set_Credentials"])

        check "ceklc09wlkm10" (hostedType.GetProperties(bindingAttr)) [| |]
        check "ceklc09wlkm11" (hostedType.GetNestedTypes(bindingAttr).Length) 1
        check "ceklc09wlkm12" 
            (set [ for x in hostedType.GetNestedTypes(bindingAttr) -> x.Name ]) 
            (set ["ServiceTypes"])

        let hostedServiceTypes = hostedType.GetNestedTypes(bindingAttr).[0]

        check "ceklc09wlkm11" (hostedServiceTypes.GetNestedTypes(bindingAttr).Length) 6
        check "ceklc09wlkm12" 
            (set [ for x in hostedServiceTypes.GetNestedTypes(bindingAttr) -> x.Name ]) 
            (set ["Address"; "Category"; "DemoService"; "Product"; "SimpleDataContextTypes"; "Supplier"])

        let productType = (hostedServiceTypes.GetNestedTypes(bindingAttr) |> Seq.find (fun t -> t.Name = "Product"))
        check "ceklc09wlkm13"  (productType.GetProperties(bindingAttr).Length) 9

        check "ceklc09wlkm14" 
            (set [ for x in productType.GetProperties(bindingAttr) -> x.Name ]) 
            (set ["ID"; "Name"; "Description"; "ReleaseDate"; "DiscontinuedDate"; "Rating"; "Price"; "Category"; "Supplier"])

let instantiateTypeProviderAndCheckOneHostedType(useLocalSchemaFile: string option, useForceUpdate: bool option, typeFullPath:string[]) = 
        //let useLocalSchemaFile : string option = None
        //let useForceUpdate : bool option = None
        let assemblyFile = typeof<FSharp.Data.TypeProviders.DesignTime.DataProviders>.Assembly.CodeBase.Replace("file:///","").Replace("/","\\")
        test "cnlkenkewe" (File.Exists assemblyFile)

        // If/when we care about the "target framework", this mock function will have to be fully implemented
        let systemRuntimeContainsType s = 
            Console.WriteLine (sprintf "Call systemRuntimeContainsType(%s) returning dummy value 'true'" s)
            true

        let tpConfig = new TypeProviderConfig(systemRuntimeContainsType, ResolutionFolder=__SOURCE_DIRECTORY__, RuntimeAssembly=assemblyFile, ReferencedAssemblies=[| |], TemporaryFolder=Path.GetTempPath(), IsInvalidationSupported=false, IsHostedExecution=true)
        use typeProvider1 = (new FSharp.Data.TypeProviders.DesignTime.DataProviders( tpConfig ) :> ITypeProvider)

        let invalidateEventCount = ref 0

        typeProvider1.Invalidate.Add(fun _ -> incr invalidateEventCount)

        // Load a type provider instance for the type and restart
        let hostedNamespace1 = typeProvider1.GetNamespaces() |> Seq.find (fun t -> t.NamespaceName = "FSharp.Data.TypeProviders")

        check "eenewioinw" (set [ for i in hostedNamespace1.GetTypes() -> i.Name ]) (set ["DbmlFile"; "EdmxFile"; "ODataService"; "SqlDataConnection";"SqlEntityConnection";"WsdlService"])

        let hostedType1 = hostedNamespace1.ResolveTypeName("ODataService")
        let hostedType1StaticParameters = typeProvider1.GetStaticParameters(hostedType1)
        check "eenewioinw2" 
            (set [ for i in hostedType1StaticParameters -> i.Name ]) 
            (set ["ServiceUri"; "LocalSchemaFile"; "ForceUpdate"; "ResolutionFolder"; "DataServiceCollection"])

        let serviceUri = "http://services.odata.org/V2/OData/OData.svc/"
        let staticParameterValues = 
            [| for x in hostedType1StaticParameters -> 
                (match x.Name with 
                 | "ServiceUri" -> box serviceUri
                 | "LocalSchemaFile" when useLocalSchemaFile.IsSome -> box useLocalSchemaFile.Value
                 | "ForceUpdate" when useForceUpdate.IsSome -> box useForceUpdate.Value
                 | _ -> box x.RawDefaultValue) |]
        Console.WriteLine (sprintf "instantiating service type... may take a while for OData service metadata to be downloaded, code generation tool to run and csc.exe to run...")
        try
            let hostedAppliedType1 = typeProvider1.ApplyStaticArguments(hostedType1, typeFullPath, staticParameterValues)

            checkHostedType hostedAppliedType1
        with
        | e ->
            Console.WriteLine (sprintf "%s" (e.ToString()))
            reportFailure()



[<Test; Category("ODataService")>]
let ``OData Tests 1`` () =
    instantiateTypeProviderAndCheckOneHostedType(None, None, [| "ODataServiceApplied" |])

[<Test; Category("ODataService")>]
let ``OData Tests 2`` () =
    let schemaFile2 = Path.Combine(__SOURCE_DIRECTORY__, "svc.csdl")
    (try File.Delete schemaFile2 with _ -> ())
    instantiateTypeProviderAndCheckOneHostedType(Some (Path.Combine(__SOURCE_DIRECTORY__, "svc.csdl")), Some true, [| "ODataServiceApplied" |])
    // schemaFile2 should now exist
    test "eoinew0c9e" (File.Exists schemaFile2)

    // Reuse the CSDL just created
    instantiateTypeProviderAndCheckOneHostedType(Some (Path.Combine(__SOURCE_DIRECTORY__, "svc.csdl")), Some false, [| "ODataServiceApplied" |])
    // schemaFile2 should now still exist
    test "eoinew0c9e" (File.Exists schemaFile2)

[<Test; Category("ODataService")>]
let ``OData Tests 4`` () =
    let schemaFile3 = Path.Combine(__SOURCE_DIRECTORY__, "svc2.csdl") 
    (try File.Delete schemaFile3 with _ -> ())
    instantiateTypeProviderAndCheckOneHostedType(Some schemaFile3, None, [| "ODataServiceApplied" |])
    
    // schemaFile3 should now exist
    test "eoinew0c9e" (File.Exists schemaFile3)


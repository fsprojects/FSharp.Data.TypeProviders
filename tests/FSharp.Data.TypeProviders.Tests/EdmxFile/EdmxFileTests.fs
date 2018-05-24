// #Conformance #TypeProviders #EdmxFile
#if COMPILED
module FSharp.Data.TypeProviders.Tests.EdmxFile
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

    let (++) a b = Path.Combine(a,b)

module CheckEdmxfileTypeProvider = 
    let bindingAttr = BindingFlags.DeclaredOnly ||| BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.Static

    let checkHostedType (hostedType: System.Type) = 
        test "ceklc09wlkm1a" (hostedType.Assembly <> typeof<FSharp.Data.TypeProviders.DesignTime.DataProviders>.Assembly)
        test "ceklc09wlkm1b" (hostedType.Assembly.FullName.StartsWith "tmp")

        check "ceklc09wlkm2r" hostedType.DeclaringType null
        check "ceklc09wlkm3e" hostedType.DeclaringMethod null
        check "ceklc09wlkm4w" hostedType.FullName "SampleModel01.EdmxFileApplied"
        check "ceklc09wlkm5q" (hostedType.GetConstructors(bindingAttr)) [| |]
        check "ceklc09wlkm6b1" (hostedType.GetCustomAttributesData().Count) 2
        check "ceklc09wlkm6b2" (hostedType.GetCustomAttributesData().[0].Constructor.DeclaringType.Name) typeof<TypeProviderEditorHideMethodsAttribute>.Name
        check "ceklc09wlkm6b3" (hostedType.GetCustomAttributesData().[1].Constructor.DeclaringType.Name) typeof<TypeProviderXmlDocAttribute>.Name
        check "ceklc09wlkm7c" (hostedType.GetEvents(bindingAttr)) [| |]
        check "ceklc09wlkm8d" (hostedType.GetFields(bindingAttr)) [| |]
        check "ceklc09wlkm9e" (hostedType.GetMethods(bindingAttr)) [| |]
        check "ceklc09wlkm10f" (hostedType.GetProperties(bindingAttr)) [| |]
        let nestedTypes = hostedType.GetNestedTypes(bindingAttr)
        check "ceklc09wlkm11g" (nestedTypes.Length) 1
        check "ceklc09wlkm12h" 
            (set [ for x in nestedTypes -> x.Name ]) 
            (set ["SampleModel01"])

        let hostedServiceTypes = nestedTypes.[0]
        let hostedServiceTypesMethods = hostedServiceTypes.GetMethods(bindingAttr)
        check "ceklc09wlkm12b" (hostedServiceTypesMethods) [| |]
        check "ceklc09wlkm12c" 
             (set [ for x in hostedServiceTypes.GetNestedTypes(bindingAttr) -> x.Name ])
             (set ["Customers"; "Orders"; "Persons"; "SampleModel01Container"])

        // Deep check on one type: Customers
        let customersType = (hostedServiceTypes.GetNestedTypes(bindingAttr) |> Seq.find (fun t -> t.Name = "Customers"))
        check "ceklc09wlkm131"  (set [ for x in customersType.GetProperties(bindingAttr) -> x.Name ]) (set [| "Id"; "Orders" |])
        check "ceklc09wlkm133a"  (set [ for x in customersType.GetFields(bindingAttr) -> x.Name ]) (set [| |])
        check "ceklc09wlkm133b"  (set [ for x in customersType.GetFields(BindingFlags.Static ||| BindingFlags.Public  ||| BindingFlags.FlattenHierarchy) -> x.Name ]) 
                                 (set [ "EntityKeyPropertyName"] )
        let customersTypeMethods = customersType.GetMethods(bindingAttr)
        check "ceklc09wlkm134"  (set [ for x in customersTypeMethods -> x.Name ]) 
            (set ["CreateCustomers"; "get_Id"; "set_Id"; "get_Orders"; "set_Orders"] )
        let customersTypeMethods2 = customersType.GetMethods(BindingFlags.Static ||| BindingFlags.Public  ||| BindingFlags.FlattenHierarchy)
        check "ceklc09wlkm135"  (set [ for x in customersTypeMethods2 -> x.Name ]) 
                                (set [ "CreateCustomers"; "CreatePersons"; "Equals"; "ReferenceEquals" ] )
        check "ceklc09wlkm136"  (customersType.GetNestedTypes(bindingAttr)) [||]

        // Not so deep check on another type: SampleModel01Container
        let SampleModel01ContainerType = (hostedServiceTypes.GetNestedTypes(bindingAttr) |> Seq.find (fun t -> t.Name = "SampleModel01Container"))
        check "ceklc09wlkm141"  (set [ for x in SampleModel01ContainerType.GetProperties(bindingAttr) -> x.Name ]) (set [|"Orders"; "Persons"|])
        check "ceklc09wlkm142"  (SampleModel01ContainerType.GetFields(bindingAttr)) [||]
        let SampleModel01ContainerTypeMethods = SampleModel01ContainerType.GetMethods(bindingAttr)
        check "ceklc09wlkm144"  (set [ for x in SampleModel01ContainerTypeMethods -> x.Name ]) 
                                (set [|"get_Orders"; "get_Persons"; "AddToOrders"; "AddToPersons" |])
        check "ceklc09wlkm146"  (SampleModel01ContainerType.GetNestedTypes(bindingAttr)) [||]


    let instantiateTypeProviderAndCheckOneHostedType( edmxfile : string, typeFullPath ) = 

        let assemblyFile = typeof<FSharp.Data.TypeProviders.DesignTime.DataProviders>.Assembly.CodeBase.Replace("file:///","").Replace("/","\\")
        test "CheckFSharpDataTypeProvidersDLLExist" (File.Exists assemblyFile) 

        // If/when we care about the "target framework", this mock function will have to be fully implemented
        let systemRuntimeContainsType s = 
            Console.WriteLine (sprintf  "Call systemRuntimeContainsType(%s) returning dummy value 'true'" s)
            true

        let tpConfig = new TypeProviderConfig(systemRuntimeContainsType, ResolutionFolder=__SOURCE_DIRECTORY__, RuntimeAssembly=assemblyFile, ReferencedAssemblies=[| |], TemporaryFolder=Path.GetTempPath(), IsInvalidationSupported=false, IsHostedExecution=true)
        use typeProvider1 = (new FSharp.Data.TypeProviders.DesignTime.DataProviders( tpConfig ) :> ITypeProvider)

        // Setup machinery to keep track of the "invalidate event" (see below)
        let invalidateEventCount = ref 0
        typeProvider1.Invalidate.Add(fun _ -> incr invalidateEventCount)

        // Load a type provider instance for the type and restart
        let hostedNamespace1 = typeProvider1.GetNamespaces() |> Seq.find (fun t -> t.NamespaceName = "FSharp.Data.TypeProviders")

        check "CheckAllTPsAreThere" (set [ for i in hostedNamespace1.GetTypes() -> i.Name ]) (set ["DbmlFile"; "EdmxFile"; "ODataService"; "SqlDataConnection";"SqlEntityConnection";"WsdlService"])

        let hostedType1 = hostedNamespace1.ResolveTypeName("EdmxFile")
        let hostedType1StaticParameters = typeProvider1.GetStaticParameters(hostedType1)
        check "VerifyStaticParam" 
            (set [ for i in hostedType1StaticParameters -> i.Name ]) 
            (set [ "File"; "ResolutionFolder" ])

        let staticParameterValues = 
            [| for x in hostedType1StaticParameters -> 
                (match x.Name with 
                 | "File" -> box edmxfile  
                 | _ -> box x.RawDefaultValue) |]
        Console.WriteLine (sprintf "instantiating type... may take a while for code generation tool to run and csc.exe to run...")
        let hostedAppliedType1 = typeProvider1.ApplyStaticArguments(hostedType1, typeFullPath, staticParameterValues)

        checkHostedType hostedAppliedType1 

        let edmxfileAbs = if Path.IsPathRooted edmxfile then edmxfile else __SOURCE_DIRECTORY__ ++ edmxfile
        // Write replacement text into the file and check that the invalidation event is triggered....
        let file1NewContents = File.ReadAllText(edmxfileAbs).Replace("Customer", "Client")       // Rename 'Customer' to 'Client'
        do File.WriteAllText(edmxfileAbs, file1NewContents)

        // Wait for invalidate event to fire....
        for i in 0 .. 30 do
            if !invalidateEventCount = 0 then 
                System.Threading.Thread.Sleep 100

        check "VerifyInvalidateEventFired" !invalidateEventCount 1

let edmxfile = Path.Combine(__SOURCE_DIRECTORY__, "SampleModel01.edmx")

[<Category("EdmxFile"); Test>]
let ``EDMX Tests 1`` () =
    // Test with absolute path
    // Copy the .edmx used for tests to avoid trashing our original (we may overwrite it when testing the event)
    File.Copy(Path.Combine(__SOURCE_DIRECTORY__, @"EdmxFiles\SampleModel01.edmx"), edmxfile, true)
    File.SetAttributes(edmxfile, FileAttributes.Normal)
    CheckEdmxfileTypeProvider.instantiateTypeProviderAndCheckOneHostedType(edmxfile, [| "EdmxFileApplied" |])

[<Test; Category("EdmxFile")>]
let ``EDMX Tests 2`` () =
    // Test with relative path
    // Copy the .edmx used for tests to avoid trashing our original (we may overwrite it when testing the event)
    File.Copy(Path.Combine(__SOURCE_DIRECTORY__, @"EdmxFiles\SampleModel01.edmx"), edmxfile, true)
    File.SetAttributes(edmxfile, FileAttributes.Normal)
    CheckEdmxfileTypeProvider.instantiateTypeProviderAndCheckOneHostedType( Path.GetFileName(edmxfile), [| "EdmxFileApplied" |])


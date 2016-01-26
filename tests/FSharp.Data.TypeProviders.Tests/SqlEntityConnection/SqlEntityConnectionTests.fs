// #Conformance #TypeProviders #SqlEntityConnection
#if COMPILED
module FSharp.Data.TypeProviders.Tests.SqlEntityConnectionTests
#else
#r "FSharp.Data.TypeProviders.dll"
#r "System.Management.dll"
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


let isSQLExpressInstalled =
    lazy
        let edition = "Express Edition"
        let instance = "MSSQL$SQLEXPRESS"

        try
            let getSqlExpress = 
                new System.Management.ManagementObjectSearcher("root\\Microsoft\\SqlServer\\ComputerManagement10",
                                                               "select * from SqlServiceAdvancedProperty where SQLServiceType = 1 and ServiceName = '" + instance + "' and (PropertyName = 'SKUNAME' or PropertyName = 'SPLEVEL')")

            // If nothing is returned, SQL Express isn't installed.
            getSqlExpress.Get().Count <> 0
        with
        | _ -> false

let checkHostedType (expectedContextTypeName, hostedType: System.Type) = 
        //let hostedType = hostedAppliedType1
        
        test "ceklc09wlkm1a" (hostedType.Assembly <> typeof<FSharp.Data.TypeProviders.DesignTime.DataProviders>.Assembly)
        test "ceklc09wlkm1b" (hostedType.Assembly.FullName.StartsWith "tmp")

        check "ceklc09wlkm2" hostedType.DeclaringType null
        check "ceklc09wlkm3" hostedType.DeclaringMethod null
        check "ceklc09wlkm4" hostedType.FullName ("SqlEntityConnection1.SqlEntityConnectionApplied")
        check "ceklc09wlkm5" (hostedType.GetConstructors()) [| |]
        check "ceklc09wlkm6" (hostedType.GetCustomAttributesData().Count) 1
        check "ceklc09wlkm6" (hostedType.GetCustomAttributesData().[0].Constructor.DeclaringType.FullName) typeof<TypeProviderXmlDocAttribute>.FullName
        check "ceklc09wlkm7" (hostedType.GetEvents()) [| |]
        check "ceklc09wlkm8" (hostedType.GetFields()) [| |]
        check "ceklc09wlkm9" [ for m in hostedType.GetMethods() -> m.Name ] [ "GetDataContext" ;"GetDataContext" ]
        let m0 = hostedType.GetMethods().[0]
        let m1 = hostedType.GetMethods().[1]
        check "ceklc09wlkm9b" (m0.GetParameters().Length) 0
        check "ceklc09wlkm9c" (m1.GetParameters().Length) 1
        check "ceklc09wlkm9d" (m0.ReturnType.Name) expectedContextTypeName
        check "ceklc09wlkm9e" (m0.ReturnType.FullName) ("SqlEntityConnection1.SqlEntityConnectionApplied+ServiceTypes+SimpleDataContextTypes+" + expectedContextTypeName)
        check "ceklc09wlkm10" (hostedType.GetProperties()) [| |]
        check "ceklc09wlkm11" (hostedType.GetNestedTypes().Length) 1
        check "ceklc09wlkm12" 
            (set [ for x in hostedType.GetNestedTypes() -> x.Name ]) 
            (set ["ServiceTypes"])

        let hostedServiceTypes = hostedType.GetNestedTypes().[0]
        check "ceklc09wlkm12b" (hostedServiceTypes.GetMethods()) [| |]
        check "ceklc09wlkm12c" (hostedServiceTypes.GetNestedTypes().Length) 28

        let hostedSimpleDataContextTypes = hostedServiceTypes.GetNestedType("SimpleDataContextTypes")
        check "ceklc09wlkm12d" (hostedSimpleDataContextTypes.GetMethods()) [| |]
        check "ceklc09wlkm12e" (hostedSimpleDataContextTypes.GetNestedTypes().Length) 1
        check "ceklc09wlkm12e" [ for x in hostedSimpleDataContextTypes.GetNestedTypes() -> x.Name] [expectedContextTypeName]

        check "ceklc09wlkm12" 
            (set [ for x in hostedServiceTypes.GetNestedTypes() -> x.Name ]) 
            (set 
               (["Territory"; "Supplier"; "Summary_of_Sales_by_Year";
                 "Summary_of_Sales_by_Quarter"; "Shipper"; "Sales_Totals_by_Amount";
                 "Sales_by_Category"; "Region"; "Products_by_Category";
                 "Products_Above_Average_Price"; "Product_Sales_for_1997"; "Product";
                 "Orders_Qry"; "Order_Subtotal"; "Order_Details_Extended"; "Order_Detail";
                 "Order"; "Invoice"; "Employee"; "CustomerDemographic";
                 "Customer_and_Suppliers_by_City"; "Customer"; "Current_Product_List";
                 "Category_Sales_for_1997"; "Category"; "Alphabetical_list_of_product"; ] @ 
                [expectedContextTypeName] @ 
                [ "SimpleDataContextTypes"]))

        let customersType = (hostedServiceTypes.GetNestedTypes() |> Seq.find (fun t -> t.Name = "Customer"))
        check "ceklc09wlkm13"  (customersType.GetProperties().Length) 15

let (++) a b = Path.Combine(a,b)

let instantiateTypeProviderAndCheckOneHostedType(connectionStringName, configFile, useDataDirectory, dataDirectory, entityContainer: string option, localSchemaFile: string option, useForceUpdate: bool option, typeFullPath: string[], resolutionFolder: string option) = 
        let expectedContextTypeName = match entityContainer with None -> "EntityContainer" | Some s -> s
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

        let hostedType1 = hostedNamespace1.ResolveTypeName("SqlEntityConnection")
        let hostedType1StaticParameters = typeProvider1.GetStaticParameters(hostedType1)
        check "eenewioinw2" 
            (set [ for i in hostedType1StaticParameters -> i.Name ]) 
            (set ["ConnectionString"; "ConnectionStringName"; "ResolutionFolder"; "DataDirectory"; "ConfigFile"; "ForceUpdate"; "Provider"; "EntityContainer"; "LocalSchemaFile"; "Pluralize"; "SuppressForeignKeyProperties"]   )


        let northwind = "NORTHWND.mdf"
        let northwindFile = 
            match resolutionFolder with 
            | None -> 
                match dataDirectory with 
                | None -> 
                    Path.Combine(__SOURCE_DIRECTORY__, northwind)
                | Some dd -> 
                    let ddAbs = if Path.IsPathRooted dd then dd else  Path.Combine(__SOURCE_DIRECTORY__, dd)
                    if not(Directory.Exists ddAbs) then Directory.CreateDirectory ddAbs |> ignore
                    Path.Combine(ddAbs, northwind)

            | Some rf -> 
                let rfAbs = if Path.IsPathRooted rf then rf else  Path.Combine(__SOURCE_DIRECTORY__, rf)
                if not(Directory.Exists rfAbs) then Directory.CreateDirectory rfAbs |> ignore
                match dataDirectory with 
                | None -> 
                    Path.Combine(rf, northwind)
                | Some dd -> 
                    let ddAbs = if Path.IsPathRooted dd then dd else  Path.Combine(rfAbs, dd)
                    if not(Directory.Exists ddAbs) then Directory.CreateDirectory ddAbs |> ignore                
                    Path.Combine(ddAbs,northwind)


        if not(File.Exists(northwindFile)) then 
            File.Copy(__SOURCE_DIRECTORY__ ++ "DB" ++ "northwnd.mdf", northwindFile, false)
            File.SetAttributes(northwindFile, FileAttributes.Normal)


        let connectionString = 
            if useDataDirectory then 
                if isSQLExpressInstalled.Value then
                    @"AttachDBFileName = '|DataDirectory|\" + northwind + "';Server='.\SQLEXPRESS';User Instance=true;Integrated Security=SSPI"
                else
                    "AttachDBFileName = '|DataDirectory|\\" + northwind + "';Server='(localdb)\\MSSQLLocalDB'"
            else
                if isSQLExpressInstalled.Value then
                    @"AttachDBFileName = '" + System.IO.Path.Combine(__SOURCE_DIRECTORY__, northwindFile) + "';Server='.\SQLEXPRESS';User Instance=true;Integrated Security=SSPI"
                else
                    "AttachDBFileName = '" + System.IO.Path.Combine(__SOURCE_DIRECTORY__, northwindFile) + "';Server='(localdb)\\MSSQLLocalDB'"

        match connectionStringName with 
        | None -> ()
        | Some connectionStringName -> 
               let configFileBase = 
                   match configFile with 
                   | None -> "app.config" 
                   | Some nm -> nm
               let configFileName = 
                   match resolutionFolder with 
                   | None -> 
                       if Path.IsPathRooted configFileBase then configFileBase else __SOURCE_DIRECTORY__ ++ configFileBase
                   | Some rf -> 
                       let rfAbs = if Path.IsPathRooted rf then rf else  Path.Combine(__SOURCE_DIRECTORY__, rf)
                       Path.Combine(rfAbs,configFileBase)
               System.IO.File.WriteAllText(configFileName,
                   sprintf """<?xml version="1.0"?>

<configuration>
  <connectionStrings>
    <add name="%s"
         connectionString="%s"
         providerName="System.Data.SqlClient" />
  </connectionStrings>

  <system.webServer>
     <modules runAllManagedModulesForAllRequests="true"/>
  </system.webServer>
</configuration>
"""               
                       connectionStringName
                       connectionString)
        let staticParameterValues = 
            [| for x in hostedType1StaticParameters -> 
                (match x.Name with 
                 | "ConnectionString" when connectionStringName.IsNone -> box connectionString  
                 | "ConnectionStringName" when connectionStringName.IsSome -> box connectionStringName.Value
                 |  "DataDirectory" when dataDirectory.IsSome -> box dataDirectory.Value
                 |  "ConfigFile"  when configFile.IsSome -> box configFile.Value
                 | "Pluralize" -> box true
                 | "EntityContainer" when entityContainer.IsSome -> box entityContainer.Value
                 | "SuppressForeignKeyProperties" -> box false
                 | "LocalSchemaFile" when localSchemaFile.IsSome -> box localSchemaFile.Value
                 | "ForceUpdate" when useForceUpdate.IsSome -> box useForceUpdate.Value
                 | _ -> box x.RawDefaultValue) |]
        Console.WriteLine (sprintf "instantiating database type... may take a while for db to attach, code generation tool to run and csc.exe to run...")
        try        
            let hostedAppliedType1 = typeProvider1.ApplyStaticArguments(hostedType1, typeFullPath, staticParameterValues)

            checkHostedType (expectedContextTypeName,hostedAppliedType1 )
        with
        | e ->
            Console.WriteLine (sprintf "%s" (e.ToString()))
            reportFailure()

[<Test; Category("SqlEntity")>]
let ``SqlEntity Tests 1`` () =
  // Database not yet installed on appveyor
  if System.Environment.GetEnvironmentVariable("APPVEYOR") = null then
    instantiateTypeProviderAndCheckOneHostedType(None, None, false, None, None, None, None, [| "SqlEntityConnectionApplied" |], None)

[<Test; Category("SqlEntity")>]
let ``SqlEntity Tests 2`` () =
  // Database not yet installed on appveyor
  if System.Environment.GetEnvironmentVariable("APPVEYOR") = null then
    // Use an implied app.config config file, use the current directory as the DataDirectory
    instantiateTypeProviderAndCheckOneHostedType(Some "ConnectionString1", None, true, None, None, None, None, [| "SqlEntityConnectionApplied" |], None)

[<Test; Category("SqlEntity")>]
let ``SqlEntity Tests 3`` () =
  // Database not yet installed on appveyor
  if System.Environment.GetEnvironmentVariable("APPVEYOR") = null then
    // Use a config file, use an explicit relative DataDirectory
    instantiateTypeProviderAndCheckOneHostedType(Some "ConnectionString2", Some "app.config", true, Some "DataDirectory", None, None, None, [| "SqlEntityConnectionApplied" |], None)

[<Test; Category("SqlEntity")>]
let ``SqlEntity Tests 4`` () =
  // Database not yet installed on appveyor
  if System.Environment.GetEnvironmentVariable("APPVEYOR") = null then
    // Use an absolute config file, use an absoltue DataDirectory
    instantiateTypeProviderAndCheckOneHostedType(Some "ConnectionString3", Some (__SOURCE_DIRECTORY__ ++ "test.config"), true, Some (__SOURCE_DIRECTORY__ ++ "DataDirectory"), None, None, None, [| "SqlEntityConnectionApplied" |], None)


[<Test; Category("SqlEntity")>]
let ``SqlEntity Tests 5`` () =
  // Database not yet installed on appveyor
  if System.Environment.GetEnvironmentVariable("APPVEYOR") = null then
    let schemaFile2 = Path.Combine(__SOURCE_DIRECTORY__, "nwind2.ssdl")
    (try File.Delete schemaFile2 with _ -> ())
    instantiateTypeProviderAndCheckOneHostedType(None, None, false, None, None, Some (Path.Combine(__SOURCE_DIRECTORY__, "nwind2.ssdl")), Some true, [| "SqlEntityConnectionApplied" |], None)
    // schemaFile2 should now exist
    test "eoinew0c9e" (File.Exists schemaFile2)

    // Reuse the SSDL just created
    instantiateTypeProviderAndCheckOneHostedType(None, None, false, None, None, Some (Path.Combine(__SOURCE_DIRECTORY__, "nwind2.ssdl")), Some false, [| "SqlEntityConnectionApplied" |], None)
    // schemaFile2 should now still exist
    test "eoinew0c9e" (File.Exists schemaFile2)

    // // A relative path should work....
    // instantiateTypeProviderAndCheckOneHostedType(Some "nwind2.ssdl", Some false)
    // // schemaFile2 should now still exist
    // check "eoinew0c9e" (File.Exists schemaFile2)

[<Test; Category("SqlEntity")>]
let ``SqlEntity Tests 6`` () =
  // Database not yet installed on appveyor
  if System.Environment.GetEnvironmentVariable("APPVEYOR") = null then
    let schemaFile3 = Path.Combine(__SOURCE_DIRECTORY__, "nwind3.ssdl") 
    (try File.Delete schemaFile3 with _ -> ())
    instantiateTypeProviderAndCheckOneHostedType(None, None, false, None, None, Some schemaFile3, None, [| "SqlEntityConnectionApplied" |],None)
    
    // schemaFile3 should now exist
    test "eoinew0c9e" (File.Exists schemaFile3)

[<Test; Category("SqlEntity")>]
let ``SqlEntity Tests 7`` () =
  // Database not yet installed on appveyor
  if System.Environment.GetEnvironmentVariable("APPVEYOR") = null then
    let schemaFile4 = Path.Combine(__SOURCE_DIRECTORY__, "nwind4.ssdl") 
    (try File.Delete schemaFile4 with _ -> ())
    instantiateTypeProviderAndCheckOneHostedType(None, None, false, None, Some "MyEntityContainer", Some schemaFile4, None, [| "SqlEntityConnectionApplied" |], None)
    
    // schemaFile4 should now exist
    test "eoinew0c9e" (File.Exists schemaFile4)


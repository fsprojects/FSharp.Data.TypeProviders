// #Conformance #TypeProviders #WsdlService
#if COMPILED
module FSharp.Data.TypeProviders.UsageTests.WsdlServiceTests
#else
#r "FSharp.Data.TypeProviders.dll"
#endif


open FSharp.Data.TypeProviders

type Wsdl1 = WsdlService<"http://api.microsofttranslator.com/V2/Soap.svc">

let ctxt = Wsdl1.GetBasicHttpBinding_LanguageService()

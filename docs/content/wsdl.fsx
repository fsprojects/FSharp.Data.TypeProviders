(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
The WsdlService Type Provider (FSharp.Data.TypeProviders)
========================

Please see [Walkthrough: Walkthrough: Accessing a Web Service by Using Type Providers](https://msdn.microsoft.com/en-us/library/hh156503.aspx)

> NOTE: Use ``FSharp.Data.TypeProviders`` instead of ``Microsoft.FSharp.Data.TypeProviders`` 

Reference 
---------

Please see the [MSDN Documentation](https://msdn.microsoft.com/en-us/library/hh362324.aspx)

Example
------

See below for an example use of the type provider:

*)
#r "System.ServiceModel"
#r "FSharp.Data.TypeProviders.dll"
open FSharp.Data.TypeProviders

type Wsdl1 = WsdlService<"http://api.microsofttranslator.com/V2/Soap.svc">

let ctxt = Wsdl1.GetBasicHttpBinding_LanguageService()

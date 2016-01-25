(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
FSharp.Data.TypeProviders
======================

The F# Type Providers SqlDataConnection, SqlEntityConnection, ODataService, 
WsdlService and EdmxFile using .NET Framework generators.

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The FSharp.Data.TypeProviders library can be <a href="https://nuget.org/packages/FSharp.Data.TypeProviders">installed from NuGet</a>:
      <pre>PM> Install-Package FSharp.Data.TypeProviders</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>



Contents:

* [DbmlFile Type Provider](dbml.html) - Provides the types for a database schema encoded in a .dbml file.

* [EdmxFile Type Provider](edmx.html) - Provides the types to access a database with the schema in an .edmx file, using a LINQ to Entities mapping.

* [ODataService Type Provider](odata.html) - Provides the types to access an OData service.

* [SqlDataConnection Type Provider](sqldata.html) - Provides the types to access a SQL database.

* [SqlEntityConnection_ Type Provider](sqlentity.html) - Provides the types to access a database, using a LINQ to Entities mapping.

* [WsdlService Type Provider](wsdl.html) - Provides the types for a Web Services Description Language (WSDL) web service.

History
-------

This component is shipped in the Visual F# Tools at version 4.3.0.0.  The proposal is that
subsequent versions and development will happen as an F# community component.

Referencing the library
-------

Reference the library as shown below.

*)
#r "FSharp.Data.TypeProviders.dll"
open FSharp.Data.TypeProviders




(**

Samples & documentation
-----------------------

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding a new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read the [library design notes][readme] to understand how it works.

The library is available under the Apache 2.0 license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/fsprojects/FSharp.Data.TypeProviders/tree/master/docs/content
  [gh]: https://github.com/fsprojects/FSharp.Data.TypeProviders
  [issues]: https://github.com/fsprojects/FSharp.Data.TypeProviders/issues
  [readme]: https://github.com/fsprojects/FSharp.Data.TypeProviders/blob/master/README.md
  [license]: https://github.com/fsprojects/FSharp.Data.TypeProviders/blob/master/LICENSE.txt
*)

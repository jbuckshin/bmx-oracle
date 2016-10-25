using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Inedo.BuildMaster.Extensibility;

[assembly: AssemblyTitle("Oracle")]
[assembly: AssemblyDescription("Database integration for Oracle 9i and later. (Custom Build)")]

[assembly: ComVisible(false)]
[assembly: AssemblyCompany("Inedo, LLC")]
[assembly: AssemblyProduct("BuildMaster")]
[assembly: AssemblyCopyright("Copyright © 2008 - 2016")]
[assembly: AssemblyVersion("5.1.1.0")]
[assembly: AssemblyFileVersion("5.1")]
[assembly: CLSCompliant(false)]
[assembly: RequiredBuildMasterVersion("5.0.0")]

#if DEBUG
[assembly: InternalsVisibleTo("OracleUnitTests")]
#endif
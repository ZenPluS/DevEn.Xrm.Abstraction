using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting; // Added for parallelization attribute

[assembly: AssemblyTitle("DevEn.Xrm.Abstraction.Plugins.UnitTests")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("DevEn.Xrm.Abstraction.Plugins.UnitTests")]
[assembly: AssemblyCopyright("Copyright ©2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("569d4747-a1da-4e2d-b9e4-072326cabba3")]

// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Explicitly disable test parallelization to satisfy MSTEST0001 (change to Parallelize if desired)
[assembly: DoNotParallelize]

using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting; // Added for parallelization attribute

[assembly: AssemblyTitle("DevEn.Xrm.Abstraction.Workflows.UnitTests")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("DevEn.Xrm.Abstraction.Workflows.UnitTests")]
[assembly: AssemblyCopyright("Copyright ©2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: Guid("5181cb4d-67c7-4b2a-b705-8872fef4ee7f")]

// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Explicitly disable test parallelization to satisfy MSTEST0001 (change to Parallelize if desired)
[assembly: DoNotParallelize]

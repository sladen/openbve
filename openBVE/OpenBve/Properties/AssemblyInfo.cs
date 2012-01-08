using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("openBVE")]
[assembly: AssemblyProduct("openBVE")]
[assembly: AssemblyCopyright("(Public Domain) http://trainsimframework.org/")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.4.0.0")]
[assembly: AssemblyFileVersion("1.4.0.0")]
[assembly: CLSCompliant(true)]

namespace OpenBve {
	internal static partial class Program {
		/// <summary>Whether this is a development version. Affects the main menu design and the version checking.</summary>
		internal const bool IsDevelopmentVersion = false;
	}
}
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("openBVE")]
[assembly: AssemblyProduct("openBVE")]
[assembly: AssemblyCopyright("http://openbve.trainsimcentral.co.uk/")]
[assembly: ComVisible(false)]
[assembly: Guid("bd68500e-8db6-4394-8fec-6adcde64c213")]
[assembly: AssemblyVersion("1.3.0.0")]
[assembly: AssemblyFileVersion("1.3.0.0")]
[assembly: CLSCompliant(true)]

namespace OpenBve {
	internal static partial class Program {
		/// <summary>Whether this is a development version. Affects the main menu design and the version checking.</summary>
		internal const bool IsDevelopmentVersion = true;
	}
}
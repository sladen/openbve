using System;
using Tao.Sdl;

namespace OpenBve {
	internal static class Timers {

		// win32-specific api-declarations
		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);
		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

		// members
		private static bool WindowsHighPrecisionTimer = false;
		private static double WindowsHighPrecisionTimerInverseFrequency = 0.0;
		private static long WindowsHighPrecisionTimerCounter = 0;
		private static double SdlTime = 0.0;

		// initialize
		internal static void Initialize() {
			if (Program.CurrentPlatform == Program.Platform.Windows) {
				long f; if (QueryPerformanceFrequency(out f)) {
					WindowsHighPrecisionTimerInverseFrequency = 1.0 / (double)f;
					QueryPerformanceCounter(out WindowsHighPrecisionTimerCounter);
					WindowsHighPrecisionTimer = true;
				}
			}
			if (!WindowsHighPrecisionTimer) {
				SdlTime = 0.001 * (double)Sdl.SDL_GetTicks();
			}
		}

		// get elapsed time
		internal static double GetElapsedTime() {
			if (WindowsHighPrecisionTimer) {
				long a; QueryPerformanceCounter(out a);
				long d = a - WindowsHighPrecisionTimerCounter;
				WindowsHighPrecisionTimerCounter = a;
				return (double)d * WindowsHighPrecisionTimerInverseFrequency;
			} else {
				double a = 0.001 * (double)Sdl.SDL_GetTicks();
				double d = a - SdlTime;
				SdlTime = a;
				return d;
			}
		}

	}
}
using System;
using Tao.Sdl;

namespace OpenBve {
    internal static class Timers {

        // win32-specific api-declarations
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        // high-precision timer
        private static double HighResFrequency;
        private static long HighResCounter;
        internal static bool HighPrecisionTimerInitialize() {
            if (Program.CurrentPlatform == Program.Platform.Windows) {
                long f; if (QueryPerformanceFrequency(out f)) {
                    HighResFrequency = (double)f;
                    QueryPerformanceCounter(out HighResCounter);
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }
        internal static double HighPrecisionTimerGetElapsedTime() {
            if (Program.CurrentPlatform == Program.Platform.Windows) {
                long a; QueryPerformanceCounter(out a);
                long d = a - HighResCounter;
                HighResCounter = a;
                return (double)d / HighResFrequency;
            } else {
                return LowPrecisionTimerGetElapsedTime();
            }
        }

        // low-precision timer
        private static double LowResReference = 0.001 * (double)Sdl.SDL_GetTicks();
        internal static double LowPrecisionTimerGetElapsedTime() {
            double a = 0.001 * (double)Sdl.SDL_GetTicks();
            double d = a - LowResReference;
            LowResReference = a;
            return d;
        }

    }
}
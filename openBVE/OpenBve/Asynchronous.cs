using System;
using System.Threading;

namespace OpenBve {
    internal static class Asynchronous {

        // members
        private static Thread Worker = null;
        private static bool WorkerStop = false;

        // initialize
        internal static void Initialize() {
            if (Worker != null) Deinitialize();
            Worker = new Thread(new ThreadStart(Perform));
            WorkerStop = false;
            Worker.IsBackground = true;
            Worker.Start();
            
        }

        // deinitialize
        internal static void Deinitialize() {
            if (Worker != null) {
                WorkerStop = true;
                Worker.Join();
                Worker = null;
            }
        }

        // perform
        private static void Perform() {
            while (!WorkerStop) {
                TextureManager.PerformAsynchronousOperations();
                Thread.Sleep(150);
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace EventGen
{
    public class DotNetRuntime_EG
    {
        public static void GenerateContentionEvent() {
            object lockObj = new object();
            void LockingOperation() {
                lock (lockObj) {
                    //Console.WriteLine("Locking obj...");
                    Thread.Sleep(100);
                }
            }
            var threads = Enumerable.Range(0, 5)
                .Select(_ => new Thread(LockingOperation))
                .ToList();
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
            Console.WriteLine("+ Conflict Event");
        }

        public static void GenerateGCEvent() {
            //Console.WriteLine("Start GC");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Console.WriteLine("+ Garbage COllector Event");
        }

        public static void GenerateExceptionEvent() {
            try {
                throw new InvalidOperationException("");
            } catch (Exception ex) {
                Console.Write($"{ex.Message}");
            }
            Console.WriteLine("+ Exception Event");
        }

        public static void GenerateInteropEvent() {
            //Console.WriteLine("Memory opperations");
            IntPtr memory = Marshal.AllocHGlobal(100);
            Marshal.FreeHGlobal(memory);
            Console.WriteLine("+ Interoperations Event");
        }

        public static void GenerateLoaderEvent() {
            //Console.WriteLine("Load module");
            Assembly.Load("System.Linq");
            Console.WriteLine("+ Loader Event");
        }

        public static void GenerateMethodEvent() {
            foo();

            void foo() {
                Console.WriteLine("+ Method Event");
            }
        }

        public static void GenerateThreadEvent()
        {
            Thread thread = new Thread(() => Console.Write(""));
            thread.Start();
            thread.Join();
            Console.WriteLine("+ Thread Event");
        }

        public static void GenerateTypeEvent() {
            //Console.WriteLine("Initialize new list");
            var list = new List<int>();
            Console.WriteLine("+ Type Event");
        }

        public static void GenerateTieredCompilationEvent() {
            //Console.WriteLine("Compile...");
            PerformJITCompilation();
            Console.WriteLine("+ Compilation Event");

            void PerformJITCompilation()
            {
                for (int i = 0; i < 100; i++) {
                    Math.Sqrt(i);
                }
            }
        }

        public static void Produce() {
            DotNetRuntime_EG.GenerateContentionEvent();
            DotNetRuntime_EG.GenerateGCEvent();
            DotNetRuntime_EG.GenerateExceptionEvent();
            DotNetRuntime_EG.GenerateInteropEvent();
            DotNetRuntime_EG.GenerateThreadEvent();
            DotNetRuntime_EG.GenerateTypeEvent();
            DotNetRuntime_EG.GenerateLoaderEvent();
            DotNetRuntime_EG.GenerateMethodEvent();
        }
    }
}

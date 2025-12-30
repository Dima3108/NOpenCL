#pragma warning disable NETSDK1138
using NOpenCL;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Buffer = NOpenCL.Buffer;

namespace CL_Test
{
    internal class Program
    {
        const string kernel_name = "kernel.cl";
        const int N = 1024 * 1024 * 256;
        static unsafe void TestAllocatedHostPointer(int* hostbuffer, Device device, Context context, string kernel_code, string c_name)
        {
            int* h = null;
            Buffer buffer = context.CreateBuffer(MemoryFlags.AllocateHostPointer | MemoryFlags.ReadWrite, sizeof(int) * N, (IntPtr)h);
            CommandQueue commandQueue = context.CreateCommandQueue(device);
            NOpenCL.Program program = context.CreateProgramWithSource(kernel_code);
            program.Build(device);

            using (Kernel kernel = program.CreateKernel(c_name))
            {
                kernel.Arguments[0].SetValue(buffer);
                commandQueue.EnqueueNDRangeKernel(kernel, (IntPtr)N, (IntPtr)4);
                commandQueue.Finish();
            }
            commandQueue.EnqueueMapBuffer(buffer, true, MapFlags.Read, 0, sizeof(int) * N, out IntPtr mappedPointer);
            commandQueue.Finish();
            // commandQueue.EnqueueReadBuffer(buffer, false, 0, sizeof(int) * N, (IntPtr)hostbuf2);
            if (mappedPointer != IntPtr.Zero)
            {
                int* j = (int*)mappedPointer;
                Console.WriteLine(j[0]);
            }

            int* k = (int*)mappedPointer;
            for (int i = 0; i < N; i++)
                if (k[i] != i)
                    Console.WriteLine("error");
            Console.ReadLine();
            commandQueue.EnqueueUnmapMemObject(buffer, mappedPointer);
            buffer.Dispose();
        }
        static unsafe void TestUsedMemory(int* hostbuffer, Device device, Context context, string kernel_code, string c_name, bool useHostMem = true)
        {
            MemoryFlags buf_mem_flags = MemoryFlags.ReadWrite;
            if (useHostMem)
                buf_mem_flags |= MemoryFlags.UseHostPointer;
            Buffer buffer = context.CreateBuffer(buf_mem_flags, sizeof(int) * N, (IntPtr)hostbuffer);
            CommandQueue commandQueue = context.CreateCommandQueue(device);
            NOpenCL.Program program = context.CreateProgramWithSource(kernel_code);
            program.Build(device);
            Console.ReadLine();
            using (Kernel kernel = program.CreateKernel(c_name))
            {
                kernel.Arguments[0].SetValue(buffer);
                commandQueue.EnqueueNDRangeKernel(kernel, (IntPtr)N, (IntPtr)4);
                commandQueue.Finish();
            }
            commandQueue.EnqueueReadBuffer(buffer, true, 0, sizeof(int) * N, (IntPtr)hostbuffer);
            commandQueue.Finish();
            Console.WriteLine(hostbuffer[20]);
            Console.WriteLine(hostbuffer[35] == 35 * 35);
            NOpenCL.CleanUp.ForcedResourceCleanup.Clear(ref buffer);
            //  buffer.Dispose();
        }
        static unsafe void Main(string[] args)
        {
            string kernel_code = System.IO.File.ReadAllText(kernel_name);
            const string c_name = "test";
            Device device = 0;
            int* hostbuffer = (int*)Marshal.AllocCoTaskMem(N * sizeof(int));
            Parallel.For(0, N, i =>
            {
                hostbuffer[i] = i;
            });
            //int* hostbuf2 = (int*)Marshal.AllocCoTaskMem(N * sizeof(int));
            Context context = Context.Create(device);

            //TestAllocatedHostPointer(hostbuffer, device, context, kernel_code, c_name);
            Console.WriteLine("host");
            TestUsedMemory(hostbuffer, device, context, kernel_code, c_name);
            Parallel.For(0, N, i =>
           {
               hostbuffer[i] = i;
           });
            Console.WriteLine("device");
            TestUsedMemory(hostbuffer, device, context, kernel_code, c_name);
            Marshal.FreeCoTaskMem((IntPtr)hostbuffer);
            //Marshal.FreeCoTaskMem((IntPtr)hostbuf2);
        }
    }
}

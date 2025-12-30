
namespace NOpenCL.CleanUp
{
    using System;
    using Buffer = NOpenCL.Buffer;
    /// <summary>
    /// Принудительная очистка ресурсов
    /// </summary>
    public static class ForcedResourceCleanup
    {
        public static void Clear(ref NOpenCL.Buffer buffer)
        {
            if (buffer != null)
            {
                int gen_ = GC.GetGeneration(buffer);
                buffer.Dispose();
                buffer = null;
                GC.Collect(gen_, GCCollectionMode.Optimized, false, true);
                GC.WaitForPendingFinalizers();

            }
        }
    }
}

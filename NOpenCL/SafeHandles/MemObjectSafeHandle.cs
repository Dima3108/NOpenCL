﻿namespace NOpenCL.SafeHandles
{
    using Microsoft.Win32.SafeHandles;
    using ErrorCode = NOpenCL.UnsafeNativeMethods.ErrorCode;

    public abstract class MemObjectSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected MemObjectSafeHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            ErrorCode result = UnsafeNativeMethods.clReleaseMemObject(handle);
            return result == ErrorCode.Success;
        }
    }
}
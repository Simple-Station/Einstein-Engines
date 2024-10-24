namespace Xilium.CefGlue.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential, Pack = libcef.ALIGN)]
    internal unsafe struct cef_base_scoped_t
    {
        internal UIntPtr _size;
        internal delegate* unmanaged<cef_base_scoped_t*, void> _del;
    }
}

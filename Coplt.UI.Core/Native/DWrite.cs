using System.Runtime.InteropServices;
using Coplt.Com;

namespace Coplt.UI.Native;

using unsafe fp_DWriteCreateFactory = delegate* unmanaged[Cdecl]<DWriteFactoryType, Guid*, void**, HRESULT>;

internal enum DWriteFactoryType
{
    Shared,
    Isolated,
}

internal static unsafe class DWrite
{
    public static readonly Guid REFIID_IDWriteFactory7 = new("35D0E0B3-9076-4D2E-A016-A91B568A06B4");

    public static void* Load()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;

        fp_DWriteCreateFactory DWriteCreateFactory;
        if (NativeLibrary.TryLoad("DWriteCore.dll", out var dwrite_handle))
        {
            DWriteCreateFactory = (fp_DWriteCreateFactory)NativeLibrary.GetExport(dwrite_handle, "DWriteCoreCreateFactory");
        }
        else
        {
            dwrite_handle = NativeLibrary.Load("DWrite.dll");
            DWriteCreateFactory = (fp_DWriteCreateFactory)NativeLibrary.GetExport(dwrite_handle, "DWriteCreateFactory");
        }
        var REFIID_IDWriteFactory7 = DWrite.REFIID_IDWriteFactory7;
        void* output;
        new HResult(DWriteCreateFactory(DWriteFactoryType.Shared, &REFIID_IDWriteFactory7, &output)).TryThrow();
        return output;
    }
}

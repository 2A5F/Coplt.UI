using System.Runtime.InteropServices;

namespace Coplt.UI.Collections;

public static class CollectionUtils
{
    #region AsSpan

    extension<T>(List<T> list)
    {
        public Span<T> Span => CollectionsMarshal.AsSpan(list);
    }

    #endregion
}

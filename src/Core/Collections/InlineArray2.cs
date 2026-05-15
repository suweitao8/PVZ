using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MegaCrit.Sts2.Core.Collections;

/// <summary>
/// Compiler-generated inline array with 2 elements.
/// Used for efficient fixed-size array operations.
/// </summary>
[StructLayout(LayoutKind.Auto)]
[InlineArray(2)]
internal struct InlineArray2<T>
{
    [CompilerGenerated]
    private T _element0;
}

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MegaCrit.Sts2.Core.Collections;

/// <summary>
/// Compiler-generated inline array with 4 elements.
/// Used for efficient fixed-size array operations.
/// </summary>
[StructLayout(LayoutKind.Auto)]
[InlineArray(4)]
internal struct InlineArray4<T>
{
    [CompilerGenerated]
    private T _element0;
}

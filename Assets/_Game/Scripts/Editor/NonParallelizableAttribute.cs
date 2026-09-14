using System;

/// <summary>
/// Source-compatibility marker for audit fixtures on the NUnit build bundled
/// with this Unity project. This NUnit version does not expose the newer
/// NonParallelizableAttribute type, and its ParallelizableAttribute is sealed.
///
/// Unity/NUnit test execution is non-parallel by default unless tests are
/// explicitly marked parallelizable, so this marker preserves the current
/// project behavior without changing gameplay/runtime code.
/// </summary>
[AttributeUsage(
    AttributeTargets.Assembly |
    AttributeTargets.Class |
    AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true
)]
internal sealed class NonParallelizableAttribute : Attribute
{
}

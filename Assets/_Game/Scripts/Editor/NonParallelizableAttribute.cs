using System;
using NUnit.Framework;

/// <summary>
/// Compatibility shim for the NUnit version bundled with this Unity project.
///
/// The project uses Unity Test Framework 1.6.0 / com.unity.ext.nunit 2.0.5,
/// whose NUnit API supports Parallelizable(ParallelScope.None) but does not
/// expose the newer NonParallelizableAttribute shorthand used by several
/// gameplay-audit fixtures.
///
/// NUnit's native NonParallelizableAttribute is equivalent to
/// Parallelizable(ParallelScope.None), so deriving from the existing attribute
/// preserves the intended test scheduling without changing production code.
/// </summary>
[AttributeUsage(
    AttributeTargets.Assembly |
    AttributeTargets.Class |
    AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true
)]
internal sealed class NonParallelizableAttribute : ParallelizableAttribute
{
    public NonParallelizableAttribute()
        : base(ParallelScope.None)
    {
    }
}

using System;

namespace Abandoned.Core
{
    /// <summary>
    /// Marks a serialized object reference that may legitimately be left empty, so the content
    /// validator doesn't report it as unwired.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OptionalReferenceAttribute : Attribute
    {
    }
}

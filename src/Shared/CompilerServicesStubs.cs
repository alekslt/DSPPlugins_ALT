using System.Diagnostics;

namespace System.Runtime.CompilerServices
{
    [Conditional("VERBOSE_LOG")]
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    sealed class CallerMemberNameAttribute : Attribute
    {
        public CallerMemberNameAttribute()
        {
        }
    }

    [Conditional("VERBOSE_LOG")]
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    sealed class CallerFilePathAttribute : Attribute
    {
        public CallerFilePathAttribute()
        {
        }
    }

    [Conditional("VERBOSE_LOG")]
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    sealed class CallerLineNumberAttribute : Attribute
    {
        public CallerLineNumberAttribute()
        {
        }
    }
}
using System.Diagnostics;

namespace System
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    readonly struct RealFakeSpan
    {
        readonly string _original;
        readonly int _start;
        readonly int _end;

        public RealFakeSpan(string original) : this(original, 0, original.Length)
        {
        }

        public RealFakeSpan(string original, int start, int length)
        {
            _original = original;
            _start = start;
            _end = start + length;
        }

        public RealFakeSpan Slice(int start)
        {
            if ((uint)start > (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            return new RealFakeSpan(_original, _start + start, Length - start);
        }

        public RealFakeSpan Slice(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if ((uint)Length < (uint)length + (uint)start)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            return new RealFakeSpan(_original, _start + start, length);
        }

        public int IndexOfAny(char[] anyOf)
        {
            return _original.IndexOfAny(anyOf, _start, Length) - _start;
        }

        public int IndexOf(char value)
        {
            return _original.IndexOf(value, _start, Length) - _start;
        }

        public int IndexOf(string value)
        {
            return _original.IndexOf(value, _start, Length) - _start;
        }

        string DebuggerDisplay { get => AsString(); }

        public string AsString()
        {
            return _original.Substring(_start, Length);
        }

        public int Length { get => _end - _start; }
    }
}

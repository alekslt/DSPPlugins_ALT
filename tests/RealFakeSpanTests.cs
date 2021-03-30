using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    class RealFakeSpanTests
    {
        [TestCase("RealFakeSpan", 0, 12, "RealFakeSpan")]
        [TestCase("RealFakeSpan", 5, 7, "akeSpan")]
        [TestCase("RealFakeSpan", 1, 2, "ea")]
        public void Init(string original, int start, int length, string expected)
        {
            var span = new RealFakeSpan(original, start, length);

            Assert.AreEqual(length, span.Length);
            Assert.AreEqual(expected, span.AsString());
        }

        [Test]
        public void Slice()
        {
            var span = new RealFakeSpan("StringHere");

            Assert.AreEqual(
                "StringHere".Substring(1),
                span.Slice(1).AsString());

            Assert.AreEqual(
                "StringHere".Substring(3, 5), 
                span.Slice(3, 5).AsString());

            Assert.AreEqual(
                "StringHere".Substring(0, 3),
                span.Slice(0, 3).AsString());
        }

        [Test]
        public void IndexOf()
        {
            string str = "Lots Of Characters ";
            var span = new RealFakeSpan(str);

            Assert.AreEqual(str.IndexOf(' '), span.IndexOf(' '));

            char[] ch = new[] { 'o', 'O' };

            Assert.AreEqual(str.IndexOfAny(ch), span.IndexOfAny(ch));

            Assert.AreEqual(str.IndexOf("Ch"), span.IndexOf("Ch"));
        }
    }
}

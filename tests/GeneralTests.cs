using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    class GeneralTests
    {
        [Test]
        public void CompilerServicesStubs()
        {
            CallMe();// This is lineNo: 17
        }

        void CallMe([CallerMemberName] string member = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNo = 0)
        {
            Assert.AreEqual(nameof(CompilerServicesStubs), member);

            Assert.That(filePath, Does.EndWith(nameof(GeneralTests) + ".cs"));
            Assert.AreEqual(17, lineNo);
        }

        [Test]
        public void TestEmptyArray()
        {
            int[] arr = ArrayUtil.Empty<int>();
            Assert.AreSame(arr, ArrayUtil.Empty<int>());

            uint[] arr2 = ArrayUtil.Empty<uint>();
            Assert.AreNotSame(arr, arr2);

            Assert.AreSame(arr2, ArrayUtil.Empty<uint>());
        }
    }
}

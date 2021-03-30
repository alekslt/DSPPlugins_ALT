using NUnit.Framework;

using WhatTheBreak;

namespace Tests
{
    [TestFixture]
    public class WhatTheBreakTests
    {
        [Test]
        public void Test1()
        {
            const string data = @"(wrapper dynamic-method) UniverseSimulator.DMD<UniverseSimulator..OnGameBegin> (UniverseSimulator) <0x00031>
(wrapper dynamic-method) GameMain.DMD<GameMain..Begin> () <0x000c0>
GameLoader.FixedUpdate () <0x0027e>";

            int count = 0;
            new StacktraceParser().ParseStackTraceLines(data, (type, method) => {
                switch (count)
                {
                    case 0:
                        Assert.AreEqual("UniverseSimulator", type);
                        Assert.AreEqual("OnGameBegin", method);
                        break;

                    case 1:
                        Assert.AreEqual("GameMain", type);
                        Assert.AreEqual("Begin", method);
                        break;
                    case 2:
                        Assert.Fail("This shouldn't be reached: {0} {1}", type, method);
                        break;
                }
                ++count;
            });

            Assert.AreEqual(2, count);
        }

        [Test]
        public void Test2()
        {
            const string data = @"at OrderSaves.OrderSavesPlugin.OnListRefreshed (System.Collections.Generic.List`1<UIGameSaveEntry>) <0x00031>
at OrderSaves.OrderSavesPlugin/Patches.RefreshList (System.Collections.Generic.List`1<UIGameSaveEntry>) <0x00037>
at (wrapper dynamic-method) UILoadGameWindow.DMD<UILoadGameWindow..RefreshList> (UILoadGameWindow) <0x0033b>
at UILoadGameWindow._OnOpen () <0x0001c>
at ManualBehaviour._Open () <0x000b5>";

            int count = 0;
            new StacktraceParser().ParseStackTraceLines(data, (type, method) => {
                switch (count)
                {
                    case 0:
                        Assert.AreEqual("UILoadGameWindow", type);
                        Assert.AreEqual("RefreshList", method);
                        break;

                    case 1:
                        Assert.Fail("This shouldn't be reached: {0} {1}", type, method);
                        break;
                }
                ++count;
            });

            Assert.AreEqual(1, count);
        }
    }
}
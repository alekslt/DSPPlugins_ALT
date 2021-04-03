using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace TestILPatch
{
    class Program
    {
        public int uiLayoutHeight;
        public int buildingViewMode;

        public void OrigMethod(int a)
        {
            this.buildingViewMode = 5;
            this.uiLayoutHeight = a;
            if (this.uiLayoutHeight > 1080) // 0x438
            {
                this.uiLayoutHeight = 1080;
            }
            if (this.uiLayoutHeight < 900) // 0x384
            {
                this.uiLayoutHeight = 900;
            }
        }

        public static void FixUILayoutHeightIL(ILContext il)
        {
            Console.WriteLine(il.ToString());

            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchStfld(typeof(Program).GetField("uiLayoutHeight")),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(Program).GetField("uiLayoutHeight")),
                x => x.MatchLdcI4(0x438)
            );
            c.Goto(c.Prev);
            //Console.WriteLine(c.ToString());
            
            //c.GotoPrev();
            //c.Remove();
            c.Remove();
            c.Emit(OpCodes.Ldc_I4, 5000);

            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdcI4(0x438)
            );
            c.Goto(c.Prev);
            //Console.WriteLine(c.ToString());
            c.Remove();
            c.Emit(OpCodes.Ldc_I4, 5000);


            //Console.WriteLine(il.ToString());
        }


        private static ILHook _changeWalkSpeedHook = new ILHook(typeof(Program).GetMethod("OrigMethod"), new ILContext.Manipulator(FixUILayoutHeightIL), new ILHookConfig {
            ManualApply = true
        });

        public void Tester(int value)
        {
            //uiLayoutHeight = value;
            OrigMethod(value);
            Console.WriteLine("Set value: " + value + ". After Method: " + uiLayoutHeight);
        }

        static void Main(string[] args)
        {


            Program a = new Program();

            a.Tester(10);
            a.Tester(1000);
            a.Tester(4000);

            _changeWalkSpeedHook.Apply();

            a.Tester(10);
            a.Tester(1000);
            a.Tester(4000);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace VeinPlanter.Patches
{
    [HarmonyPatch(typeof(PlayerAction_Mine))]
    public class Patch_PlayerAction_Mine
    {
        static void PrintDebugInfo(IEnumerable<CodeInstruction> instList)
        {
            int i = 0;
            foreach (var inst in instList)
            {
                Console.WriteLine(i++ + " " + inst.ToString());
            }
        }

        [HarmonyPatch("GameTick"), HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

            //PrintDebugInfo(matcher.InstructionEnumeration());
            matcher.Start();
            int locS = 21;

            // Go to start of mining block


            /*
            [Info   :   Console] 323 ldarg.0 NULL [Label15]
[Info   :   Console] 324 ldfld EObjectType PlayerAction_Mine::miningType
[Info   :   Console] 325 ldc.i4.2 NULL
[Info   :   Console] 326 bne.un Label26
[Info   :   Console] 327 ldloc.1 NULL
[Info   :   Console] 328 ldarg.0 NULL
[Info   :   Console] 329 ldfld System.Int32 PlayerAction_Mine::miningId
[Info   :   Console] 330 callvirt VeinData PlanetFactory::GetVeinData(System.Int32 id)
[Info   :   Console] 331 stloc.s 21 (VeinData)
            */


            // if (this.miningType == EObjectType.Vein)
            // 181 ldfld TestILPatch.EObjectType TestILPatch.PlayerActionMine::miningType
            matcher.
                MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerAction_Mine), nameof(PlayerAction_Mine.miningType))),
                    new CodeMatch(OpCodes.Ldc_I4_2),
                    new CodeMatch(OpCodes.Bne_Un),
                    new CodeMatch(OpCodes.Ldloc_1)
                    );
            
            
            Debug.Log("Found start of mining veins: " + matcher.Pos + " " + matcher.Instruction);

            /*
            268 ldarg.0 NULL
            269 ldfld TestILPatch.Player TestILPatch.PlayerAction::player
            270 ldloc.s 24 (TestILPatch.VeinProto)
            271 ldfld System.Int32 TestILPatch.VeinProto::MiningItem
            */

            /*
            283 nop NULL
            284 ldloc.s 24 (TestILPatch.VeinProto)
            285 ldfld System.Int32 TestILPatch.VeinProto::MiningItem
            286 ldloc.s 27 (System.Int32)
            287 call static System.Void TestILPatch.UIItemup::Up(System.Int32 miningItem, System.Int32 num12)
            288 nop NULL
            289 ldloc.s 24 (TestILPatch.VeinProto)
            290 ldfld System.Int32 TestILPatch.VeinProto::MiningItem
            291 ldloc.s 27 (System.Int32)
            */

            for (int i = 0; i < 3; i++)
            {
                matcher.
                    MatchForward(true,
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(VeinProto), nameof(VeinProto.MiningItem)))
                        ); // 0

                // 192 stloc.s 23 (TestILPatch.VeinData)

                Debug.Log("Patching miningItem to ProductId on: " + matcher.Pos + " " + matcher.Instruction);

                matcher.
                    Advance(-1). 
                    SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 21)).
                    SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(VeinData), nameof(VeinData.productId))));
            }

            //PrintDebugInfo(matcher.InstructionEnumeration());
            return matcher.InstructionEnumeration();
        }

    }
}

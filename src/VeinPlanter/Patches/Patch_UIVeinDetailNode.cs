using HarmonyLib;
using System.Linq;

namespace VeinPlanter
{
    [HarmonyPatch(typeof(UIVeinDetailNode))]
    public static class Patch_UIVeinDetailNode
    {

        static bool ShouldAbortSanityChecks(UIVeinDetailNode __instance, out PlanetData.VeinGroup veinGroup, out ItemProto itemProto)
        {
            veinGroup = default(PlanetData.VeinGroup);
            itemProto = null;

            if (__instance.inspectPlanet == null)
            {
                return true;
            }
            veinGroup = __instance.inspectPlanet.veinGroups[__instance.veinGroupIndex];
            if (veinGroup.count == 0 || veinGroup.type != EVeinType.Oil)
            {
                return true;
            }

            var prodId = (from vein in __instance.inspectPlanet.factory.veinPool where vein.groupIndex == __instance.veinGroupIndex select vein.productId).First();
            itemProto = LDB.items.Select(prodId);

            return false;
        }

        [HarmonyPostfix(), HarmonyPatch("Refresh")]
        public static void Refresh(UIVeinDetailNode __instance)
        {
            if (ShouldAbortSanityChecks(__instance, out PlanetData.VeinGroup veinGroup, out ItemProto itemProto)) { return; }

            __instance.veinIcon.sprite = itemProto.iconSprite;
            __instance.infoText.text = string.Concat(new string[]
            {
                    veinGroup.count.ToString(), "空格个".Translate(), itemProto.name, "产量".Translate(),
                    ((float)veinGroup.amount * VeinData.oilSpeedMultiplier).ToString("0.00"), "/s"
            });
        }

        [HarmonyPostfix(), HarmonyPatch("_OnUpdate")]
        public static void _OnUpdate(UIVeinDetailNode __instance)
        {
            if (ShouldAbortSanityChecks(__instance, out PlanetData.VeinGroup veinGroup, out ItemProto itemProto)) { return; }
            __instance.infoText.text = string.Concat(new string[]
            {
                    veinGroup.count.ToString(), "空格个".Translate(), itemProto.name, "产量".Translate(),
                    ((float)veinGroup.amount * VeinData.oilSpeedMultiplier).ToString("0.00"), "/s"
            });
        }
        /*

        [HarmonyPatch("_OnUpdate")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            return new CodeMatcher(instructions)
                .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "oilSpeedMultiplier"))
                .MatchBack(false, // false = move at the start of the match, true = move at the end of the match
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "veinProto"))
                .SetAndAdvance(OpCodes.Ldstr, "Test")
                .SetOpcodeAndAdvance(OpCodes.Nop)
                .SetOpcodeAndAdvance(OpCodes.Nop)
                .InstructionEnumeration();*/

        /*
        return new CodeMatcher(instructions)
            .MatchForward(false, // false = move at the start of the match, true = move at the end of the match
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "veinIcon"))
            .MatchBack(false, // false = move at the start of the match, true = move at the end of the match
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "veinProto"))
            .SetAndAdvance(OpCodes.Ldstr, "Test")
            .SetOpcodeAndAdvance(OpCodes.Nop)
            .SetOpcodeAndAdvance(OpCodes.Nop)
            .InstructionEnumeration();


        return new CodeMatcher(instructions)
         .InstructionEnumeration();

    }
        */
    }

    
}

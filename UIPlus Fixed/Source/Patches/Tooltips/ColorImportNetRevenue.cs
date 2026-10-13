using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Offworld.GameCore;
using Offworld.GameCore.Text;

namespace UIPlus.Patches.Tooltips
{
	[HarmonyPatch(typeof(HUDTooltipBuilder))]
	[HarmonyPatch("Import")]
	class ColorImportNetRevenue
	{
		static void makeColoredNetRevenue(TooltipCostData costData, PlayerClient player, int iSlot)
		{
			int netRevenue = player.getImportNetRevenue(iSlot);
			costData.totalCostValue = HUDTooltipBuilder.TEXT(
				(netRevenue >= 0) ? "TEXT_HUDINFO_NET_REVENUE_POSTIVE" : "TEXT_HUDINFO_NET_REVENUE_NEGATIVE",
				new TextVariable[] {
					costData.totalCostValue.ToText()
				});
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo get_instance_mi = AccessTools.Property(typeof(TooltipControl), "instance").GetGetMethod();

			int i = 0;
			foreach (var code in instructions) {
				++i;

				if (code.opcode == OpCodes.Call) {
					MethodInfo method = code.operand as MethodInfo;
					if (method != null && method == get_instance_mi) { // TooltipControl::instance
						yield return new CodeInstruction(OpCodes.Ldloc_3); // tooltipCostData
						yield return new CodeInstruction(OpCodes.Ldloc_2); // player
						yield return new CodeInstruction(OpCodes.Ldarg_1); // iSlot
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ColorImportNetRevenue), "makeColoredNetRevenue")); // ColorImportNetRevenue::makeColoredNetRevenue(tooltipCostData, player, iSlot)
					}
				}

				yield return code;
			}
		}
	}
}

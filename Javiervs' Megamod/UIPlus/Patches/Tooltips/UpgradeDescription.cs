using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Offworld.GameCore;
using Offworld.UICore;

namespace UIPlus.Patches.Tooltips
{
	[HarmonyPatch(typeof(HUDTooltipBuilder))]
	[HarmonyPatch("UpgradeDescription")]
	class UpgradeDescription
	{
		// Inject resource icons for life support into upgrade tooltip
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var inforesource_mename_fi = AccessTools.Field(typeof(InfoResource), "meName");
			var inforesource_mealtname_fi = AccessTools.Field(typeof(InfoResource), "meAltName");
			var hudtooptipbuilder_text_mi = AccessTools.Method(typeof(HUDTooltipBuilder), "TEXT", new Type[] { typeof(TextType) });

			var func = AccessTools.Method(typeof(UpgradeDescription), "GetIconAndText", new Type[] { typeof(InfoResource), typeof(bool) });

			foreach (var code in instructions) {
				if (code.opcode == OpCodes.Ldfld) {
					var field = code.operand as FieldInfo;
					if (field == inforesource_mename_fi) {
						yield return new CodeInstruction(OpCodes.Ldc_I4_0);
						yield return new CodeInstruction(OpCodes.Call, func);
						yield return Nop(code);
						continue;
					} else if (field == inforesource_mealtname_fi) {
						yield return new CodeInstruction(OpCodes.Ldc_I4_1);
						yield return new CodeInstruction(OpCodes.Call, func);
						yield return Nop(code);
						continue;
					}
				} else if (code.opcode == OpCodes.Call) {
					var method = code.operand as MethodInfo;
					if (method == hudtooptipbuilder_text_mi) {
						code.opcode = OpCodes.Nop;
						code.operand = null;
						yield return code;
						continue;
					}
				}

				yield return code;
			}
		}

		static CodeInstruction Nop(CodeInstruction code)
		{
			code.opcode = OpCodes.Nop;
			code.operand = null;
			return code;
		}

		public static string GetIconAndText(InfoResource resource, bool useAlternative)
		{
			return GuiUtils.resourceUnityIconAndTextString(resource.meType, useAlternative);
		}
	}
}

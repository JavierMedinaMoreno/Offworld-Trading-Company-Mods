using System;
using Harmony;
using Offworld.GameCore.Text;

namespace UIPlus.Patches.Tooltips
{
	[HarmonyPatch(typeof(HUDTooltipBuilder))]
	[HarmonyPatch("TEXT")]
	[HarmonyPatch(new Type[] { typeof(string), typeof(TextVariable[]) })]
	class Debt
	{
		static void Prefix(ref string key, TextVariable[] arguments)
		{
			if (key == "TEXT_HUDINFO_DEBT_CHANGE") {
				UnityEngine.Debug.Assert(arguments.Length == 1);

				string value = (string)AccessTools.Field(arguments[0].GetType(), "mzValue").GetValue(arguments[0]);
				if (value.Contains("-")) {
					key = "TEXT_HUDINFO_DEBT_CHANGE_NEGATIVE";
				} else {
					key = "TEXT_HUDINFO_DEBT_CHANGE_POSITIVE";
				}
			}
		}
	}
}

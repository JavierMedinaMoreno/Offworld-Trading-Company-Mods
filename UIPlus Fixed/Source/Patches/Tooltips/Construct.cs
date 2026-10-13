using System;
using Harmony;
using Offworld.GameCore.Text;

namespace UIPlus.Patches.Tooltips
{
	[HarmonyPatch(typeof(HUDTooltipBuilder))]
	[HarmonyPatch("TEXT")]
	[HarmonyPatch(new Type[] { typeof(string), typeof(TextVariable[]) })]
	class Construct
	{
		static void Prefix(ref string key, TextVariable[] arguments)
		{
			if (key == "TEXT_HUDHELP_CONSTRUCT_RESOURCE_NET_REVENUE") {
				UnityEngine.Debug.Assert(arguments.Length == 1);

				long value = (long)AccessTools.Field(arguments[0].GetType(), "miValue").GetValue(arguments[0]);
				if (value > 0) {
					key = "TEXT_HUDHELP_CONSTRUCT_RESOURCE_NET_REVENUE_POSITIVE";
				} else if (value < 0) {
					key = "TEXT_HUDHELP_CONSTRUCT_RESOURCE_NET_REVENUE_NEGATIVE";
				}
			}
		}
	}
}

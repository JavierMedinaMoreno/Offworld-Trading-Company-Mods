using System;
using Harmony;
using Offworld.GameCore.Text;

namespace UIPlus.Patches.Tooltips
{
	public class TerrainConstructionSpeed
	{
		[HarmonyPatch(typeof(HUDPanel))]
		[HarmonyPatch("TEXT")]
		[HarmonyPatch(new Type[] { typeof(string), typeof(TextVariable[]) })]
		class Construct
		{
			static void Prefix(ref string key, TextVariable[] arguments)
			{
				if (key == "TEXT_HUDINFO_TERRAIN_CONSTRUCTION_RATE") {
					UnityEngine.Debug.Assert(arguments.Length == 2);

					string value = (string)AccessTools.Field(arguments[1].GetType(), "mzValue").GetValue(arguments[1]);
					if (value.Contains("-")) {
						key = "TEXT_HUDINFO_TERRAIN_CONSTRUCTION_RATE_NEGATIVE";
					} else {
						key = "TEXT_HUDINFO_TERRAIN_CONSTRUCTION_RATE_POSITIVE";
					}
				}
			}
		}
	}
}

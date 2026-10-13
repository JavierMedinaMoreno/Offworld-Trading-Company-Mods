using System;
using Harmony;
using Offworld.GameCore;

namespace UIPlus.NoArtPacks
{
	// PatchLocalArtPacks removed due to Unity 2022 try-finally IL opcode incompatibility

	[HarmonyPatch(typeof(PlayerClient))]
	[HarmonyPatch("getArtPackList")]
	[HarmonyPatch(new Type[] { })]
	class Patch_PlayerClient_getArtPackList
	{
		static bool Prefix(ref ArtPackList __result)
		{
			if (ModOptionsSave.Options.disableArtPacks) {
				__result = new ArtPackList();
				return false;
			}
			return true;
		}
	}
}

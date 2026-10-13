using Harmony;
using ModUtils;
using Offworld.GameCore;

namespace UIPlus.Patches
{
	[HarmonyPatch(typeof(LobbyHelpers))]
	[HarmonyPatch("UpdateLobbyData")]
	public class LobbyCasterModeVariable_Write
	{
		static void Prefix(ref MLobbySettings settings)
		{
			foreach (InfoGameOption gameOption in Globals.Infos.gameOptions()) {
				if (gameOption.mzType == "UIPLUS_GAMEOPTION_CASTER_MODE") {
					settings.GameOptions[gameOption.miType] = ModOptionsSave.Options.casterMode;
				}
			}
		}
	}

	// Harmony patch on MLobbySettings.CopyValues removed due to Unity 2022 try-finally IL opcode incompatibility
	public class LobbyCasterModeVariable_Read
	{
		public static bool? lobbyCasterMode = false;
	}
}

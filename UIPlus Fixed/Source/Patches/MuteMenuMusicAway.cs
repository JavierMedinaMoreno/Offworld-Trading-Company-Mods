using Harmony;

namespace UIPlus.Patches
{
	[HarmonyPatch(typeof(MainScreenListener))]
	[HarmonyPatch("stopMusic")]
	class MuteMenuMusicAway_stopMusic
	{
		static bool Prefix()
		{
			if (AppMain.gApp.OptionsSave.GameOptions.muteMusicMenu)
				return true;

			if (AppMain.gApp.OptionsSave.GameOptions.muteMusicMenuAway && !RenderingSetup.IsApplicationFocused) {
				MAudioManager.IsMuted = true;
				return false;
			}
			MAudioManager.IsMuted = AppMain.gApp.OptionsSave.GameOptions.muteAudio;
			return true;
		}
	}

	[HarmonyPatch(typeof(MainScreenListener))]
	[HarmonyPatch("startMusic")]
	class MuteMenuMusicAway_startMusic
	{
		static bool Prefix()
		{
			if (AppMain.gApp.OptionsSave.GameOptions.muteMusicMenuAway) {
				MAudioManager.IsMuted = AppMain.gApp.OptionsSave.GameOptions.muteAudio;
			}
			return true;
		}	}
}

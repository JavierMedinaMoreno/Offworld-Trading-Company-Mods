using System;
using Harmony;
using Offworld.AppCore;
using Offworld.GameCore.Text;

namespace UIPlus.Patches.Tooltips
{
	[HarmonyPatch(typeof(MohawkEventHandler))]
	[HarmonyPatch("TEXT")]
	[HarmonyPatch(new Type[] { typeof(string), typeof(TextVariable[]) })]
	class Menu
	{
		static void Prefix(string key, ref TextVariable[] arguments)
		{
			if (key == "TEXT_GAME_MENU_MAP_SEED") {
				UnityEngine.Debug.Assert(arguments.Length == 1);

				Array.Resize(ref arguments, 2);
				arguments[1] = AppGlobals.GameHUDHelpers.GetHotkeyDisplayString(MyClass.KEYBINDING_COPY_SEED).ToText();
			}
		}	}
}

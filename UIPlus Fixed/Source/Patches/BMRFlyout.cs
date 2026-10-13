using System;
using Harmony;
using Offworld.GameCore;
using UnityEngine;

namespace UIPlus.Patches
{
	[HarmonyPatch(typeof(MainGameEventHandler))]
	[HarmonyPatch("OnHover")]
	[HarmonyPatch(new Type[] { typeof(GuiActionInfo), typeof(RectTransform) })]
	class BMRFlyout
	{
		static void Prefix(GuiActionInfo actionInfo, RectTransform rect)
		{
			if (actionInfo.data1 == (int)ItemType.RESOURCE_BUILDING_FLYOUT) {
				HUDResourcePanel panel = AppMain.gApp?.HUDDirector?.mainGameScreen?.resourcePanel;
				if (panel != null) {
					BuildMenuRevenue.showingRow = panel.GetResourceRow((ResourceType)actionInfo.data2);
				}
			}
		}
	}
}

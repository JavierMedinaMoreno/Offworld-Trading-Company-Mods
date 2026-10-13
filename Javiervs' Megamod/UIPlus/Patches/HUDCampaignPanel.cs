using System.Collections.Generic;
using Harmony;
using Offworld.GameCore;
using Offworld.GameCore.Text;

namespace UIPlus.Patches
{
	[HarmonyPatch(typeof(HUDCampaignPanel))]
	[HarmonyPatch("updateImportSlots")]
	class HUDCampaignPanel_ImportNetRevenueColor
	{
		static void Postfix(HUDCampaignPanel __instance)
		{
			if (AppMain.gApp.isPlayerOption(PlayerOptionType.SHOW_TOTAL_COSTS)) {
				Dictionary<int, TextButton> dictionaryImportButtons = (Dictionary<int, TextButton>)AccessTools.Field(typeof(HUDCampaignPanel), "dictionaryImportButtons").GetValue(__instance);
				PlayerClient player = AppMain.gApp.activePlayerClient();

				for (int iSlot = 0; iSlot < AppMain.gApp.gameClient().getNumImportSlots(); ++iSlot) {
					if (dictionaryImportButtons[iSlot].gameObject.activeSelf) {
						MohawkText labelSecondary = dictionaryImportButtons[iSlot].labelSecondary;
						int importNetRevenue = player.getImportNetRevenue(iSlot);

						labelSecondary.supportRichText = true;
						string[] text = labelSecondary.text.Split('\n');
						text[1] = HUDTooltipBuilder.TEXT(
										(importNetRevenue >= 0) ? "TEXT_HUDINFO_NET_REVENUE_POSTIVE" : "TEXT_HUDINFO_NET_REVENUE_NEGATIVE",
										new TextVariable[] {
											text[1].ToText()
										});
						labelSecondary.text = string.Join("\n", text);
					}
				}
			}
		}
	}
}

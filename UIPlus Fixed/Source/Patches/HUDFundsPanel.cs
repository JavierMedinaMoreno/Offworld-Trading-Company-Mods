using System;
using Harmony;
using Offworld.GameCore;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlus.Patches
{
	[HarmonyPatch(typeof(HUDFundsPanel))]
	[HarmonyPatch("Update")]
	[HarmonyPatch(new Type[] { })]
	class HUDFundsPanel_Update
	{
		static MohawkText textDebtChange;

		static void Postfix(HUDFundsPanel __instance)
		{
			if (textDebtChange == null) {
				textDebtChange = UnityEngine.Object.Instantiate(__instance.textDebtDollarSign);
				textDebtChange.transform.SetParent(__instance.textDebt.transform, false);
				textDebtChange.text = "-$999";
				LayoutElement le = textDebtChange.gameObject.GetComponent<LayoutElement>();
				if (le != null) {
					le.ignoreLayout = true;
				}
				textDebtChange.alignment = TextAnchor.MiddleLeft;
				textDebtChange.horizontalOverflow = HorizontalWrapMode.Overflow;
				textDebtChange.rectTransform.anchorMin = new Vector2(1f, 0f);
				textDebtChange.rectTransform.anchorMax = new Vector2(1f, 1f);
				textDebtChange.rectTransform.pivot = new Vector2(0f, 0.5f);
				textDebtChange.rectTransform.anchoredPosition = new Vector2(8f, 0f);
			}

			if (ModOptionsSave.Options.AllowedObserver() && ModOptionsSave.Options.showDebtChange) {
				int debtChange = GetDebtChange();
				if (debtChange == 0) {
					textDebtChange.text = "";
				} else {
					textDebtChange.text = (debtChange > 0 ? "-" : "+") + Utils.makeMoney(Math.Abs(debtChange));
				}
				textDebtChange.mohawkFontColor = debtChange > 0 ? MohawkText.FontColor.POSITIVE : MohawkText.FontColor.NEGATIVE;
			} else {
				textDebtChange.text = "";
			}
		}

		static int GetDebtChange()
		{
			AppMain gApp = AppMain.gApp;
			GameClient gameClient = gApp.gameClient();
			Infos infos = Globals.Infos;
			PlayerClient playerClient = gameClient.playerClient(gApp.getActivePlayer());

			int iValue = 0;
			for (ResourceType resourceType = ~ResourceType.NONE; resourceType < infos.resourcesNum(); ++resourceType) {
				InfoResource infoResource = infos.resource(resourceType);
				int resourceAutoPurchased = playerClient.getResourceAutoPurchased(resourceType);
				if (resourceAutoPurchased > 0) {
					iValue -= resourceAutoPurchased;
				} else if (!infoResource.mbTrade) {
					int resourceAutoSold = playerClient.getResourceAutoSold(resourceType);
					if (resourceAutoSold > 0) {
						iValue += resourceAutoSold;
					}
				}
			}
			return iValue;
		}	}
}

using System;
using System.Collections.Generic;
using Harmony;
using Offworld.AppCore;
using Offworld.GameCore;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlus.Patches
{
	public static class HUDPlayerListController_Update
	{
		public static bool ShowSpecialPlayerQueues { get; set; } = true;
		public static bool HideSpecialPlayerQueues { get; set; }

		public static void UpdateQueues()
		{
			bool show = ShowSpecialPlayerQueues;
			if (HideSpecialPlayerQueues)
				show = !show;

			foreach (var q in HUDPlayerListController_CreatePlayer.specialPlayerQueues) {
				if (q != null && q.activeSelf != show)
					q.SetActive(show);
			}
		}
	}

	public static class HUDPlayerListController_CreatePlayer
	{
		public static List<GameObject> specialPlayerQueues = new List<GameObject>();
	}

	[HarmonyPatch(typeof(HUDPlayerListing))]
	[HarmonyPatch("DoFirstUpdate")]
	class HUDPlayerListing_DoFirstUpdate_Patch
	{
		static void Postfix(HUDPlayerListing __instance)
		{
			var specialQueue = UnityEngine.Object.FindObjectOfType<HUDSpecialBuildingQueue>();
			if (specialQueue != null && __instance.textBuyoutPercent != null && __instance.textBuyoutPercent.transform.parent != null) {
				if (AppMain.gApp == null) return;
				PlayerClient player = AppMain.gApp.gameClient()?.playerClient(__instance.playerType);
				if (player == null) return;

				GameObject newGO = MohawkUI.CloneUI(specialQueue.gameObject, __instance.textBuyoutPercent.transform.parent.gameObject);
				if (newGO == null) return;

				var oldSpecialQueue = newGO.GetComponentInChildren<HUDSpecialBuildingQueue>();
				if (oldSpecialQueue == null) return;

				HUDSpecialBuildingQueuePlayerList queue = oldSpecialQueue.gameObject.GetComponent<HUDSpecialBuildingQueuePlayerList>();
				if (queue == null)
					queue = oldSpecialQueue.gameObject.AddComponent<HUDSpecialBuildingQueuePlayerList>();
				queue.SetPlayer(player);
				queue.buildingArt = oldSpecialQueue.buildingArt;
				HUDPlayerListController_CreatePlayer.specialPlayerQueues.Add(newGO);
				oldSpecialQueue.enabled = false;

				// Moves pivot without moving position
				var rectTransform = newGO.GetComponent<RectTransform>();
				var pivot = new Vector2(1, rectTransform.pivot.y);
				Vector2 size = rectTransform.rect.size;
				Vector2 deltaPivot = rectTransform.pivot - pivot;
				Vector3 deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
				rectTransform.pivot = pivot;
				rectTransform.localPosition -= deltaPosition;

				newGO.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 84);
				newGO.GetComponent<RectTransform>().anchoredPosition += Vector2.up * 32 + Vector2.left * 90;
				newGO.GetComponent<RectTransform>().localScale = new Vector3(0.79f, 0.79f, 1.0f);
				newGO.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.UpperRight;
			}
		}
	}

	[HarmonyPatch(typeof(HUDPlayerListController))]
	[HarmonyPatch("GetPlayerName")]
	class HUDPlayerListController_GetPlayerName
	{
		private static bool showAllDebt;
		public static bool ShowAllDebt
		{
			get
			{
				return showAllDebt;
			}
			set
			{
				showAllDebt = ModOptionsSave.Options.AllowedObserver() && value;
			}
		}

		static void Postfix(ref string __result, PlayerClient player)
		{
			if (player.getBondRating() != BondType.NONE) {
				if (ShowAllDebt) {
					int startIndex = __result.IndexOf('|') + 1;
					int endIndex = __result.IndexOf('|', startIndex);
					__result = __result.Remove(startIndex, endIndex - startIndex);
					__result = __result.Insert(startIndex, Utils.makeMoney(Mathf.Abs(player.getDebt()), false, true, true, false));
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using ModUtils;
using Offworld.AppCore;
using Offworld.GameCore;
using Offworld.SystemCore;
using Offworld.UICore;
using UnityEngine;
using UnityEngine.UI;

namespace UIPlus
{
	public class BuildMenuRevenue : MonoBehaviour
	{
		private static HUDFlyoutBuild flyoutBuild;
		private Dictionary<BuildingType, TextButton> buildButtonDictionary;
		public static HUDResourceRow showingRow;

		public static void Install()
		{
			BuildMenuRevenue.flyoutBuild = UnityEngine.Object.FindObjectOfType<HUDFlyoutBuild>();
			flyoutBuild.gameObject.AddComponent<BuildMenuRevenue>();
		}

		public void LateUpdate()
		{
			if (ModOptionsSave.Options.AllowedObserver()) {
				if (showingRow != null) {
					var row = showingRow;
					if (row.stackBuildButtons.activeSelf) {
						buildButtonDictionary = (Dictionary<BuildingType, TextButton>)InjectorUtils.GetPrivateMember(row, "buildButtonDictionary");
						foreach (var kv in buildButtonDictionary) {
							kv.Value.label.alignment = TextAnchor.LowerCenter;
							if (kv.Value.label.GetComponent<MohawkOutline>() == null) {
								MohawkOutline outline = kv.Value.label.gameObject.AddComponent<MohawkOutline>();
							}
							updateBuildingButtonState(row.resource, kv.Key, kv.Value);
						}
					}
				}
			}

			/*
			if (MyClass.selectedBuildingPanel != null) {
				GridLayoutGroup gridLayout = (GridLayoutGroup)InjectorUtils.GetPrivateMember(MyClass.selectedBuildingPanel, "buildingGridLayout");
				if (gridLayout != null) {
					int num = 0;
					for (int index = 0; index < gridLayout.transform.childCount; ++index) {
						if (gridLayout.transform.GetChild(index).gameObject.activeSelf)
							++num;
					}
					int count = 1;
					if (num > 7)
						count = 2;
					if (num > 14)
						count = 3;
					gridLayout.constraintCount = count;
				}
			}
			*/
		}

		private static void updateBuildingButtonState(ResourceType resource, BuildingType building, TextButton button)
		{
			if (!AppMain.gApp.isPlayerOption(PlayerOptionType.SHOW_BUILDING_NET)) {
				button.label.text = "";
				return;
			}

			GameClient client = AppGlobals.GameGlobals.GameClient;

			ResourceLevelType highestResourceLevel = client.getHighestResourceLevel();
			int num1 = 0;
			if (Globals.Infos.building(building).maiResourceMining[(int)resource] > 0) {
				if (!Globals.Infos.resource(resource).mabLocationInvalid[(int)client.getLocation()]) {
					int num2 = client.resourceMiningLevel(building, resource, highestResourceLevel, TerrainType.NONE, AppMain.gApp.getActivePlayer(), Globals.Infos.building(building).maiResourceMining[(int)resource], 0, false);
					if (num2 > 0)
						num1 += Math.Max(num1, num2 * client.marketClient().getWholePrice(resource) / 1000);
				}
			} else {
				PlayerClient playerClient = client.playerClient(AppMain.gApp.getActivePlayer());
				int num2 = !AppMain.gApp.isPlayerOption(PlayerOptionType.SHOW_NET_REVENUE_ALL) ? 0 : 100;
				num1 += client.calculateRevenue(building, (TileClient)null, playerClient.getPlayer(), 0, false, true, num2, num2, 100, false);
			}
			string str = Utils.makeMoney(num1, true, true, false, true);
			if (num1 > 0)
				str = GuiUtils.BuildColorText(str, Color.green);
			else if (num1 < 0)
				str = GuiUtils.BuildColorText(str, Color.red);
			button.label.text = str;
		}
	}
}

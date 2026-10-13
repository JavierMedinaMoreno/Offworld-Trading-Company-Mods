using Harmony;
using Offworld.GameCore;
using Offworld.GameCore.Text;

namespace UIPlus.Patches
{
	[HarmonyPatch(typeof(HUDBuildingPlacementPreview))]
	[HarmonyPatch("SetDataCanBuild")]
	class BuildingPlacementTerrainConstructionSpeed
	{
		static void Postfix(HUDBuildingPlacementPreview __instance, PlayerClient player, TileClient tile)
		{
			InfoTerrain infoTerrain = Globals.Infos.terrain(tile.getTerrain());
			int constructionModifier = infoTerrain.maiConstructionModifier[(int)AppMain.gApp.gameClient().getLocation()];
			if (constructionModifier != 0) {
				if (__instance.extraText.text.Length > 0) {
					__instance.extraText.text += "\n";
				}
				__instance.extraText.text += HUDPanel.TEXT(
					"TEXT_HUDINFO_TERRAIN_CONSTRUCTION_RATE",
					infoTerrain.meName.ToText(),
					Utils.getPercentageString(constructionModifier, true).ToText()
				);
				__instance.extraText.alignment = UnityEngine.TextAnchor.UpperCenter;
			}
		}
	}
}

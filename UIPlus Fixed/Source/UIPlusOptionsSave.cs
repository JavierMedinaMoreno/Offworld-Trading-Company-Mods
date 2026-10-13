using System;
using System.Xml.Serialization;
using Offworld.AppCore;

namespace UIPlus
{
	[Serializable]
	public class UIPlusOptionsSave : ModUtils.IOptionsSave
	{
		public bool showCashResources = true;
		public bool showDebtChange = true;
		public int cameraZoomOut = 500;
		public bool floatyIcons = true;
		public bool observerOnly = false;
		public bool buildMenuRevenue = true;
		[XmlIgnore] public bool casterMode = false;
		public int borderlessWindowed = 0;
		public bool zoomToCursor = true;
		public bool disableArtPacks = false;

		public void ApplyOptions(bool isInGame)
		{
			if (AppMain.gApp.HUDDirector != null) {
				if (AllowedObserver())
					AppGlobals.GameHUDHelpers.SetCameraMaxZoom(cameraZoomOut);
				else
					AppGlobals.GameHUDHelpers.SetCameraMaxZoom(350);
			}

			if (AppMain.gApp.GameRenderer != null && AppMain.gApp.GameRenderer.Initialized) {
				AppMain.gApp.GameRenderer.WorldTextVisible = floatyIcons;
			}
		}

		public bool AllowedObserver()
		{
			if (casterMode) {
				return AppMain.gApp.IsObserver;
			} else {
				return (!observerOnly || AppMain.gApp.IsObserver);
			}
		}
	}

}

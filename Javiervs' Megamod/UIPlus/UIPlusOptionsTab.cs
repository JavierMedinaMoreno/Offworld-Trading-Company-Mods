using System;
using System.Collections.Generic;
using System.Linq;
using Offworld.AppCore;
using Offworld.GameCore;
using UnityEngine;
using UnityEngine.Assertions;

namespace UIPlus
{
	public class UIPlusOptionsTab : ModUtils.ModOptionsTab
	{
		public static UIPlusOptionsTab Install()
		{
			return ModUtils.ModOptionsScreenListener.Install<UIPlusOptionsTab>();
		}

		private AppMain APP;

		public UIPlusOptionsTab()
		{
			APP = AppMain.gApp;
		}

		public override string MOD_ID
		{
			get { return "UI+"; }
		}

		public override string TabName()
		{
			return "Mod Options";
		}

		public override bool OnHover(GuiActionInfo actionInfo, RectTransform rect)
		{
			if (actionInfo.data1 == (int)OptionsScreenCommon.OptionsScreenUIItem.FULL_SCREEN)
				return false;

			switch ((OptionsScreenUIItem)actionInfo.data1) {
			case OptionsScreenUIItem.SUBMOD_LIMITED_LIABILITY:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("MEGAMOD_TEXT_OPTIONSSCREEN_LIMITED_LIABILITY_TT")));
				return true;
			case OptionsScreenUIItem.SUBMOD_HOLD_PRICE:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("MEGAMOD_TEXT_OPTIONSSCREEN_HOLD_PRICE_TT")));
				return true;
			case OptionsScreenUIItem.SUBMOD_NO_NUKES:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("MEGAMOD_TEXT_OPTIONSSCREEN_NO_NUKES_TT")));
				return true;
			case OptionsScreenUIItem.SUBMOD_NO_DEPLETION:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("MEGAMOD_TEXT_OPTIONSSCREEN_NO_DEPLETION_TT")));
				return true;
			case OptionsScreenUIItem.SUBMOD_COLONY_CLASSES:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("MEGAMOD_TEXT_OPTIONSSCREEN_COLONY_CLASSES_TT")));
				return true;
			case OptionsScreenUIItem.SUBMOD_UIPLUS:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("MEGAMOD_TEXT_OPTIONSSCREEN_UIPLUS_SUITE_TT")));
				return true;
			case OptionsScreenUIItem.CASH_PLUS_RESOURCES:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_OPTIONSSCREEN_CASH_PLUS_RESOURCSE_TT_DESC")));
				return true;
			case OptionsScreenUIItem.DEBT_CHANGE:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_OPTIONSSCREEN_DEBT_CHANGE_TT_DESC")));
				return true;
			case OptionsScreenUIItem.FLOATY_ICONS:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_OPTIONSSCREEN_FLOATY_ICONS_TT_DESC")));
				return true;
			case OptionsScreenUIItem.OBSERVER_ONLY:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_OPTIONSSCREEN_OBSERVER_ONLY_TT_DESC")));
				return true;
			case OptionsScreenUIItem.ZOOM_CAMERA:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_OPTIONSSCREEN_ZOOM_CAMERA_TT_DESC")));
				return true;
			case OptionsScreenUIItem.HIDE_ART_PACKS:
				TooltipControl.instance.BuildTooltip(rect, (TooltipDataBase)new TooltipGeneralData((string)null, MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_OPTIONSSCREEN_HIDE_ART_PACKS_TT_DESC")));
				return true;
			}
			return false;
		}

		public override void OnClick(GuiActionInfo actionInfo, RectTransform rect)
		{
			base.OnClick(actionInfo, rect);
		}

		public override void OnToggleChange(GuiActionInfo actionInfo, RectTransform rect, bool newValue)
		{
			base.OnToggleChange(actionInfo, rect, newValue);

			var uiplusopts = ModOptionsSave.Options;
			switch ((UIPlusOptionsTab.OptionsScreenUIItem)actionInfo.data1) {
			case UIPlusOptionsTab.OptionsScreenUIItem.SUBMOD_LIMITED_LIABILITY:
				uiplusopts.enableLimitedLiability = newValue;
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.SUBMOD_HOLD_PRICE:
				uiplusopts.enableHoldPrice = newValue;
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.SUBMOD_NO_NUKES:
				uiplusopts.enableNoUndergroundNukes = newValue;
				JaviervsMegamod.MegamodRulesManager.ApplyDataRules();
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.SUBMOD_NO_DEPLETION:
				uiplusopts.enableInterdimensionalNoResourceDepletion = newValue;
				JaviervsMegamod.MegamodRulesManager.ApplyDataRules();
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.SUBMOD_COLONY_CLASSES:
				uiplusopts.enableInterdimensionalColonyClasses = newValue;
				JaviervsMegamod.MegamodRulesManager.ApplyDataRules();
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.SUBMOD_UIPLUS:
				uiplusopts.enableUIPlus = newValue;
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.CASH_PLUS_RESOURCES:
				uiplusopts.showCashResources = newValue;
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.DEBT_CHANGE:
				uiplusopts.showDebtChange = newValue;
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.FLOATY_ICONS:
				uiplusopts.floatyIcons = newValue;
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.OBSERVER_ONLY:
				uiplusopts.observerOnly = newValue;
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.ZOOM_CAMERA:
				uiplusopts.zoomToCursor = newValue;
				break;
			case UIPlusOptionsTab.OptionsScreenUIItem.HIDE_ART_PACKS:
				uiplusopts.disableArtPacks = newValue;
				break;
			}
			uiplusopts.ApplyOptions(false);
		}

		public override void OnSliderChange(GuiActionInfo actionInfo, RectTransform rect, float newValue)
		{
			base.OnSliderChange(actionInfo, rect, newValue);

			var uiplusopts = ModOptionsSave.Options;
			switch ((UIPlusOptionsTab.OptionsScreenUIItem)actionInfo.data1) {
			case UIPlusOptionsTab.OptionsScreenUIItem.CAMERA_ZOOM_OUT:
				uiplusopts.cameraZoomOut = (int)newValue;
				if (AppMain.gApp.HUDDirector != null)
					if (uiplusopts.AllowedObserver())
						AppGlobals.GameHUDHelpers.SetCameraMaxZoom(newValue);
				break;
			}
		}

		public override void createModPanel(GameObject panel)
		{
			var grid = panel.GetComponent<UnityEngine.UI.GridLayoutGroup>() ?? panel.GetComponentInChildren<UnityEngine.UI.GridLayoutGroup>();
			if (grid != null)
			{
				grid.padding = new RectOffset(grid.padding.left, grid.padding.right, 10, grid.padding.bottom);
			}
			float totalWidth = (grid != null) ? (grid.cellSize.x * 2f + grid.spacing.x) : 780f;

			// Row 1: Submods Header
			MohawkUI.createLabel(MohawkEventHandlerMonoBehavior.TEXT("MEGAMOD_TEXT_OPTIONSSCREEN_HEADER_SUBMODS"), panel.gameObject);
			MohawkUI.createLabel("", panel.gameObject);

			// Row 2: Submods Pair 1
			addToggle(panel, (int)OptionsScreenUIItem.SUBMOD_LIMITED_LIABILITY, "MEGAMOD_TEXT_OPTIONSSCREEN_LIMITED_LIABILITY");
			addToggle(panel, (int)OptionsScreenUIItem.SUBMOD_HOLD_PRICE, "MEGAMOD_TEXT_OPTIONSSCREEN_HOLD_PRICE");

			// Row 3: Submods Pair 2
			addToggle(panel, (int)OptionsScreenUIItem.SUBMOD_NO_NUKES, "MEGAMOD_TEXT_OPTIONSSCREEN_NO_NUKES");
			addToggle(panel, (int)OptionsScreenUIItem.SUBMOD_NO_DEPLETION, "MEGAMOD_TEXT_OPTIONSSCREEN_NO_DEPLETION");

			// Row 4: Submods Pair 3
			addToggle(panel, (int)OptionsScreenUIItem.SUBMOD_COLONY_CLASSES, "MEGAMOD_TEXT_OPTIONSSCREEN_COLONY_CLASSES");
			addToggle(panel, (int)OptionsScreenUIItem.SUBMOD_UIPLUS, "MEGAMOD_TEXT_OPTIONSSCREEN_UIPLUS_SUITE");

			// UI+ Header (symmetrical padding matching top header)
			MohawkUI.createLabel(MohawkEventHandlerMonoBehavior.TEXT("MEGAMOD_TEXT_OPTIONSSCREEN_HEADER_UIPLUS"), panel.gameObject);
			MohawkUI.createLabel("", panel.gameObject);

			// Cash+Resources & Debt Change
			addToggle(panel, (int)OptionsScreenUIItem.CASH_PLUS_RESOURCES, "UIPLUS_TEXT_OPTIONSSCREEN_CASH_PLUS_RESOURCSE");
			addToggle(panel, (int)OptionsScreenUIItem.DEBT_CHANGE, "UIPLUS_TEXT_OPTIONSSCREEN_DEBT_CHANGE");

			// Floaty Icons & Zoom to Cursor
			addToggle(panel, (int)OptionsScreenUIItem.FLOATY_ICONS, "UIPLUS_TEXT_OPTIONSSCREEN_FLOATY_ICONS");
			addToggle(panel, (int)OptionsScreenUIItem.ZOOM_CAMERA, "UIPLUS_TEXT_OPTIONSSCREEN_ZOOM_CAMERA");

			// Observer Only & Disable Art Packs (out of game)
			if (!this.IsInGameOptions()) {
				addToggle(panel, (int)OptionsScreenUIItem.OBSERVER_ONLY, "UIPLUS_TEXT_OPTIONSSCREEN_OBSERVER_ONLY");
				addToggle(panel, (int)OptionsScreenUIItem.HIDE_ART_PACKS, "UIPLUS_TEXT_OPTIONSSCREEN_HIDE_ART_PACKS");
			}

			// Line 1: Camera Max Zoom Out Label
			MohawkUI.createLabel(MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_OPTIONSSCREEN_CAMERA_ZOOM_OUT"), panel.gameObject);
			MohawkUI.createLabel("", panel.gameObject);

			// Line 2: Full-Width Slider Row
			var sliderRow = MohawkUI.createLabel("", panel.gameObject);
			sliderRow.raycastTarget = false;
			MohawkUI.createLabel("", panel.gameObject);

			addSlider(sliderRow.gameObject, (int)OptionsScreenUIItem.CAMERA_ZOOM_OUT, 350, 700, false, 500f);

			var sliderCtrl = GetSlider((int)OptionsScreenUIItem.CAMERA_ZOOM_OUT);
			if (sliderCtrl != null)
			{
				var rt = sliderCtrl.GetComponent<RectTransform>();
				if (rt != null)
				{
					rt.anchorMin = new Vector2(0f, 0f);
					rt.anchorMax = new Vector2(0f, 1f);
					rt.offsetMin = new Vector2(0f, 0f);
					rt.offsetMax = new Vector2(totalWidth, 0f);
				}
			}

			this.RefreshModValues();
		}

		public override void RefreshModValues()
		{
			GetToggle((int)OptionsScreenUIItem.SUBMOD_LIMITED_LIABILITY).SetValue(ModOptionsSave.Options.enableLimitedLiability, false);
			GetToggle((int)OptionsScreenUIItem.SUBMOD_HOLD_PRICE).SetValue(ModOptionsSave.Options.enableHoldPrice, false);
			GetToggle((int)OptionsScreenUIItem.SUBMOD_NO_NUKES).SetValue(ModOptionsSave.Options.enableNoUndergroundNukes, false);
			GetToggle((int)OptionsScreenUIItem.SUBMOD_NO_DEPLETION).SetValue(ModOptionsSave.Options.enableInterdimensionalNoResourceDepletion, false);
			GetToggle((int)OptionsScreenUIItem.SUBMOD_COLONY_CLASSES).SetValue(ModOptionsSave.Options.enableInterdimensionalColonyClasses, false);
			GetToggle((int)OptionsScreenUIItem.SUBMOD_UIPLUS).SetValue(ModOptionsSave.Options.enableUIPlus, false);

			GetToggle((int)OptionsScreenUIItem.CASH_PLUS_RESOURCES).SetValue(ModOptionsSave.Options.showCashResources, false);
			GetToggle((int)OptionsScreenUIItem.DEBT_CHANGE).SetValue(ModOptionsSave.Options.showDebtChange, false);
			GetToggle((int)OptionsScreenUIItem.FLOATY_ICONS).SetValue(ModOptionsSave.Options.floatyIcons, false);
			GetToggle((int)OptionsScreenUIItem.ZOOM_CAMERA).SetValue(ModOptionsSave.Options.zoomToCursor, false);
			if (!this.IsInGameOptions()) {
				GetToggle((int)OptionsScreenUIItem.OBSERVER_ONLY).SetValue(ModOptionsSave.Options.observerOnly, false);
				GetToggle((int)OptionsScreenUIItem.HIDE_ART_PACKS).SetValue(ModOptionsSave.Options.disableArtPacks, false);
			}
			GetSlider((int)OptionsScreenUIItem.CAMERA_ZOOM_OUT).SetValue(ModOptionsSave.Options.cameraZoomOut, false);
		}

		public enum OptionsScreenUIItem
		{
			CASH_PLUS_RESOURCES = OptionsScreenCommon.OptionsScreenUIItem.NUM_ITEMS,
			DEBT_CHANGE,
			CAMERA_ZOOM_OUT,
			FLOATY_ICONS,
			OBSERVER_ONLY,
			ZOOM_CAMERA,
			HIDE_ART_PACKS,
			SUBMOD_LIMITED_LIABILITY,
			SUBMOD_HOLD_PRICE,
			SUBMOD_NO_NUKES,
			SUBMOD_NO_DEPLETION,
			SUBMOD_COLONY_CLASSES,
			SUBMOD_UIPLUS,
		}
	}
}


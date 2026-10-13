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
			return "UI+";
		}

		public override bool OnHover(GuiActionInfo actionInfo, RectTransform rect)
		{
			if (actionInfo.data1 == (int)OptionsScreenCommon.OptionsScreenUIItem.FULL_SCREEN)
				return false;

			switch ((OptionsScreenUIItem)actionInfo.data1) {
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
			if (!this.IsInGameOptions()) {
				addToggle(panel, (int)OptionsScreenUIItem.OBSERVER_ONLY, "UIPLUS_TEXT_OPTIONSSCREEN_OBSERVER_ONLY");
				MohawkUI.createLabel("", panel.gameObject);
			}
			//addHeader(panel, "UIPLUS_TEXT_OPTIONSSCREEN_UI");

			addToggle(panel, (int)OptionsScreenUIItem.CASH_PLUS_RESOURCES, "UIPLUS_TEXT_OPTIONSSCREEN_CASH_PLUS_RESOURCSE");
			addToggle(panel, (int)OptionsScreenUIItem.DEBT_CHANGE, "UIPLUS_TEXT_OPTIONSSCREEN_DEBT_CHANGE");
			addToggle(panel, (int)OptionsScreenUIItem.FLOATY_ICONS, "UIPLUS_TEXT_OPTIONSSCREEN_FLOATY_ICONS");

			if (!IsInGameOptions()) {
				addToggle(panel, (int)OptionsScreenUIItem.HIDE_ART_PACKS, "UIPLUS_TEXT_OPTIONSSCREEN_HIDE_ART_PACKS");
			} else {
				MohawkUI.createLabel("", panel.gameObject);
			}

			MohawkUI.createLabel("", panel.gameObject);
			MohawkUI.createLabel("", panel.gameObject);

			//addHeader(panel, "UIPLUS_TEXT_OPTIONSSCREEN_CAMERA");
			MohawkUI.createLabel(MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_OPTIONSSCREEN_CAMERA_ZOOM_OUT"), panel.gameObject);
			addSlider(panel, (int)OptionsScreenUIItem.CAMERA_ZOOM_OUT, 350, 700, false, 500f);
			addToggle(panel, (int)OptionsScreenUIItem.ZOOM_CAMERA, "UIPLUS_TEXT_OPTIONSSCREEN_ZOOM_CAMERA");

			this.RefreshModValues();
		}

		public override void RefreshModValues()
		{
			GetToggle((int)OptionsScreenUIItem.CASH_PLUS_RESOURCES).SetValue(ModOptionsSave.Options.showCashResources, false);
			GetToggle((int)OptionsScreenUIItem.DEBT_CHANGE).SetValue(ModOptionsSave.Options.showDebtChange, false);
			GetToggle((int)OptionsScreenUIItem.FLOATY_ICONS).SetValue(ModOptionsSave.Options.floatyIcons, false);
			if (!this.IsInGameOptions())
				GetToggle((int)OptionsScreenUIItem.OBSERVER_ONLY).SetValue(ModOptionsSave.Options.observerOnly, false);
			GetToggle((int)OptionsScreenUIItem.ZOOM_CAMERA).SetValue(ModOptionsSave.Options.zoomToCursor, false);
			GetSlider((int)OptionsScreenUIItem.CAMERA_ZOOM_OUT).SetValue(ModOptionsSave.Options.cameraZoomOut, false);
			if (!IsInGameOptions())
				GetToggle((int)OptionsScreenUIItem.HIDE_ART_PACKS).SetValue(ModOptionsSave.Options.disableArtPacks, false);
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
		}
	}
}


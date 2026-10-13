using System;
using System.Collections.Generic;
using ModUtils;
using Offworld.GameCore.Text;
using Steamworks;
using UnityEngine;

namespace UIPlus
{
	public class MultiplayerOptions : IMohawkEventHandler
	{
		public const string LOBBY_CASTER_MOD_KEY = "uiPlus_casterMode";
		private bool initialized;
		MainScreenListener mainScreenListener;
		private LobbyScreenListener lobbyScreenListener;

		private CreateLobbyScreenListener createLobbyScreenListener;
		private DropdownControl casterModeDropdown;

		private string activeLobbyID
		{
			get
			{
				return LobbyHelpers.ActiveLobbyID;
			}
		}

		public void Update()
		{
			if (mainScreenListener == null)
				mainScreenListener = UnityEngine.Object.FindObjectOfType<MainScreenListener>();
			if (mainScreenListener == null)
				return;
			if (lobbyScreenListener == null)
				lobbyScreenListener = UnityEngine.Object.FindObjectOfType<LobbyScreenListener>();
			var currentScreenIndex = (MainScreenListener.MenuScreen)InjectorUtils.GetPrivateMember(mainScreenListener, "currentScreenIndex");
			if (currentScreenIndex == MainScreenListener.MenuScreen.Lobby) {
				if (!initialized) {
					initialized = true;
					if (AppMain.gApp.NetworkP2P.IsServerActive()) {
						ModOptionsSave.Options.casterMode = casterModeDropdown.getSelectedIndex() == 1;
						InjectorUtils.CallPrivateFunc(lobbyScreenListener, "addSystemChat", MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_CREATELOBBY_CASTER_MODE_MESSAGE", new TextVariable[1]
						{
							(ModOptionsSave.Options.casterMode ? "ON" : "OFF").ToText()
						}));
					}
				}
				if (!AppMain.gApp.NetworkP2P.IsServerActive()) {
					if (Patches.LobbyCasterModeVariable_Read.lobbyCasterMode.HasValue && ModOptionsSave.Options.casterMode != Patches.LobbyCasterModeVariable_Read.lobbyCasterMode.Value) {
						ModOptionsSave.Options.casterMode = Patches.LobbyCasterModeVariable_Read.lobbyCasterMode.Value;
						InjectorUtils.CallPrivateFunc(lobbyScreenListener, "addSystemChat", MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_CREATELOBBY_CASTER_MODE_MESSAGE", new TextVariable[1]
						{
							(ModOptionsSave.Options.casterMode ? "ON" : "OFF").ToText()
						}));
					}
				}
			} else if (currentScreenIndex != MainScreenListener.MenuScreen.Multiplayer) {
				if (currentScreenIndex != MainScreenListener.MenuScreen.CreateLobby && currentScreenIndex != MainScreenListener.MenuScreen.GameStarting) {
					ModOptionsSave.Options.casterMode = false;
					Patches.LobbyCasterModeVariable_Read.lobbyCasterMode = null;
				}
				initialized = false;
			}

			if (casterModeDropdown == null) {
				createLobbyScreenListener = UnityEngine.Object.FindObjectOfType<CreateLobbyScreenListener>();
				if (createLobbyScreenListener != null) {
					List<DropdownOption> options = new List<DropdownOption>();
					options.Add(new DropdownOption(MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_CREATELOBBY_CASTER_MODE_OFF"), new GuiActionInfo(6, 0, 0)));
					options.Add(new DropdownOption(MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_CREATELOBBY_CASTER_MODE_ON"), new GuiActionInfo(6, 1, 0)));
					MohawkUI.createLabel(MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_CREATELOBBY_CASTER_MODE"), createLobbyScreenListener.gridPanel);
					int startIndex = 0;
					foreach (var a in AppDomain.CurrentDomain.GetAssemblies()) {
						if (a.GetName().Name == "philothanicsCompetativeMod") {
							ModUtils.Debug.Log("Detected FFA Ranks, defaulting Caster Mode to On.");
							ModOptionsSave.Options.casterMode = true;
							startIndex = 1;
							break;
						}
					}
					casterModeDropdown = MohawkUI.createDropdownControl(this, new GuiActionInfo(6, -1, -1), options, startIndex, createLobbyScreenListener.gridPanel, true);
					var rect = createLobbyScreenListener.gridPanel.GetComponent<RectTransform>();
					var size = rect.sizeDelta;
					size.y += 40;
					rect.sizeDelta = size;
				}
			}
		}

		public void OnClick(GuiActionInfo actionInfo, RectTransform rect)
		{
			switch (actionInfo.data1) {
			case 6:
				ModOptionsSave.Options.casterMode = actionInfo.data2 == 1;
				break;
			}
		}

		public bool OnHover(GuiActionInfo actionInfo, RectTransform rect)
		{
			switch (actionInfo.data1) {
			case 6:
				TooltipControl.instance.BuildTooltip(rect, new TooltipGeneralData(null, MohawkEventHandlerMonoBehavior.TEXT("UIPLUS_TEXT_CREATELOBBY_CASTER_MODE_TT_DESC"), true));
				return true;
			}
			return false;
		}

		public void OnMouseEnter(GuiActionInfo actionInfo, RectTransform rect)
		{
		}

		public void OnMouseExit(GuiActionInfo actionInfo, RectTransform rect)
		{
		}

		public void OnDisabledClick(GuiActionInfo actionInfo, RectTransform rect)
		{
		}

		public void OnRightClick(GuiActionInfo actionInfo, RectTransform rect)
		{
		}

		public void OnDoubleClick(GuiActionInfo actionInfo, RectTransform rect)
		{
		}

		public void OnSliderChange(GuiActionInfo actionInfo, RectTransform rect, float newValue)
		{
		}

		public void OnToggleChange(GuiActionInfo actionInfo, RectTransform rect, bool newValue)
		{
		}

		public void OnTextChange(GuiActionInfo actionInfo, RectTransform rect, string newValue)
		{
		}

		public void OnTextEndEdit(GuiActionInfo actionInfo, RectTransform rect, string newValue)
		{
		}
	}
}

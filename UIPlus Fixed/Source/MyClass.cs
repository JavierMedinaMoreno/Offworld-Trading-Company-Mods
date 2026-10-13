using System;
using ModUtils;
using UnityEngine;
using Offworld.AppCore;
using Offworld.GameCore;
using System.Collections.Generic;
using Debug = ModUtils.Debug;
using UnityEngine.UI;
using System.Reflection;
using Harmony;

namespace UIPlus
{
	public class GameEventListener : GameEventAdapter
	{
		public override void onDataEvent(GameEventData dataEvent)
		{
			try {
				Debug.Msg("DataEvent: " + dataEvent.meType.ToString());
				if (dataEvent.meType == GameEventsClient.DataType.HQFound) {
					string msg = "";
					foreach (int i in dataEvent.maiData) {
						msg += i.ToString() + ",";
					}
					Debug.Msg(msg);
				}
			} catch (Exception e) {
				Debug.Msg(e.ToString());
			}
		}
	}


	public class UIPlusMouseover : MohawkMouseover
	{
		protected int id;
		public UIPlusMouseover(int id)
		{
			this.id = id;

			SetEventHandler(AppMain.gApp.HUDDirector.mainGameScreen.GetEventHandler());
			SetActionInfo(new GuiActionInfo(id));
		}
	}

	public class MapSeedMouseover : UIPlusMouseover
	{
		public MapSeedMouseover() : base(-4)
		{
		}
	}


	public class MyClass : ModEntryPointAdapter
	{
		public static KeyBindingType KEYBINDING_COPY_SEED = KeyBindingType.NONE;
		public static KeyBindingType KEYBINDING_SHOW_ALL_DEBT = KeyBindingType.NONE;
		public static KeyBindingType KEYBINDING_HIDE_SPECIAL_BUILDING_OVERVIEW = KeyBindingType.NONE;
		public static KeyBindingType KEYBINDING_TOGGLE_SPECIAL_BUILDING_OVERVIEW = KeyBindingType.NONE;
		private GameClient client;
		private bool installedUI = false;
		public UIPlusOptionsTab modOptionsScreen = null;
		private bool observing = false;
		private bool inGame = false;
		public static HUDSelectedBuildingPanel selectedBuildingPanel;

		private MultiplayerOptions mpOptions = new MultiplayerOptions();

		public static string Clipboard
		{
			get { return GUIUtility.systemCopyBuffer; }
			set { GUIUtility.systemCopyBuffer = value; }
		}

		public static HarmonyInstance harmony;

		public override void Initialize()
		{
			if (harmony == null) {
				harmony = HarmonyInstance.Create("offworld.mod.uiplusfixed");
				foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
					try {
						var methods = type.GetHarmonyMethods();
						if (methods != null && methods.Count > 0) {
							var processor = new PatchProcessor(harmony, type, HarmonyMethod.Merge(methods));
							processor.Patch();
						}
					} catch (Exception patchEx) {
					}
				}
			}
			Debug.ModName = "UI+ Fixed";

			ModOptionsSaveBank.gBank.Register(ModOptionsSave.id, new ModOptionsSave());

			// Bump the shadow distance for our higher max zoom
			QualitySettings.shadowDistance = 800f;

			KEYBINDING_COPY_SEED = KeyBindingUtils.Get("KEYBINDING_COPY_SEED");
			KEYBINDING_SHOW_ALL_DEBT = KeyBindingUtils.Get("KEYBINDING_SHOW_ALL_DEBT");
			KEYBINDING_HIDE_SPECIAL_BUILDING_OVERVIEW = KeyBindingUtils.Get("KEYBINDING_HIDE_SPECIAL_BUILDING_OVERVIEW");
			KEYBINDING_TOGGLE_SPECIAL_BUILDING_OVERVIEW = KeyBindingUtils.Get("KEYBINDING_TOGGLE_SPECIAL_BUILDING_OVERVIEW");
		}

		public override void Shutdown()
		{
			harmony.RestoreAll(Assembly.GetExecutingAssembly());

			if (modOptionsScreen != null) {
				modOptionsScreen.Uninstall();
				modOptionsScreen = null;
			}

			BuildMenuRevenue bmr = UnityEngine.Object.FindObjectOfType<BuildMenuRevenue>();
			if (bmr != null)
				Component.Destroy(bmr);

			PopupManager.addPopup("Restart Game", "For technical reasons, it is highly advised to restart the game after disabling UI+ before continuing to play.",
			new List<string>()
			{
				"Restart Game",
				MohawkEventHandlerMonoBehavior.TEXT("TEXT_STARTSCREEN_EXIT_TT_DESC"),
				MohawkEventHandlerMonoBehavior.TEXT("TEXT_GENERIC_NO")
			}, new List<Action>()
			{
				RestartOffworld,
				Application.Quit,
				PopupManager.removeTopPopup
			}, null);
		}

		private static void RestartOffworld()
		{
			Application.Quit();
			System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe"));
		}

		public override void OnGameReady()
		{
			//AppGlobals.GameGlobals.GameClient.gameEventsClient().addGameEventListener(new GameEventListener());
		}

		public override void OnExitGame()
		{
			inGame = false;
			installedUI = false;
			UIPlus.Patches.HUDPlayerListController_CreatePlayer.specialPlayerQueues.Clear();
		}

		public override void OnGameClientReady()
		{
			client = AppGlobals.GameGlobals.GameClient;
		}

		public override void OnUIReady()
		{
			ModOptionsSave.Options.ApplyOptions(true);

			inGame = true;

			/*
			GameObject[] arrallobjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
			Debug.All(string.Format("{0} objects", arrallobjects.Length));
			gameobjects.Clear();
			gameobjects.AddRange(arrallobjects);
			foreach (GameObject go in gameobjects) {
				Debug.Log(go.name);
			}
			//*/

			BuildMenuRevenue.Install();
		}

		public override void Update()
		{
			if (inGame) {
				if (AppMain.gApp.HUDDirector.IsTopScreen(ScreenType.OPTIONS)) {
					if (modOptionsScreen == null || modOptionsScreen.Destroyed) {
						modOptionsScreen = UIPlusOptionsTab.Install();
					}
				}

				if (!installedUI) {
					installedUI = true;
					// force one update so keybinding is ready
					(AppMain.gApp.HUDDirector.mainGameScreen.GetEventHandler() as MainGameEventHandler).update();

					KeyBindingUtils.AssignKeyboundAction(KEYBINDING_COPY_SEED, delegate
					{
						int seed = client.getSeed();
						Clipboard = seed.ToString();
						Debug.Msg("Copied seed to clipboard");
					});
					KeyBindingUtils.AssignKeyboundAction(KEYBINDING_SHOW_ALL_DEBT, delegate
					{
						Patches.HUDPlayerListController_GetPlayerName.ShowAllDebt = AppMain.gApp.KeyBindingManager.CheckKeyBindingPressed(KEYBINDING_SHOW_ALL_DEBT);
					});
					KeyBindingUtils.AssignKeyboundAction(KEYBINDING_HIDE_SPECIAL_BUILDING_OVERVIEW, delegate
					{
						Patches.HUDPlayerListController_Update.HideSpecialPlayerQueues = AppMain.gApp.KeyBindingManager.CheckKeyBindingPressed(KEYBINDING_HIDE_SPECIAL_BUILDING_OVERVIEW);
					});
					KeyBindingUtils.AssignKeyboundAction(KEYBINDING_TOGGLE_SPECIAL_BUILDING_OVERVIEW, delegate
					{
						Patches.HUDPlayerListController_Update.ShowSpecialPlayerQueues = !Patches.HUDPlayerListController_Update.ShowSpecialPlayerQueues;
					});
				}

				// Alter hologram UI
				/*
				if (selectedBuildingPanel == null) {
					selectedBuildingPanel = UnityEngine.Object.FindObjectOfType<HUDSelectedBuildingPanel>();
					if (selectedBuildingPanel != null) {
						TextButton buttonPower = (TextButton)InjectorUtils.GetPrivateMember(selectedBuildingPanel, "buttonPower");
						if (buttonPower != null) {
							GameObject panelButtons = buttonPower.gameObject.transform.parent.gameObject;
							GridLayoutGroup gridLayout = panelButtons.GetComponent<GridLayoutGroup>();
							panelButtons.transform.SetParent(panelButtons.transform.parent.FindChild("HLayout"));
						} else {
							selectedBuildingPanel = null;
						}
					}
				}
				*/

				AlterReplayFile.Update();
				UIPlus.Patches.HUDPlayerListController_Update.UpdateQueues();
			} else {
				MatchHistoryScreenListenerInjector.AttemptInstall();

				mpOptions.Update();

				if (modOptionsScreen == null || modOptionsScreen.Destroyed) {
					modOptionsScreen = UIPlusOptionsTab.Install();
				}
			}

			if (observing != AppMain.gApp.IsObserver) {
				observing = AppMain.gApp.IsObserver;
				ModOptionsSave.Options.ApplyOptions(true);
			}
		}
	}
}

using System;
using System.IO;
using System.Reflection;
using ModUtils;
using UnityEngine;

// To use, call MatchHistoryScreenListenerInjector.AttemptInstall() in mod's Update()

namespace UIPlus
{
	public class MatchHistoryScreenListenerInjector : IMohawkEventHandler
	{
		private static int VIEW_REPLAYS = -1;
		private static int QUICK_MATCH_WATCH_REPLAY = -1;
		private static MatchHistoryScreenListener matchHistoryListener;
		private static MultiplayerScreenListener multiplayerScreenListener;
		private static TextButton queue_watchReplayButton;

		public static void AttemptInstall()
		{
			if (matchHistoryListener == null) {
				matchHistoryListener = UnityEngine.Object.FindObjectOfType<MatchHistoryScreenListener>();
				InstallMatchHistory();
			}

			if (queue_watchReplayButton == null) {
				if (multiplayerScreenListener == null)
					multiplayerScreenListener = UnityEngine.Object.FindObjectOfType<MultiplayerScreenListener>();
				if (multiplayerScreenListener != null) {
					queue_watchReplayButton = (TextButton)InjectorUtils.GetPrivateMember(multiplayerScreenListener, "watchReplayButton");
					InstallMultiplayer();
				}
			}
		}

		private static void InstallMatchHistory()
		{
			if (matchHistoryListener == null)
				return;

			matchHistoryListener.buttonViewReplays.SetEventHandler(new MatchHistoryScreenListenerInjector());
			UnityEngine.Debug.Log("Replaced MatchHistoryScreenListener for Replay Button");
		}

		private static void InstallMultiplayer()
		{
			if (queue_watchReplayButton == null)
				return;

			queue_watchReplayButton.SetEventHandler(new MatchHistoryScreenListenerInjector());
			UnityEngine.Debug.Log("Replaced MultiplayerScreenListener for Replay Button");
		}

		public MatchHistoryScreenListenerInjector()
		{
			Type t = typeof(MatchHistoryScreenListener).Assembly.GetType("MatchHistoryScreenListener+MatchHistoryUIItem");
			int idx = Array.IndexOf(Enum.GetNames(t), "VIEW_REPLAYS");
			VIEW_REPLAYS = (int)Enum.GetValues(t).GetValue(idx);

			t = typeof(MultiplayerScreenListener).Assembly.GetType("MultiplayerScreenListener+MultiplayerScreenUIItem");
			idx = Array.IndexOf(Enum.GetNames(t), "QUICK_MATCH_WATCH_REPLAY");
			QUICK_MATCH_WATCH_REPLAY = (int)Enum.GetValues(t).GetValue(idx);
		}

		public void OnClick(GuiActionInfo actionInfo, UnityEngine.RectTransform rect)
		{
			if (actionInfo.data1 == VIEW_REPLAYS) {
				MohawkFileBrowser.OpenFilePanel(ReplayFile.replayFolderName, (x => PlayReplayFileAction(x)), new string[2]
				{
					ReplayFile.replayFileExtentionWithoutPeriod,
					ReplayFile.compressedreplayFileExtentionWithoutPeriod
				}, false);
			} else if (actionInfo.data1 == QUICK_MATCH_WATCH_REPLAY) {
				if (!(bool)InjectorUtils.CallPrivateFunc(multiplayerScreenListener, "AllowGameWhileWaiting") || !(bool)InjectorUtils.CallPrivateFunc(multiplayerScreenListener, "canShowReplay"))
					return;
				MohawkFileBrowser.OpenFilePanel(ReplayFile.replayFolderName, (x => PlayReplayFileAction(x)), new string[2]
				{
					ReplayFile.replayFileExtentionWithoutPeriod,
					ReplayFile.compressedreplayFileExtentionWithoutPeriod
				}, false);
			} else {
				matchHistoryListener.OnClick(actionInfo, rect);
			}
		}

		private static void PlayReplayFileAction(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return;
			if (Path.GetExtension(filePath) == ReplayFile.compressedReplayFileExtention) {
				string saveName = ReplayFile.GetSaveName(filePath);
				LZF.DecompressFile(filePath, saveName);
				filePath = saveName;
			}
			string fileVersionNumber;
			if (ReplayFileInjector.ValidReplayFileForPlayback(filePath, out fileVersionNumber)) {
				if (!ReplayFileInjector.StartFilePlayback(filePath))
					return;
				AppMain.LoadMainGameScene();
			} else {
				//this.showInvalidVersion(filePath, fileVersionNumber);
				MethodInfo mi = matchHistoryListener.GetType().GetMethod("showInvalidVersion",
																  BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
																  null,
																  new Type[] { typeof(string), typeof(string) },
																  null);
				if (mi == null) {
					UnityEngine.Debug.Log("No method showInvalidVersion found");
				} else {
					mi.Invoke(matchHistoryListener, new object[] { filePath, fileVersionNumber });
				}
			}
		}

		public void OnMouseEnter(GuiActionInfo actionInfo, RectTransform rect)
		{
			matchHistoryListener.OnMouseEnter(actionInfo, rect);
		}

		public void OnMouseExit(GuiActionInfo actionInfo, RectTransform rect)
		{
			matchHistoryListener.OnMouseExit(actionInfo, rect);
		}

		public bool OnHover(GuiActionInfo actionInfo, RectTransform rect)
		{
			return matchHistoryListener.OnHover(actionInfo, rect);
		}

		public void OnDisabledClick(GuiActionInfo actionInfo, RectTransform rect)
		{
			matchHistoryListener.OnDisabledClick(actionInfo, rect);
		}

		public void OnRightClick(GuiActionInfo actionInfo, RectTransform rect)
		{
			matchHistoryListener.OnRightClick(actionInfo, rect);
		}

		public void OnDoubleClick(GuiActionInfo actionInfo, RectTransform rect)
		{
			matchHistoryListener.OnDoubleClick(actionInfo, rect);
		}

		public void OnSliderChange(GuiActionInfo actionInfo, RectTransform rect, float newValue)
		{
			matchHistoryListener.OnSliderChange(actionInfo, rect, newValue);
		}

		public void OnToggleChange(GuiActionInfo actionInfo, RectTransform rect, bool newValue)
		{
			matchHistoryListener.OnToggleChange(actionInfo, rect, newValue);
		}

		public void OnTextChange(GuiActionInfo actionInfo, RectTransform rect, string newValue)
		{
			matchHistoryListener.OnTextChange(actionInfo, rect, newValue);
		}

		public void OnTextEndEdit(GuiActionInfo actionInfo, RectTransform rect, string newValue)
		{
			matchHistoryListener.OnTextEndEdit(actionInfo, rect, newValue);
		}
	}
}

using System;
using System.IO;
using ModUtils;
using UnityEngine;

namespace UIPlus
{
	public class ReplayFileInjector
	{
		private class VersionAndMod
		{
			private ulong mHeader;
			private int mMajor;
			private int mMinor;
			private int mChangelist;
			private bool mInternalBuild;
			private string mModName;
			private int mCRC;

			public VersionAndMod(ModPath.VersionAndMod version)
			{
				mHeader = (ulong)InjectorUtils.GetPrivateMember(version, "mHeader");
				mMajor = (int)InjectorUtils.GetPrivateMember(version, "mMajor");
				mMinor = (int)InjectorUtils.GetPrivateMember(version, "mMinor");
				mChangelist = (int)InjectorUtils.GetPrivateMember(version, "mChangelist");
				mInternalBuild = (bool)InjectorUtils.GetPrivateMember(version, "mInternalBuild");
				mModName = (string)InjectorUtils.GetPrivateMember(version, "mModName");
				mCRC = (int)InjectorUtils.GetPrivateMember(version, "mCRC");
			}

			public bool CheckIfCompatable(VersionAndMod other)
			{
				// Compatable with current UI+ and vanilla
				bool flag = (ModPath.NoMod == other.mModName && 0 == other.mCRC) || (ModPath.ModName == other.mModName && ModPath.CRC == other.mCRC);
				if ((long)this.mHeader == (long)other.mHeader && this.mChangelist <= other.mChangelist)
					return flag;
				return false;
			}
		}

		public static bool CompatibleVersion(ModPath.VersionAndMod other)
		{
			VersionAndMod version = new VersionAndMod(new ModPath.VersionAndMod());
			return version.CheckIfCompatable(new VersionAndMod(other));
		}


		public static bool LoadVersionAndCheckCompatibility(FileStream file, out ModPath.VersionAndMod fileVersion)
		{
			fileVersion = new ModPath.VersionAndMod(0UL, 0, 0, 0, true, "NONE", 0);
			try {
				BinaryReader binaryReader = new BinaryReader((Stream)file);
				fileVersion.Serialize((object)binaryReader);
			} catch (Exception ex) {
				fileVersion = (ModPath.VersionAndMod)null;
				UnityEngine.Debug.Log("Error trying to load version number from file: " + ex);
				return false;
			}
			if (new ModPath.VersionAndMod().StrictEquals(fileVersion))
				return true;
			return CompatibleVersion(fileVersion);
		}

		public static bool ValidReplayFileForPlayback(string replayFileName, out string fileVersionNumber)
		{
			fileVersionNumber = string.Empty;
			if (File.Exists(replayFileName)) {
				try {
					using (FileStream file = File.OpenRead(replayFileName)) {
						ModPath.VersionAndMod fileVersion;
						bool flag = ReplayFileInjector.LoadVersionAndCheckCompatibility(file, out fileVersion);
						if (fileVersion != null)
							fileVersionNumber = fileVersion.VersionPlusMod();
						if (flag)
							return fileVersion.Minor == Version.minor;
					}
				} catch (Exception) {
				}
			}
			return false;
		}


		public static bool StartFilePlayback(string replayFileName = null)
		{
			// Save our current ModName and CRC
			string cur_ModName = ModPath.ModName;
			int cur_CRC = ModPath.CRC;
			// Set ModName and CRC to default
			InjectorUtils.SetPrivateMember<ModPath>(null, "mModName", ModPath.NoMod);
			InjectorUtils.SetPrivateMember<ModPath>(null, "mCRC", 0);
			bool flag = AppMain.gApp.ReplayFile.OpenForPlayback(Time.unscaledTime, replayFileName);
			// Reset ModName and CRC
			InjectorUtils.SetPrivateMember<ModPath>(null, "mModName", cur_ModName);
			InjectorUtils.SetPrivateMember<ModPath>(null, "mCRC", cur_CRC);

			LobbyHelpers.LobbySettings.WantReplayFile = false;
			return flag;
		}
	}
}

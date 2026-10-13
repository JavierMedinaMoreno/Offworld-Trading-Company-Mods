using System.IO;
using System.Reflection;
using ModUtils;

// To use, call AlterReplayFile.Update() in mod's Update()

namespace UIPlus
{
	public static class AlterReplayFile
	{
		private static FileStream mLog;
		private static bool skip;

		public static void Update()
		{
			// If Tournament Mod loaded, don't alter replays, FFA Ranks will handle it
			if (skip)
				return;
			try {
				if (Assembly.Load("TournamentMod") != null) {
					skip = true;
					return;
				}
			} catch {
				// Nothing
			}
			
			if (AppMain.gApp.ReplayFile.WritingToReplayFile()) {
				FileStream curLog = (FileStream)InjectorUtils.GetPrivateMember(AppMain.gApp.ReplayFile, "mLog");
				if (!ReferenceEquals(curLog, mLog)) {
					mLog = curLog;

					Debug.Log("Altering Version of Replay File...");
					Debug.Log(mLog.Length);
					ModPath.VersionAndMod version = new ModPath.VersionAndMod();
					string modname = (string)InjectorUtils.GetPrivateMember(version, "mModName");

					long none_version_size;
					long version_size;
					using (MemoryStream buff = new MemoryStream()) {
						version.Serialize(new BinaryWriter(buff));
						version_size = buff.Position;
					}
					InjectorUtils.SetPrivateMember(version, "mModName", ModPath.NoMod);
					InjectorUtils.SetPrivateMember(version, "mCRC", 0);
					using (MemoryStream buff = new MemoryStream()) {
						version.Serialize(new BinaryWriter(buff));
						none_version_size = buff.Position;
					}
					long leftshift = none_version_size - version_size;

					Debug.Log("Shifting file {0} bytes...", leftshift);
					long position = mLog.Position;

					byte[] buffer = new byte[32768];
					using (MemoryStream buf = new MemoryStream()) {
						mLog.Seek(version_size, SeekOrigin.Begin);
						version.Serialize(new BinaryWriter(buf));

						int bytesRead = -1;
						while ((bytesRead = mLog.Read(buffer, 0, buffer.Length)) > 0) {
							buf.Write(buffer, 0, bytesRead);
						}

						// Write back
						buf.Seek(0, SeekOrigin.Begin);
						mLog.Seek(0, SeekOrigin.Begin);
						while ((bytesRead = buf.Read(buffer, 0, buffer.Length)) > 0) {
							mLog.Write(buffer, 0, bytesRead);
						}
						mLog.SetLength(buf.Length);
					}
					mLog.Seek(0, SeekOrigin.End);
					Debug.Log(mLog.Length);
					Debug.Log("Done");
				}
			} else {
				mLog = null;
			}
		}
	}
}

using System.Reflection;
using Harmony;

namespace UIPlus
{
	public static class HarmonyExtensions
	{
		public static void RestoreAll(this HarmonyInstance instance, Assembly assembly)
		{
			assembly.GetTypes().Do(type =>
			{
				var parentMethodInfos = type.GetHarmonyMethods();
				if (parentMethodInfos != null && parentMethodInfos.Count > 0) {
					var info = HarmonyMethod.Merge(parentMethodInfos);
					var processor = new PatchProcessor(instance, type, info);
					processor.Restore();
				}
			});
		}
	}
}

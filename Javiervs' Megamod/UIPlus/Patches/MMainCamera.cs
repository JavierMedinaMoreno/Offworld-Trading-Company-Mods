using System;
using Harmony;
using UnityEngine;

namespace UIPlus.Patches
{
	[HarmonyPatch(typeof(MMainCamera))]
	[HarmonyPatch("Zoom")]
	public class MMainCamera_Zoom
	{
		static float zoomOutLerp = 0f;
		static Vector3 targetPosition;

		static void Prefix(MMainCamera __instance, ref float __state, float fZoomFactor)
		{
			__state = __instance.TargetZoomDistance;
		}

		static void Postfix(ref MMainCamera __instance, float __state, float fZoomFactor)
		{
			if (targetPosition == null) {
				targetPosition = __instance.transform.position;
			}

			if (ModOptionsSave.Options.AllowedObserver() && ModOptionsSave.Options.zoomToCursor) {
				MInput input = (MInput)AccessTools.Field(__instance.GetType(), "input").GetValue(__instance);

				// Stops zoom weirdness when already at max/min zoom
				fZoomFactor = __state - __instance.TargetZoomDistance;
				if (System.Math.Abs(fZoomFactor) > 0.001f) {
					Ray ray;
					if (fZoomFactor > 0) {
						ray = __instance.GetComponent<Camera>().ScreenPointToRay(input.CurrentMouseSmoothed);
					} else {
						ray = new Ray(__instance.transform.position, __instance.transform.forward);
					}
					Vector3 hitPoint;
					if (RaycastPlane(ray, out hitPoint)) {
						// When zooming out, more centered zoom out
						if (fZoomFactor < 0) {
							Vector3 cursorHitPoint;
							if (RaycastTerrain(__instance.GetComponent<Camera>().ScreenPointToRay(input.LastMouseSmoothed), out cursorHitPoint)) {
								hitPoint = Vector3.Lerp(hitPoint, cursorHitPoint, zoomOutLerp);
							}
						}

						float distance1 = Vector3.Distance(hitPoint, __instance.transform.position);
						float distance2 = distance1 * (__instance.TargetZoomDistance / __instance.CurrentZoomDistance);
						Vector3 direction = Vector3.Normalize(hitPoint - __instance.transform.position) * (distance1 - distance2);
						targetPosition = __instance.transform.position + direction;

						if (RaycastPlane(new Ray(targetPosition, __instance.transform.forward), out hitPoint)) {
							__instance.TargetLookAt = hitPoint;
							__instance.TargetZoomDistance = Vector3.Distance(targetPosition, hitPoint);
						}
					}
				}
			}
		}

		static bool RaycastPlane(Ray ray, out Vector3 hitPoint)
		{
			hitPoint = Vector3.zero;
			float enter;
			if (!new Plane(Vector3.up, Vector3.zero).Raycast(ray, out enter))
				return false;
			hitPoint = ray.GetPoint(enter);
			return true;
		}

		static bool RaycastTerrain(Ray ray, out Vector3 hitPoint)
		{
			hitPoint = Vector3.zero;
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo)) {
				hitPoint = hitInfo.point;
				return true;
			}
			float enter;
			if (!new Plane(Vector3.up, Vector3.zero).Raycast(ray, out enter))
				return false;
			hitPoint = ray.GetPoint(enter);
			return true;
		}
	}

	[HarmonyPatch(typeof(MMainCamera))]
	[HarmonyPatch("MoveTowardsTarget")]
	public class MMainCamera_MoveTowardsTarget
	{
		static void Prefix(MMainCamera __instance, ref Quaternion __state)
		{
			__state = __instance.transform.rotation;
		}

		static void Postfix(ref MMainCamera __instance, Quaternion __state)
		{
			if (ModOptionsSave.Options.AllowedObserver() && ModOptionsSave.Options.zoomToCursor)
				__instance.transform.rotation = __state;
		}
	}
}

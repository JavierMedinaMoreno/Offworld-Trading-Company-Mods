using System;
using UnityEngine;

namespace UIPlus
{
	public class HighlightResource
	{
		private GameObject gameObject;
		private Renderer renderer;
		private MAppearanceController appearanceController;
		private SimpleAnimation simpleAnimation;

		public HighlightResource(GameObject gameObject)
		{
			this.gameObject = gameObject;
			this.renderer = gameObject.GetComponentInChildren<Renderer>(true);
			this.appearanceController = gameObject.GetComponent<MAppearanceController>();
			this.simpleAnimation = gameObject.GetComponentInChildren<SimpleAnimation>(true);
		}

		public void SetPosition(Vector3 position)
		{
			this.gameObject.transform.position = position;
		}

		public void SetColor(Color color)
		{
			if ((UnityEngine.Object)this.appearanceController != (UnityEngine.Object)null)
				this.appearanceController.SetColor(color);
			else
				this.renderer.material.color = color;
		}

		public void SetActive(bool active)
		{
			if (this.gameObject.activeSelf == active)
				return;
			this.gameObject.SetActive(active);
		}

		public void SetPhaseOffset(float value)
		{
			if (!((UnityEngine.Object)this.simpleAnimation != (UnityEngine.Object)null))
				return;
			this.simpleAnimation.phaseOffset = value;
		}
	}
}

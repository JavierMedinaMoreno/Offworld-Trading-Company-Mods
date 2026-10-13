using System.Collections.Generic;
using Offworld.GameCore;
using Offworld.GameCore.Text;
using Mohawk.SystemCore;
using Offworld.SystemCore;
using UnityEngine;

namespace UIPlus
{
	public class HUDSpecialBuildingQueuePlayerList : HUDPanel, iFrankenQueue
	{
		public HUDSpecialBuildingQueue.UIOrderSprites[] buildingArt = new HUDSpecialBuildingQueue.UIOrderSprites[4];
		private HUDSpecialBuildingQueue specialQueue;
		private PlayerClient player;
		private Dictionary<int, HUDFlyoutOrderButton> buildingButtonDictionary = new Dictionary<int, HUDFlyoutOrderButton>();
		private ResetPool<HUDFlyoutOrderButton> poolBuildingButtons;
		private Infos mInfo;
		private HUDFrankenguiController mController;

		public void SetPlayer(PlayerClient p)
		{
			player = p;
		}

		protected override void DoFirstUpdate()
		{
			base.DoFirstUpdate();
			this.mInfo = Globals.Infos;
			this.CreateLayout();
		}

		protected override void Update()
		{
			base.Update();
			if (specialQueue == null)
				specialQueue = FindObjectOfType<HUDSpecialBuildingQueue>();
			if (specialQueue != null && player != null && (APP.activePlayerClient().isSubsidiary() || APP.IsObserver))
				this.UpdateLayout();
		}

		private void CreateLayout()
		{
			this.poolBuildingButtons = new ResetPool<HUDFlyoutOrderButton>(
				() => MohawkUI.CloneUI(specialQueue.prefabButtonSpecBuildingQueue, this.gameObject).GetComponent<HUDFlyoutOrderButton>(),
				t => t.gameObject.SetActive(true),
				null,
				t => t.gameObject.SetActive(false)
			);
		}

		private void UpdateLayout()
		{
			this.poolBuildingButtons.Reset();
			this.buildingButtonDictionary.Clear();
			for (OrderType orderType = OrderType.PATENT; orderType < OrderType.NUM_TYPES; ++orderType) {
				foreach (int building in player.getBuildingList()) {
					BuildingClient buildingClient = this.APP.gameClient().buildingClient(building);
					if (this.mInfo.buildingClass(buildingClient.getClass()).meOrderType == orderType) { // && !this.IsQueueFull()) {
						Sprite logo = this.buildingArt[(int)orderType].logo;
						string str;
						if (buildingClient.isFrozen())
							str = HUDPanel.TEXT("TEXT_SECONDS", new TextVariable[1]
							{
			  Utils.getSecondsFromTurns(buildingClient.tileClient().getFrozenTime(), this.APP.gameClient()).ToText()
							});
						else
							str = string.Empty;
						string strLabel = str;
						HUDFlyoutOrderButton flyoutOrderButton = this.poolBuildingButtons.Acquire();
						flyoutOrderButton.backgroundImage.sprite = this.buildingArt[(int)orderType].background;
						flyoutOrderButton.SetIcon(logo);
						flyoutOrderButton.icon.color = !((UnityEngine.Object)logo == (UnityEngine.Object)null) ? Color.white : Color.clear;
						flyoutOrderButton.SetStatus(buildingClient.isFrozen());
						flyoutOrderButton.SetText(strLabel);
						flyoutOrderButton.SetData(this.APP.HUDDirector.GetMainEventHandler(), new GuiActionInfo((int)ItemType.SPECIAL_BUILDING_SELECT, buildingClient.getID(), 0));
						this.buildingButtonDictionary.Add(buildingClient.getID(), flyoutOrderButton);
					}
				}
			}
			for (OrderType orderType = OrderType.PATENT; orderType < OrderType.NUM_TYPES; ++orderType) {
				foreach (OrderInfo orderInfo1 in player.getOrderInfos(orderType)) {
					int miBuildingId = orderInfo1.miBuildingID;
					if (miBuildingId != -1 && this.buildingButtonDictionary.ContainsKey(miBuildingId)) {
						HUDFlyoutOrderButton buildingButton = this.buildingButtonDictionary[miBuildingId];
						if ((UnityEngine.Object)buildingButton != (UnityEngine.Object)null) {
							BuildingClient pBuilding = this.APP.gameClient().buildingClient(miBuildingId);
							UIOrderInfo orderInfo2 = this.GetOrderInfo(orderInfo1, pBuilding);
							this.UpdateButton(buildingButton, orderType, pBuilding.getType(), pBuilding.tileClient().getFrozenTime(), orderInfo2);
							this.buildingButtonDictionary.Remove(pBuilding.getID());
						}
					}
				}
			}
			foreach (KeyValuePair<int, HUDFlyoutOrderButton> buildingButton in this.buildingButtonDictionary) {
				BuildingClient buildingClient = this.APP.gameClient().buildingClient(buildingButton.Key);
				BuildingClassType eIndex = buildingClient.getClass();
				this.UpdateButton(buildingButton.Value, this.mInfo.buildingClass(eIndex).meOrderType, buildingClient.getType(), buildingClient.tileClient().getFrozenTime(), null);
			}
			this.poolBuildingButtons.PostReset();
		}

		private UIOrderInfo GetOrderInfo(OrderInfo order, BuildingClient pBuilding)
		{
			UIOrderInfo uiOrderInfo = new UIOrderInfo();
			PlayerClient playerClient = player;
			switch (order.meType) {
			case OrderType.PATENT:
				PatentType miData1_1 = (PatentType)order.miData1;
				if (this.mInfo.patent(miData1_1).miTime != 0) {
					uiOrderInfo.time = playerClient.getOrderTurns(OrderType.PATENT, order.miTime, pBuilding.tileClient());
					uiOrderInfo.iProgress = 100 - order.miTime * 100 / this.mInfo.patent(miData1_1).miTime;
					uiOrderInfo.spriteResearchIcon = this.APP.HUDDirector.gameSpriteRepository.GetPatentIcon(miData1_1);
					break;
				}
				uiOrderInfo.spriteResearchIcon = this.buildingArt[(int)order.meType].logo;
				break;
			case OrderType.RESEARCH:
				TechnologyType miData1_2 = (TechnologyType)order.miData1;
				int researchTime = PlayerClient.getResearchTime(miData1_2, (TechnologyLevelType)order.miData2, this.mInfo);
				if (researchTime != 0) {
					ResourceType resource = (ResourceType)this.mInfo.technology(miData1_2).mabResourceProduction.IndexOf(true, 0, (int)this.mInfo.technologiesNum());
					uiOrderInfo.time = playerClient.getOrderTurns(OrderType.RESEARCH, order.miTime, pBuilding.tileClient());
					uiOrderInfo.iProgress = 100 - order.miTime * 100 / researchTime;
					uiOrderInfo.spriteResearchModifierIcon = this.APP.HUDDirector.gameSpriteRepository.GetResourceIcon(resource);
					break;
				}
				break;
			case OrderType.HACK:
				EspionageType miData1_3 = (EspionageType)order.miData1;
				InfoEspionage infoEspionage = this.mInfo.espionage(miData1_3);
				if (infoEspionage.miTime != 0) {
					InfoEventGame infoEventGame = this.mInfo.eventGame(infoEspionage.meEventGame);
					uiOrderInfo.time = playerClient.getOrderTurns(OrderType.HACK, order.miTime, pBuilding.tileClient());
					uiOrderInfo.iProgress = 100 - order.miTime * 100 / this.mInfo.espionage(miData1_3).miTime;
					for (ResourceType resource = ~ResourceType.NONE; resource < this.mInfo.resourcesNum(); ++resource) {
						if (infoEventGame.maiResourceChange[(int)resource] != 0) {
							uiOrderInfo.spriteResearchIcon = !infoEspionage.mbSurplus ? specialQueue.spriteShortage : specialQueue.spriteSurplus;
							uiOrderInfo.spriteResearchModifierIcon = this.APP.HUDDirector.gameSpriteRepository.GetResourceIcon(resource);
							break;
						}
					}
					break;
				}
				break;
			case OrderType.LAUNCH:
				ResourceType miData1_4 = (ResourceType)order.miData1;
				if (this.mInfo.Globals.LAUNCH_TIME != 0) {
					uiOrderInfo.time = playerClient.getOrderTurns(OrderType.LAUNCH, order.miTime, pBuilding.tileClient());
					uiOrderInfo.iProgress = 100 - order.miTime * 100 / this.mInfo.Globals.LAUNCH_TIME;
					uiOrderInfo.spriteResearchModifierIcon = this.APP.HUDDirector.gameSpriteRepository.GetResourceIcon(miData1_4);
					break;
				}
				break;
			}
			return uiOrderInfo;
		}

		private void UpdateButton(HUDFlyoutOrderButton button, OrderType eOrder, BuildingType eBuilding, int frozenTime, UIOrderInfo uiOrderInfo)
		{
			button.SetPercentNum(uiOrderInfo == null ? 0 : 100 - uiOrderInfo.iProgress);
			Sprite spriteIcon = this.buildingArt[(int)eOrder].logo;
			Sprite sprite = this.APP.HUDDirector.gameSpriteRepository.GetBuildingOrderIcon(eBuilding);
			if ((UnityEngine.Object)sprite == (UnityEngine.Object)null)
				sprite = this.buildingArt[(int)eOrder].background;
			if (uiOrderInfo != null && (UnityEngine.Object)uiOrderInfo.spriteResearchIcon != (UnityEngine.Object)null)
				spriteIcon = uiOrderInfo.spriteResearchIcon;
			button.SetIcon(spriteIcon);
			button.backgroundImage.sprite = sprite;
			if (uiOrderInfo != null) {
				button.SetText(HUDPanel.TEXT("TEXT_SECONDS", new TextVariable[1]
				{
		Utils.getSecondsFromTurns(uiOrderInfo.time, this.APP.gameClient()).ToText()
				}));
				button.infinityBackgroundImage.sprite = uiOrderInfo.spriteResearchModifierIcon;
				button.infinityBackgroundImage.gameObject.SetActive((UnityEngine.Object)uiOrderInfo.spriteResearchModifierIcon != (UnityEngine.Object)null);
			} else {
				string str;
				if (frozenTime > 0)
					str = HUDPanel.TEXT("TEXT_SECONDS", new TextVariable[1]
					{
		  Utils.getSecondsFromTurns(frozenTime, this.APP.gameClient()).ToText()
					});
				else
					str = (string)null;
				string strLabel = str;
				button.SetText(strLabel);
				button.infinityBackgroundImage.sprite = (Sprite)null;
				button.infinityBackgroundImage.gameObject.SetActive(false);
			}
		}

		public void InitFrankenQueue(HUDFrankenguiController controller)
		{
			this.mController = controller;
		}

		public int GetNumEntries()
		{
			if (this.poolBuildingButtons == null)
				return 0;
			return this.poolBuildingButtons.AcquiredCount;
		}

		private bool IsQueueFull()
		{
			return this.mController.GetNumTotalEntries() >= this.mController.GetNumMaxEntries();
		}

		private class UIOrderInfo
		{
			public int iProgress = -1;
			public Sprite spriteResearchIcon;
			public Sprite spriteResearchModifierIcon;
			public int time;
		}
	}
}

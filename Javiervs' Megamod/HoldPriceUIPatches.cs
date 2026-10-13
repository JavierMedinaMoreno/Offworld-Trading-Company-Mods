using System;
using Harmony;
using Offworld.AppCore;
using Offworld.GameCore;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JaviervsMegamod
{
    /// <summary>
    /// Harmony UI patches for TextButton and HUDTooltipBuilder to handle Hold Price toggling
    /// via CTRL + SHIFT + Click on the Buy button, display gold stockpile text, and inject tooltips.
    /// </summary>
    public static class HoldPriceUIPatches
    {
        public static bool IsHoveringBuyTrade = false;

        [HarmonyPatch(typeof(TextButton), "OnPointerClick")]
        public static class TextButton_OnPointerClick_Patch
        {
            public static bool Prefix(TextButton __instance, PointerEventData eventData)
            {
                if (!UIPlus.ModOptionsSave.Options.enableHoldPrice)
                {
                    return true;
                }

                if (eventData.button != PointerEventData.InputButton.Left)
                {
                    return true;
                }

                bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (!isCtrl || !isShift)
                {
                    return true;
                }

                HUDResourceRow row = __instance.GetComponentInParent<HUDResourceRow>();
                if (row != null && (row.buttonBuy == __instance || __instance.name.IndexOf("Buy", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    PlayerClient activePlayer = AppMain.gApp?.activePlayerClient();
                    if (activePlayer != null)
                    {
                        HoldPricePlayerServer serverPlayer = AppMain.gApp.gameServer()?.playerServer(activePlayer.getPlayer()) as HoldPricePlayerServer;
                        if (serverPlayer != null)
                        {
                            bool wasHeld = serverPlayer.isPriceHeld(row.resource);
                            serverPlayer.toggleHoldPrice(row.resource);
                            return false; // Suppress standard buy transaction
                        }
                        else
                        {
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Captures when the player is hovering the Buy button in HUDTooltipBuilder.Trade.
        /// </summary>
        [HarmonyPatch(typeof(HUDTooltipBuilder), "Trade")]
        public static class HUDTooltipBuilder_Trade_Patch
        {
            public static void Prefix(bool buying)
            {
                if (!UIPlus.ModOptionsSave.Options.enableHoldPrice)
                {
                    IsHoveringBuyTrade = false;
                    return;
                }
                IsHoveringBuyTrade = buying;
            }

            public static void Postfix()
            {
                IsHoveringBuyTrade = false;
            }
        }

        /// <summary>
        /// Injects the Hold Price toggle shortcut into TooltipBuySellData.hotkeys so it renders
        /// in the dedicated hotkeys area at the bottom of the Buy tooltip.
        /// Includes deduplication check to prevent repeating lines on subsequent hover frame updates.
        /// </summary>
        [HarmonyPatch(typeof(TooltipControl), "BuildTooltip")]
        public static class TooltipControl_BuildTooltip_TradePatch
        {
            public static void Prefix(RectTransform hovered, TooltipDataBase data)
            {
                if (!UIPlus.ModOptionsSave.Options.enableHoldPrice)
                {
                    return;
                }

                if (IsHoveringBuyTrade)
                {
                    string shortcutText = "<color=#FFD700>SHIFT+CTRL to Toggle Hold Price</color>";

                    TooltipBuySellData buySellData = data as TooltipBuySellData;
                    if (buySellData != null)
                    {
                        if (string.IsNullOrEmpty(buySellData.hotkeys))
                        {
                            buySellData.hotkeys = shortcutText;
                        }
                        else if (!buySellData.hotkeys.Contains("Toggle Hold Price"))
                        {
                            buySellData.hotkeys += "\n" + shortcutText;
                        }
                        return;
                    }

                    TooltipMainDataBase mainData = data as TooltipMainDataBase;
                    if (mainData != null)
                    {
                        mainData.AddDescriptionLine(shortcutText, false);
                    }
                }
            }
        }

        /// <summary>
        /// Frame update visual indicator: called every frame from ModEntryPoint.Update().
        /// Sets native Graphic.color on textRate and textQuantity so the numbers render yellow
        /// persistently without being overwritten by vanilla string updates.
        /// </summary>
        public static void UpdateVisuals()
        {
            if (!UIPlus.ModOptionsSave.Options.enableHoldPrice)
            {
                return;
            }

            if (AppMain.gApp == null)
            {
                return;
            }

            GameClient gameClient = AppMain.gApp.gameClient();
            if (gameClient == null)
            {
                return;
            }

            GameServer gameServer = AppMain.gApp.gameServer();
            if (gameServer == null)
            {
                return;
            }

            PlayerClient activePlayer = null;
            try
            {
                activePlayer = AppMain.gApp.activePlayerClient();
            }
            catch
            {
                return;
            }

            if (activePlayer == null)
            {
                return;
            }

            HoldPricePlayerServer serverPlayer = gameServer.playerServer(activePlayer.getPlayer()) as HoldPricePlayerServer;
            if (serverPlayer == null)
            {
                return;
            }

            HUDResourceRow[] rows = UnityEngine.Object.FindObjectsOfType<HUDResourceRow>();
            if (rows == null || rows.Length == 0)
            {
                return;
            }

            Color goldColor = new Color(1.0f, 0.84f, 0.0f, 1.0f); // Bright Gold / Yellow
            Color defaultColor = Color.white;

            for (int i = 0; i < rows.Length; i++)
            {
                HUDResourceRow row = rows[i];
                if (row == null)
                {
                    continue;
                }

                bool isHeld = serverPlayer.isPriceHeld(row.resource);
                Color targetColor = isHeld ? goldColor : defaultColor;

                if (row.textRate != null && row.textRate.color != targetColor)
                {
                    row.textRate.color = targetColor;
                }

                if (row.textQuantity != null && row.textQuantity.color != targetColor)
                {
                    row.textQuantity.color = targetColor;
                }
            }
        }
    }
}

using System;
using Harmony;
using Offworld.AppCore;
using Offworld.GameCore;
using UnityEngine;

namespace BuyoutDebtAI
{
    /// <summary>
    /// Patches HUDTooltipBuilder and TooltipControl to display attached debt liabilities
    /// directly on the stock buy tooltips for human players.
    /// </summary>
    public static class BuyoutTooltipPatches
    {
        public static PlayerType CurrentTarget = PlayerType.NONE;

        [HarmonyPatch(typeof(HUDTooltipBuilder), "BuyStock")]
        public static class HUDTooltipBuilder_BuyStock_Patch
        {
            public static void Prefix(RectTransform rect, PlayerType targetPlayerType)
            {
                CurrentTarget = targetPlayerType;
            }

            public static void Postfix()
            {
                CurrentTarget = PlayerType.NONE;
            }
        }

        [HarmonyPatch(typeof(TooltipControl), "BuildTooltip")]
        public static class TooltipControl_BuildTooltip_Patch
        {
            public static void Prefix(RectTransform hovered, TooltipDataBase data)
            {
                PlayerType targetPlayer = CurrentTarget;
                if (targetPlayer == PlayerType.NONE && hovered != null)
                {
                    HUDPlayerListing listing = hovered.GetComponentInParent<HUDPlayerListing>();
                    if (listing != null)
                    {
                        targetPlayer = listing.playerType;
                    }
                }

                if (targetPlayer == PlayerType.NONE)
                {
                    return;
                }

                PlayerClient activePlayer = AppMain.gApp?.activePlayerClient();
                // Skip if target is self (buying back own shares carries no debt transfer liability)
                if (activePlayer != null && targetPlayer == activePlayer.getPlayer())
                {
                    return;
                }

                PlayerClient target = AppMain.gApp?.gameClient()?.playerClient(targetPlayer);
                if (target == null || target.isSubsidiary())
                {
                    return;
                }

                int actualDebt = Math.Abs(target.getDebt());
                if (actualDebt <= 0)
                {
                    return;
                }

                long tangibleAssetsLong = target.getCashResourceValue() + target.calculateStructuresValue() + target.calculateModulesValue();
                int tangibleAssets = (int)Math.Max(1, tangibleAssetsLong);
                int transferableDebt = Math.Min(actualDebt, tangibleAssets);
                int cappedDebtPerShare = transferableDebt / 10;
                int dischargedDebt = Math.Max(0, actualDebt - transferableDebt);

                if (cappedDebtPerShare <= 0 && dischargedDebt <= 0)
                {
                    return;
                }

                TooltipMainDataBase mainData = data as TooltipMainDataBase;
                if (mainData == null)
                {
                    return;
                }

                // Deduplication check to prevent duplicate description lines on hover refresh
                if (mainData.descriptions != null)
                {
                    for (int i = 0; i < mainData.descriptions.Count; i++)
                    {
                        TooltipDescTextData textData = mainData.descriptions[i] as TooltipDescTextData;
                        if (textData != null && textData.text != null && textData.text.Contains("Attached Debt Liability"))
                        {
                            return;
                        }
                    }
                }

                string warningColor = (target.getBondRating() >= Globals.Infos.bondsNum() - 1) ? "#FF4444" : "#FFB347";
                string warningText = string.Format("<color={0}>Attached Debt Liability: +${1:N0} / share upon acquisition</color>", 
                    warningColor, cappedDebtPerShare);

                mainData.AddDescriptionLine(warningText, false);

                if (dischargedDebt > 0)
                {
                    string dischargedText = string.Format("<color=#88DDAA>Discharged Debt: ${0:N0} written off</color>", dischargedDebt);
                    mainData.AddDescriptionLine(dischargedText, false);
                }

                if (activePlayer != null && activePlayer.getTeam() != target.getTeam())
                {
                    int sharesOwned = activePlayer.getSharesOwned(targetPlayer);
                    int sharesAvailable = target.getSharesAvailable();
                    int totalPostBuyoutShares = sharesOwned + sharesAvailable;

                    if (totalPostBuyoutShares > 0)
                    {
                        int projectedBuyoutDebt = cappedDebtPerShare * totalPostBuyoutShares;
                        string buyoutWarning = string.Format("<color={0}>Projected Buyout Debt: +${1:N0} total</color>", 
                            warningColor, projectedBuyoutDebt);
                        mainData.AddDescriptionLine(buyoutWarning, false);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Offworld.AppCore;
using Offworld.GameCore;

namespace JaviervsMegamod
{
    /// <summary>
    /// Custom PlayerServer subclass providing:
    /// 1. Buyout Debt Mechanic (Limited Liability): Upon acquiring a rival company, the acquired company's debt
    ///    is distributed proportionally to all shareholders (out of 10 shares), capped at the company's tangible asset value.
    /// 2. Debt-Aware AI Decision Making & Divestment: AI players simulate solvency, evaluate capped debt liabilities,
    ///    divest from toxic companies before impending buyouts, and prioritize liquidating indebted stock.
    /// 3. Monopoly Victory Preservation: Subsidiaries are non-win-eligible, allowing remaining players to win by monopoly.
    /// </summary>
    public class BuyoutAIPlayerServer : HoldPricePlayerServer
    {
        public BuyoutAIPlayerServer(GameClient pGame) : base(pGame)
        {
        }

        #region Asset Valuation & Capped Debt Helpers

        /// <summary>
        /// Calculates the total company equity value for stock pricing and acquisition evaluations.
        /// In vanilla OTC, unbounded debt is subtracted from physical assets, driving company equity negative
        /// when debt exceeds physical assets.
        /// Under Limited Liability, this override caps the debt deduction to gross assets:
        ///   Gross Assets = HQ + Cash/Stockpiles + Structures + Modules
        ///   Debt Deduction = Minimum(Debt * Multiplier, Gross Assets)
        ///   Net Company Value = Gross Assets - Debt Deduction
        /// This ensures company equity floors cleanly at $0 without negative valuation artifacts.
        /// </summary>
        /// <param name="iDebt">The debt amount (stored as negative for outstanding debt).</param>
        /// <returns>The calculated company equity value (minimum 0 when debt exceeds assets).</returns>
        public override long calculateTotalStockValue(int iDebt)
        {
            if (!UIPlus.ModOptionsSave.Options.enableLimitedLiability)
            {
                return base.calculateTotalStockValue(iDebt);
            }

            long grossAssets = 0;
            grossAssets += calculateHQValue();
            grossAssets += getCashResourceValue();
            grossAssets += calculateStructuresValue();
            grossAssets += calculateModulesValue();

            if (iDebt >= 0)
            {
                return grossAssets + iDebt;
            }

            // Under Limited Liability, debt deduction cannot exceed gross asset value
            long debtMultiplier = infos().handicap(getHandicap()).miDebtMultiplier;
            long debtDeduction = (long)Math.Abs(iDebt) * debtMultiplier;

            // Cap the debt deduction to gross assets so net company equity floors cleanly at 0
            long cappedDeduction = Math.Min(debtDeduction, grossAssets);

            return grossAssets - cappedDeduction;
        }

        /// <summary>
        /// Calculates the tangible physical asset value of a corporation:
        /// Structures (including physical HQ resource construction cost) + Cash and resource stockpiles + Colony modules.
        /// Excludes vanilla's artificial multi-million paper HQ value multiplier.
        /// </summary>
        /// <param name="ePlayer">The player whose tangible assets are being evaluated.</param>
        /// <returns>The total positive dollar tangible asset value (minimum 1).</returns>
        public virtual int getTangibleAssetValue(PlayerType ePlayer)
        {
            PlayerServer pPlayer = gameServer().playerServer(ePlayer);
            if (pPlayer == null)
            {
                return 1;
            }

            long tangible = pPlayer.getCashResourceValue() + pPlayer.calculateStructuresValue() + pPlayer.calculateModulesValue();
            return (int)Math.Max(1, tangible);
        }

        /// <summary>
        /// Calculates the gross asset value of a corporation without factoring in debt liabilities.
        /// Accounts for HQ value, cash and resource stockpiles, production structures, and modules.
        /// </summary>
        /// <param name="ePlayer">The player whose assets are being evaluated.</param>
        /// <returns>The total positive dollar asset value (minimum 1).</returns>
        public virtual int getGrossAssetValue(PlayerType ePlayer)
        {
            PlayerServer pPlayer = gameServer().playerServer(ePlayer);
            if (pPlayer == null)
            {
                return 1;
            }
            long val = pPlayer.calculateTotalStockValue(0);
            return (int)Math.Max(1, val);
        }

        /// <summary>
        /// Calculates the cash market equity value of 1 share of a company when its debt is $0 (its post-bankruptcy value).
        /// This serves as the benchmark for zero-arbitrage parity.
        /// </summary>
        /// <param name="ePlayer">The target company.</param>
        /// <returns>The dollar market price of 1 share with $0 debt.</returns>
        public virtual int getMarketEquityValuePerShare(PlayerType ePlayer)
        {
            PlayerServer pPlayer = gameServer().playerServer(ePlayer);
            if (pPlayer == null)
            {
                return 1;
            }

            int basePriceDebtFree = pPlayer.calculateBaseSharePrice(0, true);
            int sharePriceDebtFree = pPlayer.calculateSharePrice(0, basePriceDebtFree);
            int cashPriceDebtFree = (sharePriceDebtFree * infos().Globals.SHARE_PURCHASE_SIZE) / Constants.STOCK_MULTIPLIER;

            return Math.Max(1, cashPriceDebtFree);
        }

        /// <summary>
        /// Computes the transferable debt under the Tangible Asset Limited Liability rule.
        /// Debt transferred to shareholders/acquirers is capped to the company's tangible physical asset value
        /// (Structures + Cash/Stockpiles + Modules).
        /// Any debt in excess of tangible assets is discharged/written off upon corporate restructuring or buyout.
        /// </summary>
        /// <param name="ePlayer">The target player.</param>
        /// <returns>The capped dollar amount of debt that can be transferred across all 10 shares (minimum 0).</returns>
        public virtual int getCappedTransferableDebt(PlayerType ePlayer)
        {
            PlayerServer pPlayer = gameServer().playerServer(ePlayer);
            if (pPlayer == null)
            {
                return 0;
            }

            int actualDebt = Math.Abs(pPlayer.getDebt());
            if (actualDebt <= 0)
            {
                return 0;
            }

            // Limited Liability Tangible Cap: debt transferred across all 10 shares cannot exceed tangible physical assets
            int tangibleAssets = getTangibleAssetValue(ePlayer);
            return Math.Min(actualDebt, tangibleAssets);
        }

        /// <summary>
        /// Calculates the sell price of a share.
        /// Under the Zero-Arbitrage principle, the sell price of a share equals its debt-free market equity value
        /// minus its attached debt liability (flooring at $0.00).
        /// This guarantees that selling pre-bankruptcy vs holding through bankruptcy produces the exact same net value ($0).
        /// </summary>
        /// <param name="ePlayer">The company whose share is being sold.</param>
        /// <returns>The cash payout for selling 1 share (minimum 0).</returns>
        public override int getSellSharePrice(PlayerType ePlayer)
        {
            if (!UIPlus.ModOptionsSave.Options.enableLimitedLiability)
            {
                return base.getSellSharePrice(ePlayer);
            }

            PlayerServer pPlayer = gameServer().playerServer(ePlayer);
            if (pPlayer == null)
            {
                return base.getSellSharePrice(ePlayer);
            }

            // Subsidiaries have zero debt and trade at standard 90% price
            if (pPlayer.isSubsidiary())
            {
                return base.getSellSharePrice(ePlayer);
            }

            int marketEquityPerShare = getMarketEquityValuePerShare(ePlayer);
            int debtPerShare = getCappedTransferableDebt(ePlayer) / 10;

            // Zero-Arbitrage Sell Price: Market Equity Value - Debt Liability
            int netShareValue = marketEquityPerShare - debtPerShare;
            if (netShareValue <= 0)
            {
                return 0;
            }

            // Apply standard 90% sell factor to net equity value
            int sellPrice = (netShareValue * 9) / 10;
            return Math.Max(0, sellPrice);
        }

        #endregion

        #region Buyout Debt Mechanic (Limited Liability Capped)

        /// <summary>
        /// Executes a company buyout. In vanilla OTC, the acquired player's debt is discarded.
        /// This override calculates the transferable debt capped by gross asset value,
        /// executes the base buyout routine to transfer remaining shares and create a subsidiary,
        /// and distributes capped debt proportionally to all shareholders based on their share count.
        /// </summary>
        /// <param name="ePlayer">The rival player being bought out.</param>
        public override void buyout(PlayerType ePlayer)
        {
            if (!UIPlus.ModOptionsSave.Options.enableLimitedLiability)
            {
                base.buyout(ePlayer);
                return;
            }

            PlayerServer pPlayer = gameServer().playerServer(ePlayer);

            // Limited Liability: Transferable debt is capped at gross asset value
            int transferableDebt = getCappedTransferableDebt(ePlayer);
            int debtPerShare = transferableDebt / 10;

            // Execute base buyout logic:
            // - Deducts buyout cost from buyer's cash
            // - Transfers all remaining available shares to the buyer
            // - Converts the target into a subsidiary
            // - Awards extra claims and triggers buyout achievements
            base.buyout(ePlayer);

            // Distribute the capped debt to each player according to their post-buyout share count.
            if (debtPerShare > 0)
            {
                for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
                {
                    PlayerServer pLoopPlayer = gameServer().playerServer(eLoopPlayer);
                    int sharesOwned = pLoopPlayer.getSharesOwned(ePlayer);
                    if (sharesOwned > 0)
                    {
                        // changeDebt takes a negative number to add debt
                        pLoopPlayer.changeDebt(-(debtPerShare * sharesOwned));
                    }
                }
            }
        }

        #endregion

        #region AI Debt Projection & Valuation Helpers

        /// <summary>
        /// Calculates the total debt increase this AI will absorb if it acquires the target player.
        /// Uses the Limited Liability capped debt rather than raw unbounded debt.
        /// </summary>
        /// <param name="eTarget">The rival being evaluated for buyout.</param>
        /// <returns>The projected dollar amount of debt to be added to this AI's balance sheet.</returns>
        protected virtual int getProjectedBuyoutDebtIncrease(PlayerType eTarget)
        {
            PlayerServer pTarget = gameServer().playerServer(eTarget);
            if (pTarget == null)
            {
                return 0;
            }

            int transferableDebt = getCappedTransferableDebt(eTarget);
            if (transferableDebt <= 0)
            {
                return 0;
            }

            int debtPerShare = transferableDebt / 10;
            int totalSharesHeldPostBuyout = getSharesOwned(eTarget) + pTarget.getSharesAvailable();

            return debtPerShare * totalSharesHeldPostBuyout;
        }

        /// <summary>
        /// Computes the total enterprise buyout cost to acquire a target corporation:
        /// Total Enterprise Cost = Cash Buyout Price (getTotalBuyoutPrice) + Projected Capped Debt Inherited
        /// Both components are on the stock market scale, representing the true total liability assumed.
        /// </summary>
        /// <param name="eTarget">The rival corporation being acquired.</param>
        /// <returns>The combined cash and debt cost to execute the buyout.</returns>
        public virtual int getTotalEnterpriseBuyoutCost(PlayerType eTarget)
        {
            int cashPrice = getTotalBuyoutPrice(eTarget);
            int debtAbsorbed = getProjectedBuyoutDebtIncrease(eTarget);
            return cashPrice + debtAbsorbed;
        }

        /// <summary>
        /// Simulates whether absorbing the target's capped debt will keep this AI solvent.
        /// Computes the AI's projected bond rating with the added debt. If the projected rating
        /// drops to junk status (lowest rating level) or worse, the acquisition is deemed unsafe.
        /// </summary>
        /// <param name="eTarget">The rival being evaluated.</param>
        /// <returns>True if the AI can safely absorb the debt; false if it risks junk credit/bankruptcy.</returns>
        protected virtual bool isBuyoutDebtSafe(PlayerType eTarget)
        {
            int debtIncrease = getProjectedBuyoutDebtIncrease(eTarget);
            int projectedTotalDebt = -(Math.Abs(getDebt()) + debtIncrease);

            int iBetter = 0;
            int iWorse = 0;
            int iExcess = 0;
            BondType projectedBond = calculateBondRating(projectedTotalDebt, 0, ref iBetter, ref iWorse, ref iExcess, true);

            // Avoid buyouts that would plunge the AI into junk credit or insolvency.
            if (projectedBond >= (infos().bondsNum() - 1))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Computes the perceived cost of purchasing a single share of a rival company.
        /// Incorporates the capped debt liability per share (cappedTransferableDebt / 10).
        /// </summary>
        /// <param name="eTarget">The rival whose share is being priced.</param>
        /// <returns>The combined market price plus per-share capped debt liability.</returns>
        protected virtual int getPerceivedShareCost(PlayerType eTarget)
        {
            int price = getBuySharePrice(eTarget);
            int transferableDebt = getCappedTransferableDebt(eTarget);
            if (transferableDebt > 0)
            {
                price += (transferableDebt / 10);
            }
            return price;
        }

        /// <summary>
        /// Determines whether this AI should proactively dump/sell its shares in a rival company
        /// because the rival's debt poses a dangerous liability and an acquisition by a third party is impending.
        /// </summary>
        /// <param name="eRival">The rival corporation being evaluated.</param>
        /// <returns>True if the AI should sell shares in this rival immediately.</returns>
        protected virtual bool shouldDivestIndebtedRivalShares(PlayerType eRival)
        {
            if (!canSellShares(eRival) || getSharesOwned(eRival) <= 0)
            {
                return false;
            }

            PlayerServer pRival = gameServer().playerServer(eRival);
            if (pRival == null || pRival.getDebt() <= 0)
            {
                return false;
            }

            // Do not divest if this AI is actively targeting this rival for its own buyout
            if (eRival == AI_getBestBuyoutTarget(3, 50))
            {
                return false;
            }

            int transferableDebt = getCappedTransferableDebt(eRival);
            int potentialDebtAbsorption = (transferableDebt / 10) * getSharesOwned(eRival);
            if (potentialDebtAbsorption <= 0)
            {
                return false;
            }

            // Check 1: Would holding these shares plunge this AI into junk credit if the rival gets bought out?
            int projectedTotalDebt = -(Math.Abs(getDebt()) + potentialDebtAbsorption);
            int iBetter = 0;
            int iWorse = 0;
            int iExcess = 0;
            BondType projectedBond = calculateBondRating(projectedTotalDebt, 0, ref iBetter, ref iWorse, ref iExcess, true);
            bool isDangerousCredit = projectedBond >= (infos().bondsNum() - 1);

            // Check 2: Is an acquisition or bankruptcy by another player imminent?
            bool bBuyoutImminent = false;
            if (pRival.getSharesAvailable() <= 2)
            {
                bBuyoutImminent = true;
            }
            else
            {
                foreach (PlayerServer pOther in gameServer().getPlayerServerAliveAll())
                {
                    if (pOther != this && pOther != pRival && pOther.getTeam() != getTeam())
                    {
                        if (pOther.getBuyoutPercent(eRival) >= 50 || pOther.getSharesOwned(eRival) >= 4)
                        {
                            bBuyoutImminent = true;
                            break;
                        }
                    }
                }
            }

            if (isDangerousCredit || (bBuyoutImminent && potentialDebtAbsorption > 10000))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Exposes protected AI_vulnerableMajorityBuyout() as a public virtual method
        /// so teammate instances can inspect vulnerability without C# protected access violations.
        /// </summary>
        public virtual bool isVulnerableMajorityBuyout()
        {
            if (!UIPlus.ModOptionsSave.Options.enableLimitedLiability)
            {
                return false;
            }
            return AI_vulnerableMajorityBuyout();
        }

        #endregion

        #region AI Stock Decision Overrides

        /// <summary>
        /// Selects the best buyout target based on owned shares and buyout percentage.
        /// Overridden to filter out targets whose debt burden would destabilize the AI's credit rating.
        /// </summary>
        protected override PlayerType AI_getBestBuyoutTarget(int iMinShares, int iMinPercent)
        {
            if (!UIPlus.ModOptionsSave.Options.enableLimitedLiability)
            {
                return base.AI_getBestBuyoutTarget(iMinShares, iMinPercent);
            }

            if (isHumanTeam())
            {
                return PlayerType.NONE;
            }

            PlayerType eBestPlayer = PlayerType.NONE;
            int iBestValue = 0;

            foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
            {
                if (pLoopPlayer.getTeam() != getTeam())
                {
                    // DEBT-AWARE: Do not prioritize targets whose debt absorption would cause insolvency
                    if (!isBuyoutDebtSafe(pLoopPlayer.getPlayer()))
                    {
                        continue;
                    }

                    int iShares = getSharesOwned(pLoopPlayer.getPlayer());
                    int iPercent = getBuyoutPercent(pLoopPlayer.getPlayer());

                    if ((iShares >= iMinShares) || (iPercent >= iMinPercent))
                    {
                        int iValue = ((iShares + 1) * (iPercent + 1));
                        if (iValue > iBestValue)
                        {
                            eBestPlayer = pLoopPlayer.getPlayer();
                            iBestValue = iValue;
                        }
                    }
                }
            }

            return eBestPlayer;
        }

        /// <summary>
        /// Overridden stock liquidation logic.
        /// Prioritizes liquidating shares of heavily indebted rivals first to offload toxic liabilities
        /// while preserving equity in solvent companies.
        /// </summary>
        protected override int AI_fundStock(bool bTest, PlayerType eSkipPlayer)
        {
            if (!UIPlus.ModOptionsSave.Options.enableLimitedLiability)
            {
                return base.AI_fundStock(bTest, eSkipPlayer);
            }

            PlayerType eBestCandidate = PlayerType.NONE;
            int iHighestDebt = -1;
            int iBestProfit = 0;

            // Search for rival shares with the highest debt liability to offload first
            for (PlayerType eLoopPlayer = 0; eLoopPlayer < gameServer().getNumPlayers(); eLoopPlayer++)
            {
                if (eLoopPlayer != eSkipPlayer && eLoopPlayer != getPlayer())
                {
                    PlayerServer pRival = gameServer().playerServer(eLoopPlayer);
                    if (pRival != null && pRival.getTeam() != getTeam() && canSellShares(eLoopPlayer))
                    {
                        int rivalDebt = Math.Abs(pRival.getDebt());
                        if (rivalDebt > iHighestDebt)
                        {
                            iHighestDebt = rivalDebt;
                            eBestCandidate = eLoopPlayer;
                            iBestProfit = getSellSharePrice(eLoopPlayer);
                        }
                    }
                }
            }

            if (eBestCandidate != PlayerType.NONE && iHighestDebt > 0)
            {
                if (!bTest)
                {
                    sellShares(eBestCandidate);
                }
                return iBestProfit;
            }

            return base.AI_fundStock(bTest, eSkipPlayer);
        }

        /// <summary>
        /// Core AI stock evaluation routine called periodically during game turns.
        /// Overrides vanilla PlayerAI logic with debt-aware valuation, solvency checks,
        /// and proactive toxic asset divestment.
        /// </summary>
        protected override void AI_doStockPlayers()
        {
            if (!UIPlus.ModOptionsSave.Options.enableLimitedLiability)
            {
                base.AI_doStockPlayers();
                return;
            }

            // Level 1 HQs do not participate in the stock market
            if (getHQLevelInt() == 1)
            {
                return;
            }

            // Self-Defense: Buy own stock immediately if vulnerable to an opponent's majority buyout
            if (canBuyShares(getPlayer(), false))
            {
                if (AI_vulnerableMajorityBuyout())
                {
                    int iPrice = getBuySharePrice(getPlayer());

                    if (AI_fundResources(iPrice, 1, false, false, null, null, true, getPlayer()))
                    {
                        buyShares(getPlayer());
                        return;
                    }
                }
            }

            // Level 2 HQs prioritize using claims before investing in stock
            if (getHQLevelInt() == 2)
            {
                if (getClaims() > 0)
                {
                    return;
                }
            }

            // Proactive Divestment: Check if any owned rival stock represents a toxic debt liability
            foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
            {
                if (pLoopPlayer.getTeam() != getTeam() && shouldDivestIndebtedRivalShares(pLoopPlayer.getPlayer()))
                {
                    sellShares(pLoopPlayer.getPlayer());
                    return;
                }
            }

            // Randomized turn roll based on player handicap
            if (gameServer().random().Next(infos().handicap(gameServer().getHumanHandicap()).miAIStockRoll) != 0)
            {
                return;
            }

            // Personality-based cash threshold check
            if (getCashResourceValue() < gameServer().random().Next(infos().personality(getPersonality()).miStockThreshold))
            {
                if (gameServer().random().Next(10) != 0)
                {
                    return;
                }
            }

            // Team Game: Defend allied teammates if they are vulnerable to majority takeover
            if (gameServer().isTeamGame())
            {
                foreach (PlayerServer pTeamPlayer in getAliveTeammatesAll())
                {
                    if (pTeamPlayer != this)
                    {
                        if (canBuyShares(pTeamPlayer.getPlayer(), false))
                        {
                            BuyoutAIPlayerServer teamAI = pTeamPlayer as BuyoutAIPlayerServer;
                            if (teamAI != null && teamAI.isVulnerableMajorityBuyout())
                            {
                                int iPrice = getBuySharePrice(pTeamPlayer.getPlayer());

                                if (AI_fundResources(iPrice, 1, false, false, null, null, true, pTeamPlayer.getPlayer()))
                                {
                                    buyShares(pTeamPlayer.getPlayer());
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            if (getHQLevelInt() == 2)
            {
                return;
            }

            // Defensive Buyback: Secure own majority shares if rivals have accumulated too many
            if (canBuyShares(getPlayer(), false))
            {
                if (countOwnSharesOwned() < gameServer().getMajorityShares())
                {
                    if (getSharesBoughtRivals() > Math.Max(0, (countOwnSharesOwned() - 3)))
                    {
                        int iPrice = getBuySharePrice(getPlayer());

                        if (AI_fundResources(iPrice, 10, false, false, null, null, false, PlayerType.NONE))
                        {
                            buyShares(getPlayer());
                            return;
                        }
                    }
                }
            }

            // Phase 1: Offensive Majority Pursuit
            if (isWinEligible())
            {
                const int MIN_PRICE_MULTIPLIER = 1;

                PlayerType eBestPlayer = PlayerType.NONE;
                bool bBestSaveUpgrade = false;
                int iBestValue = int.MaxValue;

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                {
                    if (pLoopPlayer.getTeam() != getTeam())
                    {
                        if (canBuyShares(pLoopPlayer.getPlayer(), false))
                        {
                            int iMajoritySharesNeeded = ((gameServer().getMajorityShares() + 1) - pLoopPlayer.getSharesBoughtRivals());

                            if ((iMajoritySharesNeeded > 0) && (iMajoritySharesNeeded <= pLoopPlayer.getSharesAvailable()))
                            {
                                int iTargetMoney = (getBuySharePrice(pLoopPlayer.getPlayer()) * iMajoritySharesNeeded);
                                bool bSaveUpgrade = (iMajoritySharesNeeded > 1);

                                if (AI_fundResources(iTargetMoney, MIN_PRICE_MULTIPLIER, bSaveUpgrade, true, null, null, !bSaveUpgrade, pLoopPlayer.getPlayer()))
                                {
                                    // DEBT-AWARE: Perceived cost incorporates capped debt per share
                                    int iValue = getPerceivedShareCost(pLoopPlayer.getPlayer());

                                    iValue += gameServer().random().Next(100);

                                    if (iValue < iBestValue)
                                    {
                                        eBestPlayer = pLoopPlayer.getPlayer();
                                        bBestSaveUpgrade = bSaveUpgrade;
                                        iBestValue = iValue;
                                    }
                                }
                            }
                        }
                    }
                }

                if (eBestPlayer != PlayerType.NONE)
                {
                    AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, bBestSaveUpgrade, false, null, null, !bBestSaveUpgrade, eBestPlayer);
                    buyShares(eBestPlayer);
                    return;
                }
            }

            // Phase 2: Instant Buyout Execution (Target already at majority / canOnlyBuyout)
            if (isWinEligible() && !isHumanTeam())
            {
                const int MIN_PRICE_MULTIPLIER = 1;

                PlayerType eBestPlayer = PlayerType.NONE;
                long iBestValue = 0;

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                {
                    if (pLoopPlayer.getTeam() != getTeam())
                    {
                        if (canBuyout(pLoopPlayer.getPlayer(), false))
                        {
                            if (canOnlyBuyout(pLoopPlayer.getPlayer()))
                            {
                                // DEBT-AWARE: Verify post-buyout credit solvency before proceeding
                                if (isBuyoutDebtSafe(pLoopPlayer.getPlayer()))
                                {
                                    if (AI_fundResources(getTotalBuyoutPrice(pLoopPlayer.getPlayer()), MIN_PRICE_MULTIPLIER, false, true, null, null, true, pLoopPlayer.getPlayer()))
                                    {
                                        // DEBT-AWARE: Net Buyout Value on unified stock market scale
                                        // Net Buyout Value = Total Market Equity - Total Enterprise Buyout Cost
                                        int totalMarketEquity = getMarketEquityValuePerShare(pLoopPlayer.getPlayer()) * 10;
                                        int enterpriseCost = getTotalEnterpriseBuyoutCost(pLoopPlayer.getPlayer());
                                        long iValue = totalMarketEquity - enterpriseCost;

                                        if (iValue > 0)
                                        {
                                            iValue *= (100 + gameServer().random().Next(50));
                                            iValue /= 100;

                                            if (iValue > iBestValue)
                                            {
                                                eBestPlayer = pLoopPlayer.getPlayer();
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (eBestPlayer != PlayerType.NONE)
                {
                    AI_fundResources(getTotalBuyoutPrice(eBestPlayer), MIN_PRICE_MULTIPLIER, false, false, null, null, true, eBestPlayer);
                    buyout(eBestPlayer);
                    return;
                }
            }

            // Phase 3: Gradual Buyout Building (Accumulating shares towards full acquisition)
            if (isWinEligible())
            {
                const int MIN_PRICE_MULTIPLIER = 10;

                PlayerType eBestPlayer = PlayerType.NONE;
                long iBestValue = 0;

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAliveAll())
                {
                    if (pLoopPlayer.getTeam() != getTeam())
                    {
                        if (canBuyout(pLoopPlayer.getPlayer(), false))
                        {
                            if (!canOnlyBuyout(pLoopPlayer.getPlayer()))
                            {
                                // DEBT-AWARE: Verify post-acquisition solvency
                                if (isBuyoutDebtSafe(pLoopPlayer.getPlayer()))
                                {
                                    if (AI_fundResources(getTotalBuyoutPrice(pLoopPlayer.getPlayer()), MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                                    {
                                        // DEBT-AWARE: Net Buyout Value on unified stock market scale
                                        // Net Buyout Value = Total Market Equity - Total Enterprise Buyout Cost
                                        int totalMarketEquity = getMarketEquityValuePerShare(pLoopPlayer.getPlayer()) * 10;
                                        int enterpriseCost = getTotalEnterpriseBuyoutCost(pLoopPlayer.getPlayer());
                                        long iValue = totalMarketEquity - enterpriseCost;

                                        if (iValue > 0)
                                        {
                                            iValue *= (100 + gameServer().random().Next(50));
                                            iValue /= 100;

                                            if (iValue > iBestValue)
                                            {
                                                eBestPlayer = pLoopPlayer.getPlayer();
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (eBestPlayer != PlayerType.NONE)
                {
                    AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                    buyShares(eBestPlayer);
                    return;
                }
            }

            if (getClaims() > 0)
            {
                return;
            }

            // Defensive: Buy own shares if rivals have accumulated half or more of our own stock
            if (canBuyShares(getPlayer(), false))
            {
                if (getSharesBoughtRivals() >= (countOwnSharesOwned() / 2))
                {
                    if (AI_fundResources(getBuySharePrice(getPlayer()), 10, true, false, null, null, false, PlayerType.NONE))
                    {
                        buyShares(getPlayer());
                        return;
                    }
                }
            }

            // Phase 4: Indebted Rivals Evaluation
            if (isWinEligible())
            {
                const int MIN_PRICE_MULTIPLIER = 20;

                PlayerType eBestPlayer = PlayerType.NONE;
                bool bBestSaveUpgrade = false;
                int iBestValue = int.MaxValue;

                foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                {
                    if ((pLoopPlayer != this) && !(pLoopPlayer.isSubsidiary()))
                    {
                        if (canBuyShares(pLoopPlayer.getPlayer(), false))
                        {
                            int iBetterThreshold = 0;
                            int iWorseThreshold = 0;
                            int iExcessBond = 0;
                            BondType eBond = pLoopPlayer.calculateBondRating(pLoopPlayer.getDebt(), 0, ref iBetterThreshold, ref iWorseThreshold, ref iExcessBond, true);

                            // Target is in junk bond status (lowest rating)
                            if (eBond == (infos().bondsNum() - 1))
                            {
                                // DEBT-AWARE: Only pursue if absorbing capped debt will not bankrupt us
                                if (isBuyoutDebtSafe(pLoopPlayer.getPlayer()))
                                {
                                    bool bSaveUpgrade = (pLoopPlayer.countOwnSharesOwned() >= gameServer().getMajorityShares());

                                    if (AI_fundResources(getBuySharePrice(pLoopPlayer.getPlayer()), MIN_PRICE_MULTIPLIER, bSaveUpgrade, true, null, null, false, pLoopPlayer.getPlayer()))
                                    {
                                        // DEBT-AWARE: Evaluates perceived cost with capped debt liability included
                                        int iValue = getPerceivedShareCost(pLoopPlayer.getPlayer());

                                        iValue += gameServer().random().Next(100);

                                        if (iValue < iBestValue)
                                        {
                                            eBestPlayer = pLoopPlayer.getPlayer();
                                            bBestSaveUpgrade = bSaveUpgrade;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (eBestPlayer != PlayerType.NONE)
                {
                    AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, bBestSaveUpgrade, false, null, null, false, eBestPlayer);
                    buyShares(eBestPlayer);
                    return;
                }
            }

            if (getHQLevelInt() == 3)
            {
                return;
            }

            // HQ Level 3+ defensive buybacks and share locking
            if (canBuyShares(getPlayer(), false))
            {
                if ((getMoney() / 3) > getBuySharePrice(getPlayer()))
                {
                    buyShares(getPlayer());
                    return;
                }

                if (countOwnSharesOwned() < gameServer().getMajorityShares())
                {
                    if (AI_fundResources(getBuySharePrice(getPlayer()), 10, true, false, null, null, false, PlayerType.NONE))
                    {
                        buyShares(getPlayer());
                    }

                    return;
                }
                else
                {
                    if (getSharesBoughtRivals() > (countOwnSharesOwned() - gameServer().getMajorityShares()))
                    {
                        if (AI_fundResources(getBuySharePrice(getPlayer()), 10, true, false, null, null, false, PlayerType.NONE))
                        {
                            buyShares(getPlayer());
                            return;
                        }
                    }
                }

                {
                    int iSharesOwnedOfOthers = 0;

                    foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                    {
                        if (pLoopPlayer != this)
                        {
                            iSharesOwnedOfOthers += getSharesOwned(pLoopPlayer.getPlayer());
                        }
                    }

                    if (countOwnSharesOwned() <= iSharesOwnedOfOthers)
                    {
                        if (AI_fundResources(getBuySharePrice(getPlayer()), 10, true, false, null, null, false, PlayerType.NONE))
                        {
                            buyShares(getPlayer());
                            return;
                        }
                    }
                }
            }

            // Phase 5: Best Target Execution & General Stock Purchases
            if (isWinEligible())
            {
                PlayerType eBestTarget = AI_getBestBuyoutTarget(3, 50);

                if (eBestTarget != PlayerType.NONE && isBuyoutDebtSafe(eBestTarget))
                {
                    if (canOnlyBuyout(eBestTarget))
                    {
                        if (canBuyout(eBestTarget, true))
                        {
                            buyout(eBestTarget);
                            return;
                        }
                        else if (AI_fundResources(getTotalBuyoutPrice(eBestTarget), 1, false, false, null, null, true, eBestTarget))
                        {
                            buyout(eBestTarget);
                            return;
                        }
                    }
                    else
                    {
                        if (canBuyShares(eBestTarget, true))
                        {
                            buyShares(eBestTarget);
                            return;
                        }
                        else if (AI_fundResources(getBuySharePrice(eBestTarget), 10, true, false, null, null, false, PlayerType.NONE))
                        {
                            buyShares(eBestTarget);
                            return;
                        }
                    }
                }
                else
                {
                    // Fallback 1: Buy cheapest shares using perceived cost when AI has excess cash
                    {
                        PlayerType eBestPlayer = PlayerType.NONE;
                        int iBestValue = int.MaxValue;

                        foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                        {
                            if ((pLoopPlayer != this) && !(pLoopPlayer.isSubsidiary()))
                            {
                                if (canBuyShares(pLoopPlayer.getPlayer(), true))
                                {
                                    if ((getMoney() / 2) > getBuySharePrice(pLoopPlayer.getPlayer()))
                                    {
                                        int iValue = getPerceivedShareCost(pLoopPlayer.getPlayer());

                                        iValue += gameServer().random().Next(100);

                                        if (iValue < iBestValue)
                                        {
                                            eBestPlayer = pLoopPlayer.getPlayer();
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }

                        if (eBestPlayer != PlayerType.NONE)
                        {
                            buyShares(eBestPlayer);
                            return;
                        }
                    }

                    // Fallback 2: Opportunistic share acquisition via resource liquidation (Multiplier 40)
                    {
                        const int MIN_PRICE_MULTIPLIER = 40;

                        PlayerType eBestPlayer = PlayerType.NONE;
                        int iBestValue = int.MaxValue;

                        foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                        {
                            if ((pLoopPlayer != this) && !(pLoopPlayer.isSubsidiary()))
                            {
                                if (canBuyShares(pLoopPlayer.getPlayer(), false))
                                {
                                    if (AI_fundResources(getBuySharePrice(pLoopPlayer.getPlayer()), MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                                    {
                                        int iValue = getPerceivedShareCost(pLoopPlayer.getPlayer());

                                        iValue += gameServer().random().Next(100);

                                        if (iValue < iBestValue)
                                        {
                                            eBestPlayer = pLoopPlayer.getPlayer();
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }

                        if (eBestPlayer != PlayerType.NONE)
                        {
                            AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                            buyShares(eBestPlayer);
                            return;
                        }
                    }

                    // Fallback 3: Subsidiary dividend share investment (Multiplier 60)
                    {
                        const int MIN_PRICE_MULTIPLIER = 60;

                        PlayerType eBestPlayer = PlayerType.NONE;
                        int iBestValue = 0;

                        foreach (PlayerServer pLoopPlayer in gameServer().getPlayerServerAll())
                        {
                            if ((pLoopPlayer != this) && pLoopPlayer.isSubsidiary())
                            {
                                if (canBuyShares(pLoopPlayer.getPlayer(), false))
                                {
                                    int iPrice = getBuySharePrice(pLoopPlayer.getPlayer());
                                    int iDividend = pLoopPlayer.getDividend(pLoopPlayer.getMoney(), 1);

                                    if (iPrice < (iDividend * 200))
                                    {
                                        if (AI_fundResources(iPrice, MIN_PRICE_MULTIPLIER, true, true, null, null, false, PlayerType.NONE))
                                        {
                                            int iValue = iDividend;

                                            iValue += gameServer().random().Next(10);

                                            if (iValue > iBestValue)
                                            {
                                                eBestPlayer = pLoopPlayer.getPlayer();
                                                iBestValue = iValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (eBestPlayer != PlayerType.NONE)
                        {
                            AI_fundResources(getBuySharePrice(eBestPlayer), MIN_PRICE_MULTIPLIER, true, false, null, null, false, PlayerType.NONE);
                            buyShares(eBestPlayer);
                            return;
                        }
                    }
                }
            }
        }

        #endregion
    }
}


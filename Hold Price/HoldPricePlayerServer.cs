using System;
using Offworld.AppCore;
using Offworld.GameCore;

namespace HoldPrice
{
    /// <summary>
    /// Custom PlayerServer extending the vanilla game player logic to support
    /// market price holding (pegging), manual override, and automatic release.
    /// </summary>
    public class HoldPricePlayerServer : PlayerServer
    {
        protected bool[] mabPriceHeld;
        protected int[] maiTargetHeldPrice;

        public HoldPricePlayerServer(GameClient pGame) : base(pGame)
        {
            int resourceCount = (int)infos().resourcesNum();
            mabPriceHeld = new bool[resourceCount];
            maiTargetHeldPrice = new int[resourceCount];
        }

        public virtual bool isPriceHeld(ResourceType eResource)
        {
            int idx = (int)eResource;
            if (idx < 0 || idx >= mabPriceHeld.Length)
                return false;
            return mabPriceHeld[idx];
        }

        public virtual int getTargetHeldPrice(ResourceType eResource)
        {
            int idx = (int)eResource;
            if (idx < 0 || idx >= maiTargetHeldPrice.Length)
                return 0;
            return maiTargetHeldPrice[idx];
        }

        public virtual void setHoldPrice(ResourceType eResource, bool bHold, int iTargetPrice)
        {
            int idx = (int)eResource;
            if (idx < 0 || idx >= mabPriceHeld.Length)
                return;

            mabPriceHeld[idx] = bHold;
            maiTargetHeldPrice[idx] = iTargetPrice;
        }

        public virtual void toggleHoldPrice(ResourceType eResource)
        {
            int idx = (int)eResource;
            if (idx < 0 || idx >= mabPriceHeld.Length)
                return;

            if (mabPriceHeld[idx])
            {
                mabPriceHeld[idx] = false;
            }
            else
            {
                int currentPrice = gameServer().marketServer().getWholePrice(eResource);
                mabPriceHeld[idx] = true;
                maiTargetHeldPrice[idx] = currentPrice;
            }
        }

        public override int trade(ResourceType eResource, int iQuantity, bool bManual)
        {
            int result = base.trade(eResource, iQuantity, bManual);

            // Manual Override: Whenever the player manually trades a pegged commodity,
            // the target price is immediately overridden by the resulting market price.
            if (bManual && isPriceHeld(eResource))
            {
                int newMarketPrice = gameServer().marketServer().getWholePrice(eResource);
                int idx = (int)eResource;
                if (idx >= 0 && idx < maiTargetHeldPrice.Length)
                {
                    maiTargetHeldPrice[idx] = newMarketPrice;
                }
            }

            return result;
        }

        public override void doTurn()
        {
            base.doTurn();
            executeHoldPriceMaintenance();
        }

        /// <summary>
        /// Automated Market Maintenance & Auto-Release Loop:
        /// Buys or sells the commodity to maintain target price. Automatically disengages
        /// if cash is exhausted (when buying) or inventory is exhausted (when selling).
        /// </summary>
        protected virtual void executeHoldPriceMaintenance()
        {
            if (isSubsidiary() || isConcede() || isDropped())
            {
                return;
            }

            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
            {
                int idx = (int)eLoopResource;
                if (!mabPriceHeld[idx])
                {
                    continue;
                }

                int currentPrice = gameServer().marketServer().getWholePrice(eLoopResource);
                int targetPrice = maiTargetHeldPrice[idx];

                if (currentPrice == targetPrice)
                {
                    continue;
                }

                if (currentPrice < targetPrice)
                {
                    // Price is too low: buy units to lift price up
                    int minCost = gameServer().marketServer().calculateBuyCost(eLoopResource, 1);
                    if (getMoney() < minCost)
                    {
                        mabPriceHeld[idx] = false;
                        continue;
                    }

                    int[] batchSizes = { 10, 5, 2, 1 };
                    int safetyLoop = 0;
                    while (currentPrice < targetPrice && safetyLoop++ < 20)
                    {
                        int tradeAmount = 0;
                        for (int b = 0; b < batchSizes.Length; b++)
                        {
                            int batch = batchSizes[b];
                            int cost = gameServer().marketServer().calculateBuyCost(eLoopResource, batch);
                            if (getMoney() >= cost)
                            {
                                tradeAmount = batch;
                                break;
                            }
                        }

                        if (tradeAmount == 0)
                        {
                            break;
                        }

                        trade(eLoopResource, tradeAmount, false);
                        currentPrice = gameServer().marketServer().getWholePrice(eLoopResource);
                    }

                    if (currentPrice < targetPrice && getMoney() < gameServer().marketServer().calculateBuyCost(eLoopResource, 1))
                    {
                        mabPriceHeld[idx] = false;
                    }
                }
                else if (currentPrice > targetPrice)
                {
                    // Price is too high: sell units to push price down
                    int availableStockpile = getWholeResourceStockpile(eLoopResource, false);
                    if (availableStockpile <= 0)
                    {
                        mabPriceHeld[idx] = false;
                        continue;
                    }

                    int safetyLoop = 0;
                    while (currentPrice > targetPrice && safetyLoop++ < 20)
                    {
                        availableStockpile = getWholeResourceStockpile(eLoopResource, false);
                        if (availableStockpile <= 0)
                        {
                            break;
                        }

                        int tradeAmount = Math.Min(10, availableStockpile);
                        trade(eLoopResource, -tradeAmount, false);
                        currentPrice = gameServer().marketServer().getWholePrice(eLoopResource);
                    }

                    if (currentPrice > targetPrice && getWholeResourceStockpile(eLoopResource, false) <= 0)
                    {
                        mabPriceHeld[idx] = false;
                    }
                }
            }
        }
    }
}


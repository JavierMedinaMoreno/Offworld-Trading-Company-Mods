using Offworld.GameCore;

namespace JaviervsMegamod
{
    /// <summary>
    /// Object factory for Javiervs' Megamod.
    /// Overrides createPlayerServer to instantiate the merged BuyoutAIPlayerServer
    /// (which inherits from HoldPricePlayerServer).
    /// </summary>
    public class MegamodFactory : GameFactory
    {
        public override PlayerServer createPlayerServer(GameClient pGame)
        {
            return new BuyoutAIPlayerServer(pGame);
        }
    }
}

using Offworld.GameCore;

namespace HoldPrice
{
    public class HoldPriceFactory : GameFactory
    {
        public override PlayerServer createPlayerServer(GameClient pGame)
        {
            return new HoldPricePlayerServer(pGame);
        }
    }
}

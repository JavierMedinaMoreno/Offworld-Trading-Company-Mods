using Offworld.GameCore;

namespace BuyoutDebtAI
{
    /// <summary>
    /// Custom factory extending Offworld Trading Company's core GameFactory.
    /// OTC relies on the Abstract Factory pattern for instantiating server and client objects,
    /// enabling mods to inject custom subclasses without binary patching.
    /// </summary>
    public class BuyoutAIFactory : GameFactory
    {
        /// <summary>
        /// Creates a server-side player instance for every human and AI participant in the match.
        /// Returning BuyoutAIPlayerServer enables both the debt-distribution buyout mechanic
        /// and debt-aware AI stock evaluation for all AI players.
        /// </summary>
        /// <param name="pGame">The active GameClient instance.</param>
        /// <returns>A new instance of BuyoutAIPlayerServer.</returns>
        public override PlayerServer createPlayerServer(GameClient pGame)
        {
            return new BuyoutAIPlayerServer(pGame);
        }
    }
}

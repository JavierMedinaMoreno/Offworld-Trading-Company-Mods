using System;
using System.Reflection;
using Harmony;
using Offworld.AppCore;
using Offworld.GameCore;

namespace BuyoutDebtAI
{
    /// <summary>
    /// Lifecycle entry point for the Buyout Debt AI mod.
    /// Offworld Trading Company discovers and instantiates classes implementing IModEntryPoint
    /// when the mod assembly is loaded at game startup.
    /// </summary>
    public class ModEntryPoint : ModEntryPointAdapter
    {
        private HarmonyInstance harmony;

        /// <summary>
        /// Called immediately after the mod DLL is loaded.
        /// Replaces the game's default object factory with BuyoutAIFactory and initializes Harmony patches.
        /// </summary>
        public override void Initialize()
        {
            Globals.SetFactory(new BuyoutAIFactory());

            try
            {
                harmony = HarmonyInstance.Create("offworld.mod.buyoutdebtai");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[BuyoutDebtAI] Harmony patch failed: " + ex);
            }
        }

        /// <summary>
        /// Called when the mod DLL is unloaded (e.g., game shutdown or mod disable).
        /// Restores the engine's original GameFactory.
        /// </summary>
        public override void Shutdown()
        {
            Globals.SetFactory(new GameFactory());
        }
    }
}

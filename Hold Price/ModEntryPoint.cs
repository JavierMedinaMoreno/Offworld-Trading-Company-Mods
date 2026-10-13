using System;
using System.Reflection;
using Harmony;
using Offworld.AppCore;
using Offworld.GameCore;

namespace HoldPrice
{
    public class ModEntryPoint : ModEntryPointAdapter
    {
        private HarmonyInstance harmony;

        public override void Initialize()
        {
            Globals.SetFactory(new HoldPriceFactory());

            try
            {
                harmony = HarmonyInstance.Create("offworld.mod.holdprice");
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    try
                    {
                        var methods = type.GetHarmonyMethods();
                        if (methods != null && methods.Count > 0)
                        {
                            var processor = new PatchProcessor(harmony, type, HarmonyMethod.Merge(methods));
                            processor.Patch();
                        }
                    }
                    catch (Exception patchEx)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[HoldPrice] Harmony initialization failed: " + ex);
            }
        }

        public override void Shutdown()
        {
            Globals.SetFactory(new GameFactory());
        }

        public override void Update() => HoldPriceUIPatches.UpdateVisuals();
    }
}


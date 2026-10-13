using System;
using System.Reflection;
using Harmony;
using Offworld.AppCore;
using Offworld.GameCore;

namespace JaviervsMegamod
{
    public class ModEntryPoint : ModEntryPointAdapter
    {
        private HarmonyInstance harmony;
        private readonly UIPlus.MyClass uiplus = new UIPlus.MyClass();

        public override void Initialize()
        {
            Globals.SetFactory(new MegamodFactory());

            try
            {
                harmony = HarmonyInstance.Create("offworld.mod.javiervsmegamod");
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
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[JaviervsMegamod] Harmony initialization failed: " + ex);
            }

            try
            {
                uiplus.Initialize();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[JaviervsMegamod] UIPlus Initialize failed: " + ex);
            }

            try
            {
                MegamodRulesManager.ApplyDataRules();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[JaviervsMegamod] MegamodRulesManager ApplyDataRules failed: " + ex);
            }
        }

        public override void Shutdown()
        {
            Globals.SetFactory(new GameFactory());

            try
            {
                uiplus.Shutdown();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[JaviervsMegamod] UIPlus Shutdown failed: " + ex);
            }
        }

        public override void OnGameReady()
        {
            MegamodRulesManager.ApplyDataRules();
            uiplus.OnGameReady();
        }

        public override void OnUIReady() => uiplus.OnUIReady();
        public override void OnExitGame() => uiplus.OnExitGame();

        public override void OnGameClientReady()
        {
            MegamodRulesManager.ApplyDataRules();
            uiplus.OnGameClientReady();
        }
        public override void Update()
        {
            HoldPriceUIPatches.UpdateVisuals();
            uiplus.Update();
        }
    }
}


using System;
using System.Collections.Generic;
using Offworld.GameCore;

namespace JaviervsMegamod
{
    /// <summary>
    /// Dynamically applies and reverts game data rules at runtime based on user options.
    /// Manages the three XML-based submods: No Underground Nukes, Interdimensional No Resource Depletion,
    /// and Interdimensional Colony Classes without requiring file modifications.
    /// </summary>
    public static class MegamodRulesManager
    {
        private static readonly HashSet<string> DimensionColonyClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "COLONYCLASS_RESIDENTIAL",
            "COLONYCLASS_AUTOMATED",
            "COLONYCLASS_PENAL",
            "COLONYCLASS_RESEARCH",
            "COLONYCLASS_TRANSIT_HUB",
            "COLONYCLASS_SECURITY_CENTER",
            "COLONYCLASS_TOURISM",
            "COLONYCLASS_INDUSTRIAL",
            "COLONYCLASS_AGRICULTURAL",
            "COLONYCLASS_ENTERTAINMENT",
            "COLONYCLASS_FINANCIAL",
            "COLONYCLASS_UNIVERSITY",
            "COLONYCLASS_LEISURE",
            "COLONYCLASS_RELIGIOUS",
            "COLONYCLASS_GOVERNMENT",
            "COLONYCLASS_MEDICAL",
            "COLONYCLASS_MINING",
            "COLONYCLASS_WASTE_COLLECTION"
        };

        public static void ApplyDataRules()
        {
            try
            {
                if (Globals.Infos == null)
                    return;

                var options = UIPlus.ModOptionsSave.Options;
                if (options == null)
                    return;

                ApplyNoUndergroundNukes(options.enableNoUndergroundNukes);
                ApplyNoResourceDepletion(options.enableInterdimensionalNoResourceDepletion);
                ApplyInterdimensionalColonyClasses(options.enableInterdimensionalColonyClasses);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[JaviervsMegamod] Failed to apply data rules: " + ex);
            }
        }

        private static void ApplyNoUndergroundNukes(bool enabled)
        {
            for (BlackMarketType i = 0; i < Globals.Infos.blackMarketsNum(); i++)
            {
                InfoBlackMarket info = Globals.Infos.blackMarket(i);
                if (info != null && info.mzType == "BLACKMARKET_UNDERGROUND_NUKE")
                {
                    if (enabled)
                    {
                        info.miAppearanceProb = 0;
                        info.mbAuction = true;
                        if (info.maiLocationAppearanceModifiers != null)
                        {
                            for (int loc = 0; loc < info.maiLocationAppearanceModifiers.Count; loc++)
                            {
                                info.maiLocationAppearanceModifiers[loc] = -99999;
                            }
                        }
                    }
                    else
                    {
                        info.miAppearanceProb = 100;
                        info.mbAuction = true;
                        if (info.maiLocationAppearanceModifiers != null)
                        {
                            for (int loc = 0; loc < info.maiLocationAppearanceModifiers.Count; loc++)
                            {
                                info.maiLocationAppearanceModifiers[loc] = 0;
                            }
                        }
                    }
                    break;
                }
            }
        }

        private static void ApplyNoResourceDepletion(bool enabled)
        {
            for (LocationType i = 0; i < LocationType.NUM_TYPES; i++)
            {
                InfoLocation info = Globals.Infos.location(i);
                if (info != null && info.mzType == "LOCATION_DIMENSION")
                {
                    info.miDiminishOverrideProbability = enabled ? 0 : 20;
                    break;
                }
            }
        }

        private static void ApplyInterdimensionalColonyClasses(bool enabled)
        {
            LocationType dimensionLoc = LocationType.NONE;
            for (LocationType loc = 0; loc < LocationType.NUM_TYPES; loc++)
            {
                InfoLocation info = Globals.Infos.location(loc);
                if (info != null && info.mzType == "LOCATION_DIMENSION")
                {
                    dimensionLoc = loc;
                    break;
                }
            }

            if (dimensionLoc == LocationType.NONE)
                return;

            for (ColonyClassType i = 0; i < Globals.Infos.colonyClassesNum(); i++)
            {
                InfoColonyClass info = Globals.Infos.colonyClass(i);
                if (info != null && DimensionColonyClasses.Contains(info.mzType))
                {
                    if (info.mabLocationInvalid != null && (int)dimensionLoc < info.mabLocationInvalid.Count)
                    {
                        info.mabLocationInvalid[(int)dimensionLoc] = !enabled;
                    }
                }
            }
        }
    }
}

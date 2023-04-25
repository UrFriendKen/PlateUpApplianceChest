using HarmonyLib;
using Kitchen;
using KitchenData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KitchenApplianceChest.Patches
{
    [HarmonyPatch]
    static class ShowPingedApplianceInfo_Patch
    {
        //[HarmonyPatch(typeof(ShowPingedApplianceInfo), "Perform")]
        //[HarmonyPrefix]
        //static bool Perform_Prefix(ref InteractionData data, ref int ___ApplianceID)
        //{
        //    if (___ApplianceID == Main.ChestApplianceID && Main.TryGetStoredAppliances(data.Target, out IEnumerable<Appliance> appliances))
        //    {
        //        ApplianceInfoView_Patch.ApplianceStorageString = String.Empty;

        //        // Custom ApplianceInfoView?

        //        return false;
        //    }
        //    return true;
        //}

        [HarmonyPatch(typeof(ShowPingedApplianceInfo), "Perform")]
        [HarmonyPrefix]
        static bool Perform_Prefix(ref InteractionData data, ref int ___ApplianceID)
        {
            ApplianceInfoView_Patch.StoredAppliancesString = String.Empty;
            if (___ApplianceID == Main.ChestApplianceID && Main.TryGetStoredAppliances(data.Target, out IEnumerable<Appliance> storedAppliances) && storedAppliances.Count() > 0)
            {
                List<string> applianceNames = new List<string>();
                foreach (Appliance appliance in storedAppliances)
                {
                    applianceNames.Add(appliance.Name);
                }
                ApplianceInfoView_Patch.StoredAppliancesString = String.Join(", ", applianceNames);
            }
            return true;
        }
    }
}

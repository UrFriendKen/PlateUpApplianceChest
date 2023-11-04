using HarmonyLib;
using Kitchen;
using KitchenApplianceChest.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KitchenApplianceChest.Patches
{
    [HarmonyPatch]
    static class LocalViewRouter_Patch
    {
        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPostfix]
        static void GetPrefab_Postfix(ViewType view_type, ref GameObject __result)
        {
            if (view_type == ViewType.ApplianceInfo && __result != null && __result.GetComponent<StoredAppliancesInfoView>() == null)
            {
                __result.AddComponent<StoredAppliancesInfoView>();
            }
        }
    }
}

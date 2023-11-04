using HarmonyLib;
using Kitchen;
using KitchenApplianceChest.Customs;
using KitchenMods;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace KitchenApplianceChest.Patches
{
    public struct CApplianceStorageInfo : IComponentData, IModComponent
    {
        public int Capacity;

        public FixedListInt64 ApplianceIDs;

        public List<int> GetApplianceIDs()
        {
            List<int> result = new List<int>();
            for (int i = 0; i < Capacity; i++)
            {
                result.Add(i < ApplianceIDs.Length ? ApplianceIDs[i] : 0);
            }
            return result;
        }
    }

    [HarmonyPatch]
    static class ManageApplianceIndicators_Patch
    {
        [HarmonyPatch(typeof(ManageApplianceIndicators), "CreateIndicator")]
        [HarmonyPostfix]
        static void CreateIndicator_Postfix(ref ManageApplianceIndicators __instance, ref Entity __result, Entity source)
        {
            if (__result == default || !(__instance?.EntityManager.HasComponent<CApplianceStorage>(source) ?? false))
                return;

            CApplianceStorage applianceStorage = __instance.EntityManager.GetComponentData<CApplianceStorage>(source);
            FixedListInt64 ids = new FixedListInt64();
            foreach (int id in applianceStorage.GetApplianceIDs())
            {
                if (id == 0)
                    continue;
                ids.Add(id);
            }
            __instance.EntityManager.AddComponentData(__result, new CApplianceStorageInfo()
            {
                Capacity = applianceStorage.Capacity,
                ApplianceIDs = ids
            });
        }
    }
}

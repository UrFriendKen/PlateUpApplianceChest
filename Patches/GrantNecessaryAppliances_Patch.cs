using HarmonyLib;
using Kitchen;
using KitchenData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KitchenApplianceChest.Patches
{
    [HarmonyPatch]
    static class GrantNecessaryAppliances_Patch
    {
        [HarmonyPatch(typeof(GrantNecessaryAppliances), "TotalPlates")]
        [HarmonyPostfix]
        static void TotalPlates_Postfix(ref int __result)
        {
            __result += GrantNecessaryAppliancesController.StoredPlates;
        }


        [HarmonyPatch(typeof(GrantNecessaryAppliances), "MaxTableSize")]
        [HarmonyPostfix]
        static void MaxTableSize_Prefix(ref int __result, ref Dictionary<int, int> ___TablesOfType)
        {
            int num = 0;

            foreach (KeyValuePair<int, int> storedTable in GrantNecessaryAppliancesController.TablesDict)
            {
                if (!___TablesOfType.ContainsKey(storedTable.Key))
                {
                    ___TablesOfType.Add(storedTable.Key, 0);
                }
                ___TablesOfType[storedTable.Key] += storedTable.Value;
            }

            foreach (KeyValuePair<int, int> item in ___TablesOfType)
            {
                if (item.Value > 0 && GameData.Main.TryGet<Appliance>(item.Key, out var output) && output.GetProperty<CApplianceTable>(out var result) && !result.IsWaitingTable)
                {
                    int maxSeats = GetMaxSeats(result.MaxSeats, item.Value, !result.IsIndividualTable);
                    num = Mathf.Max(maxSeats, num);
                }
            }
            __result = num;
        }

        private static int GetMaxSeats(int per_table, int table_count, bool can_combine)
        {
            if (!can_combine || table_count == 1)
            {
                return per_table;
            }
            if (table_count == 2)
            {
                return Mathf.Clamp(per_table, 0, 3) * 2;
            }
            return Mathf.Clamp(per_table, 0, 3) * 2 + Mathf.Clamp(per_table, 0, 2) * (table_count - 2);
        }
    }
}

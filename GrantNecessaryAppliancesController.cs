using Kitchen;
using KitchenApplianceChest.Customs;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace KitchenApplianceChest
{
    [UpdateBefore(typeof(GrantNecessaryAppliances))]
    internal class GrantNecessaryAppliancesController : NightSystem
    {
        public static int StoredPlates = 0;

        public static Dictionary<int, int> TablesDict = new Dictionary<int, int>();

        EntityQuery _storedPlates;
        EntityQuery _storedTables;

        protected override void Initialise()
        {
            base.Initialise();
            _storedPlates = GetEntityQuery(typeof(CStoredPlates));
            _storedTables = GetEntityQuery(typeof(CStoredTables));
        }

        protected override void OnUpdate()
        {
            TablesDict.Clear();
            NativeArray<CStoredTables> tables = _storedTables.ToComponentDataArray<CStoredTables>(Allocator.Temp);
            foreach (var table in tables)
            {
                foreach(KeyValuePair<int, int> tableData in table.GetDictionary())
                {
                    if (!TablesDict.ContainsKey(tableData.Key))
                    {
                        TablesDict.Add(tableData.Key, 0);
                    }
                    TablesDict[tableData.Key] += tableData.Value;
                }
            }


            StoredPlates = 0;
            NativeArray<CStoredPlates> providers = _storedPlates.ToComponentDataArray<CStoredPlates>(Allocator.Temp);
            foreach (var provider in providers)
            {
                StoredPlates += provider.PlatesCount;
            }

            tables.Dispose();
            providers.Dispose();
        }
    }
}

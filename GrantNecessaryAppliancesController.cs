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
        public static int StoredPlates => GetStoredPlatesCount();
        public static Dictionary<int, int> TablesDict => GetStoredTablesDict();

        private static int _storedPlatesCount = 0;
        private static Dictionary<int, int> _tablesDict = new Dictionary<int, int>();

        static EntityQuery _storedPlates;
        static EntityQuery _storedTables;

        protected override void Initialise()
        {
            base.Initialise();
            _storedPlates = GetEntityQuery(typeof(CStoredPlates));
            _storedTables = GetEntityQuery(typeof(CStoredTables));
        }

        protected static int GetStoredPlatesCount()
        {
            _storedPlatesCount = 0;
            using NativeArray<CStoredPlates> providers = _storedPlates.ToComponentDataArray<CStoredPlates>(Allocator.Temp);
            foreach (var provider in providers)
            {
                _storedPlatesCount += provider.PlatesCount;
            }
            return _storedPlatesCount;
        }

        protected static Dictionary<int, int> GetStoredTablesDict()
        {
            _tablesDict.Clear();
            using NativeArray<CStoredTables> tables = _storedTables.ToComponentDataArray<CStoredTables>(Allocator.Temp);
            foreach (var table in tables)
            {
                foreach (KeyValuePair<int, int> tableData in table.GetDictionary())
                {
                    if (!_tablesDict.ContainsKey(tableData.Key))
                    {
                        _tablesDict.Add(tableData.Key, 0);
                    }
                    _tablesDict[tableData.Key] += tableData.Value;
                }
            }
            return _tablesDict;
        }

        protected override void OnUpdate()
        {
        }
    }
}

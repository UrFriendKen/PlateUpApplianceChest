using KitchenApplianceChest.Views;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenApplianceChest.Customs
{
    public struct CStoredPlates : IApplianceProperty, IAttachableProperty, IComponentData
    {
        public int PlatesCount;
    }
    public struct CStoredTables : IApplianceProperty, IAttachableProperty, IComponentData
    {
        FixedListInt64 TableIDs;
        FixedListInt64 TableCounts;

        public CStoredTables()
        {
            TableIDs = new FixedListInt64();
            TableCounts = new FixedListInt64();
        }

        public int Add(int TableID, int count = 1)
        {
            if (!TableIDs.Contains(TableID))
            {
                TableIDs.Add(TableID);
                TableCounts.Add(0);
            }
            return TableCounts[TableIDs.IndexOf(TableID)] += count;
        }

        public int Remove(int TableID, int count = 1)
        {
            if (!TableIDs.Contains(TableID))
            {
                return 0;
            }
            int index = TableIDs.IndexOf(TableID);
            int remaining = TableCounts[index] -= count;
            if (remaining == 0)
            {
                TableIDs.RemoveAt(index);
                TableCounts.RemoveAt(index);
            }
            return remaining;
        }

        public bool RemoveTable(int TableID)
        {
            if (!TableIDs.Contains(TableID))
            {
                return false;
            }
            int index = TableIDs.IndexOf(TableID);
            TableIDs.RemoveAt(index);
            TableCounts.RemoveAt(index);
            return true;
        }

        public void Clear()
        {
            TableIDs.Clear();
            TableCounts.Clear();
        }

        public bool TryGet(int TableID, out int count)
        {
            return GetDictionary().TryGetValue(TableID, out count);
        }

        public Dictionary<int, int> GetDictionary()
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = 0; i < TableIDs.Length; i++)
            {
                if (!dict.ContainsKey(TableIDs[i]))
                {
                    dict.Add(TableIDs[i], 0);
                }
                dict[TableIDs[i]] += TableCounts[i];
            }
            return dict;
        }
    }

    public struct CApplianceStorage : IApplianceProperty, IAttachableProperty, IComponentData
    {
        public int Capacity { get; set; } = 1;
        public bool IgnoreCapacity { get; set; } = false;
        FixedListInt64 applianceIDs;
        public int ProvidedAppliance => StoredAppliancesCount > 0 ? applianceIDs[StoredAppliancesCount - 1] : 0;
        public int StoredAppliancesCount => applianceIDs.Length;
        public bool IsFull => StoredAppliancesCount >= Capacity;
        public bool IsEmpty => StoredAppliancesCount == 0;

        public CApplianceStorage() { }

        public bool Store(int applianceID, bool ignoreCapacity = false)
        {
            bool result = false;
            if (IgnoreCapacity || !IsFull)
            {
                applianceIDs.Add(applianceID);
                result = true;
            }
            return result;
        }

        public bool Retrieve(out int applianceID)
        {
            applianceID = ProvidedAppliance;
            if (!IsEmpty)
                applianceIDs.RemoveAt(StoredAppliancesCount - 1);
            return IsEmpty;
        }

        public bool Peek(out int applianceID)
        {
            applianceID = ProvidedAppliance;
            return IsEmpty;
        }

        public List<int> GetApplianceIDs()
        {
            List<int> result = new List<int>();
            for (int i = 0; i < Capacity; i++)
            {
                result.Add(i < applianceIDs.Length ? applianceIDs[i] : 0);
            }
            return result;
        }
    }

    public class Chest : CustomAppliance
    {
        public override int BaseGameDataObjectID => ApplianceReferences.Countertop;
        public override string UniqueNameID => "applianceChest";
        public override GameObject Prefab => Main.Bundle.LoadAsset<GameObject>("Chest");
        public override List<IApplianceProperty> Properties => new List<IApplianceProperty>()
        {
            new CStoredPlates()
            {
                PlatesCount = 0
            },

            new CStoredTables(),

            new CApplianceStorage()
            {
                Capacity = 8
            }
        };
        public override bool IsNonInteractive => false;
        public override OccupancyLayer Layer => OccupancyLayer.Default;
        public override bool IsPurchasable => true;
        public override bool IsPurchasableAsUpgrade => false;
        public override DecorationType ThemeRequired => DecorationType.Null;
        public override ShoppingTags ShoppingTags => ShoppingTags.Office | ShoppingTags.Technology;
        public override RarityTier RarityTier => RarityTier.Common;
        public override PriceTier PriceTier => PriceTier.Cheap;
        public override bool StapleWhenMissing => false;
        public override bool SellOnlyAsDuplicate => false;
        public override bool PreventSale => false;
        public override bool IsNonCrated => false;


        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (Locale.English, new ApplianceInfo()
            {
                Name = "Appliance Chest",
                Description = "Horde everything! You never know when you might need it...or suffocate under it.",
                Sections = new List<Appliance.Section>()
                {
                    new Appliance.Section()
                    {
                        Title = "Organisation",
                        Description = "Store up to 8 appliances inside"
                    },


                    new Appliance.Section()
                    {
                        Title = "Janitor",
                        Description = "Clears items stored in container appliances"
                    }
                },
            })
        };

        public override List<Appliance> Upgrades => new List<Appliance>()
        {
            GDOUtils.GetCastedGDO<Appliance, LockedChest>()
        };

        bool isRegistered = false;



        public override void OnRegister(GameDataObject gameDataObject)
        {
            base.OnRegister(gameDataObject);

            if (!isRegistered)
            {
                ApplyMaterials();
                ApplyComponents();
                isRegistered = true;
            }

        }

        private void ApplyMaterials()
        {
            var materials = new Material[1];

            materials[0] = MaterialUtils.GetExistingMaterial("Wood - Barrel");
            MaterialUtils.ApplyMaterial(Prefab, "Offset/Bottom/Base", materials);
            MaterialUtils.ApplyMaterial(Prefab, "Offset/Cover/Top", materials);

            materials[0] = MaterialUtils.GetExistingMaterial("Plastic - Shiny Gold");
            MaterialUtils.ApplyMaterial(Prefab, "Offset/Cover/Lock", materials);

            materials[0] = MaterialUtils.GetExistingMaterial("Metal Dark");
            MaterialUtils.ApplyMaterial(Prefab, "Offset/Bottom/Rim3", materials);
            MaterialUtils.ApplyMaterial(Prefab, "Offset/Cover/Rim", materials);
            MaterialUtils.ApplyMaterial(Prefab, "Offset/Cover/Rim2", materials);
        }

        private void ApplyComponents()
        {
            ApplianceStorageView view = Prefab.AddComponent<ApplianceStorageView>();
            Transform child = Prefab.transform.Find("Offset");
            view.HoldPoint0 = child.Find("HoldPoint (1)").gameObject;
            view.HoldPoint1 = child.Find("HoldPoint (2)").gameObject;
            view.HoldPoint2 = child.Find("HoldPoint (3)").gameObject;
            view.HoldPoint3 = child.Find("HoldPoint (4)").gameObject;
            view.HoldPoint4 = child.Find("HoldPoint (5)").gameObject;
            view.HoldPoint5 = child.Find("HoldPoint (6)").gameObject;
            view.HoldPoint6 = child.Find("HoldPoint (7)").gameObject;
            view.HoldPoint7 = child.Find("HoldPoint (8)").gameObject;
        }
    }
}

using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace KitchenApplianceChest.Customs
{
    public struct CPopulateApplianceStorage : IApplianceProperty, IAttachableProperty, IComponentData
    {
        public int Minimum;
        public int Maximum;
    }

    public class LockedChest : CustomAppliance
    {
        public override int BaseGameDataObjectID => ApplianceReferences.Countertop;
        public override string UniqueNameID => "applianceChestLocked";
        public override GameObject Prefab => Main.Bundle.LoadAsset<GameObject>("ClosedChest");
        public override List<IApplianceProperty> Properties => new List<IApplianceProperty>()
        {
            new CPopulateApplianceStorage()
            {
                Minimum = 5,
                Maximum = 8
            },

            new CTakesDuration()
            {
                Total = 10f,
                Manual = true,
                ManualNeedsEmptyHands = false,
                IsInverse = false,
                Mode = InteractionMode.Items,
                PreserveProgress = false,
                IsLocked = true
            },

            new CDisplayDuration()
            {
                IsBad = false,
                Process = ProcessReferences.Knead,
                ShowWhenEmpty = false
            },

            new CLockDurationTimeOfDay()
            {
                LockDuringDay = false,
                LockDuringNight = true
            },

            new CIsLockedApplianceStorage()
            {
                UnlockedReplacementApplianceID = GDOUtils.GetCustomGameDataObject<Chest>().ID,
                ViewUpdateDelay = 0.5f
            },

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
        public override bool IsPurchasable => false;
        public override bool IsPurchasableAsUpgrade => false;
        public override DecorationType ThemeRequired => DecorationType.Null;
        public override ShoppingTags ShoppingTags => ShoppingTags.Office | ShoppingTags.Technology;
        public override RarityTier RarityTier => RarityTier.Common;
        public override PriceTier PriceTier => PriceTier.ExtremelyExpensive;
        public override bool StapleWhenMissing => false;
        public override bool SellOnlyAsDuplicate => false;
        public override bool PreventSale => false;
        public override bool IsNonCrated => false;


        public override List<(Locale, ApplianceInfo)> InfoList => new List<(Locale, ApplianceInfo)>
        {
            (Locale.English, new ApplianceInfo()
            {
                Name = "Locked Chest",
                Description = "Curiosity calls, treasures await!",
                Sections = new List<Appliance.Section>()
                {
                    new Appliance.Section()
                    {
                        Title = "Mystery",
                        Description = "Unlock to find up to 8 appliances inside"
                    }
                },
            })
        };

        public override List<Appliance> Upgrades => new List<Appliance>()
        {
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
            //ApplianceStorageView view = Prefab.AddComponent<ApplianceStorageView>();
            //Transform child = Prefab.transform.Find("Offset");
            //view.HoldPoint0 = child.Find("HoldPoint (1)").gameObject;
            //view.HoldPoint1 = child.Find("HoldPoint (2)").gameObject;
            //view.HoldPoint2 = child.Find("HoldPoint (3)").gameObject;
            //view.HoldPoint3 = child.Find("HoldPoint (4)").gameObject;
            //view.HoldPoint4 = child.Find("HoldPoint (5)").gameObject;
            //view.HoldPoint5 = child.Find("HoldPoint (6)").gameObject;
            //view.HoldPoint6 = child.Find("HoldPoint (7)").gameObject;
            //view.HoldPoint7 = child.Find("HoldPoint (8)").gameObject;
        }
    }
}

using Kitchen;
using KitchenApplianceChest.Customs;
using KitchenData;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenApplianceChest
{
    internal class PopulateApplianceStorage : GameSystemBase
    {
        EntityQuery ApplianceStorages;

        static List<int> ApplianceIDs;

        protected override void Initialise()
        {
            base.Initialise();
            ApplianceStorages = GetEntityQuery(new QueryHelper()
                .All(typeof(CApplianceStorage), typeof(CPopulateApplianceStorage)));

            ApplianceIDs = GameData.Main.Get<Appliance>()
                .Where(x => x.Properties.OfType<CApplianceStorage>().Count() == 0)
                .Where(x => x.Properties.OfType<CPopulateApplianceStorage>().Count() == 0)
                .Where(x => x.Properties.OfType<CBlueprintStore>().Count() == 0)
                .Where(x => x.IsPurchasable || x.IsPurchasableAsUpgrade)
                .Where(x => x.ShoppingTags != ShoppingTags.None)
                .Where(x => !x.ShoppingTags.HasFlag(ShoppingTags.BlueprintStore) &&
                    !x.ShoppingTags.HasFlag(ShoppingTags.SpecialEvent) && 
                    !x.ShoppingTags.HasFlag(ShoppingTags.BlueprintStore) &&
                    !x.ShoppingTags.HasFlag(ShoppingTags.Decoration))
                .Where(x => !(x.ShoppingTags.HasFlag(ShoppingTags.Cooking) && x.ShoppingTags.HasFlag(ShoppingTags.Misc)))
                .Select(x => x.ID).ToList();
        }

        protected override void OnUpdate()
        {
            using NativeArray<Entity> entities = ApplianceStorages.ToEntityArray(Allocator.Temp);
            using NativeArray<CApplianceStorage> storages = ApplianceStorages.ToComponentDataArray<CApplianceStorage>(Allocator.Temp);
            using NativeArray<CPopulateApplianceStorage> populaters = ApplianceStorages.ToComponentDataArray<CPopulateApplianceStorage>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                CApplianceStorage storage = storages[i];
                CPopulateApplianceStorage populater = populaters[i];

                int min = populater.Minimum;
                int max = Mathf.Clamp(populater.Maximum, min, storage.Capacity);

                for (int j = 0; j < max; j++)
                {
                    if (j < min || Random.value < 0.5f)
                    {
                        int randomIndex = Random.Range(0, ApplianceIDs.Count);
                        storage.Store(ApplianceIDs[randomIndex]);
                    }
                }

                Set(entity, storage);
                EntityManager.RemoveComponent<CPopulateApplianceStorage>(entity);
            }
        }
    }
}

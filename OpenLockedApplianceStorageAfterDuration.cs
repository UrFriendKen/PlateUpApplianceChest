using Kitchen;
using KitchenApplianceChest.Customs;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace KitchenApplianceChest
{
    public struct CIsLockedApplianceStorage : IApplianceProperty, IAttachableProperty, IComponentData, IModComponent
    {
        public int UnlockedReplacementApplianceID;
        public float ViewUpdateDelay { get; set; } = 0f;
        public CIsLockedApplianceStorage() { }
    }

    public struct CApplianceStorageUnlockRequest : IComponentData, IModComponent { }

    internal class OpenLockedApplianceStorageAfterDuration : DaySystem
    {
        EntityQuery ApplianceStorages;

        protected override void Initialise()
        {
            base.Initialise();
            ApplianceStorages = GetEntityQuery(new QueryHelper()
                .All(typeof(CAppliance), typeof(CTakesDuration), typeof(CApplianceStorage), typeof(CIsLockedApplianceStorage)));

        }

        protected override void OnUpdate()
        {
            using NativeArray<Entity> entities = ApplianceStorages.ToEntityArray(Allocator.Temp);
            using NativeArray<CTakesDuration> durations = ApplianceStorages.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
            using NativeArray<CAppliance> appliances = ApplianceStorages.ToComponentDataArray<CAppliance>(Allocator.Temp);
            using NativeArray<CIsLockedApplianceStorage> storageLocks = ApplianceStorages.ToComponentDataArray<CIsLockedApplianceStorage>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                CTakesDuration duration = durations[i];
                CAppliance appliance = appliances[i];
                CIsLockedApplianceStorage storageLock = storageLocks[i];


                if (!duration.Active || duration.Remaining > 0f)
                {
                    continue;
                }
                EntityManager.RemoveComponent<CTakesDuration>(entity);
                EntityManager.RemoveComponent<CDisplayDuration>(entity);
                EntityManager.RemoveComponent<CLockDurationTimeOfDay>(entity);

                appliance.ID = storageLock.UnlockedReplacementApplianceID;
                Set(entity, appliance);
                Set<CApplianceStorageUnlockRequest>(entity);
            }
        }
    }
}

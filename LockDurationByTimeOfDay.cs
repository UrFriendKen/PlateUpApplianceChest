using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace KitchenApplianceChest
{
    public struct CLockDurationTimeOfDay : IApplianceProperty, IAttachableProperty, IComponentData, IModComponent
    {
        public bool LockDuringDay;
        public bool LockDuringNight;
    }

    [UpdateInGroup(typeof(DurationLocks))]
    internal class LockDurationByTimeOfDay : RestaurantSystem
    {
        EntityQuery Locks;

        protected override void Initialise()
        {
            base.Initialise();
            Locks = GetEntityQuery(typeof(CTakesDuration), typeof(CLockDurationTimeOfDay));
        }

        protected override void OnUpdate()
        {
            using NativeArray<Entity> entities = Locks.ToEntityArray(Allocator.Temp);
            using NativeArray<CTakesDuration> durations = Locks.ToComponentDataArray<CTakesDuration>(Allocator.Temp);
            using NativeArray<CLockDurationTimeOfDay> times = Locks.ToComponentDataArray<CLockDurationTimeOfDay>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                CTakesDuration duration = durations[i];
                CLockDurationTimeOfDay time = times[i];

                if ((Has<SIsDayTime>() && time.LockDuringDay) || (Has<SIsNightTime>() && time.LockDuringNight))
                {
                    duration.IsLocked = true;
                    Set(entity, duration);
                }
            }
        }
    }
}
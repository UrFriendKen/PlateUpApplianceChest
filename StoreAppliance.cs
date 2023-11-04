using Kitchen;
using KitchenApplianceChest.Customs;
using KitchenData;
using Unity.Entities;

namespace KitchenApplianceChest
{
    [UpdateBefore(typeof(PickUpAndDropAppliance))]
    public class StoreAppliance : ApplianceInteractionSystem
    {
        private CItemHolder Holder;

        private CApplianceStorage Store;

        private CAppliance Appliance;

        private CStoredPlates StoredPlates;

        private CStoredTables StoredTables;

        protected override InteractionType RequiredType => InteractionType.Grab;

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require<CItemHolder>(data.Interactor, out Holder))
            {
                return false;
            }
            if (!Require<CApplianceStorage>(data.Target, out Store))
            {
                return false;
            }
            if (Has<CApplianceBlueprint>(Holder.HeldItem))
            {
                return false;
            }
            if (!Require<CAppliance>(Holder.HeldItem, out Appliance))
            {
                return false;
            }
            if (!Require<CStoredPlates>(data.Target, out StoredPlates))
            {
                return false;
            }
            if (!Require<CStoredTables>(data.Target, out StoredTables))
            {
                return false;
            }
            if (Store.IsFull && !Store.IgnoreCapacity)
            {
                return false;
            }
            if (Has<CApplyDecor>(Holder.HeldItem))
            {
                return false;
            }
            if ((Require(Holder.HeldItem, out CBlueprintStore blueprintStore) && blueprintStore.InUse) || Has<CApplianceStorage>(Holder.HeldItem))
            {
                return false;
            }
            if (Has<CIsLockedApplianceStorage>(data.Target))
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            data.Context.Destroy(Holder.HeldItem);
            data.Context.Set(data.Interactor, default(CItemHolder));

            if (GameData.Main.TryGet<Appliance>(Appliance.ID, out var appliance) && appliance.GetProperty<CItemProvider>(out var provider) && provider.DefaultProvidedItem == 793377380)    // Plate
            {
                StoredPlates.PlatesCount += provider.Maximum;
                Set(data.Target, StoredPlates);
            }

            if (appliance.GetProperty<CApplianceTable>(out _))
            {
                StoredTables.Add(appliance.ID);
                Set(data.Target, StoredTables);
            }

            Store.Store(Appliance.ID);
            data.Context.Set(data.Target, Store);
        }
    }
}


// Kitchen.RetrieveBlueprint
using Kitchen;
using KitchenApplianceChest.Customs;
using KitchenData;
using Unity.Entities;

namespace KitchenApplianceChest
{
    [UpdateBefore(typeof(PickUpAndDropAppliance))]
    public class RetrieveAppliance : ApplianceInteractionSystem
    {
        private CItemHolder Holder;

        private CStoredPlates StoredPlates;

        private CStoredTables StoredTables;

        private CApplianceStorage Store;

        protected override InteractionType RequiredType => InteractionType.Grab;

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Require<CItemHolder>(data.Interactor, out Holder))
            {
                return false;
            }
            if (Holder.HeldItem != default(Entity))
            {
                return false;
            }
            if (!Require<CApplianceStorage>(data.Target, out Store))
            {
                return false;
            }
            if (Store.IsEmpty)
            {
                return false;
            }
            if (Has<CIsLockedApplianceStorage>(data.Target))
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
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            Entity entity = data.Context.CreateEntity();
            Store.Retrieve(out int applianceID);

            if (GameData.Main.TryGet<Appliance>(applianceID, out var appliance) && appliance.GetProperty<CItemProvider>(out var provider) && provider.DefaultProvidedItem == 793377380) // Plate
            {
                StoredPlates.PlatesCount -= provider.Maximum;
                Set(data.Target, StoredPlates);
            }

            if (appliance.GetProperty<CApplianceTable>(out _))
            {
                StoredTables.Remove(appliance.ID);
                Set(data.Target, StoredTables);
            }

            data.Context.Set(data.Target, Store);
            data.Context.Set(entity, new CCreateAppliance
            {
                ID = applianceID
            });

            data.Context.Set(entity, GetComponent<CPosition>(data.Interactor));
            data.Context.Add<CHeldAppliance>(entity);

            data.Context.Set(entity, new CHeldBy
            {
                Holder = data.Interactor
            });
            data.Context.Set(data.Interactor, new CItemHolder
            {
                HeldItem = entity
            });
        }
    }

}


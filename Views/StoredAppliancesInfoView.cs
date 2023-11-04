using Kitchen;
using KitchenApplianceChest.Patches;
using KitchenData;
using KitchenMods;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace KitchenApplianceChest.Views
{
    public class StoredAppliancesInfoView : UpdatableObjectView<StoredAppliancesInfoView.ViewData>
    {
        [UpdateBefore(typeof(ApplianceInfoView.UpdateView))]
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            EntityQuery ApplianceInfos;

            protected override void Initialise()
            {
                base.Initialise();
                ApplianceInfos = GetEntityQuery(typeof(CApplianceInfo), typeof(CApplianceStorageInfo), typeof(CLinkedView));
            }

            protected override void OnUpdate()
            {
                using NativeArray<Entity> entities = ApplianceInfos.ToEntityArray(Allocator.Temp);
                using NativeArray<CLinkedView> views = ApplianceInfos.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                using NativeArray<CApplianceInfo> applianceInfos = ApplianceInfos.ToComponentDataArray<CApplianceInfo>(Allocator.Temp);
                using NativeArray<CApplianceStorageInfo> applianceStorageInfos = ApplianceInfos.ToComponentDataArray<CApplianceStorageInfo>(Allocator.Temp);

                int money = GetOrDefault<SMoney>().Amount;

                for (int i = 0; i < entities.Length; i++)
                {
                    Entity entity = entities[i];
                    CLinkedView view = views[i];
                    CApplianceInfo applianceInfo = applianceInfos[i];
                    CApplianceStorageInfo applianceStorageInfo = applianceStorageInfos[i];

                    SendUpdate(view, new ViewData()
                    {
                        ApplianceIDs = applianceStorageInfo.GetApplianceIDs(),
                        ID = applianceInfo.ID,
                        PlayerMoney = money,
                        Mode = applianceInfo.Mode,
                        Price = applianceInfo.Price
                    });
                }
            }
        }

        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)]
            public List<int> ApplianceIDs;

            [Key(1)]
            public int ID;

            [Key(2)]
            public int PlayerMoney;

            [Key(3)]
            public CApplianceInfo.ApplianceInfoMode Mode;

            [Key(4)]
            public int Price;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<StoredAppliancesInfoView>();

            public bool IsChangedFrom(ViewData check)
            {
                if (ID != check.ID || PlayerMoney != check.PlayerMoney || Mode != check.Mode || Price != check.Price || ApplianceIDs.Count != check.ApplianceIDs.Count)
                    return true;
                for (int i = 0; i < ApplianceIDs.Count; i++)
                {
                    if (ApplianceIDs[i] != check.ApplianceIDs[i])
                    {
                        return true;
                    }
                }
                return false;

            }
        }

        private ViewData Data;

        public string StoredApplianceNames { get; protected set; }

        protected override void UpdateData(ViewData data)
        {
            Data = data;

            List<int> nonZeroIDs = data.ApplianceIDs.Where(id => id != 0).ToList();
            if (nonZeroIDs.Count > 0)
                StoredApplianceNames = string.Join(", ", nonZeroIDs.Select(id => GameData.Main.TryGet(id, out Appliance appliance) ? appliance.Name : "Unknown"));
            else
                StoredApplianceNames = "No Appliances";
        }
    }
}

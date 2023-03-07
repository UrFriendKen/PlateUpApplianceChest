using Kitchen;
using KitchenApplianceChest.Customs;
using KitchenData;
using MessagePack;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenApplianceChest.Views
{
    public class ApplianceStorageView : UpdatableObjectView<ApplianceStorageView.ViewData>
    {
        public GameObject HoldPoint0;
        public GameObject HoldPoint1;
        public GameObject HoldPoint2;
        public GameObject HoldPoint3;
        public GameObject HoldPoint4;
        public GameObject HoldPoint5;
        public GameObject HoldPoint6;
        public GameObject HoldPoint7;

        public class ApplianceStorageViewSystem : IncrementalViewSystemBase<ApplianceStorageView.ViewData>
        {
            EntityQuery Stores;
            protected override void Initialise()
            {
                base.Initialise();
                Stores = GetEntityQuery(new QueryHelper()
                    .All(typeof(CLinkedView), typeof(CApplianceStorage)));

            }
            protected override void OnUpdate()
            {
                NativeArray<CLinkedView> views = Stores.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                NativeArray<CApplianceStorage> stores = Stores.ToComponentDataArray<CApplianceStorage>(Allocator.Temp);

                for (int i = 0; i < views.Length; i++)
                {
                    var view = views[i];
                    var store = stores[i];

                    ViewData data = new ViewData();
                    List<int> ids = stores[i].GetApplianceIDs();
                    data.ID0 = ids[0];
                    data.ID1 = ids[1];
                    data.ID2 = ids[2];
                    data.ID3 = ids[3];
                    data.ID4 = ids[4];
                    data.ID5 = ids[5];
                    data.ID6 = ids[6];
                    data.ID7 = ids[7];

                    SendUpdate(view, data);
                }
            }
        }


        [MessagePackObject(false)]
        public class ViewData : ISpecificViewData, IViewData.ICheckForChanges<ApplianceStorageView.ViewData>
        {
            [Key(0)]
            public int ID0;
            [Key(1)]
            public int ID1;
            [Key(2)]
            public int ID2;
            [Key(3)]
            public int ID3;
            [Key(4)]
            public int ID4;
            [Key(5)]
            public int ID5;
            [Key(6)]
            public int ID6;
            [Key(7)]
            public int ID7;

            public IUpdatableObject GetRelevantSubview(IObjectView view)
            {
                return view.GetSubView<ApplianceStorageView>();
            }

            public bool IsChangedFrom(ViewData check)
            {
                return
                    ID0 != check.ID0 ||
                    ID1 != check.ID1 ||
                    ID2 != check.ID2 ||
                    ID3 != check.ID3 ||
                    ID4 != check.ID4 ||
                    ID5 != check.ID5 ||
                    ID6 != check.ID6 ||
                    ID7 != check.ID7;
            }
        }

        protected override void UpdateData(ViewData data)
        {
            int[] ids = new int[] { data.ID0, data.ID1, data.ID2, data.ID3, data.ID4, data.ID5, data.ID6, data.ID7 };
            GameObject[] parents = new GameObject[] { HoldPoint0, HoldPoint1, HoldPoint2, HoldPoint3, HoldPoint4, HoldPoint5, HoldPoint6, HoldPoint7 };

            for (int i = 0; i < 8; i++)
            {
                if (parents[i].transform.childCount > 0)
                {
                    for (int j = parents[i].transform.childCount - 1; j > -1; j--)
                    {
                        Destroy(parents[i].transform.GetChild(j).gameObject);
                    }
                }

                if (ids[i] != 0)
                {
                    GameObject appliancePrefab = GameObject.Instantiate(GameData.Main.Get<Appliance>(ids[i]).Prefab);
                    if (appliancePrefab.TryGetComponent(out ApplianceView appView))
                    {
                        Destroy(appView);
                    }
                    Collider[] colliders = appliancePrefab.GetComponentsInChildren<Collider>();
                    foreach (Collider collider in colliders)
                    {
                        collider.enabled = false;
                    }
                    appliancePrefab.transform.parent = parents[i].transform;
                    appliancePrefab.transform.localPosition = Vector3.zero;
                    appliancePrefab.transform.localRotation = Quaternion.identity;
                    appliancePrefab.transform.localScale = Vector3.one;
                }
            }
        }
    }
}

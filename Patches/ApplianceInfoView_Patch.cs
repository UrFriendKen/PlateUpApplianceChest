using HarmonyLib;
using Kitchen;
using KitchenApplianceChest.Views;
using KitchenData;
using System.Reflection;

namespace KitchenApplianceChest.Patches
{
    [HarmonyPatch]
    static class ApplianceInfoView_Patch
    {
        static bool _beforeFirstTag = false;

        static MethodInfo m_AddSection = typeof(ApplianceInfoView).GetMethod("AddSection", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(ApplianceInfoView), "UpdateData")]
        [HarmonyPrefix]
        static void UpdateData_Prefix(ApplianceInfoView.ViewData data)
        {
            _beforeFirstTag = true;
        }

        [HarmonyPatch(typeof(ApplianceInfoView), "AddTag")]
        [HarmonyPrefix]
        static void AddTag_Prefix(ref float __state, ref float offset, ref ApplianceInfoView __instance, ref float __result)
        {
            __state = 0f;
            if (_beforeFirstTag &&
                (__instance?.gameObject.TryGetComponent(out StoredAppliancesInfoView storedAppliancesInfoView) ?? false) &&
                storedAppliancesInfoView.StoredApplianceNames != null)
            {
                Appliance.Section section = default;
                section.Title = "Stored Items";
                section.Description = storedAppliancesInfoView.StoredApplianceNames;
                float num = (float)m_AddSection.Invoke(__instance, new object[3]
                {
                    offset,
                    section,
                    false
                });
                offset += num;
                __state = num;
            }
            _beforeFirstTag = false;
        }

        [HarmonyPatch(typeof(ApplianceInfoView), "AddTag")]
        [HarmonyPostfix]
        static void AddTag_Postfix(ref float __state, float offset, ref ApplianceInfoView __instance, ref float __result)
        {
            __result += __state;
        }
    }
}

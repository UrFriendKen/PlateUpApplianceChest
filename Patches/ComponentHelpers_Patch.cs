using HarmonyLib;
using Kitchen;
using KitchenData;
using KitchenMods;
using System.Reflection;
using Unity.Entities;

namespace KitchenApplianceChest.Patches
{
    [HarmonyPatch]
    static class ApplianceComponentHelpers_Patch
    {
        [HarmonyTargetMethod]
        static MethodBase ApplianceComponentSetDynamic_TargetMethod()
        {
            return AccessTools.FirstMethod(typeof(ApplianceComponentHelpers), method => method.Name.Contains("SetDynamic") && method.IsGenericMethod).MakeGenericMethod(typeof(IApplianceProperty));
        }

        [HarmonyPrefix]
        [HarmonyPriority(int.MinValue)]
        static bool ApplianceComponentSetDynamic_Prefix(bool __runOriginal, EntityContext ctx, Entity e, IApplianceProperty component)
        {
            if (!__runOriginal ||
                !(component is IModComponent))
                return true;
            ctx.Set(e, (dynamic)component);
            return false;
        }
    }

    [HarmonyPatch]
    static class ItemComponentHelpers_Patch
    {
        [HarmonyTargetMethod]
        static MethodBase ItemComponentSetDynamic_TargetMethod()
        {
            return AccessTools.FirstMethod(typeof(ItemComponentHelpers), method => method.Name.Contains("SetDynamic") && method.IsGenericMethod).MakeGenericMethod(typeof(IItemProperty));
        }

        [HarmonyPrefix]
        [HarmonyPriority(int.MinValue)]
        static bool ItemComponentSetDynamic_Prefix(bool __runOriginal, EntityContext ctx, Entity e, IItemProperty component)
        {
            if (!__runOriginal ||
                !(component is IModComponent))
                return true;
            ctx.Set(e, (dynamic)component);
            return false;
        }
    }

    [HarmonyPatch]
    static class EffectHelpers_Patch
    {
        [HarmonyPatch(typeof(EffectHelpers), nameof(EffectHelpers.AddApplianceEffectComponents))]
        [HarmonyPrefix]
        [HarmonyPriority(int.MinValue)]
        static bool AddApplianceEffectComponents_Prefix(bool __runOriginal, EntityCommandBuffer ecb, Entity e, IEffectPropertySource prop)
        {
            if (!__runOriginal ||
                prop.EffectRange == null ||
                prop.EffectType == null)
                return true;

            if (!(prop.EffectCondition is IModComponent) &&
                !(prop.EffectRange is IModComponent) &&
                !(prop.EffectType is IModComponent))
                return true;

            ecb.AddComponent(e, default(CAppliesEffect));

            if (prop.EffectCondition == null)
                ecb.AddComponent(e, default(CEffectAlways));
            else
                ecb.AddComponent(e, (dynamic)prop.EffectCondition);

            ecb.AddComponent(e, (dynamic)prop.EffectRange);
            ecb.AddComponent(e, (dynamic)prop.EffectType);

            return false;
        }

        [HarmonyPatch(typeof(EffectHelpers), nameof(EffectHelpers.AddAttachedEffectComponents))]
        [HarmonyPrefix]
        [HarmonyPriority(int.MinValue)]
        static bool AddApplianceEffectComponents_Prefix(bool __runOriginal, EntityCommandBuffer ecb, Entity e, Effect eff)
        {
            if (!__runOriginal || eff.Properties == null)
            {
                return true;
            }
            foreach (IEffectProperty property in eff.Properties)
            {
                ecb.AddComponent(e, (dynamic)property);
            }
            return false;
        }
    }
}

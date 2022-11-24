using ABI_RC.Core.Player;
using HarmonyLib;

namespace Tayou {
    [HarmonyPatch]
    class PlayerNameplatePatch {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerNameplate), nameof(PlayerNameplate.UpdateNamePlate))]
        static void UpdateNamePlatePatch(PlayerNameplate __instance) {
            if (!CustomNameplatesMod.customNameplateHelpers.TryGetValue(__instance, out CustomNameplateHelper customNameplateHelper)) {
                customNameplateHelper = CustomNameplatesMod.customNameplateHelpers[__instance] = __instance.gameObject.AddComponent<CustomNameplateHelper>();
                customNameplateHelper.Init(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerNameplate), nameof(PlayerNameplate.TalkerState))]
        static void TalkerStatePatch(PlayerNameplate __instance) {
            if (CustomNameplatesMod.customNameplateHelpers.TryGetValue(__instance, out CustomNameplateHelper customNameplateHelper)) {
                customNameplateHelper.UpdateNameplate();
            }
        }
    }
}
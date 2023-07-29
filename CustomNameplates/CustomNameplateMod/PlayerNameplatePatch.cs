using System;
using ABI_RC.Core.Player;
using HarmonyLib;

namespace Tayou.CustomNameplateMod {
    [HarmonyPatch]
    class PlayerNameplatePatch {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerNameplate), nameof(PlayerNameplate.UpdateNamePlate))]
        static void UpdateNamePlatePatch(PlayerNameplate __instance) {
            try {
                if (!Mod.CustomNameplateHelpers.TryGetValue(__instance, out Helper customNameplateHelper)) {
                    customNameplateHelper = Mod.CustomNameplateHelpers[__instance] = __instance.gameObject.AddComponent<Helper>();
                    customNameplateHelper.Init(__instance);
                } else {
                    customNameplateHelper.UpdateNameplate();
                }
            } catch (Exception e) {
                Mod.ConsoleLog(e.ToString());
                throw;
            }
        }
    }
}
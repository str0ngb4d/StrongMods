namespace DynamicGameStats.Patches {
  //[HarmonyPatch(typeof(EntityBuffs), nameof(EntityBuffs.SetCustomVar))]
  public class EntityBuffsSetCustomVarPatch {
    private static void Postfix(EntityBuffs __instance, string _name) {
      DynamicGameStats.OnCVarChange(__instance.parent, _name);
    }
  }
}
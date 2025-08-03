namespace DynamicGameStats.Patches {
  //[HarmonyPatch(typeof(EntityBuffs), nameof(EntityBuffs.RemoveCustomVar))]
  public class EntityBuffsRemoveCustomVarPatch {
    private static void Postfix(EntityBuffs __instance, string _name) {
      DynamicGameStats.OnCVarChange(__instance.parent, _name);
    }
  }
}
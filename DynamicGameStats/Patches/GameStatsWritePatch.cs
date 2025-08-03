using System.IO;

namespace DynamicGameStats.Patches {
  //[HarmonyPatch(typeof(GameStats), nameof(GameStats.Write))]
  public class GameStatsWritePatch {
    private static bool Prefix(GameStats __instance, BinaryWriter _write) {
      if (__instance is DynamicGameStats stats) {
        stats.WriteDynamic(_write);
        return false;
      }

      return true;
    }
  }
}
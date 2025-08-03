using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace DynamicGameStats.Patches {
  // Because of the IEnumerator, the IL code is actually not in RequestToEnterGame() but a sealed inner class
  //[HarmonyPatch("<RequestToEnterGame>d__189", "MoveNext")]
  public class GameManagerRequestToEnterGamePatch {
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
      ILGenerator generator) {
      var codeMatcher = new CodeMatcher(instructions, generator);
      codeMatcher.MatchStartForward(
          CodeMatch.Calls(AccessTools.PropertyGetter(typeof(GameStats), nameof(GameStats.Instance)))
        )
        .ThrowIfInvalid("[DynamicGameStats] Could not find insertion point")
        .RemoveInstruction()
        .InsertAndAdvance(codeMatcher.InstructionsInRange(codeMatcher.Pos - 3, codeMatcher.Pos - 2))
        .Insert(
          new CodeInstruction(OpCodes.Newobj,
            AccessTools.Constructor(typeof(DynamicGameStats), new[] { typeof(ClientInfo) }))
        );
      //Log.Out($"[DynamicGameStats] GameManager instructions:\n    {string.Join("\n    ", codeMatcher.Instructions())}");
      return codeMatcher.Instructions();
    }
  }
}
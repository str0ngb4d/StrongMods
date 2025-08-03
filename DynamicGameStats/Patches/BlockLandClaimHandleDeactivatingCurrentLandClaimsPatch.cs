using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace DynamicGameStats.Patches {
  [HarmonyPatch(typeof(BlockLandClaim), nameof(BlockLandClaim.HandleDeactivatingCurrentLandClaims))]
  public class BlockLandClaimHandleDeactivatingCurrentLandClaimsPatch {
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
      ILGenerator generator) {
      var codeMatcher = new CodeMatcher(instructions, generator);
      codeMatcher
        .MatchStartForward(
          //CodeMatch.LoadsConstant(EnumGameStats.LandClaimCount), // on linux this doesn't match; bug?
          CodeMatch.Calls(() => GameStats.GetInt(default)),
          CodeMatch.StoresLocal()
        )
        .ThrowIfInvalid("[DynamicGameStats] Could not find insertion point")
        .Advance(-1) // make up for removing the LoadsConstant in the match
        .InsertAndAdvance(
          CodeInstruction.LoadArgument(1), // persistentPlayerData
          new CodeInstruction(OpCodes.Newobj,
            AccessTools.Constructor(typeof(DynamicGameStats), new[] { typeof(PersistentPlayerData) }))
        )
        .Advance(1)
        .RemoveInstruction()
        .Insert(
          CodeInstruction.Call(() => ((DynamicGameStats)null).GetIntDynamic(default))
        );
      //Log.Out($"[DynamicGameStats] BlockLandClaim instructions:\n    {string.Join("\n    ", codeMatcher.Instructions())}");
      return codeMatcher.Instructions();
    }
  }
}
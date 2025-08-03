using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CSMM_Patrons;
using HarmonyLib;

namespace CPMFixes {
  [HarmonyPatch(typeof(ChatFilter), nameof(ChatFilter.Exec))]
  public class ChatFilterExecPatch {
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
      var codeMatcher = new CodeMatcher(instructions, generator);
      codeMatcher
        .End()
        .MatchStartBackwards(
          CodeMatch.LoadsConstant(ModEvents.EModEventResult.StopHandlersRunVanilla),
          new CodeMatch(OpCodes.Ret)
        )
        .ThrowIfInvalid("[CPMFixes] Could not find insertion point")
        .RemoveInstruction()
        .InsertAndAdvance(
          new CodeInstruction(OpCodes.Ldc_I4_0)
        );
      Log.Out($"[CPMFixes] ChatFilter.Exec instructions:\n    {string.Join("\n    ", codeMatcher.Instructions())}");
      return codeMatcher.Instructions();
    }
  }

  public class Initializer : IModApi {
    public void InitMod(Mod _modInstance) {
      var harmony = new Harmony(_modInstance.Name);
      harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
  }
}
using System.Reflection;
using HarmonyLib;

namespace AuthZ.Patches {
  public class Initializer : IModApi {
    public void InitMod(Mod _modInstance) {
      var harmony = new Harmony(_modInstance.Name);
      harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
  }
}
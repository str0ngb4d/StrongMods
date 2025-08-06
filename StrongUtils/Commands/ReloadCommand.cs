using System;
using System.Collections.Generic;

namespace StrongUtils.Commands {
  public class ReloadCommand : ConsoleCmdAbstract {
    private const string Usage = @"Usage: reload";

    public override string getDescription() => "Reloads configuration.";

    public override string getHelp() => Usage;

    public override string[] getCommands() => new[] { "reload" };

    public override void Execute(List<string> @params, CommandSenderInfo senderInfo) {
      try {
        WorldStaticData.ReloadAllXmlsSync();
      } catch (Exception e) {
        Log.Error("Error in ReloadCommand.Execute: " + e.Message);
      }
    }
  }
}

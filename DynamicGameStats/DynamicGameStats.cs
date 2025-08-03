using System.Collections.Generic;
using System.IO;

namespace DynamicGameStats {
  public class DynamicGameStats : GameStats {
    public bool IsDynamic;
    public int PlayerEntityId;

    public DynamicGameStats(int playerEntityId) {
      PlayerEntityId = playerEntityId;
      var timeLimitActive = GetBool(EnumGameStats.TimeLimitActive);
      if (timeLimitActive) {
        // When time limit is active, GameStateManager.OnUpdateTick() will periodically re-send GameStats to all clients in a way that is complicated to patch, so instead we disable this mod when detected
        Log.Warning("[DyanamicGameStats] Mod disabled because TimeLimitActive is set to True.");
      }

      // entity IDs start at 1 and will be -1 before player first spawns
      IsDynamic = !timeLimitActive && PlayerEntityId > 0;
    }

    public DynamicGameStats(PersistentPlayerData player) : this(player?.EntityId ?? -1) { }

    // client.entityId might be -1 before player has spawned, in which case we look up by platform ID
    public DynamicGameStats(ClientInfo client) : this(
      GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(client.entityId) ??
      GameManager.Instance.persistentPlayers.GetPlayerData(client.PlatformId)) { }

    public string GetStringDynamic(EnumGameStats property) {
      return GetString(property);
    }

    public float GetFloatDynamic(EnumGameStats property) {
      return GetFloat(property);
    }

    public int GetIntDynamic(EnumGameStats property) {
      if (IsDynamic) {
        if (property == EnumGameStats.LandClaimCount) {
          return GetLandClaimCount();
        }
      }

      return GetInt(property);
    }

    public bool GetBoolDynamic(EnumGameStats property) {
      return GetBool(property);
    }

    public int GetLandClaimCount() {
      var count = GetInt(EnumGameStats.LandClaimCount);
      var p = GameManager.Instance.World?.Players?.dict[PlayerEntityId];
      var biomeBadgeProgressionOn = (int)(p?.GetCVar("$BiomeProgressionOn") ?? 0);
      var biomeBadgeLevel = biomeBadgeProgressionOn * (int)(p?.GetCVar("$BiomeBadgeLevel") ?? 0);
      var result = count + biomeBadgeLevel;
      Log.Out(
        $"[DynamicGameStats] GetLandClaimCount() => {result} (count: {count} | biomeBadgeLevel: {biomeBadgeLevel})");
      return result;
    }

    public void WriteDynamic(BinaryWriter _write) {
      foreach (var property in Instance.propertyList) {
        if (property.bPersistent) {
          switch (property.type) {
            case EnumType.Int:
              _write.Write(GetIntDynamic(property.name));
              continue;
            case EnumType.Float:
              _write.Write(GetFloatDynamic(property.name));
              continue;
            case EnumType.String:
              _write.Write(GetStringDynamic(property.name));
              continue;
            case EnumType.Bool:
              _write.Write(GetBoolDynamic(property.name));
              continue;
            case EnumType.Binary:
              _write.Write(Utils.ToBase64(GetStringDynamic(property.name)));
              continue;
            default:
              continue;
          }
        }
      }
    }

    public static void OnCVarChange(EntityAlive entity, string cvar) {
      if (entity is not EntityPlayer player || cvar != "$BiomeBadgeLevel") {
        return;
      }

      SendToClient(player, cvar);
    }

    public static void SendToClient(EntityPlayer player, string cvar) {
      var client = ConnectionManager.Instance.Clients.ForEntityId(player.entityId);
      Log.Out($"[DynamicGameStats] Clients.ForEntityId({player.entityId}) => {client.entityId}");
      SendToClient(client, cvar);
    }

    public static void SendToClient(ClientInfo client, string cvar) {
      var player = GameManager.Instance.persistentPlayers.GetPlayerData(client.PlatformId);
      Log.Out($"[DynamicGameStats] PersistentPlayerList.GetPlayerData({client.PlatformId}) => {player?.EntityId}");
      var stats = new DynamicGameStats(client);
      Log.Out($"[DynamicGameStats] new DynamicGameStats({client.PlatformId}) => {stats.PlayerEntityId}");
      client.SendPackage(NetPackageManager.GetPackage<NetPackageGameStats>().Setup(stats));
      NotifyPlayerOfChange(client, cvar, stats);
    }

    public static void NotifyPlayerOfChange(ClientInfo client, string cvar, DynamicGameStats stats) {
      if (cvar != "$BiomeBadgeLevel") {
        return;
      }

      var message = $"Your max land claim count is now {stats.GetIntDynamic(EnumGameStats.LandClaimCount)}";
      GameManager.Instance.ChatMessageServer(client, EChatType.Whisper, -1, $"[A0]{message}",
        new List<int> { client.entityId }, EMessageSender.None);
    }
  }
}
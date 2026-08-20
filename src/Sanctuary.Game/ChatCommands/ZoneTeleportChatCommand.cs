using System;
using System.Linq;

using Sanctuary.Core.Helpers;
using Sanctuary.Game.Entities;
using Sanctuary.Game.Helpers;
using Sanctuary.Game.Resources.Definitions.Zones;

namespace Sanctuary.Game.ChatCommands;

public class ZoneTeleportChatCommand : IChatCommand
{
    private readonly IChatCommandManager _chatCommandManager;
    private readonly IZoneManager _zoneManager;
    private readonly IResourceManager _resourceManager;

    public string KeyWord => "gozone";
    public string Usage => "<name> [ownerId]";
    public string Description => "Teleports you into the given zone, creating an instance if needed.";
    public ChatCommandRole RequiredRole => ChatCommandRole.Player;

    public ZoneTeleportChatCommand(IChatCommandManager chatCommandManager, IZoneManager zoneManager,
        IResourceManager resourceManager)
    {
        _chatCommandManager = chatCommandManager;
        _zoneManager = zoneManager;
        _resourceManager = resourceManager;
    }

    public bool Handle(Player invoker, string[] args)
    {
        if (args.Length is not (1 or 2))
            return false;

        var name = args[0];

        ulong? ownerId = null;

        if (args.Length == 2)
        {
            if (!ulong.TryParse(args[1], out var parsedOwnerId))
                return false;

            ownerId = parsedOwnerId;
        }

        var definition = _resourceManager.Zones.Values
            .FirstOrDefault(candidate => candidate.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            ChatHelper.SendSystemMessage(invoker, $"Unknown zone '{name}'.");
            return true;
        }

        // Owned zone types (housing, dungeons) default to the invoker's own character when no
        // owner is given. Shared zones (World) are left as-is - null unless explicitly overridden.
        if (ownerId is null && definition is not WorldZoneDefinition)
            ownerId = GuidHelper.GetPlayerId(invoker.Guid);

        if (!_zoneManager.TryGetOrCreateZoneInstance(definition.Id, ownerId, out var zone))
        {
            ChatHelper.SendSystemMessage(invoker, $"Failed to get or create an instance of '{name}'.");
            return true;
        }

        if (!invoker.TeleportToZone(zone, zone.SpawnPosition, zone.SpawnRotation))
        {
            ChatHelper.SendSystemMessage(invoker, $"Failed to teleport to '{name}'.");
            return true;
        }

        _chatCommandManager.LogAction(this, invoker, "Zone teleport", name, $"ownerId={ownerId?.ToString() ?? "none"}");
        ChatHelper.SendSystemMessage(invoker, $"Teleported to '{name}'.");

        return true;
    }
}

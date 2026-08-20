using System.Linq;

using Sanctuary.Game.Entities;
using Sanctuary.Game.Helpers;

namespace Sanctuary.Game.ChatCommands;

public class ZoneListChatCommand : IChatCommand
{
    private readonly IZoneManager _zoneManager;
    private readonly IResourceManager _resourceManager;

    public string KeyWord => "zones";
    public string Usage => "";
    public string Description => "Lists all known zones, whether they're currently running, and their owner.";
    public ChatCommandRole RequiredRole => ChatCommandRole.Player;

    public ZoneListChatCommand(IZoneManager zoneManager, IResourceManager resourceManager)
    {
        _zoneManager = zoneManager;
        _resourceManager = resourceManager;
    }

    public bool Handle(Player invoker, string[] args)
    {
        var runningByDefinition = _zoneManager.Zones
            .GroupBy(zone => zone.DefinitionId)
            .ToDictionary(group => group.Key, group => group.ToList());

        foreach (var definition in _resourceManager.Zones.Values.OrderBy(definition => definition.Id))
        {
            if (!runningByDefinition.TryGetValue(definition.Id, out var instances) || instances.Count == 0)
            {
                ChatHelper.SendSystemMessage(invoker, $"[{definition.Id}] {definition.Name} - not running");
                continue;
            }

            foreach (var instance in instances)
            {
                var owner = instance.OwnerId?.ToString() ?? "none";
                ChatHelper.SendSystemMessage(invoker, $"[{definition.Id}] {definition.Name} - running (owner: {owner})");
            }
        }

        return true;
    }
}

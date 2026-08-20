using Sanctuary.Core.Helpers;
using Sanctuary.Game.Entities;
using Sanctuary.Game.Helpers;

namespace Sanctuary.Game.ChatCommands;

public class HouseChatCommand : IChatCommand
{
    private const int DefaultHousingZoneDefinitionId = 2; // hsg_emptylot_seaside_beach_01

    private readonly IChatCommandManager _chatCommandManager;
    private readonly IZoneManager _zoneManager;

    public string KeyWord => "house";
    public string Usage => "[zoneDefinitionId]";
    public string Description => "Grants (or confirms) a house for your character, for testing.";
    public ChatCommandRole RequiredRole => ChatCommandRole.Player;

    public HouseChatCommand(IChatCommandManager chatCommandManager, IZoneManager zoneManager)
    {
        _chatCommandManager = chatCommandManager;
        _zoneManager = zoneManager;
    }

    public bool Handle(Player invoker, string[] args)
    {
        var zoneDefinitionId = DefaultHousingZoneDefinitionId;

        if (args.Length == 1)
        {
            if (!int.TryParse(args[0], out zoneDefinitionId))
                return false;
        }
        else if (args.Length != 0)
        {
            return false;
        }

        var ownerId = GuidHelper.GetPlayerId(invoker.Guid);

        if (!_zoneManager.TryGrantHouse(zoneDefinitionId, ownerId))
        {
            ChatHelper.SendSystemMessage(invoker,
                $"Failed to add house for zone {zoneDefinitionId}. Is that a valid housing zone id?");
            return true;
        }

        _chatCommandManager.LogAction(this, invoker, "House test grant", null, $"zoneDefinitionId={zoneDefinitionId}");
        ChatHelper.SendSystemMessage(invoker, $"You now own a house for zone {zoneDefinitionId}.");

        return true;
    }
}

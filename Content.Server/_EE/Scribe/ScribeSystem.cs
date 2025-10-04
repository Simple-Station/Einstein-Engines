using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.Paper;
using Content.Server.Discord;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server._EE.Scribe;

public sealed partial class ScribeSystem : EntitySystem
{
    [Dependency] private readonly DiscordWebhook _discord = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    private Dictionary<string, WebhookIdentifier> _webHooks = new();

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_config, CCVars.DiscordScribeWebhooks, cvar => _webHooks = ParseCVarTable(cvar), true);

        SubscribeLocalEvent<RoundEndedEvent>(OnRoundEnded);
    }

    private void OnRoundEnded(RoundEndedEvent ev)
    {
        // Collect all the scribebooks into groups based on the Webhook they target.
        foreach (var result in EntityQuery<ScribeBookComponent, PaperComponent>().GroupBy(q => q.Item1.WebhookKey))
        {
            if (!_webHooks.TryGetValue(result.Key, out var identifier))
                continue;

            // Collect them into messages.
            var embeds = new List<WebhookEmbed>();

            foreach (var (scribeComp, paperComp) in result)
            {
                var embed = new WebhookEmbed();

                if (scribeComp.NameFormat is { } nameLoc)
                    // embed.Title = Loc.GetString(nameLoc, ("name", "Urist McHands")); //TODO: Need to work out how to hold on to people's names.
                    embed.Title = Loc.GetString(nameLoc);

                embed.Description = scribeComp.ContentFormat is { } contentLoc
                    ? Loc.GetString(contentLoc, ("content", paperComp.Content))
                    : paperComp.Content;

                if (scribeComp.Footer is { } footer)
                {
                    embed.Footer = new()
                    {
                        Text = Loc.GetString(footer, ("round-id", ev.RoundId)),
                    };
                }

                embed.Color = scribeComp.Color;

                embeds.Add(embed);
            }

            var _ = _discord.CreateMessage(identifier, new() { Embeds = embeds, });
        }
    }

    private Dictionary<string, WebhookIdentifier> ParseCVarTable(string src)
    {
        var dict = new Dictionary<string, WebhookIdentifier>();
        foreach (var line in src.Split('\n'))
        {
            // Expected format is:
            //
            // <key>: <id>/<token>
            // <key>: <id>/<token>
            // <key>: <id>/<token>

            // Maps as CCVars when? ;~;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (Split(src, ':') is not { } keyValue)
                continue;

            if (Split(keyValue.Item2, '/') is not { } idToken)
                continue;

            dict.Add(keyValue.Item1.Trim(), new(idToken.Item1.Trim(), idToken.Item2.Trim()));

            // Helper function to split the lines.
            (string, string)? Split(string input, char c)
            {
                var parts = input.Split(c, 2);
                if (parts.Length != 2)
                {
                    Log.Warning($"Webhook table line missing key: {line}");
                    return null;
                }

                var (part1, part2) = (parts[0], parts[1]);
                if (string.IsNullOrWhiteSpace(part1) || string.IsNullOrWhiteSpace(part2))
                {
                    Log.Warning($"Webhook table line malformed: {line}");
                    return null;
                }

                return (part1, part2);
            }
        }

        return dict;
    }
}

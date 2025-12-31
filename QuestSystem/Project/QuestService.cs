using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;

namespace QuestSystem;

public interface IQuestService
{
    List<Quest> GetAvailableQuests(IExecutionContext context, IGameApiClient gameApiClient);
}

public class QuestService : IQuestService
{
    private const string QuestKey = "QUESTS";

    private readonly ILogger<QuestService> _logger;
    public QuestService(ILogger<QuestService> logger)
    {
        _logger = logger;
    }

    private DateTime? CacheExpiryTime { get; set; }

    // Reminder: cache cannot be guaranteed to be consistent across all requests
    private List<Quest>? QuestCache { get; set; }

    public List<Quest> GetAvailableQuests(IExecutionContext context, IGameApiClient gameApiClient)
    {
        if (QuestCache == null || DateTime.Now > CacheExpiryTime)
        {
            var quests = FetchQuestsFromConfig(context, gameApiClient);
            QuestCache = quests;
            CacheExpiryTime = DateTime.Now.AddMinutes(5); // data in cache expires after 5 mins
        }

        return QuestCache;
    }

    private List<Quest> FetchQuestsFromConfig(IExecutionContext ctx, IGameApiClient gameApiClient)
    {
        try
        {
            var result = gameApiClient.RemoteConfigSettings.AssignSettingsGetAsync(ctx, ctx.AccessToken, ctx.ProjectId,
                ctx.EnvironmentId, null, new List<string> { "QUESTS" });

            var settings = result.Result.Data.Configs.Settings;

            return JsonConvert.DeserializeObject<List<Quest>>(settings[QuestKey].ToString());
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to assign Remote Config settings. Error: {Error}", ex.Message);
            throw new Exception($"Failed to assign Remote Config settings. Error: {ex.Message}");
        }
    }
}
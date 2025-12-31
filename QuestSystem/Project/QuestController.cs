using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace QuestSystem;

public class QuestController
{
    private const string QuestDataKey = "quest-data";
    private const string PlayerInventoryKey = "player-inventory";
    private const string PlayerFactionKey = "player-faction";

    private const string PlayerHasQuestInProgress = "You already have a quest in progress!";
    private const string PlayerHasNoQuestInProgress = "You currently do not have a quest in progress!";
    private const string PlayerHasCompletedTheQuest = "Player has already completed their quest!";
    private const string PlayerCannotProgress = "Player cannot make quest progress yet!";
    private const string PlayerProgressed = "Player made quest progress!";
    private const string PlayerHasFinishedTheQuest = "Player has finished the quest!";

    private readonly ILogger<QuestController> _logger;
    public QuestController(ILogger<QuestController> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction("AssignQuest")]
    public async Task<string> AssignQuest(IExecutionContext context, IQuestService questService,
        IGameApiClient gameApiClient)
    {
        var questData = await GetQuestData(context, gameApiClient);
        // var factionData = await gameApiClient.CloudSaveData.GetItemsAsync(context, context.AccessToken,
        //     context.ProjectId, context.PlayerId, new List<string> { PlayerFactionKey });
        // bool isVampireFaction = (bool)factionData.Data.Results.Where(value => value != null).ToList().First().Value;

        if (questData?.QuestName != null) return PlayerHasQuestInProgress;

        var availableQuests = questService.GetAvailableQuests(context, gameApiClient);
        var random = new Random();
        var index = random.Next(availableQuests.Count);
        var quest = availableQuests[index];

        string questName = quest.VampiresName ?? "QuestVampiresName";

        
        // if (isVampireFaction)
        // {
        //     if (quest.VampiresName != null) questName = quest.VampiresName;
        //     else questName = "QuestVampiresName";
        // }
        // else
        // {
        //     if (quest.HuntersName != null) questName = quest.HuntersName;
        //     else questName = "QuestHuntersName";
        // }

        int currencyReward = random.Next(quest.RewardCurrencyMin, quest.RewardCurrencyMax + 1);
        int woodReward = random.Next(quest.RewardWoodMin, quest.RewardWoodMax + 1);
        int stoneReward = random.Next(quest.RewardStoneMin, quest.RewardStoneMax + 1);
        int metalReward = random.Next(quest.RewardMetalMin, quest.RewardMetalMax + 1);

        questData = new QuestData(
            questName,
            currencyReward,
            woodReward,
            stoneReward,
            metalReward,
            DateTime.Now,
            DateTime.Now + new TimeSpan(quest.DurationHours, quest.DurationMinutes, quest.DurationSeconds));

        await SetData(context, gameApiClient, QuestDataKey, JsonConvert.SerializeObject(questData));

        return $"Player was assigned quest: {questName}!";
    }

    // [CloudCodeFunction("PerformAction")]
    // public async Task<string> PerformAction(IExecutionContext context, IGameApiClient gameApiClient, PushClient pushClient)
    // {
    //     var questData = await GetQuestData(context, gameApiClient);
    //     var factionData = await gameApiClient.CloudSaveData.GetItemsAsync(context, context.AccessToken,
    //         context.ProjectId, context.PlayerId, new List<string> { PlayerFactionKey });
    //     bool isVampireFaction = (bool)factionData.Data.Results.Where(value => value != null).ToList().First().Value;

    //     if (questData?.QuestName == null) return PlayerHasNoQuestInProgress;
    //     if (questData.ProgressLeft == 0) return PlayerHasCompletedTheQuest;

    //     await SetData(context, gameApiClient, QuestDataKey, JsonConvert.SerializeObject(questData));
    //     if (questData.ProgressLeft <= 0)
    //     {
    //         // await HandleQuestCompletion(context, gameApiClient, pushClient, questData);
    //         return PlayerHasFinishedTheQuest;
    //     }

    //     return PlayerProgressed;
    // }

    public async Task HandleQuestCompletion(IExecutionContext context, IGameApiClient gameApiClient, PushClient pushClient, QuestData? questData = null)
    {
        await NotifyPlayer(context, pushClient);
        try
        {
            questData ??= await GetQuestData(context, gameApiClient);
            PlayerInventory? inventory = await GetPlayerInventory(context, gameApiClient);
            if (inventory != null && questData != null)
            {
                inventory.Currency += questData.CurrencyReward;
                inventory.Wood += questData.WoodReward;
                inventory.Stone += questData.StoneReward;
                inventory.Metal += questData.MetalReward;

                await SetData(context, gameApiClient, PlayerInventoryKey, JsonConvert.SerializeObject(inventory));
            }

            await gameApiClient.CloudSaveData.DeleteItemAsync(context, context.ServiceToken, QuestDataKey,
                context.ProjectId, context.PlayerId);
        }
        catch (ApiException e)
        {
            _logger.LogError("Failed to delete a quest for player. Error: {Error}",  e.Message);
            throw new Exception($"Failed to delete a quest for player. Error. Error: {e.Message}");
        }
    }

    [CloudCodeFunction("GetQuestStatus")]
    public async Task<string> GetQuestStatus(IExecutionContext context, IGameApiClient gameApiClient, PushClient pushClient)
    {
        try
        {
            QuestData? questData = await GetQuestData(context, gameApiClient);
            if (questData?.QuestName != null && questData?.QuestStartTime != null && questData?.QuestFinishTime != null)
            {
                TimeSpan timeLeft = questData.QuestFinishTime - DateTime.Now;
                if (timeLeft > TimeSpan.Zero)
                {
                    return $"You are currently on a quest to {questData.QuestName}!@{timeLeft}";
                }
                else
                {
                    await HandleQuestCompletion(context, gameApiClient, pushClient, questData);
                    string rewardsString = "";
                    if (questData.CurrencyReward != 0) rewardsString += $"Currency: {questData.CurrencyReward}\n";
                    if (questData.WoodReward != 0) rewardsString += $"Wood: {questData.WoodReward}\n";
                    if (questData.StoneReward != 0) rewardsString += $"Stone: {questData.StoneReward}\n";
                    if (questData.MetalReward != 0) rewardsString += $"Metal: {questData.MetalReward}";
                        
                    string resultString = $"Your quest to {questData.QuestName} is complete!";
                    if (rewardsString != "") resultString = string.Concat(resultString, "$", rewardsString);
                    return resultString;
                }
            }
            else return PlayerHasNoQuestInProgress;
        }
        catch (ApiException e)
        {
            _logger.LogError("Failed to delete a quest for player. Error: {Error}",  e.Message);
            throw new Exception($"Failed to delete a quest for player. Error. Error: {e.Message}");
        }
    }

    private async Task NotifyPlayer(IExecutionContext context, PushClient pushClient)
    {
        const string message = "Quest completed!";
        const string messageType = "Announcement";

        try
        {
            await pushClient.SendPlayerMessageAsync(context, message, messageType, context.PlayerId);
        }
        catch (ApiException e)
        {
            _logger.LogError("Failed to send player message. Error: {Error}",  e.Message);
            throw new Exception($"Failed to send player message. Error: {e.Message}");
        }
    }

    private async Task<PlayerInventory?> GetPlayerInventory(IExecutionContext context, IGameApiClient gameApiClient)
    {
        try
        {
            var result = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.AccessToken, context.ProjectId, context.PlayerId,
                new List<string> { PlayerInventoryKey });

            if (result.Data.Results.Count == 0) return null;
            return JsonConvert.DeserializeObject<PlayerInventory>(result.Data.Results.First().Value.ToString());
        }
        catch (ApiException e)
        {
            _logger.LogError("Failed to retrieve player inventory from Cloud Save. Error: {Error}", e.Message);
            throw new Exception($"Failed to retrieve player inventory from Cloud Save. Error: {e.Message}");
        }
    }

    private async Task<QuestData?> GetQuestData(IExecutionContext context, IGameApiClient gameApiClient)
    {
        try
        {
            var result = await gameApiClient.CloudSaveData.GetItemsAsync(
                context, context.AccessToken, context.ProjectId, context.PlayerId,
                new List<string> { QuestDataKey });

            if (result.Data.Results.Count == 0) return null;
            return JsonConvert.DeserializeObject<QuestData>(result.Data.Results.First().Value.ToString());
        }
        catch (ApiException e)
        {
            _logger.LogError("Failed to retrieve quest data from Cloud Save. Error: {Error}", e.Message);
            throw new Exception($"Failed to retrieve quest data from Cloud Save. Error: {e.Message}");
        }
    }

    private async Task SetData(IExecutionContext context, IGameApiClient gameApiClient, string key,
        string value)
    {
        try
        {
            await gameApiClient.CloudSaveData
                .SetItemAsync(context, context.ServiceToken, context.ProjectId, context.PlayerId,
                    new SetItemBody(key, value));
        }
        catch (ApiException e)
        {
            _logger.LogError("Failed to save data in Cloud Save. Error: {Error}", e.Message);
            throw new Exception($"Failed to save data in Cloud Save. Error: {e.Message}");
        }
    }
}
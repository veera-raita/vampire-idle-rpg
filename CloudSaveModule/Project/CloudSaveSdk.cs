using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace CloudSaveModule;

public class CloudSaveSdk
{
    private readonly ILogger<CloudSaveSdk> _logger;

    private const string PlayerInventoryKey = "player-inventory";
    private const string InventoryCurrencyKey = "currency";
    private const string InventoryWoodKey = "wood";
    private const string InventoryStoneKey = "stone";
    private const string InventoryMetalKey = "metal";

    public CloudSaveSdk(ILogger<CloudSaveSdk> logger)
    {
        _logger = logger;
    }

    [CloudCodeFunction("SaveValue")]
    public async Task SaveValue(IExecutionContext context, IGameApiClient gameApiClient, string key, string value)
    {
        if (context.PlayerId == null) return;
        try
        {
            await gameApiClient.CloudSaveData.SetItemAsync(context, context.ServiceToken, context.ProjectId,
                context.PlayerId, new SetItemBody(key, value));
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to save data. Error: {Error}", ex.Message);
            throw new Exception($"Failed to save data for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }

    public async Task InitializeInventory(IExecutionContext context, IGameApiClient gameApiClient)
    {
        if (context.PlayerId == null) return;
        try
        {
            await gameApiClient.CloudSaveData.SetItemAsync(context, context.ServiceToken, context.ProjectId,
                context.PlayerId, new SetItemBody(PlayerInventoryKey, 
                JsonConvert.SerializeObject(new PlayerInventory())));
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to save data. Error: {Error}", ex.Message);
            throw new Exception($"Failed to save data for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }

    [CloudCodeFunction("GetInventory")]
    public async Task<Dictionary<string, int>?>GetInventory(IExecutionContext context, IGameApiClient gameApiClient)
    {
        if (context.PlayerId == null) return null;
        try
        {
            var result = await gameApiClient.CloudSaveData.GetItemsAsync(context, context.AccessToken,
                context.ProjectId, context.PlayerId, new List<string> { PlayerInventoryKey });
            PlayerInventory inventory;

            if (result.Data.Results.Count == 0)
            {
                await InitializeInventory(context, gameApiClient);
                inventory = new();
            }
            else
            {
                string? stringResult = result.Data.Results
                    .Select(item => item.Value?.ToString())
                    .Where(value => value != null)
                    .ToList()
                    .FirstOrDefault();

                if (stringResult == null) inventory = new();
                else
                {
                    inventory = JsonConvert.DeserializeObject<PlayerInventory>(stringResult) ?? new();
                }
            }

            return new()
            {
                { InventoryCurrencyKey, (int)inventory.Currency },
                { InventoryWoodKey, (int)inventory.Wood },
                { InventoryStoneKey, (int)inventory.Stone },
                { InventoryMetalKey, (int)inventory.Metal },
            };
        }
        catch (ApiException ex)
        {
            _logger.LogError("Failed to get data. Error: {Error}", ex.Message);
            throw new Exception($"Failed to get data for playerId {context.PlayerId}. Error: {ex.Message}");
        }
    }
}
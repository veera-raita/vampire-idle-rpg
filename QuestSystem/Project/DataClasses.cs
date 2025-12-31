using System;
using Newtonsoft.Json;

namespace QuestSystem;

public class Quest
{
    [JsonProperty("id")]
    public int ID { get; set; }

    [JsonProperty("hunters_name")]
    public string? HuntersName { get; set; }

    [JsonProperty("vampires_name")]
    public string? VampiresName { get; set; }

    [JsonProperty("duration_hours")]
    public int DurationHours { get; set; }

    [JsonProperty("duration_minutes")]
    public int DurationMinutes { get; set; }

    [JsonProperty("duration_seconds")]
    public int DurationSeconds { get; set; }

    [JsonProperty("reward_currency_min")]
    public int RewardCurrencyMin { get; set; }

    [JsonProperty("reward_currency_max")]
    public int RewardCurrencyMax { get; set; }

    [JsonProperty("reward_wood_min")]
    public int RewardWoodMin { get; set; }

    [JsonProperty("reward_wood_max")]
    public int RewardWoodMax { get; set; }

    [JsonProperty("reward_stone_min")]
    public int RewardStoneMin { get; set; }

    [JsonProperty("reward_stone_max")]
    public int RewardStoneMax { get; set; }

    [JsonProperty("reward_metal_min")]
    public int RewardMetalMin { get; set; }

    [JsonProperty("reward_metal_max")]
    public int RewardMetalMax { get; set; }
}

public class QuestData
{
    public QuestData()
    {

    }

    public QuestData(string questName, int currencyReward, int woodReward, int stoneReward, int metalReward,
        DateTime questStartTime, DateTime questFinishTime)
    {
        QuestName = questName;
        CurrencyReward = currencyReward;
        WoodReward = woodReward;
        StoneReward = stoneReward;
        MetalReward = metalReward;
        QuestStartTime = questStartTime;
        QuestFinishTime = questFinishTime;
    }

    [JsonProperty("quest-name")]
    public string? QuestName { get; set; }

    [JsonProperty("currency-reward")]
    public long CurrencyReward { get; set; }

    [JsonProperty("wood-reward")]
    public long WoodReward { get; set; }

    [JsonProperty("stone-reward")]
    public long StoneReward { get; set; }

    [JsonProperty("metal-reward")]
    public long MetalReward { get; set; }

    [JsonProperty("quest-start-time")]
    public DateTime QuestStartTime { get; set; }

    [JsonProperty("quest-finish-time")]
    public DateTime QuestFinishTime { get; set; }
}

public class PlayerInventory
{
    public PlayerInventory() { }

    public PlayerInventory(int currency, int wood, int stone, int metal)
    {
        Currency = currency;
        Wood = wood;
        Stone = stone;
        Metal = metal;
    }

    [JsonProperty("currency")]
    public long Currency { get; set; }

    [JsonProperty("wood")]
    public long Wood { get; set; }

    [JsonProperty("stone")]
    public long Stone { get; set; }

    [JsonProperty("metal")]
    public long Metal { get; set; }
}
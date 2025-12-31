using Newtonsoft.Json;

namespace CloudSaveModule;

public class PlayerInventory
{
    public PlayerInventory()
    {
        Currency = 0;
        Wood = 0;
        Stone = 0;
        Metal = 0;
    }

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
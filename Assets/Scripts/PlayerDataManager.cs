using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerDataManager : MonoSingleton<PlayerDataManager>
{
    public int PlayerLevel { get; private set; } = 1;
    public int PlayerXp { get; private set; } = 0;

    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI woodText;
    [SerializeField] private TextMeshProUGUI stoneText;
    [SerializeField] private TextMeshProUGUI metalText;

    private int _currency;
    private int _wood;
    private int _stone;
    private int _metal;

    public int Currency
    {
        get => _currency;

        private set
        {
            _currency = value;
            currencyText.text = _currency.ToString();
        }
    }

    public int Wood
    {
        get => _wood;

        private set
        {
            _wood = value;
            woodText.text = _wood.ToString();
        }
    }

    public int Stone
    {
        get => _stone;

        private set
        {
            _stone = value;
            stoneText.text = _stone.ToString();
        }
    }

    public int Metal
    {
        get => _metal;

        private set
        {
            _metal = value;
            metalText.text = _metal.ToString();
        }
    }

    private const string InventoryCurrencyKey = "currency";
    private const string InventoryWoodKey = "wood";
    private const string InventoryStoneKey = "stone";
    private const string InventoryMetalKey = "metal";

    public static event Action OnInitialLoadCompleted;

    private void Start()
    {
        CloudSaveManager.OnBindingsCreated += SetInventoryValues;
        QuestManager.OnQuestComplete += SetInventoryValues;
    }

    private async void SetInventoryValues()
    {
        Dictionary<string, int> inventoryDict = await CloudSaveManager.Instance.CloudSaveModuleBindings.GetInventory();
        Currency = inventoryDict[InventoryCurrencyKey];
        Wood = inventoryDict[InventoryWoodKey];
        Stone = inventoryDict[InventoryStoneKey];
        Metal = inventoryDict[InventoryMetalKey];
        OnInitialLoadCompleted?.Invoke();
    }

    public bool CanAffordBuild(BuildCost buildCost)
    {
        return Currency >= buildCost.CurrencyCost &&
            Wood >= buildCost.WoodCost &&
            Stone >= buildCost.StoneCost &&
            Metal >= buildCost.MetalCost;
    }
}
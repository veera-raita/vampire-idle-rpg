using System;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public int UpgradeLevel;
    public int MaxUpgradeLevel;
    [field: SerializeField] public string BuildingLevelKey { get; private set; }
    [field: SerializeField] public string BuildingDurationKey { get; private set; }
    public Duration[] upgradeDurations;
    public BuildCost[] upgradeCosts;
    public Sprite[] upgradeSprites;

    public bool IsDataValid
    {
        get => upgradeDurations.Length == MaxUpgradeLevel - 1 && upgradeSprites.Length == MaxUpgradeLevel - 1;
    }

    public bool IsMaxUpgraded
    {
        get => UpgradeLevel >= MaxUpgradeLevel;
    }

    public Duration GetUpgradeDuration()
    {
        return upgradeDurations[UpgradeLevel];
    }

    public BuildCost GetUpgradeCost()
    {
        return upgradeCosts[UpgradeLevel];
    }

    public Sprite GetCurrentSprite()
    {
        return upgradeSprites[UpgradeLevel];
    }
}
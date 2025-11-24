using System;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public int UpgradeLevel;
    public int MaxUpgradeLevel;
    public Duration[] upgradeDurations;
    public Sprite[] upgradeSprites;

    public bool IsDataValid()
    {
        return upgradeDurations.Length == MaxUpgradeLevel - 1 && upgradeSprites.Length == MaxUpgradeLevel - 1;
    }

    public bool IsMaxUpgraded()
    {
        return UpgradeLevel >= MaxUpgradeLevel;
    }

    public bool CanBeUpgraded()
    {
        return UpgradeLevel < MaxUpgradeLevel;
    }

    public Sprite GetCurrentSprite()
    {
        return upgradeSprites[UpgradeLevel];
    }
}
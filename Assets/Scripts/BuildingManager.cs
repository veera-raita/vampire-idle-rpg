using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{
    [Header ("Building Data")]
    [SerializeField] private BuildingData wallData;
    [SerializeField] private BuildingData throneData;
    [SerializeField] private BuildingData vaultData;

    [Header("Button References")]
    [SerializeField] private Button wallUpgradeButton;
    [SerializeField] private Button throneUpgradeButton;
    [SerializeField] private Button vaultUpgradeButton;

    [Header ("Object References")]
    [SerializeField] private GameObject errorPopupTemplate;
    public List<GameObject> ActiveErrorPopups { get; private set; }
    [SerializeField] private GameObject notificationPopupTemplate;
    public List<GameObject> ActiveNotifications { get; private set; }

    //Events related to data management
    public static event Action OnInitialLoadCompleted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AuthenticationService.Instance.SignedIn += TryFetchSavedData;

        // wallUpgradeButton.onClick.AddListener(() => StartBuild(wallData));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async void StartBuild(BuildingData buildingData)
    {
        if (buildingData.IsMaxUpgraded)
        {
            SetupNewNotification("Building already at maximum upgrade level!");
            return;
        }
        if (!PlayerDataManager.Instance.CanAffordBuild(buildingData.GetUpgradeCost()))
        {
            SetupNewNotification("Can't afford upgrade!");
            return;
        }

        try
        {
            
        }
        catch(CloudCodeException cce)
        {
            SetupNewError(cce.Message.FirstCharacterToUpper() + ".");
        }
    }

    private async void TryFetchSavedData()
    {
        double startTime = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        Dictionary<string, Item> retrievedData;

        try
        {
            retrievedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>(){
                wallData.BuildingLevelKey,
                throneData.BuildingLevelKey, 
                vaultData.BuildingLevelKey});
            Debug.Log($"TryFetchSavedData took {(int)(new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() - startTime)}ms");

            if (retrievedData.TryGetValue(wallData.BuildingLevelKey, out var wallValue))
            {
                
            }
        }
        catch(CloudSaveException cse)
        {
            SetupNewError(cse.Message.FirstCharacterToUpper() + ".");
        }
    }

    private void SetupNewNotification(string notificationText)
    {
        GameObject newNotification = Instantiate(notificationPopupTemplate);
        if (newNotification.TryGetComponent(out TextMeshProUGUI text) && newNotification.TryGetComponent(out Button button))
        {
            text.text = notificationText;
            button.onClick.AddListener(() =>
            {
                if (ActiveNotifications.Contains(newNotification)) ActiveNotifications.Remove(newNotification);
                Destroy(newNotification);
            });
            ActiveErrorPopups.Add(newNotification);
        }
        else
        {
            Debug.LogError("Error occured while trying to display notification. Unsure how something got this fucked up.");
            Destroy(newNotification);
        }
    }

    private void SetupNewError(string errorText)
    {
        GameObject newError = Instantiate(errorPopupTemplate);
        if (newError.TryGetComponent(out TextMeshProUGUI text) && newError.TryGetComponent(out Button button))
        {
            text.text = errorText;
            button.onClick.AddListener(() =>
            {
                if (ActiveErrorPopups.Contains(newError)) ActiveErrorPopups.Remove(newError);
                Destroy(newError);
            });
            ActiveErrorPopups.Add(newError);
        }
        else
        {
            Debug.LogError("Error occured while trying to display error message. Unsure how something got this fucked up.");
            Destroy(newError);
        }
    }
}

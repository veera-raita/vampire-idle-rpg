using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class NetworkManager : MonoSingleton<NetworkManager>
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject waitPanel;
    [SerializeField] private GameObject networkNotRespondingPanel;

    public bool IsInitialized { get; private set; } = false;
    private const int maxInitializeTries = 5;
    private const float timeUntilRetry = 5f;
    private const float maxErrorAwaitTime = 15f;
    private const float initializeStateCheckDelay = 0.1f;
    private WaitForSeconds initializeStateWait;

    public static event Action OnInitialized;
    public static event Action OnAllLoadsCompleted;

    //Initial load related variables
    private bool buildingDataLoaded = false;
    private bool characterDataLoaded = false;
    private bool playerDataLoaded = false;
    private int _finishedLoads;
    private int FinishedLoads
    {
        get => _finishedLoads;
        set
        {
            _finishedLoads = Math.Clamp(value, 0, totalLoads);
            if (_finishedLoads == totalLoads) OnAllLoadsCompleted?.Invoke();
        }
    }
    private readonly int totalLoads = 3;

    private async void Awake()
    {
        startPanel.SetActive(true);
        await StartCore();
    }

    private void Start()
    {
        initializeStateWait = new(initializeStateCheckDelay);

        BuildingManager.OnInitialLoadCompleted += () =>
        {
            if (!buildingDataLoaded)
            {
                buildingDataLoaded = true;
                FinishedLoads++;
            }
        };

        PlayerDataManager.OnInitialLoadCompleted += () =>
        {
            if (!playerDataLoaded)
            {
                playerDataLoaded = true;
                FinishedLoads++;
            }
        };
    }

    private async Awaitable StartCore()
    {
        int triesCount = 0;
        double startTime = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();

        while (!IsInitialized && triesCount < maxInitializeTries)
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log($"Initialization took {(int)(new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds() - startTime)}ms");
                IsInitialized = true;
                Debug.Log("Successfully initialized UnityServices Core.");
                OnInitialized?.Invoke();
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to initialize Core, retrying in {timeUntilRetry} seconds.\n{e}");
                triesCount++;
                await Task.Delay(TimeSpan.FromSeconds(timeUntilRetry));
            }
        }

        if (IsInitialized)
        {
            if (waitPanel.activeSelf) waitPanel.SetActive(false);
            if (startPanel.activeSelf) startPanel.SetActive(false);
        
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();
        }
        else if (triesCount >= maxInitializeTries)
        {
            networkNotRespondingPanel.SetActive(true);
        }
    }

    public async void ReinitializeCore()
    {
        var servicesState = UnityServices.State;

        switch (servicesState)
        {
            case ServicesInitializationState.Initializing:
                IsInitialized = false;
                StartCoroutine(InitializeAwaitCoroutine());
                break;
            case ServicesInitializationState.Initialized:
                //Already initialized, do nothing.
                break;
            case ServicesInitializationState.Uninitialized:
                IsInitialized = false;
                waitPanel.SetActive(true);
                await StartCore();
                break;
        }
    }

    private IEnumerator InitializeAwaitCoroutine()
    {
        float timer = 0f;
        bool initializedSuccessfully = false;
        waitPanel.SetActive(true);

        while (true)
        {
            yield return initializeStateWait;
            timer += initializeStateCheckDelay;
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                waitPanel.SetActive(false);
                initializedSuccessfully = true;
                break;
            }
            if (timer >= maxErrorAwaitTime) break;
        }

        if (!initializedSuccessfully)
        {
            networkNotRespondingPanel.SetActive(true);
        }
    }

    public static void LogTakenTime(double startTime, string taskName)
    {
        Debug.Log($"{taskName} took {(int)((Time.unscaledTimeAsDouble - startTime) * 1000)}ms");
    }
}
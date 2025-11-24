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

    private void Awake()
    {
        StartCore();
        startPanel.SetActive(true);
    }

    private void Start()
    {
        initializeStateWait = new(initializeStateCheckDelay);
    }

    public async void StartCore()
    {
        int triesCount = 0;
        while (!IsInitialized && triesCount < maxInitializeTries)
        {
            try
            {
                await UnityServices.InitializeAsync();
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

        if (triesCount >= maxInitializeTries)
        {
            networkNotRespondingPanel.SetActive(true);
        }
        else
        {
            if (waitPanel.activeSelf) waitPanel.SetActive(false);
            if (startPanel.activeSelf) startPanel.SetActive(false);
        }
        
        if (AuthenticationService.Instance.IsSignedIn)
            AuthenticationService.Instance.SignOut();
    }

    public void ReinitializeCore()
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
                StartCore();
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
}
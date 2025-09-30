using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using System;
using TMPro;
using Unity.Services.CloudCode;

public class Testing : MonoBehaviour
{
    private bool initialized = false;
    private bool signedIn = false;
    [SerializeField] private GameObject signInButton;
    [SerializeField] private GameObject saveTestButton;
    [SerializeField] private GameObject serverTimeButton;
    [SerializeField] private TextMeshProUGUI serverTimeText;

    public async void StartCore()
    {
        try
        {
            await UnityServices.InitializeAsync();
            signInButton.SetActive(true);
            initialized = true;
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to initialize Core:\n{e}");
        }
    }

    public async void SignInAnon()
    {
        if (!initialized) return;

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            saveTestButton.SetActive(true);
            serverTimeButton.SetActive(true);
            signedIn = true;
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to sign in:\n{e}");
        }
    }

    public async void SaveHelloWorld()
    {
        if (!signedIn) return;

        var data = new Dictionary<string, object> { { "MySaveKey", "Hello World" } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    public async void CheckServerTime()
    {
        CloudCodeTimeResponse response = null;

        if (!signedIn) return;

        try
        {
            response = await CloudCodeService.Instance.CallEndpointAsync<CloudCodeTimeResponse>("TimeTest");
        }
        catch (Exception e)
        {
            Debug.Log($"Time check failed:\n{e}");
        }

        if (response != null)
        {
            serverTimeText.text = string.Concat("Current Server time is:\n", response.formattedDate);
        }
    }

    public class CloudCodeTimeResponse
    {
        public UInt64 timestamp;
        public string formattedDate;
    }
}
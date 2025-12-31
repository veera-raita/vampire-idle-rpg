using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudSave;
using UnityEngine;

public class CloudSaveManager : MonoSingleton<CloudSaveManager>
{
    public CloudSaveSdkBindings CloudSaveModuleBindings { get; private set; }

    [SerializeField] private TextMeshProUGUI serverTimeText;

    public static event Action OnBindingsCreated;

    private void Start()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            CloudSaveModuleBindings = new(CloudCodeService.Instance);
            OnBindingsCreated?.Invoke();
        };
    }

    public async void SaveHelloWorld()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var data = new Dictionary<string, object> { { "MySaveKey", "Hello World" } };
        double startTime = Time.unscaledTimeAsDouble;
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        NetworkManager.LogTakenTime(startTime, "Signup");
    }

    public async void SaveTimes()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;
        CloudCodeTimeResponse response = null;

        try
        {
            double getTimeStartTime = Time.unscaledTimeAsDouble;
            response = await CloudCodeService.Instance.CallEndpointAsync<CloudCodeTimeResponse>("TimeTest");
            NetworkManager.LogTakenTime(getTimeStartTime, "Fetching server time");
        }
        catch (Exception e)
        {
            Debug.Log($"Time check failed:\n{e}");
        }

        if (response == null) return;

        TimeSpan timeSpan = new(0, 7, 30, 0);
        DateTime dt = DateTime.Parse(response.formattedDate);

        var times = new Dictionary<string, object> {
            { "StartTimeKey", dt },
            { "EndTimeKey", dt + timeSpan }  };

        try
        {
            double timeSaveStartTime = Time.unscaledTimeAsDouble;
            await CloudSaveService.Instance.Data.Player.SaveAsync(times);
            NetworkManager.LogTakenTime(timeSaveStartTime, "Saving times");
            Debug.Log($"Time save successful");
        }
        catch (Exception e)
        {
            Debug.Log($"Time save failed:\n{e}");
        }
    }

    public async void CheckServerTime()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;
        CloudCodeTimeResponse response = null;

        try
        {
            double getTimeStartTime = Time.unscaledTimeAsDouble;
            response = await CloudCodeService.Instance.CallEndpointAsync<CloudCodeTimeResponse>("TimeTest");
            NetworkManager.LogTakenTime(getTimeStartTime, "Fetching server time");
        }
        catch (Exception e)
        {
            Debug.Log($"Time check failed:\n{e}");
        }

        if (response != null)
        {
            TimeSpan timeSpan = new(0, 7, 30, 0);
            DateTime dt = DateTime.Parse(response.formattedDate);
            serverTimeText.text = string.Concat("Current Server time is:\n", dt,
            "\n", "Completed at: ", dt + timeSpan);
        }
    }

    public class CloudCodeTimeResponse
    {
        public UInt64 timestamp;
        public string formattedDate;
    }
}

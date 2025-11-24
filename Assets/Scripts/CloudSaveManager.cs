using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudSave;
using UnityEngine;

public class CloudSaveManager : MonoSingleton<CloudSaveManager>
{
    
    [SerializeField] private TextMeshProUGUI serverTimeText;

    public async void SaveHelloWorld()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;

        var data = new Dictionary<string, object> { { "MySaveKey", "Hello World" } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    public async void SaveTimes()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;
        CloudCodeTimeResponse response = null;

        try
        {
            response = await CloudCodeService.Instance.CallEndpointAsync<CloudCodeTimeResponse>("TimeTest");
        }
        catch (Exception e)
        {
            Debug.Log($"Time check failed:\n{e}");
        }

        if (response == null) return;

        TimeSpan timeSpan = new(0, 7, 30, 0);
        DateTime dt = DateTime.Parse(response.formattedDate);

        var startTime = new Dictionary<string, object> { { "StartTimeKey", dt } };
        var endTime = new Dictionary<string, object> { { "EndTimeKey", dt + timeSpan } };

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(startTime);
            await CloudSaveService.Instance.Data.Player.SaveAsync(endTime);
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
            response = await CloudCodeService.Instance.CallEndpointAsync<CloudCodeTimeResponse>("TimeTest");
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

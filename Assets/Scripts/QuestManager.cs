using System;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using UnityEngine;

public class QuestManager : MonoSingleton<QuestManager>
{
    public QuestControllerBindings questControllerBindings;

    [SerializeField] private GameObject questCompletePopup;
    [SerializeField] private TextMeshProUGUI questCompleteText;
    [SerializeField] private TextMeshProUGUI questCompleteRewards;
    [SerializeField] private TextMeshProUGUI questStatusText;
    [SerializeField] private TextMeshProUGUI questTimerText;
    private TimeSpan questTimeLeft;
    private float secondsTimer = 0;

    private bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;

    private event Action OnQuestTimerDone;
    public static event Action OnQuestComplete;

    private void Start()
    {
        AuthenticationService.Instance.SignedIn += () => questControllerBindings = new(CloudCodeService.Instance);
        OnQuestTimerDone += GetQuestStatus;
    }

    private void Update()
    {
        secondsTimer += Time.deltaTime;

        if (secondsTimer >= 1f)
        {
            secondsTimer -= 1f;
            if (questTimeLeft != null && questTimeLeft > TimeSpan.Zero)
            {
                questTimeLeft = questTimeLeft.Add(new(0, 0, -1));
                string timeAsString = questTimeLeft.ToString("g");
                int dotIndex = timeAsString.IndexOf('.');
                if (dotIndex != -1) questTimerText.text = timeAsString[..dotIndex];
                else questTimerText.text = timeAsString;
            }
            else if (questTimeLeft != null)
            {
                OnQuestTimerDone?.Invoke();
            }
        }
    }

    public async void StartRandomQuest()
    {
        if (!IsSignedIn) return;
        string result = await questControllerBindings.AssignQuest();
        Debug.Log(result);
    }

    public async void GetQuestStatus()
    {
        if (!IsSignedIn) return;
        string response = await questControllerBindings.GetQuestStatus();
        int timeIndex = response.IndexOf('@');
        int rewardIndex = response.IndexOf('$');

        if (timeIndex != -1)
        {
            if (TimeSpan.TryParse(response[(timeIndex + 1)..], out TimeSpan timeLeft))
            {
                questTimeLeft = timeLeft;
                string timeAsString = questTimeLeft.ToString("g");
                int dotIndex = timeAsString.IndexOf('.');
                if (dotIndex != -1) questTimerText.text = timeAsString[..dotIndex];
                else questTimerText.text = timeAsString;
            }
            questStatusText.text = response[..timeIndex];
        }
        else if (rewardIndex != -1)
        {
            questCompleteText.text = response[..rewardIndex];
            questCompleteRewards.text = response[(rewardIndex + 1)..];
            questCompletePopup.SetActive(true);
            OnQuestComplete?.Invoke();
        }
        else questStatusText.text = response;
    }
}

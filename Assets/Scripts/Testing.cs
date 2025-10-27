using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using System;
using TMPro;
using Unity.Services.CloudCode;
using System.Threading.Tasks;

public class Testing : MonoBehaviour
{
    private const int maxInitializeTries = 5;
    private const float timeUntilRetry = 5f;
    private bool initialized = false;
    private bool signedIn = false;

    [Header("Login elements")]
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject registerScreen;
    [SerializeField] private TMP_InputField signUpUsernameField;
    [SerializeField] private TMP_InputField signUpPasswordField;
    [SerializeField] private TMP_InputField loginUsernameField;
    [SerializeField] private TMP_InputField loginPasswordField;
    [SerializeField] private GameObject saveTestButton;
    [SerializeField] private GameObject saveTimeTestButton;
    [SerializeField] private GameObject serverTimeButton;
    [SerializeField] private TextMeshProUGUI serverTimeText;
    [SerializeField] private GameObject usernameLengthWarning;
    [SerializeField] private GameObject usernameSymbolWarning;
    [SerializeField] private GameObject passwordLengthWarning;
    [SerializeField] private GameObject passwordSymbolWarning;
    [SerializeField] private GameObject registerSuccessfulPopup;

    private void Awake()
    {
        StartCore();
    }

    public async void StartCore()
    {
        int triesCount = 0;
        while (!initialized && triesCount < maxInitializeTries)
        {
            try
            {
                await UnityServices.InitializeAsync();
                initialized = true;
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to initialize Core, retrying in {timeUntilRetry} seconds.\n{e}");
                triesCount++;
                await Task.Delay(TimeSpan.FromSeconds(timeUntilRetry));
            }
        }
        AuthenticationService.Instance.SignOut();
    }

    public async void UserSignUp()
    {
        if (!initialized) return;
        if (!SignUpFieldsValid()) return;

        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(signUpUsernameField.text, signUpPasswordField.text);
            registerSuccessfulPopup.SetActive(true);
            Debug.Log("Signed up succesfully.");
        }
        catch (AuthenticationException ae)
        {
            Debug.LogException(ae);
        }
        catch (RequestFailedException rfe)
        {
            Debug.LogException(rfe);
        }
    }

    public async void UserSignIn()
    {
        if (!initialized) return;
        // if (!SignUpFieldsValid()) return;

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(loginUsernameField.text, loginPasswordField.text);
            loginScreen.SetActive(false);
            Debug.Log("Signed in succesfully.");
        }
        catch (AuthenticationException ae)
        {
            Debug.LogException(ae);
        }
        catch (RequestFailedException rfe)
        {
            Debug.LogException(rfe);
        }
    }

    public async void SignInAnon()
    {
        if (!initialized) return;

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            saveTestButton.SetActive(true);
            saveTimeTestButton.SetActive(true);
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

    public async void SaveTimes()
    {
        if (!signedIn) return;
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
            Duration duration = new(0, 7, 30);
            TimeSpan timeSpan = new(duration.days, duration.hours, duration.minutes, 0);
            DateTime dt = DateTime.Parse(response.formattedDate);
            serverTimeText.text = string.Concat("Current Server time is:\n", dt,
            "\n", "Completed at: ", dt + timeSpan);
        }
    }

    private bool SignUpFieldsValid()
    {
        bool usernameValid = false;
        bool passwordValid = false;
        if (signUpUsernameField.text.Length < 3 || signUpUsernameField.text.Length > 20)
        {
            usernameLengthWarning.SetActive(true);
        }
        else if (!IsUsernameValid(signUpUsernameField.text))
        {
            usernameLengthWarning.SetActive(true);
        }
        else
        {
            usernameValid = true;
            Debug.Log("Username OK!");
        }

        if (signUpPasswordField.text.Length < 8 || signUpPasswordField.text.Length > 30)
        {
            passwordLengthWarning.SetActive(true);
        }
        else if (!IsPasswordValid(signUpPasswordField.text))
        {
            passwordSymbolWarning.SetActive(true);
        }
        else
        {
            passwordValid = true;
            Debug.Log("Password OK!");
        }

        return usernameValid && passwordValid;
    }

    private bool IsPasswordValid(string s)
    {
        bool hasUppercaseLetter = false;
        bool hasLowercaseLetter = false;
        bool hasNumber = false;
        bool hasSymbol = false;

        foreach (char c in s)
        {
            if (char.IsUpper(c)) hasUppercaseLetter = true;
            else if (char.IsLower(c)) hasLowercaseLetter = true;
            else if (char.IsNumber(c)) hasNumber = true;
            else if (c == '.' || c == '-' || c == '_' || c == '@' || c == '!' || c == '?' || c == '€' || c == '$' || c == '£' ||
            c == '(' || c == ')' || c == '{' || c == '}' || c == '/' || c == '\\' || c == '[' || c == ']' || c == '=' || c == '+' ||
            c == '\'' || c == '*' || c == ',' || c == ';' || c == ':' || c == '<' || c == '>')
            hasSymbol = true;
        }

        return hasUppercaseLetter && hasLowercaseLetter && hasNumber && hasSymbol;
    }

    private bool IsUsernameValid(string s)
    {
        bool isValid = true;
        foreach (char c in s)
        {
            if (!IsLetterValid(c))
            {
                isValid = false;
                break;
            }
        }
        return isValid;
    }

    private bool IsLetterValid(char c)
    {
        bool isValid = false;

        if (char.IsLetterOrDigit(c)) isValid = true;
        else if (c == '.' || c == '-' || c == '_' || c == '@') isValid = true;

        return isValid;
    }

    public class CloudCodeTimeResponse
    {
        public UInt64 timestamp;
        public string formattedDate;
    }

    public class Duration
    {
        public int days;
        public int hours;
        public int minutes;

        public Duration(int _days, int _hours, int _minutes)
        {
            days = _days;
            hours = _hours;
            minutes = _minutes;
        }
    }
}
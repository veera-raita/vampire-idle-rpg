using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEngine;

public class AuthenticationManager : MonoSingleton<AuthenticationManager>
{
    public BuildingData buildingData;
    
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
    [SerializeField] private GameObject usernameLengthWarning;
    [SerializeField] private GameObject usernameSymbolWarning;
    [SerializeField] private GameObject passwordLengthWarning;
    [SerializeField] private GameObject passwordSymbolWarning;
    [SerializeField] private GameObject miscRegWarningObj;
    [SerializeField] private TextMeshProUGUI miscRegWarningText;
    [SerializeField] private GameObject miscLogWarningObj;
    [SerializeField] private TextMeshProUGUI miscLogWarningText;
    [SerializeField] private GameObject registerSuccessfulPopup;
    private const int minUsernameLength = 3;
    private const int maxUsernameLength = 20;
    private const int minPasswordLength = 8;
    private const int maxPasswordLength = 30;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loginScreen.SetActive(false);
        registerScreen.SetActive(false);

        if (NetworkManager.Instance.IsInitialized) loginScreen.SetActive(true);
        else NetworkManager.OnInitialized += ActivateLoginScreen;
    }
    
    private void ActivateLoginScreen()
    {
        loginScreen.SetActive(true);
    }

    public async void UserSignUp()
    {
        if (!NetworkManager.Instance.IsInitialized) return;
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
            miscRegWarningObj.SetActive(true);
            miscRegWarningText.text = ae.Message;
            miscRegWarningText.text = string.Concat(miscRegWarningText.text.FirstCharacterToUpper(), ".");
        }
        catch (RequestFailedException rfe)
        {
            Debug.LogException(rfe);
        }
    }

    public async void UserSignIn()
    {
        if (!NetworkManager.Instance.IsInitialized) return;

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(loginUsernameField.text, loginPasswordField.text);
            loginScreen.SetActive(false);
            Debug.Log("Signed in succesfully.");
        }
        catch (AuthenticationException ae)
        {
            Debug.LogException(ae);
            miscLogWarningObj.SetActive(true);
            miscLogWarningText.text = ae.Message;
            miscLogWarningText.text = string.Concat(miscLogWarningText.text.FirstCharacterToUpper(), ".");
        }
        catch (RequestFailedException rfe)
        {
            Debug.LogException(rfe);
        }
    }
    
    #region Login and Signup Validation

    private bool SignUpFieldsValid()
    {
        bool usernameValid = true;
        bool passwordValid = true;

        if (signUpUsernameField.text.Length < minUsernameLength || signUpUsernameField.text.Length > maxUsernameLength)
        {
            usernameLengthWarning.SetActive(true);
            usernameValid = false;
        }
        if (!IsUsernameValid(signUpUsernameField.text))
        {
            usernameLengthWarning.SetActive(true);
            usernameValid = false;
        }

        if (signUpPasswordField.text.Length < minPasswordLength || signUpPasswordField.text.Length > maxPasswordLength)
        {
            passwordLengthWarning.SetActive(true);
            passwordValid = false;
        }
        if (!IsPasswordValid(signUpPasswordField.text))
        {
            passwordSymbolWarning.SetActive(true);
            passwordValid = false;
        }

        if (passwordValid) Debug.Log("Password OK!");
        if (usernameValid) Debug.Log("Username OK!");

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

    #endregion
}

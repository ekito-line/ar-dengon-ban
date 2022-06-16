using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{

    public Firebase.Auth.FirebaseAuth auth;
    public static Firebase.Auth.FirebaseUser loggedinUser;

    protected Firebase.Auth.FirebaseAuth otherAuth;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
        new Dictionary<string, Firebase.Auth.FirebaseUser>();

    [SerializeField] private InputField emailField;
    [SerializeField] private InputField passwordField;
    public Text logText;

    protected string email = "";
    protected string password = "";

    // Whether to sign in / link or reauthentication *and* fetch user profile data.
    protected bool signInAndFetchProfile = false;

    // Flag set when a token is being fetched.  This is used to avoid printing the token
    // in IdTokenChanged() when the user presses the get token button.
    private bool fetchingToken = false;

    // Options used to setup secondary authentication object.
    /* private Firebase.AppOptions otherAuthOptions = new Firebase.AppOptions {
        ApiKey = "",
        AppId = "",
        ProjectId = ""
    }; */

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    public virtual void Awake()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        dependencyStatus = task.Result;
        if (dependencyStatus == Firebase.DependencyStatus.Available) {
          InitializeFirebase();
        } else {
          Debug.LogError(
            "Could not resolve all Firebase dependencies: " + dependencyStatus);
        }
      });
    }

    // Handle initialization of the necessary firebase modules:
    protected void InitializeFirebase() {
      Debug.Log("Setting up Firebase Auth");
      auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
      auth.StateChanged += AuthStateChanged;
      auth.IdTokenChanged += IdTokenChanged;
      // Specify valid options to construct a secondary authentication object.
      /* if (otherAuthOptions != null &&
          !(String.IsNullOrEmpty(otherAuthOptions.ApiKey) ||
            String.IsNullOrEmpty(otherAuthOptions.AppId) ||
            String.IsNullOrEmpty(otherAuthOptions.ProjectId))) {
        try {
          otherAuth = Firebase.Auth.FirebaseAuth.GetAuth(Firebase.FirebaseApp.Create(
            otherAuthOptions, "Secondary"));
          otherAuth.StateChanged += AuthStateChanged;
          otherAuth.IdTokenChanged += IdTokenChanged;
        } catch (Exception) {
          Debug.Log("ERROR: Failed to initialize secondary authentication object.");
        }
      } */
      AuthStateChanged(this, null);
    }

    public virtual void Start()
    {
        
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
      }
    }

    public void OnLoginButtonClicked()
    {
        logText.text = "";
        email = emailField.text;
        password = passwordField.text;
        SigninWithEmailAsync();
    }

    void OnDestroy() {
      if (auth != null) {
        auth.StateChanged -= AuthStateChanged;
        auth.IdTokenChanged -= IdTokenChanged;
        auth = null;
      }
      if (otherAuth != null) {
        otherAuth.StateChanged -= AuthStateChanged;
        otherAuth.IdTokenChanged -= IdTokenChanged;
        otherAuth = null;
      }
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs) {
      Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
      Firebase.Auth.FirebaseUser user = null;
      if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
      if (senderAuth == auth && senderAuth.CurrentUser != user) {
        bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
        if (!signedIn && user != null) {
          Debug.Log("Signed out " + user.UserId);
        }
        user = senderAuth.CurrentUser;
        userByAuth[senderAuth.App.Name] = user;
        if (signedIn) {
          Debug.Log("AuthStateChanged Signed in " + user.UserId);
          // displayName = user.DisplayName ?? "";
          // DisplayDetailedUserInfo(user, 1);
        }
      }
    }

    // Track ID token changes.
    void IdTokenChanged(object sender, System.EventArgs eventArgs) {
      Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
      if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken) {
        senderAuth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(
          task => Debug.Log(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
      }
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation) {
      bool complete = false;
      if (task.IsCanceled) {
        Debug.Log(operation + " canceled.");
        logText.text = operation + "はキャンセルされました。";
      } else if (task.IsFaulted) {
        Debug.Log(operation + " encounted an error.");
        logText.text = operation + "できません。";
        foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
          string authErrorCode = "";
          Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
          if (firebaseEx != null) {
            authErrorCode = String.Format("AuthError.{0}: ",
              ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
          }
          Debug.Log(authErrorCode + exception.ToString());
        }
      } else if (task.IsCompleted) {
        Debug.Log(operation + " completed");
        complete = true;
      }
      return complete;
    }

    // Sign-in with an email and password.
    public Task SigninWithEmailAsync() {
      Debug.Log(String.Format("Attempting to sign in as {0}...", email));
      if (signInAndFetchProfile) {
        return auth.SignInAndRetrieveDataWithCredentialAsync(
          Firebase.Auth.EmailAuthProvider.GetCredential(email, password)).ContinueWithOnMainThread(
            HandleSignInWithSignInResult);
      } else {
        return auth.SignInWithEmailAndPasswordAsync(email, password)
          .ContinueWithOnMainThread(HandleSignInWithUser);
      }
    }

    // Called when a sign-in without fetching profile data completes.
    void HandleSignInWithUser(Task<Firebase.Auth.FirebaseUser> task) {
      if (LogTaskCompletion(task, "サインイン")) {
        Debug.Log(String.Format("{0} signed in", task.Result.DisplayName));

        loggedinUser = task.Result;
        SceneManager.LoadScene("HomeAndViewScene");
      }
    }

    // Called when a sign-in with profile data completes.
    void HandleSignInWithSignInResult(Task<Firebase.Auth.SignInResult> task) {
      if (LogTaskCompletion(task, "サインイン")) {
        // DisplaySignInResult(task.Result, 1);
      }
    }

    public static Firebase.Auth.FirebaseUser getUser()
    {
      return loggedinUser;
    }

    public static void registerUser(Firebase.Auth.FirebaseUser newUser)
    {
      loggedinUser = newUser;
    }

}

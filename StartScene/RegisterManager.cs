using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RegisterManager : MonoBehaviour
{

    public Firebase.Auth.FirebaseAuth auth;
    protected Firebase.Auth.FirebaseAuth otherAuth;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
        new Dictionary<string, Firebase.Auth.FirebaseUser>();
    
    public InputField displayNameField;
    public InputField emailField;
    public InputField passwordField;
    public InputField rePasswordField;
    public Text logText;

    protected string displayName = "";
    protected string email = "";
    protected string password = "";
    protected string rePassword = "";

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    public virtual void Start()
    {

    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
      }
    }

    public void OnRegisterButtonClicked()
    {
        displayName = displayNameField.text;
        email = emailField.text;
        password = passwordField.text;
        rePassword = rePasswordField.text;

        if ((password == "") || (rePassword == "") || (password != rePassword))
        {
            Debug.Log("Password is not correct.");
            logText.text = "パスワードが未入力、または一致していません。";
        }
        else
        {
            logText.text = "";
            CreateUserWithEmailAsync();
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

    // Create a user with the email and password.
    public Task CreateUserWithEmailAsync() {
      Debug.Log(String.Format("Attempting to create user {0}...", email));

      // This passes the current displayName through to HandleCreateUserAsync
      // so that it can be passed to UpdateUserProfile().  displayName will be
      // reset by AuthStateChanged() when the new user is created and signed in.
      string newDisplayName = displayName;
      return auth.CreateUserWithEmailAndPasswordAsync(email, password)
        .ContinueWithOnMainThread((task) => {
          if (LogTaskCompletion(task, "ユーザー登録")) {
            // var user = task.Result;
            // DisplayDetailedUserInfo(user, 1);
            return UpdateUserProfileAsync(newDisplayName: newDisplayName);
          }
          return task;
        }).Unwrap();
    }

    // Update the user's display name with the currently selected display name.
    public Task UpdateUserProfileAsync(string newDisplayName = null) {
      if (auth.CurrentUser == null) {
        Debug.Log("Not signed in, unable to update user profile");
        return Task.FromResult(0);
      }
      displayName = newDisplayName ?? displayName;
      Debug.Log("Updating user profile");
      return auth.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile {
        DisplayName = displayName,
        // PhotoUrl = auth.CurrentUser.PhotoUrl,
      }).ContinueWithOnMainThread(task => {
        if (LogTaskCompletion(task, "User profile")) {
          // DisplayDetailedUserInfo(auth.CurrentUser, 1);

          LoginManager.registerUser(auth.CurrentUser);
          SceneManager.LoadScene("HomeAndViewScene");
        }
      });
    }


}

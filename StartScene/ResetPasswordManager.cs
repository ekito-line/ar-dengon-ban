using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResetPasswordManager : MonoBehaviour
{

    public Firebase.Auth.FirebaseAuth auth;

    public InputField emailField;
    public Text logText;

    protected string email = "";

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    public virtual void Start()
    {

    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
      }
    }

    public void OnResetPasswordButtonClicked()
    {
        email = emailField.text;
        if (email == "")
        {
            logText.text = "メールアドレスが入力されていません。";
        }
        else
        {
            logText.text = "";
            SendPasswordResetEmail();
        }
    }

    // Send a password reset email to the current email address.
    protected void SendPasswordResetEmail() {
      auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread((authTask) => {
        if (LogTaskCompletion(authTask, "パスワード再設定用メールの送信")) {
          Debug.Log("Password reset email sent to " + email);
          logText.text = "パスワード再設定用メールが送信されました。";
        }
      });
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
        logText.text = operation + "ができません。";
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
}

using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BeforeLoginManager : MonoBehaviour
{
    public Firebase.Auth.FirebaseAuth auth;
    public GameObject loginButton;
    public Text logText;

    void Start()
    {
   
    }

    // Exit if escape (or back, on mobile) is pressed.
    void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
      }
    }

    public void OnToResetPasswordButtonClicked()
    {
        logText.text = "";
        auth = loginButton.GetComponent<LoginManager>().auth;  
        SceneManager.sceneLoaded += PassAuthToResetPassword;
        SceneManager.LoadScene("ResetPassword_Scene");
    }

    public void OnToRegisterButtonClicked()
    {
        logText.text = "";
        auth = loginButton.GetComponent<LoginManager>().auth; 
        SceneManager.sceneLoaded += PassAuthToRegister;
        SceneManager.LoadScene("Register_Scene");
    }

    private void PassAuthToResetPassword(Scene next, LoadSceneMode mode)
    {  
        var resetPasswordManager = GameObject.FindWithTag("ResetPasswordButton").GetComponent<ResetPasswordManager>();
        resetPasswordManager.auth = auth;
        SceneManager.sceneLoaded -= PassAuthToResetPassword;
    }

    private void PassAuthToRegister(Scene next, LoadSceneMode mode)
    {  
        var registerManager = GameObject.FindWithTag("RegisterButton").GetComponent<RegisterManager>();
        registerManager.auth = auth;
        SceneManager.sceneLoaded -= PassAuthToRegister;
    }

}

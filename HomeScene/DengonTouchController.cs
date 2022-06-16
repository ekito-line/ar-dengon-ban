using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DengonTouchController : MonoBehaviour
{
    //public GameObject viewPanel;
    public GameObject viewPanelControllerObject;
    ViewPanelController viewPanelController;
    private GameObject viewPanel;
    // Start is called before the first frame update
    //private int shown = 0;
    void Start()
    {
        //viewPanelController = viewPanelControllerObject.GetComponent<ViewPanelController>();
        viewPanelController = GameObject.Find("ViewPanelController").GetComponent<ViewPanelController>();
        // viewPanelControllerObject = GameObject.Find("ViewPanelController");
        // viewPanelController = viewPanelControllerObject.GetComponent<ViewPanelController>();
        viewPanel = viewPanelController.viewPanel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClickAct()
    {
        Debug.Log(transform.name + " clicked");
        viewPanel.SetActive(true); //ViewPanelを表示
        viewPanelController.isPanellShown = true;
        viewPanelController.documentName = transform.name; //objectの名前をdocumentNameに格納
        viewPanelController.ShowMessage(); 
        viewPanelController.LoadComment();

        // ViewPanel.transform.position -= moveDegree;
        // ShowPanel();
    }
}

using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeManager : MonoBehaviour
{

    public float getLocationAndTimeInterval = 30;
    public float displayInterval = 10;

    // 緯度36度における緯度経度からメートルへの変換
    public double latitudeToMeter = 110959.0097f;
    public double longitudeToMeter = 90163.2924f;

    public GameObject object0;
    public GameObject object1;
    public GameObject object2;

    private double currentLatitude;
    private double currentLongitude;
    // private float currentAltitude;

    // private double aprxCurrentLatitude;
    // private double aprxCurrentLongitude;
    private int aprxCurrentLatitude = 0;
    private int aprxCurrentLongitude = 0;

    private DateTime currentTime;

    private List<string> existingDengonList = new List<string>();

    private GameObject dengonObject;

    private Firebase.Auth.FirebaseUser user;

    void Awake()
    {
        user = LoginManager.getUser();
        Debug.Log(user.UserId);
        Debug.Log(user.Email);
        Debug.Log(user.DisplayName);

        StartCoroutine(StartService());

    }

    void Start()
    {
        StartCoroutine(DisplayService());
    }

    void Update()
    {

    }

    private IEnumerator StartService()
    {
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS not enabled");
            yield break;
        }

        // 端末の磁気センサー有効化
        Input.compass.enabled = true;

        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }

        while (true)
        {
            // 現在位置と時刻を一定間隔で取得
            currentLatitude = Input.location.lastData.latitude;
            currentLongitude = Input.location.lastData.longitude;
            // currentAltitude = Input.location.lastData.altitude;

            currentTime = DateTime.Now;

            yield return new WaitForSeconds(getLocationAndTimeInterval);
        }

    }

    private IEnumerator DisplayService()
    {
        while (true)
        {
            DisplayDengons();
            yield return new WaitForSeconds(displayInterval);
        }
    }

    // 非同期処理
    private async void DisplayDengons()
    {
        // 現在地のおよその緯度経度
        // aprxCurrentLatitude = Math.Round(currentLatitude, 4, MidpointRounding.AwayFromZero);
        aprxCurrentLatitude = Mathf.FloorToInt((float)currentLatitude*10000f);
        Debug.Log("aprxCurrentLatitude: " + aprxCurrentLatitude);
        // aprxCurrentLongitude = Math.Round(currentLongitude, 4, MidpointRounding.AwayFromZero);
        aprxCurrentLongitude = Mathf.FloorToInt((float)currentLongitude*10000f);
        Debug.Log("aprxCurrentLongitude: " + aprxCurrentLongitude);

        var db = FirebaseFirestore.DefaultInstance;
        Query dengonQuery = db.Collection("dengons").WhereEqualTo("aprxLatitude", (double)aprxCurrentLatitude).WhereEqualTo("aprxLongitude", (double)aprxCurrentLongitude);
        QuerySnapshot dengonQuerySnapshot = await dengonQuery.GetSnapshotAsync();
        foreach (DocumentSnapshot documentSnapshot in dengonQuerySnapshot.Documents)
        {
            Console.WriteLine("Document data for {0} document:", documentSnapshot.Id);
            Dictionary<string, object> dengon = documentSnapshot.ToDictionary();

            Debug.Log("shareWith: " + dengon["shareWith"]);
            // Debug.Log(dengon["releaseTime"]);
            // Debug.Log(dengon["objectType"]);
            Debug.Log("latitude: " + dengon["latitude"]);
            Debug.Log("longitude: " + dengon["longitude"]);
            Debug.Log("aprxLongitude: " + dengon["aprxLongitude"]);
            Debug.Log("aprxLatitude: " + dengon["aprxLatitude"]);
            Debug.Log("aprxLatitude Type: " + dengon["aprxLatitude"].GetType());

            // すでに表示している伝言はスキップ
            if (existingDengonList.Contains(documentSnapshot.Id))
            {
                continue;
            }
            // 自分のみ公開の設定で、投稿者が自分でない伝言はスキップ
            // if (dengon["shareWith"].ToString() == 1)
            if (Convert.ToInt32(dengon["shareWith"]) == 1)
            {
                if (!(dengon["writerId"].Equals(user.UserId)))
                {
                    continue;
                }
            }
            // 公開日時以前の伝言はスキップ
            // if (Convert.ToDateTime(dengon["releaseTime"]) > currentTime)
            // {
            //     continue;
            // }

            // objectTypeごとに伝言を表示させる
            // if (dengon["objectType"] == 0)
            if (Convert.ToInt32(dengon["objectType"]) == 0)
            {
                dengonObject = Instantiate(object0);
                Debug.Log("object0 instantiate");
            }
            // else if (dengon["objectType"] == 1)
            if (Convert.ToInt32(dengon["objectType"]) == 1)
            {
                dengonObject = Instantiate(object1);
                Debug.Log("object1 instantiate");
            }
            else
            {
                dengonObject = Instantiate(object2);
                Debug.Log("object2 instantiate");
            }

            // 伝言の名前にドキュメントIDをつける
            dengonObject.name = documentSnapshot.Id;

            // 伝言とユーザーの位置の差を計算
            double north = (Convert.ToDouble(dengon["latitude"]) - currentLatitude) * latitudeToMeter;
            double east = (Convert.ToDouble(dengon["longitude"]) - currentLongitude) * longitudeToMeter;
            Vector3 absoluteDirection = new Vector3(-1 * (float)east, 0, (float)north);

            // 端末の向きを磁気センサーより取得(degree)
            float phone_deg = Input.compass.trueHeading;
            Vector3 relativeDirection = Quaternion.Euler(0, -1 * phone_deg, 0) * absoluteDirection; // 正負あってる？

            // 伝言を移動
            dengonObject.transform.position = relativeDirection;
            Debug.Log("object moved");

            // 表示させた伝言をリストに追加
            existingDengonList.Add(documentSnapshot.Id);

        }

    }

}

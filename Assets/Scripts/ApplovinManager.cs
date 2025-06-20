using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using static MaxSdkBase;

public class ApplovinManager : MonoBehaviour
{
    [SerializeField] private string m_ApplovinSdkKey;
    
    public static bool m_Sended
    {
        get => bool.Parse(PlayerPrefs.GetString("EcpmCollectionSystem_DataSended"));
        set => PlayerPrefs.SetString("EcpmCollectionSystem_DataSended",value.ToString());
    }

    private void Awake()
    {
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaid;

        MaxSdk.SetSdkKey(m_ApplovinSdkKey);
        MaxSdk.InitializeSdk();

    }

    private void OnDestroy()
    {
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnAdRevenuePaid;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnAdRevenuePaid;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnAdRevenuePaid;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnAdRevenuePaid;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent -= OnAdRevenuePaid;
    }

    private void OnAdRevenuePaid(string adUnitId, AdInfo adInfo)
    {
        double revenue = adInfo.Revenue; 
        string network = adInfo.NetworkName;
        string country = MaxSdk.GetSdkConfiguration().CountryCode;
        string adUnit = adInfo.AdUnitIdentifier;
        string data = DateTime.UtcNow.ToString();
        double ecpm = revenue * 1000.0;

        UserData userData = new UserData(FirebaseInitializator.UserId,((float)revenue).ToString("0.00"),country,adUnit, ((float)ecpm).ToString("0.00"), data);

        string saveKey = JsonConvert.SerializeObject(userData);
        PlayerPrefs.SetString("LasteCPM", saveKey);

        FirebaseInitializator.updateDataToFirebase?.Invoke(saveKey);
    }
}

[Serializable]
public struct UserData
{
    public string userId;
    public string data;
    public string revenue;
    public string country;
    public string adUnit;

    public string ecpm;

    public UserData(string userId, string revenue, string country, string adUnit, string ecmp, string data)
    {
        this.userId = userId;
        this.revenue = revenue;
        this.country = country;
        this.adUnit = adUnit;
        this.ecpm = ecmp;
        this.data = data;
    }
}

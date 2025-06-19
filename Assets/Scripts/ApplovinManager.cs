using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using static MaxSdkBase;

public class ApplovinManager : MonoBehaviour
{
    [SerializeField] private string m_ApplovinSdkKey;

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

        UserData userData = new UserData(FirebaseInitializator.UserId,(float)revenue,country,adUnit, (float)ecpm, data);

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
    public float revenue;
    public string country;
    public string adUnit;

    public float ecpm;

    public UserData(string userId,float revenue, string country, string adUnit, float ecmp, string data)
    {
        this.userId = userId;
        this.revenue = revenue;
        this.country = country;
        this.adUnit = adUnit;
        this.ecpm = ecmp;
        this.data = data;
    }
}

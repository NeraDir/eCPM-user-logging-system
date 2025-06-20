using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using static MaxSdkBase;

public class ApplovinManager : MonoBehaviour
{
    [SerializeField] private string m_ApplovinSdkKey;
    [SerializeField] private int m_DaysCollect = 5;

    private DateTime m_LastDate
    {
        get => DateTime.Parse(PlayerPrefs.GetString("LastSendEcpmDateTime",DateTime.UtcNow.ToString()));
        set => PlayerPrefs.SetString("LastSendEcpmDateTime",value.ToString());
    }

    private DateTime m_FirstDate
    {
        get => DateTime.Parse(PlayerPrefs.GetString("FirstSendEcpmDateTime", DateTime.UtcNow.ToString()));
        set => PlayerPrefs.SetString("FirstSendEcpmDateTime", value.ToString());
    }

    /// <summary>
    /// Test Variables not for production
    /// </summary>
    private string[] datas = {"22.01.2002", "25.01.2002", "27.01.2002", "29.01.2002", "24.01.2002", "23.01.2002", "20.01.2002" };
    private string[] countries = { "RU", "US", "EN", "AU", "NG", "NL" };

    

    private void Awake()
    {
        if(!PlayerPrefs.HasKey("FirstSendEcpmDateTime"))
            m_FirstDate = DateTime.UtcNow;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaid;

        MaxSdk.SetSdkKey(m_ApplovinSdkKey);
        MaxSdk.InitializeSdk();
    }

    /// <summary>
    /// TEST IENUM FOR TEST SEND TO BD
    /// </summary>
    //private IEnumerator Start()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(5);
    //        UserData userData = new UserData(FirebaseInitializator.UserId + UnityEngine.Random.Range(0, 100000000), ((float)UnityEngine.Random.Range(0.1f, 5)).ToString("0.00"), countries[UnityEngine.Random.Range(0, countries.Length)], "dfdgdf", ((float)UnityEngine.Random.Range(0.1f, 5)).ToString("0.00"), datas[UnityEngine.Random.Range(0, datas.Length)]);

    //        string saveKey = JsonConvert.SerializeObject(userData);
    //        PlayerPrefs.SetString("LasteCPM", saveKey);

    //        FirebaseInitializator.updateDataToFirebase?.Invoke(saveKey);
    //    }
    //}

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
        m_LastDate = DateTime.UtcNow;
        TimeSpan difference = m_LastDate - m_FirstDate;
        if (difference.TotalDays > m_DaysCollect)
            return;
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

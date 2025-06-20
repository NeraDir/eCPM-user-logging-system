using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseInitializator : MonoBehaviour
{
    [SerializeField] private int m_DaysCollect = 5;

    public static Action<string> updateDataToFirebase;
    public static Func<string> getDataFromTable;
    
    private DatabaseReference m_DbReference;
    public static string UserId = "";
    private string m_GettedJson;

    private void Awake()
    {
        UserId = "user_" + SystemInfo.deviceUniqueIdentifier;
        getDataFromTable = () => m_GettedJson;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                m_DbReference = FirebaseDatabase.DefaultInstance.RootReference;
                updateDataToFirebase += UpdateDataToFirebaseDataBase;
                TryToInitDate();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void OnDestroy()
    {
        updateDataToFirebase -= UpdateDataToFirebaseDataBase;
        getDataFromTable = null;
    }

    public void UpdateDataToFirebaseDataBase(string json)
    {
        UserData data = JsonConvert.DeserializeObject<UserData>(json);

        string country = data.country;
        string userId = data.userId;

        DatabaseReference dateRef = m_DbReference.Child("Data").Child("1_InitDate");

        dateRef.GetValueAsync().ContinueWithOnMainThread(dateTask =>
        {
            if (dateTask.IsCompleted)
            {
                DataSnapshot snapshot = dateTask.Result;

                if (snapshot.Exists && DateTime.TryParse(snapshot.Value.ToString(), out DateTime initDate))
                {
                    TimeSpan diff = DateTime.UtcNow - initDate;

                    if (diff.TotalDays >= m_DaysCollect)
                    {
                        Debug.Log(diff);
                        CalculateAvgEcpm(country);
                    }
                    else
                    {
                        if (ApplovinManager.m_Sended)
                            return;
                        ApplovinManager.m_Sended = true;
                        m_DbReference.Child("Data")
                            .Child(country)
                            .Child("Users")
                            .Child(userId)
                            .SetRawJsonValueAsync(json)
                            .ContinueWithOnMainThread(task =>
                            {
                                if (task.IsCompleted)
                                {
                                    Debug.Log("User data uploaded!");
                                }
                                else
                                {
                                    Debug.LogError("Upload failed: " + task.Exception);
                                }
                            });
                    }
                }
            }
        });
    }

    public void TryToInitDate()
    {
        DatabaseReference dateRef = m_DbReference.Child("Data")
            .Child("1_InitDate");

        dateRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                var snapshot = task.Result;

                if (snapshot.Exists && !string.IsNullOrEmpty(snapshot.Value?.ToString()))
                {
                    Debug.Log("InitDate already set: " + snapshot.Value.ToString());
                }
                else
                {
                    string utcNow = DateTime.UtcNow.ToString("O"); 
                    dateRef.SetValueAsync(utcNow).ContinueWithOnMainThread(setTask =>
                    {
                        if (setTask.IsCompleted)
                            Debug.Log("InitDate set: " + utcNow);
                        else
                            Debug.LogError("Failed to set InitDate: " + setTask.Exception);
                    });
                }
            }
        });
    }

    private void CalculateAvgEcpm(string country)
    {
        DatabaseReference usersRef = m_DbReference.Child("Data").Child(country).Child("Users");

        usersRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                float totalEcpm = 0f;
                int userCount = 0;

                foreach (var child in snapshot.Children)
                {
                    if (child.Child("ecpm").Value != null)
                    {
                        float ecpm = 0f;
                        float.TryParse(child.Child("ecpm").Value.ToString(), out ecpm);
                        totalEcpm += ecpm;
                        userCount++;
                    }
                }

                if (userCount > 0)
                {
                    float averageEcpm = totalEcpm / userCount;

                    m_DbReference.Child("Data")
                        .Child(country)
                        .Child("AvgEcpm")
                        .SetValueAsync(averageEcpm.ToString("0.00"))
                        .ContinueWithOnMainThread(setTask =>
                        {
                            if (setTask.IsCompleted)
                            {
                                Debug.Log("Average eCPM updated!");
                            }
                            else
                                Debug.LogError("Failed: " + setTask.Exception);
                        });
                }
            }
            else
            {
                Debug.LogError("Failed: " + task.Exception);
            }
        });
        FindAllMoreAVGEcpm();
    }

    private void FindAllMoreAVGEcpm()
    {
        DatabaseReference dataRef = m_DbReference.Child("Data");

        dataRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot countriesSnapshot = task.Result;

                foreach (var countrySnapshot in countriesSnapshot.Children)
                {
                    string country = countrySnapshot.Key;

                    string avgEcpmStr = countrySnapshot.Child("AvgEcpm").Value?.ToString();

                    if (float.TryParse(avgEcpmStr, out float avgEcpm))
                    {
                        var usersSnapshot = countrySnapshot.Child("Users");
                        List<UserData> aboveAvgUsers = new List<UserData>();

                        foreach (var userSnapshot in usersSnapshot.Children)
                        {
                            var ecpmObj = userSnapshot.Child("ecpm").Value;
                            if (ecpmObj != null && float.TryParse(ecpmObj.ToString(), out float ecpm))
                            {
                                if (ecpm > avgEcpm)
                                {
                                    string userJson = JsonConvert.SerializeObject(userSnapshot.Value);
                                    UserData userData = JsonConvert.DeserializeObject<UserData>(userJson);
                                    aboveAvgUsers.Add(userData);
                                }
                            }
                        }

                        if (aboveAvgUsers.Count > 0)
                        {
                            string listJson = JsonConvert.SerializeObject(aboveAvgUsers);
                            m_DbReference.Child("Data")
                                .Child(country)
                                .Child("AboveAvgUsers")
                                .SetRawJsonValueAsync(listJson)
                                .ContinueWithOnMainThread(setTask =>
                                {

                                });
                        }
                    }
                }
            }
        });
    }
}

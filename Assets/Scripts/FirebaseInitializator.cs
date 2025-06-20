using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;

public class FirebaseInitializator : MonoBehaviour
{
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
                    CalculateAvgEcpm(country);
                }
                else
                {
                    Debug.LogError("Upload failed: " + task.Exception);
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
                                Debug.Log("Average eCPM updated!");
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
    }
}

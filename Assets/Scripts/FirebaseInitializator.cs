using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEngine;

public class FirebaseInitializator : MonoBehaviour
{
    public static Action<string> updateDataToFirebase;
    
    private DatabaseReference m_DbReference;
    public static string UserId = "";

    private void Awake()
    {
        UserId = "user_" + SystemInfo.deviceUniqueIdentifier;

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
    }

    public void UpdateDataToFirebaseDataBase(string json)
    {
        m_DbReference.Child("adData")
            .Child(UserId)
            .Push()
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log("Data uploaded!");
                else
                    Debug.LogError("Upload failed: " + task.Exception);
            });
    }
}

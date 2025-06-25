using UnityEngine;

public class PlayerPrefsViewer : MonoBehaviour
{
    void Start()
    {
        Debug.Log("PlayerPrefs contents:");
        foreach (string key in new[] { "save_data" }) // Add known keys here
        {
            Debug.Log($"{key}: {PlayerPrefs.GetString(key, "(not found)")}"); // Change to GetInt/GetFloat as needed
        }
    }
}
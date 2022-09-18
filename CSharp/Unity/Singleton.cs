using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T m_instance;
    public static T Instance {
        get {
            if (m_instance != null) return m_instance;
            m_instance = GameObject.FindObjectOfType<T>();
            if (m_instance == null) {
                Debug.LogError($"Error while creating a singleton for type {typeof(T)}");
            }
            return m_instance;
        }
    }


}

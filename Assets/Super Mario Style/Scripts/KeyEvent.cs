using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyEvent : MonoBehaviour
{
    [SerializeField] private KeyCode key;
    [SerializeField] private KeyCode keyAlternative;
    [SerializeField] private UnityEvent onPressKey;

    void Update()
    {
        if (GameManager.instance.isEndGame)
            return;

        if (Input.GetKeyDown(key) || Input.GetKeyDown(keyAlternative))
        {
            Pressed();
        }
    }

    public void Pressed()
    {
        onPressKey.Invoke();
    }
}

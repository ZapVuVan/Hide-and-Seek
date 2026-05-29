using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWinUI : MonoBehaviour
{
    public void Start()
    {
        Hide();
    }   
    public void Show()
    {
        gameObject.SetActive(true); 
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

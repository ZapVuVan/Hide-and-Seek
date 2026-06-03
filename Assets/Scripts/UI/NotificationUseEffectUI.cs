using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NotificationUseEffectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private float timehide = 1.5f;
    public static NotificationUseEffectUI Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        panel.SetActive(false);
    }
    public void Hide()
    {
        panel.SetActive(false);    
    }
    public void Show()
    {
        panel.SetActive(true);
    }

    public float GetTimeHide()
    {
        return timehide;
    }
}

// RoleUI.cs
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RoleUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI hiderText;
    [SerializeField] private TextMeshProUGUI seekerText;

    public void Start()
    {
        Hide();
        RoleManager.Instance.OnRolesChanged += Refresh;
    }
    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void Refresh()
    {
        // Ghi tất cả Hider vào 1 text, mỗi người 1 dòng
        var hiders = RoleManager.Instance.GetAllByRole(GameRole.Hider);
        string hiderNames = "";
        foreach (var hider in hiders)
            hiderNames += hider.gameObject.name + "\n";
        hiderText.text = hiderNames;

        // Ghi tất cả Seeker vào 1 text
        var seekers = RoleManager.Instance.GetAllByRole(GameRole.Seeker);
        string seekerNames = "";
        foreach (var seeker in seekers)
            seekerNames += seeker.gameObject.name + "\n";
        seekerText.text = seekerNames;
        Show();
    }
}
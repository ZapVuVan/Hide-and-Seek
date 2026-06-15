using UnityEngine;
using TMPro;

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

    // ✅ Luôn unsubscribe để tránh event gọi vào UI đã bị destroy
    private void OnDestroy()
    {
        if (RoleManager.Instance != null)
            RoleManager.Instance.OnRolesChanged -= Refresh;
    }

    public void Show()
    {
        // ✅ Guard null cho panel
        if (panel == null) return;
        panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }

    public void Refresh()
    {
        // ✅ Guard: nếu UI này đã bị destroy thì bỏ qua
        if (this == null || panel == null) return;

        var hiders = RoleManager.Instance.GetAllByRole(GameRole.Hider);
        string hiderNames = "";
        foreach (var hider in hiders)
            if (hider != null) // ✅ double-check từng item
                hiderNames += hider.gameObject.name + "\n";
        hiderText.text = hiderNames;

        var seekers = RoleManager.Instance.GetAllByRole(GameRole.Seeker);
        string seekerNames = "";
        foreach (var seeker in seekers)
            if (seeker != null)
                seekerNames += seeker.gameObject.name + "\n";
        seekerText.text = seekerNames;

        Show();
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PingUI : MonoBehaviour
{
    [SerializeField] private Image pingFill;

    private void OnEnable()
    {
        GameManager.Instance.OnPingCooldownUpdate += HandleCooldown;
        GameManager.Instance.OnPingStart += HandlePingStart;
        GameManager.Instance.OnPingEnd += HandlePingEnd;
        GameManager.Instance.OnPingHiders += HandleHiders;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnPingCooldownUpdate -= HandleCooldown;
        GameManager.Instance.OnPingStart -= HandlePingStart;
        GameManager.Instance.OnPingEnd -= HandlePingEnd;
        GameManager.Instance.OnPingHiders -= HandleHiders;
    }
    private void HandleCooldown(float value)
    {
        pingFill.fillAmount = value;
    }

    private void HandlePingStart()
    {
    }

    private void HandlePingEnd()
    {
        pingFill.fillAmount = 1f;
    }

    private void HandleHiders(List<RoleComponent> hiders)
    {
        foreach (var hider in hiders)
        {
            if (hider == null) continue;

            hider.GetComponentInChildren<HiderPingIcon>()?.Show();
        }
    }
}
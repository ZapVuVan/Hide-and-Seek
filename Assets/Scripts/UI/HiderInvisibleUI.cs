using UnityEngine;
using UnityEngine.UI;

public class HiderInvisibleUI : MonoBehaviour
{
    [SerializeField] private Image invisibleBar;

    public void SetFill(float value)
    {
        invisibleBar.fillAmount = Mathf.Clamp01(value);
    }

    public void SetBarVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
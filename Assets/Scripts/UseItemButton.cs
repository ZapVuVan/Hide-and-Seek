using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UseItemButton : MonoBehaviour, IPointerDownHandler
{
    [Header("Skill Data")]
    [SerializeField] private ItemEffect effect;
    [SerializeField] private PlayerMovement player;

    [Header("UI")]
    [SerializeField] private Button button;

    private bool isCooldown;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isCooldown || effect == null || player == null)
            return;
        if(GameManager.Instance.CurrentState != GameState.Playing)
        {
            NotificationUseEffectUI.Instance.Show();
            StartCoroutine(Notification());
        }
        else
        {
            effect.Apply(player.gameObject);
            StartCoroutine(CooldownRoutine());
        }
        
    }

    private IEnumerator CooldownRoutine()
    {
        isCooldown = true;
        button.interactable = false;

        float timer = effect.cooldown;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        button.interactable = true;
        isCooldown = false;
    }

    private IEnumerator Notification()
    {
        float timer = NotificationUseEffectUI.Instance.GetTimeHide();
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        NotificationUseEffectUI.Instance.Hide();

    }

}
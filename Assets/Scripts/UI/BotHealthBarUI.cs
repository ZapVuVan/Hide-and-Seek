using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BotHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, 0);

    private Health health;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        health = target.GetComponent<Health>();
        health.OnHealthChanged += OnHealthChanged;
        health.OnDie += OnDie;
        gameObject.SetActive(false);
    }

    private void OnHealthChanged(object sender, float percent)
    {
        if (percent <= 0f) return;

        fillImage.fillAmount = percent;
        gameObject.SetActive(true);

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay(1f));
    }

    private void OnDie()
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        gameObject.SetActive(false);
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.forward = Camera.main.transform.forward;
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= OnHealthChanged;
            health.OnDie -= OnDie;
        }
    }
}
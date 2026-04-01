using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI References")]
    public TMP_Text healthText;
    public GameObject BlackPanel;
    public TMP_Text DeathText;

    [Header("Death Settings")]
    public float blackFadeDuration = 6f;
    public float textFadeDuration = 4f;
    public float textDelay = 1.5f;
    public string deathMessage = "ВОЛК ТЕБЯ СЪЕЛ";
    public Color deathTextColor = new Color(0.8f, 0f, 0f);
    public string sceneToReload;
    private bool isDead = false;

    public static int killsAtDeath = 0;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        if (BlackPanel != null)
        {
            BlackPanel.SetActive(false);
            // Add CanvasGroup if it doesn't exist, then set alpha
            var cg = BlackPanel.GetComponent<CanvasGroup>() ?? BlackPanel.AddComponent<CanvasGroup>();
            cg.alpha = 0;
        }

        if (DeathText != null)
        {
            DeathText.gameObject.SetActive(false);
            // Add CanvasGroup if it doesn't exist, then set alpha
            var cg = DeathText.GetComponent<CanvasGroup>() ?? DeathText.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0;
        }

        sceneToReload = SceneManager.GetActiveScene().name;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (healthText != null)
            healthText.text = currentHealth.ToString();
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player Died!");

        if (BlackPanel != null)
        {
            BlackPanel.SetActive(true);
            CanvasGroup panelGroup = BlackPanel.GetComponent<CanvasGroup>();
            if (panelGroup != null)
            {
                panelGroup.alpha = 0;
            }
        }

        if (DeathText != null)
        {
            DeathText.gameObject.SetActive(true);
            DeathText.text = deathMessage;
            DeathText.color = deathTextColor;
            CanvasGroup textGroup = DeathText.GetComponent<CanvasGroup>();
            if (textGroup != null)
            {
                textGroup.alpha = 0;
            }
        }

        if (QuestManager.Instance != null)
        {
            killsAtDeath = QuestManager.Instance.GetCurrentKills();
            Debug.Log($"[ReloadScene] Saved kills at death: {killsAtDeath}");
        }

        StartCoroutine(DeathSequence());
    }

    System.Collections.IEnumerator DeathSequence()
    {
        // Фаза 1: Затемнение экрана
        float timer = 0f;
        CanvasGroup panelGroup = BlackPanel.GetComponent<CanvasGroup>();

        while (timer < blackFadeDuration)
        {
            timer += Time.deltaTime;
            panelGroup.alpha = Mathf.Clamp01(timer / blackFadeDuration);
            yield return null;
        }

        // Фаза 2: Появление текста с задержкой
        yield return new WaitForSeconds(textDelay);

        timer = 0f;
        CanvasGroup textGroup = DeathText.GetComponent<CanvasGroup>();

        while (timer < textFadeDuration)
        {
            timer += Time.deltaTime;
            textGroup.alpha = Mathf.Clamp01(timer / textFadeDuration);
            yield return null;
        }

        // Дополнительная пауза перед перезагрузкой
        yield return new WaitForSeconds(1f);

        ReloadScene();
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(sceneToReload);
    }
}
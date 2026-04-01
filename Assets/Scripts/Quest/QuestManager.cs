using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public QuestGiver questGiver;

    [Header("Quest UI References")]
    public GameObject questTrackerPanel;
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questProgressText;
    public GameObject questCompletePanel;
    public TextMeshProUGUI questCompleteText;

    [Header("Find NPC Quest")]
    public Quest findNpcQuest;

    public bool FindNpcQuestCompleted { get { return findNpcQuestCompleted; } }
    private bool findNpcQuestCompleted = false;

    private static QuestManager _instance;
    public static QuestManager Instance { get { return _instance; } }

    private int currentKills = 0;
    public int killsNeeded = 1;
    public bool QuestReadyForCompletion = false;
    private bool isQuestActive = false;
    private Quest currentQuest;

    private void Awake()
    {
        Debug.Log("QuestManager Awake() called");

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        // Инициализация UI
        if (questTrackerPanel != null)
        {
            questTrackerPanel.SetActive(false);
            Debug.Log("questTrackerPanel set to inactive in Awake()");
        }
        if (questCompletePanel != null)
        {
            questCompletePanel.SetActive(false);
            Debug.Log("questCompletePanel set to inactive in Awake()");
        }

        // Начинаем с квеста "Найди NPC", если он еще не выполнен
        if (findNpcQuest != null && !findNpcQuestCompleted)
        {
            Debug.Log("findNpcQuest is not null and findNpcQuestCompleted is false, showing quest tracker");
            UpdateFindNPCQuestUI();
            questTrackerPanel.SetActive(true);
            questCompletePanel.SetActive(false);
        }
        else if (currentQuest != null && isQuestActive)
        {
            Debug.Log("Current quest is active, showing quest tracker");
            UpdateQuestUI();
            questTrackerPanel.SetActive(true);
            questCompletePanel.SetActive(false);
        }

    }

    public void AcceptQuest(Quest quest)
    {
        Debug.Log("AcceptQuest() is called with quest: " + quest.title);

        if (questGiver != null && quest != null)
        {
            currentQuest = quest;
            killsNeeded = quest.targetAmount;
            currentKills = 0;
            isQuestActive = true;
            QuestReadyForCompletion = false;

            UpdateQuestUI();

            Debug.Log($"Trying to set questTrackerPanel active: {questTrackerPanel.name}");

            if (questTrackerPanel != null)
            {
                questTrackerPanel.SetActive(true); // Показываем трекер
                Debug.Log("questTrackerPanel set to active in AcceptQuest()");
            }
            else
            {
                Debug.LogError("questTrackerPanel is null in AcceptQuest()!");
            }

            // Показываем название и прогресс квеста
            if (questTitleText != null)
            {
                questTitleText.gameObject.SetActive(true);
                Debug.Log("questTitleText set to active in AcceptQuest()");
            }
            else
            {
                Debug.LogError("questTitleText is null in AcceptQuest()!");
            }

            if (questProgressText != null)
            {
                questProgressText.gameObject.SetActive(true);
                Debug.Log("questProgressText set to active in AcceptQuest()");
            }
            else
            {
                Debug.LogError("questProgressText is null in AcceptQuest()!");
            }

            // Скрываем панель завершения
            if (questCompletePanel != null)
            {
                questCompletePanel.SetActive(false);
                Debug.Log("questCompletePanel set to inactive in AcceptQuest()");
            }
            else
            {
                Debug.LogError("questCompletePanel is null in AcceptQuest()!");
            }

            Debug.Log($"Квест принят: {quest.title}");
        }
    }

    public bool IsQuestActive()
    {
        return isQuestActive;
    }

    public int GetCurrentKills()
    {
        return currentKills;
    }

    public void ReportKill()
    {
        if (!isQuestActive) return;

        currentKills++;
        UpdateQuestUI();
        Debug.Log($"Убийство зарегистрировано! {currentKills}/{killsNeeded}");

        if (currentKills >= killsNeeded)
        {
            QuestReadyForCompletion = true;
            Debug.Log("Цель квеста достигнута!");
            ShowQuestCompletePanel();
        }
    }

    public void FinalizeQuest()
    {
        if (!QuestReadyForCompletion) return;

        if (currentQuest != null)
        {
            PlayerMoney.Instance.AddMoney(currentQuest.moneyReward);
            Debug.Log($"Награда за квест: {currentQuest.moneyReward} монет");
        }

        currentKills = 0;
        QuestReadyForCompletion = false;
        isQuestActive = false;
        currentQuest = null;

        // Скрываем все элементы
        if (questTrackerPanel != null)
        {
            questTrackerPanel.SetActive(false);
            Debug.Log("questTrackerPanel set to inactive in FinalizeQuest()");
        }
        else
        {
            Debug.LogError("questTrackerPanel is null in FinalizeQuest()!");
        }

        if (questCompletePanel != null)
        {
            questCompletePanel.SetActive(false);
            Debug.Log("questCompletePanel set to inactive in FinalizeQuest()");
        }
        else
        {
            Debug.LogError("questCompletePanel is null in FinalizeQuest()!");
        }

        if (questTitleText != null)
        {
            questTitleText.gameObject.SetActive(false);
            Debug.Log("questTitleText set to inactive in FinalizeQuest()");
        }
        else
        {
            Debug.LogError("questTitleText is null in FinalizeQuest()!");
        }

        if (questProgressText != null)
        {
            questProgressText.gameObject.SetActive(false);
            Debug.Log("questProgressText set to inactive in FinalizeQuest()");
        }
        else
        {
            Debug.LogError("questProgressText is null in FinalizeQuest()!");
        }

        Debug.Log("Квест завершен!");
    }

    private void UpdateQuestUI()
    {
        Debug.Log("UpdateQuestUI() is called");
        if (currentQuest != null && questTitleText != null && questProgressText != null)
        {
            Debug.Log("Updating quest UI: " + currentQuest.title);
            questTitleText.text = currentQuest.title;
            questProgressText.text = $"{currentKills}/{killsNeeded}";
            Debug.Log("questTitleText.text is now: " + questTitleText.text); // Добавлено логирование
            Debug.Log("questProgressText.text is now: " + questProgressText.text); // Добавлено логирование
        }
        else
        {
            if (currentQuest == null) Debug.LogError("currentQuest is null!");
            if (questTitleText == null) Debug.LogError("questTitleText is null!");
            if (questProgressText == null) Debug.LogError("questProgressText is null!");
        }
    }

    private void UpdateFindNPCQuestUI()
    {
        Debug.Log("UpdateFindNPCQuestUI() is called");
        if (findNpcQuest != null && questTitleText != null && questProgressText != null)
        {
            Debug.Log("Updating quest UI: " + findNpcQuest.title);
            questTitleText.text = findNpcQuest.title;
            Debug.Log("questTitleText.text is now: " + questTitleText.text); // Добавлено логирование
            questProgressText.text = $"Поговорите с Егором";
            Debug.Log("questProgressText.text is now: " + questProgressText.text); // Добавлено логирование
            questCompleteText.text = $"Квест \"{findNpcQuest.title}\" выполнен!";
            Debug.Log("questCompleteText.text is now: " + questCompleteText.text); // Добавлено логирование
        }
        else
        {
            if (findNpcQuest == null) Debug.LogError("findNpcQuest is null!");
            if (questTitleText == null) Debug.LogError("questTitleText is null!");
            if (questProgressText == null) Debug.LogError("questProgressText is null!");
        }
    }

    private void ShowQuestCompletePanel()
    {
        if (questCompletePanel != null && questCompleteText != null && currentQuest != null)
        {
            questCompleteText.text = $"Квест \"{currentQuest.title}\" выполнен!";
            Debug.Log("questCompleteText.text is now: " + questCompleteText.text); // Добавлено логирование

            // Скрываем название и прогресс квеста
            if (questTitleText != null)
            {
                questTitleText.gameObject.SetActive(false);
                Debug.Log("questTitleText set to inactive in ShowQuestCompletePanel()");
            }
            else
            {
                Debug.LogError("questTitleText is null in ShowQuestCompletePanel()!");
            }

            if (questProgressText != null)
            {
                questProgressText.gameObject.SetActive(false);
                Debug.Log("questProgressText set to inactive in ShowQuestCompletePanel()");
            }
            else
            {
                Debug.LogError("questProgressText is null in ShowQuestCompletePanel()!");
            }

            // Показываем панель завершения
            if (questCompletePanel != null)
            {
                questCompletePanel.SetActive(true);
                Debug.Log("questCompletePanel set to active in ShowQuestCompletePanel()");
            }
            else
            {
                Debug.LogError("questCompletePanel is null in ShowQuestCompletePanel()!");
            }

            // Показываем трекер
            if (questTrackerPanel != null)
            {
                questTrackerPanel.SetActive(true);
                Debug.Log("questTrackerPanel set to active in ShowQuestCompletePanel()");
            }
            else
            {
                Debug.LogError("questTrackerPanel is null in ShowQuestCompletePanel()!");
            }
        }
    }

    public void CompleteFindNPCQuest()
    {
        Debug.Log("CompleteFindNPCQuest() called");
        findNpcQuestCompleted = true;

        if (questTrackerPanel != null)
        {
            questTrackerPanel.SetActive(false); // Скрываем панель после завершения квеста "Найди NPC"
            Debug.Log("questTrackerPanel set to inactive in CompleteFindNPCQuest()");
        }
        else
        {
            Debug.LogError("questTrackerPanel is null in CompleteFindNPCQuest()!");
        }

        if (findNpcQuest != null)
        {
            PlayerMoney.Instance.AddMoney(findNpcQuest.moneyReward);
            Debug.Log($"Награда за квест: {findNpcQuest.moneyReward} монет");
        }
    }
}
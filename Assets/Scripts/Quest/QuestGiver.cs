using TMPro;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public Dialogue startDialogue;
    public Dialogue completeDialogue;
    public Canvas dialogueCanvas;
    public TextMeshProUGUI dialogueText;

    [Header("Quest Settings")]
    public Quest wolfQuest;

    [Header("UI References")]
    public GameObject HUDCanvas;
    public GameObject dialoguePanel;

    [Header("Find NPC Quest")]
    public Quest findNpcQuest;
    public bool hasGivenFindNpcQuest = false;

    private bool playerInRange;
    private bool isDialogueActive;
    private int currentDialogueIndex = 0;
    private Dialogue currentDialogue;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed while in range of NPC"); // Проверяем нажатие клавиши
            if (!isDialogueActive)
            {
                Debug.Log("Starting dialogue"); // Проверяем вызов StartDialogue()
                StartDialogue();
            }
            else
            {
                ContinueDialogue();
            }
        }
    }

    void StartDialogue()
    {
        Debug.Log("StartDialogue() called"); // Проверяем, что метод вызывается

        isDialogueActive = true;
        currentDialogueIndex = 0;
        dialogueCanvas.enabled = true;
        dialoguePanel.SetActive(true);

        if (!QuestManager.Instance.FindNpcQuestCompleted)
        {
            Debug.Log("Find NPC quest not completed"); // Проверяем условие
            QuestManager.Instance.CompleteFindNPCQuest();
        }

        if (QuestManager.Instance.FindNpcQuestCompleted)
        {
            Debug.Log("Find NPC quest completed"); // Проверяем условие
            if (QuestManager.Instance.QuestReadyForCompletion)
            {
                ShowCompleteDialogue();
            }
            else if (!QuestManager.Instance.IsQuestActive())
            {
                ShowStartDialogue();
            }
            else
            {
                ShowAlreadyStartedDialogue();
            }
        }
        else
        {
            Debug.Log("Find NPC quest not completed"); // Проверяем условие
            ShowFindNpcDialogue();
        }

        HUDCanvas.SetActive(false);
    }

    private void ShowAlreadyStartedDialogue()
    {
        Debug.Log("ShowAlreadyStartedDialogue() called");
        currentDialogue = startDialogue;
        ShowDialogueLine();
    }

    void ContinueDialogue()
    {
        currentDialogueIndex++;
        ShowDialogueLine();
    }

    void ShowDialogueLine()
    {
        if (currentDialogue != null && currentDialogue.lines.Count > currentDialogueIndex)
        {
            dialogueText.text = currentDialogue.lines[currentDialogueIndex].text;
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialogueCanvas.enabled = false;
        dialoguePanel.SetActive(false);
        HUDCanvas.SetActive(true);
        currentDialogue = null;
        currentDialogueIndex = 0;
    }

    void ShowStartDialogue()
    {
        Debug.Log("ShowStartDialogue() called"); // Проверяем вызов метода
        currentDialogue = startDialogue;
        ShowDialogueLine();

        QuestManager.Instance.AcceptQuest(wolfQuest);
    }

    void ShowCompleteDialogue()
    {
        Debug.Log("ShowCompleteDialogue() called");
        currentDialogue = completeDialogue;
        ShowDialogueLine();
        QuestManager.Instance.FinalizeQuest();
    }

    void ShowFindNpcDialogue()
    {
        Debug.Log("ShowFindNpcDialogue() called");
        currentDialogue = startDialogue;
        ShowDialogueLine();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered the trigger");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player exited the trigger");
            EndDialogue();
        }
    }
}
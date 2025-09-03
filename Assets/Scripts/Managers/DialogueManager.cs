using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    [SerializeField] private TextMeshProUGUI dialogueText; 
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private float autoTime = 3f;

    private string[] currentLines;
    private int lineIndex;
    private float timer;
    private bool isActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowLine();
        }

        timer += Time.deltaTime;
        if (timer >= autoTime)
        {
            ShowLine();
        }
    }

    public void StartDialogue(string[] lines)
    {
        currentLines = lines;
        lineIndex = 0;
        timer = 0f;
        isActive = true;

        dialoguePanel.SetActive(true);
        ShowLine();
    }

    public void ShowLine()
    {
        if (lineIndex < currentLines.Length)
        {
            dialogueText.text = currentLines[lineIndex];
            lineIndex++;
            timer = 0f; 
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        isActive = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }
}

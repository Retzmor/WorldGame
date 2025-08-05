using UnityEngine;
using UnityEngine.UI;

public class ButtonQuitConector : MonoBehaviour
{
    [SerializeField] private Button button;

    void Start()
    {
        button.onClick.AddListener(() =>
        {
            GameManager.instance.QuitGame();
        });
    }
}

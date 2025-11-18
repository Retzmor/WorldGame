using UnityEngine;
using UnityEngine.UI;

public class OnMainMenuButton : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(CallMainMenuEvent);
    }

    public void CallMainMenuEvent()
    {
        GameManager.instance.GoToMainMenu();
    }
}

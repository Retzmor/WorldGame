using UnityEngine;
using UnityEngine.UI;

public class ButtonPlayWorldSelected : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(GameManager.instance.LoadWorld);
    }

}

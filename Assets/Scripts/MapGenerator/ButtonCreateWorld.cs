using UnityEngine;
using UnityEngine.UI;

public class ButtonCreateWorld : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(GameManager.instance.InitWorld);
    }


}

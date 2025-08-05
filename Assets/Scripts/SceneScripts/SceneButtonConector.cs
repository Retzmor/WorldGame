using UnityEngine;

using UnityEngine.UI;
public class SceneButtonConector : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private string sceneToLoad;

    void Start()
    {
        button.onClick.AddListener(() =>
        {
            SceneController.Instance.LoadScene(sceneToLoad);
        });
    }
}

using UnityEngine;
using TMPro;
public class VersionGameText : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TMP_Text>().text = $"V {Application.version}";
    }
}

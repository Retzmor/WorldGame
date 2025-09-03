using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [SerializeField] Image imageHeart;
    [SerializeField] bool isActive;

    public void ActiveHeart()
    {
        imageHeart.enabled = true;
        isActive = true;
    }

    public void DesactiveHeart()
    {
        imageHeart.enabled = false;
        isActive = false;
    }

    public bool IsActive() => isActive;
}

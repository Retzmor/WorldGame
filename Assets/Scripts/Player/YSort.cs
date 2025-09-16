
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Mientras más abajo esté en el mundo, más adelante se dibuja
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}

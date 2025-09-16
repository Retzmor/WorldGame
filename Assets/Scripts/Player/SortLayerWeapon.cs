using UnityEngine;

public class SortLayerWeapon : MonoBehaviour
{
    private SpriteRenderer playerSP;
    private SpriteRenderer mySpriteRenderer;
    private void Start()
    {
      playerSP = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        mySpriteRenderer.sortingOrder = playerSP.sortingOrder;
    }
}

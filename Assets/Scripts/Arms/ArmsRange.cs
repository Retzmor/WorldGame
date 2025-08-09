using UnityEngine;

public class ArmsRange : Arms
{
    [SerializeField] GameObject arrow;
    public void Arrow()
    {
        Vector3 wordPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wordPosition.z = 0;
        Vector3 direction = (wordPosition - transform.position).normalized;
        Instantiate(arrow, transform.position, Quaternion.Euler(direction));
    }
}

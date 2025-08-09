using UnityEngine;

public class ArmsRange : Arms
{
    [SerializeField] GameObject arrow;
    [SerializeField] Transform controllerArrow;
    public void Arrow()
    {
        Instantiate(arrow, controllerArrow.position, controllerArrow.rotation);
    }
}

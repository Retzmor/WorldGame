using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class ShowWorldsController : MonoBehaviour
{
    [SerializeField] private GameObject buttonSlotPrefab;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private List<ButtonSlot> buttonSlots; 

    private void Start()
    {
        InstantiateWorldSlots();
    }


    private void InstantiateWorldSlots()
    {
        WorldMetaList metaData = GameManager.instance.LoadMetaData();
        Debug.Log(metaData.worlds[0].name);

        foreach (var item in metaData.worlds)
        {
           
            //instanciar boton 
            GameObject obj = Instantiate(buttonSlotPrefab, buttonsContainer);
            //Obtenerscript y pasar metodo con parameros
            //ejem, item,getcomponent<buttonslot>(). setData(string name, fecha, )
            obj.GetComponent<ButtonSlot>().SetData(item);
            buttonSlots.Add(obj.GetComponent<ButtonSlot>());

        }
    }
    
    public void DeselectButtons()
    {
        foreach (var item in buttonSlots)
        {
            item.shadow.enabled = false;
        }
    }
}

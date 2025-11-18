using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class ShowWorldsController : MonoBehaviour
{
    [SerializeField] private GameObject buttonSlotPrefab;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private List<ButtonSlot> buttonSlots = new List<ButtonSlot>();

    [Space]
    [Header("ButtonsOfWorld")]
    [SerializeField] private List<Button> ButtonsOfWorld;

    // Pool de botones reutilizables
    private Queue<GameObject> buttonPool = new Queue<GameObject>();

    private void OnEnable()
    {
        InstantiateWorldSlots();
    }

    private void OnDisable()
    {
        DesActivateButtonOfTheWorld();
        // Devuelve todos los botones al pool
        foreach (var slot in buttonSlots)
        {
            slot.gameObject.SetActive(false);
            buttonPool.Enqueue(slot.gameObject);
        }
        buttonSlots.Clear();
    }

    private void InstantiateWorldSlots()
    {
        // Carga de metadatos
        WorldMetaList metaData = GameManager.instance.LoadMetaData();

        foreach (var item in metaData.worlds)
        {
            GameObject obj;

            // 1️⃣ Si hay botones disponibles en el pool, los reutilizamos
            if (buttonPool.Count > 0)
            {
                obj = buttonPool.Dequeue();
                obj.SetActive(true);
            }
            else
            {
                // 2️⃣ Si no hay, instanciamos uno nuevo
                obj = Instantiate(buttonSlotPrefab, buttonsContainer);
            }

            // 3️⃣ Configuramos el botón
            var slot = obj.GetComponent<ButtonSlot>();
            slot.SetData(item);

            // 4️⃣ Lo guardamos en la lista activa
            buttonSlots.Add(slot);

            // Aseguramos que el botón esté en el contenedor correcto (por si cambió)
            obj.transform.SetParent(buttonsContainer, false);
        }
    }

    public void ActivateButtonOfTheWorld()
    {
        foreach (var item in ButtonsOfWorld)
        {
            item.interactable = true;
        }
    }

    public void DesActivateButtonOfTheWorld()
    {
        foreach (var item in ButtonsOfWorld)
        {
            item.interactable = false;
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

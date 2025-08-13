using UnityEngine;

public class PauseUIController : MonoBehaviour
{
    [SerializeField] private GameObject[] panels;

    // Llamar cuando se pausa
    public void ShowMainPanel()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == 0); // Solo panel 0 activo
        }
    }

    // Llamar cuando se reanuda
    public void HideAllPanels()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(false); // Todos desactivados
        }
    }

    // Para usar desde botones
    public void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }
    }

    public void BackToMain()
    {
        ShowMainPanel();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSelectionManager : MonoBehaviour
{
    public void OnPlayPressed()
    {
        
    }

    public void OnDeletePressed()
    {
       GameManager.instance.DeleteWorld(GameManager.instance.saveSystem.currentData.slot);
    }

    public void OnEditPressed()
    {
        
    }

    public void OnRemakePressed()
    {
       
    }

    public void OnBackPressed()
    {
      
    }
}

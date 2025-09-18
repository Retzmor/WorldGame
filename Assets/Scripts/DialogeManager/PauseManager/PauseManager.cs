using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private string sceneName = "Nivel1"; // escena donde SÍ funciona la pausa
    [SerializeField] private GameObject pausePanel; // tu UI panel de pausa
    [SerializeField] private PlayerInput playerInput;

    private bool isPaused = false;

    private void OnEnable()
    {
        playerInput.actions["Pause"].performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        playerInput.actions["Pause"].performed -= OnPausePerformed;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // Evitar que funcione fuera de la escena definida
        if (SceneManager.GetActiveScene().name != sceneName) return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // Cambiar al mapa de UI
        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Global");
            playerInput.actions["Pause"].performed += OnPausePerformed;
            Debug.Log(playerInput.currentActionMap);
        }
           
            
        // Activar el panel de pausa y mostrar primer hijo
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            for (int i = 0; i < pausePanel.transform.childCount; i++)
                pausePanel.transform.GetChild(i).gameObject.SetActive(i == 0);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Cambiar al mapa de Gameplay
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Gameplay");

        // Desactivar todo el panel de pausa
        if (pausePanel != null)
        {
            for (int i = 0; i < pausePanel.transform.childCount; i++)
                pausePanel.transform.GetChild(i).gameObject.SetActive(false);
            pausePanel.SetActive(false);
        }
    }

    // Opción para un botón "Salir al menú"
    public void QuitToMenu(string menuScene)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }
}

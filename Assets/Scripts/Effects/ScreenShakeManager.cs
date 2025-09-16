using UnityEngine;
using Unity.Cinemachine;

public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance { get; private set; }

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
            Debug.LogError("❌ Falta CinemachineImpulseSource en el objeto ScreenShakeManager");
    }

    /// <summary>
    /// Genera un screen shake usando Cinemachine Impulse.
    /// </summary>
    /// <param name="intensity">Magnitud de la sacudida.</param>
    public void Shake(float intensity = 1f)
    {
        if (impulseSource == null) return;

        // Cinemachine 3.x: GenerateImpulse ya escala la fuerza directamente
        impulseSource.GenerateImpulse(intensity);
    }
}

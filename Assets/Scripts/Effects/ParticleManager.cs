using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [Header("Prefab de Partícula")]
    [SerializeField] private ParticleSystem hitEffectPrefab;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 10;

    private List<ParticleSystem> pool = new List<ParticleSystem>();

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Crear el pool
        for (int i = 0; i < poolSize; i++)
        {
            var ps = Instantiate(hitEffectPrefab, transform);
            ps.Stop();
            ps.gameObject.SetActive(false);
            pool.Add(ps);
        }
    }

    /// <summary>
    /// Activa un efecto de golpe en la posición indicada, con el color deseado.
    /// </summary>
    public void SpawnHitEffect(Vector3 position, Color color)
    {
        ParticleSystem ps = GetAvailableParticle();

        if (ps == null)
        {
            Debug.LogWarning("⚠️ No hay partículas disponibles, se instanciará una nueva.");
            ps = Instantiate(hitEffectPrefab, transform);
            pool.Add(ps);
        }

        var main = ps.main;
        main.startColor = color;

        ps.transform.position = position;
        ps.gameObject.SetActive(true);
        ps.Play();

        StartCoroutine(DisableAfterSeconds(ps, main.duration));
    }

    private ParticleSystem GetAvailableParticle()
    {
        foreach (var ps in pool)
        {
            if (!ps.isPlaying)
                return ps;
        }
        return null;
    }

    private System.Collections.IEnumerator DisableAfterSeconds(ParticleSystem ps, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ps.Stop();
        ps.gameObject.SetActive(false);
    }
}

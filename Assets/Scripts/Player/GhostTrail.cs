using System.Collections.Generic;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeTime = 0.5f;

    // 🔹 Pool estático
    private static Queue<GhostTrail> pool = new Queue<GhostTrail>();

    private float timer;

    /// <summary>
    /// Obtener un ghost desde el pool o instanciar uno nuevo.
    /// </summary>
    public static GhostTrail GetGhost(GhostTrail prefab)
    {
        GhostTrail ghost;
        if (pool.Count > 0)
        {
            ghost = pool.Dequeue();
            ghost.gameObject.SetActive(true);
        }
        else
        {
            ghost = Instantiate(prefab);
        }
        return ghost;
    }

    /// <summary>
    /// Inicializar el ghost cuando se spawnea.
    /// </summary>
    public void Init(Sprite sprite, Vector3 position, Vector3 scale, Color color, float duration = -1f)
    {
        transform.position = position;
        transform.localScale = scale;
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        timer = duration > 0 ? duration : fadeTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(timer / fadeTime);
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;

        if (timer <= 0f)
        {
            // En lugar de Destroy, vuelve al pool
            gameObject.SetActive(false);
            pool.Enqueue(this);
        }
    }
}


using System;
using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [ColorUsage(true,true)]
    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private float flashTime = 0.25f;
    private SpriteRenderer[] spriteRenderers;
    private Material[] materials;
    [SerializeField] private AnimationCurve flashSpeedCurve;

    private Coroutine DamageCoroutine;


    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        Init();
    }

    private void Init()
    {
        materials = new Material[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            materials[i] = spriteRenderers[i].material;
        }
    }

    public void CallDamageFlash()
    {
        DamageCoroutine = StartCoroutine(DamageFlasher());
    }

    private IEnumerator DamageFlasher()
    {
        SetFlashColor();
       
        float currentFlashAmount = 0;
        float elapsedTime = 0;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;
            currentFlashAmount = Mathf.Lerp(1f, flashSpeedCurve.Evaluate(elapsedTime), (elapsedTime / flashTime));
            SetFlashAmount(currentFlashAmount);
            yield return null;
        }
    }

    private void SetFlashColor()
    {
        for (int i = 0;i < materials.Length; i++)
        {
            materials[i].SetColor("_FlashColor", _flashColor);
        }

    }

    private void SetFlashAmount(float amount)
    {
        for(int i = 0;i< materials.Length ; i++)
        {
            materials[i].SetFloat("_FlashAmount", amount);
        }   
    }

    private void OnEnable()
    {
        SetFlashAmount(0);
        SetFlashColor();
    }
}

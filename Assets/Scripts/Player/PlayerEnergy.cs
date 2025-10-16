using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class PlayerEnergy : MonoBehaviour
{
    private int currentEnergy;
    private Coroutine regenCoroutine;

    [SerializeField] private int maxEnergy = 3;
    [SerializeField] private float energyDrainRate = 1f;
    [SerializeField] private float energyRegenDelay = 2f; 
    [SerializeField] private float energyRegenRate = 1f;
    [SerializeField] private Image[] imageEnergy;

    private void Start()
    {
        currentEnergy = maxEnergy;
    }

    public bool HasEnergy => currentEnergy > 0;

    private Coroutine drainCoroutine;
    public void ConsumeWhileRunning(bool isRunning)
    {
        if (isRunning && HasEnergy)
        {
            if (drainCoroutine == null)
                drainCoroutine = StartCoroutine(DrainEnergy());

            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = null;
            }
        }
        else if (!isRunning && drainCoroutine != null)
        {
            StopCoroutine(drainCoroutine);
            drainCoroutine = null;

            if (regenCoroutine == null)
                regenCoroutine = StartCoroutine(RegenEnergy());
        }
    }

    private IEnumerator DrainEnergy()
    {
        while (true)
        {
            yield return new WaitForSeconds(energyDrainRate);
            if (currentEnergy > 0)
            {
                currentEnergy--;
                UpdateUI();
            }
            else
            {
                break;
            }
        }
    }

    private IEnumerator RegenEnergy()
    {
        yield return new WaitForSeconds(energyRegenDelay);

        while (currentEnergy < maxEnergy)
        {
            yield return new WaitForSeconds(energyRegenRate);
            currentEnergy++;
            UpdateUI();
        }

        regenCoroutine = null;
    }
    private void UpdateUI()
    {
        for (int i = 0; i < imageEnergy.Length; i++)
        {
            bool activo = i < currentEnergy;
            if (imageEnergy[i] != null)
                imageEnergy[i].gameObject.SetActive(activo);
        }
    }
}

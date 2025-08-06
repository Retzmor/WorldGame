using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class ButtonBounceDOTween : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rect;
    private Vector2 originalPos;
    private Shadow shadowButton;

    [Header("Animaci�n")]
    public Vector2 offset = new Vector2(4f, -2f);
    public float duration = 0.1f;
    public Ease ease = Ease.OutQuad;
    

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
        shadowButton = GetComponent<Shadow>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        rect.DOKill(); // Detiene cualquier animaci�n previa

        // Mueve el bot�n al offset
        rect.DOAnchorPos(originalPos + offset, duration)
            .SetEase(ease);
        shadowButton.enabled = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        rect.DOKill(); // Detiene cualquier animaci�n previa

        // Devuelve el bot�n a su posici�n original
        rect.DOAnchorPos(originalPos, duration)
            .SetEase(ease);
        shadowButton.enabled = true;
    }
}

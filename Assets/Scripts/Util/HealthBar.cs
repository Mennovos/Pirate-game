using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private bool useFillAmount;
    [SerializeField] private Image image;
    
    private float maxWidth;

    private void Awake()
    {
        maxWidth = image.rectTransform.rect.width;
    }

    public void setHealth(float health)
    {
        float clampedHealth = Mathf.Clamp01(health);
        
        if (useFillAmount)
        {
            image.fillAmount = clampedHealth;
        }
        else
        {
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clampedHealth * maxWidth);
        }
    }
}

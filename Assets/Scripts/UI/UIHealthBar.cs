using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private float smoothSpeed = 5f;
    
    private float _targetFill = 1f;
    private float _currentFill = 1f;
    
    private void Awake()
    {
        // Make sure we have a fill image
        if (fillImage == null)
        {
            fillImage = GetComponentInChildren<Image>();
            if (fillImage == null)
            {
                Debug.LogError("No fill image found in UIHealthBar!");
            }
        }
    }
    

    
    /// <summary>
    /// Updates the health bar fill amount (0-1 range)
    /// </summary>
    public void UpdateFillAmount(float fillAmount)
    {
        _targetFill = Mathf.Clamp01(fillAmount);
    }
    
    /// <summary>
    /// Sets the fill amount immediately without animation
    /// </summary>
    public void SetFillAmountImmediate(float fillAmount)
    {
        _targetFill = _currentFill = Mathf.Clamp01(fillAmount);
        UpdateFillVisual(_currentFill);
    }
    
    /// <summary>
    /// Resets the fill to full
    /// </summary>
    public void ResetFill()
    {
        SetFillAmountImmediate(1f);
    }
    
    private void UpdateFillVisual(float amount)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = amount;
            
            // Update color based on gradient
            if (colorGradient != null)
            {
                fillImage.color = colorGradient.Evaluate(amount);
            }
        }
    }
    
    
    public void Initialize(float maxHealth)
    {
        // Set the initial fill amount to full
        _currentFill = 1f;
        UpdateFillVisual(_currentFill);
    }
}
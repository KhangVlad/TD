using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class WaveUI : MonoBehaviour
{
    [Header("Wave Info")]
    [SerializeField] private TextMeshProUGUI currentWaveText;
    [SerializeField] private TextMeshProUGUI waveNameText;
    [SerializeField] private TextMeshProUGUI totalWavesText;
    [SerializeField] private TextMeshProUGUI enemiesRemainingText;
    [SerializeField] private Slider waveProgressSlider;
    
    [Header("Timer")]
    [SerializeField] private TextMeshProUGUI nextWaveTimerText;
    [SerializeField] private GameObject timerContainer;
    
    [Header("Wave Banner")]
    [SerializeField] private GameObject waveBanner;
    [SerializeField] private TextMeshProUGUI waveBannerText;
    [SerializeField] private float bannerDisplayTime = 3f;
    
    [Header("Controls")]
    [SerializeField] private Button startWaveButton;
    [SerializeField] private GameObject startWaveContainer;
    
    [Header("Resources")]
    [SerializeField] private TextMeshProUGUI resourcesText;
    
    private WaveManager _waveManager;
    private float _nextWaveTimer = 0f;
    private Coroutine _bannerCoroutine;
    
    private void Start()
    {
        // Get references
        _waveManager = WaveManager.Instance;
        
        if (_waveManager == null)
        {
            Debug.LogError("WaveManager instance not found!");
            return;
        }
        
        // Set up event listeners
        _waveManager.onWaveStart.AddListener(OnWaveStart);
        _waveManager.onWaveComplete.AddListener(OnWaveComplete);
        _waveManager.onLevelComplete.AddListener(OnLevelComplete);
        _waveManager.onWaveProgress.AddListener(OnWaveProgress);
        
        if (GameDataManager.Instance != null)
        {
          
        }
        
        // Set up button
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(OnStartWaveButtonClicked);
        }
        
        // Initialize UI
        InitializeUI();
        
        // Hide banner initially
        if (waveBanner != null)
        {
            waveBanner.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (_waveManager != null)
        {
            _waveManager.onWaveStart.RemoveListener(OnWaveStart);
            _waveManager.onWaveComplete.RemoveListener(OnWaveComplete);
            _waveManager.onLevelComplete.RemoveListener(OnLevelComplete);
            _waveManager.onWaveProgress.RemoveListener(OnWaveProgress);
        }
        
        if (GameDataManager.Instance != null)
        {
            
        }
        
        if (startWaveButton != null)
        {
            startWaveButton.onClick.RemoveListener(OnStartWaveButtonClicked);
        }
    }
    
    private void Update()
    {
        // Update next wave timer
        if (_waveManager != null && !_waveManager.IsWaveInProgress)
        {
            _nextWaveTimer -= Time.deltaTime;
            if (_nextWaveTimer <= 0f)
            {
                _nextWaveTimer = 0f;
            }
            
            UpdateTimerText();
        }
    }
    
    private void InitializeUI()
    {
        if (_waveManager == null)
            return;
        
        // Set total waves
        if (totalWavesText != null)
        {
            totalWavesText.text = _waveManager.TotalWaves.ToString();
        }
        
        // Initialize current wave text
        if (currentWaveText != null)
        {
            currentWaveText.text = "0";
        }
        
        // Initialize wave progress
        if (waveProgressSlider != null)
        {
            waveProgressSlider.value = 0f;
        }
        
        // Initialize enemies remaining
        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text = "0";
        }
        
        // Initialize timer
        if (timerContainer != null)
        {
            timerContainer.SetActive(false);
        }
        
        // Initialize wave name
        if (waveNameText != null)
        {
            waveNameText.text = "";
        }
        
        // Set start wave button visibility
        UpdateStartWaveButtonVisibility();
    }
    
    private void UpdateTimerText()
    {
        if (nextWaveTimerText != null)
        {
            int seconds = Mathf.CeilToInt(_nextWaveTimer);
            nextWaveTimerText.text = seconds.ToString();
        }
    }
    
    private void UpdateStartWaveButtonVisibility()
    {
        if (startWaveContainer == null || _waveManager == null)
            return;
            
        bool showButton = !_waveManager.IsWaveInProgress && !_waveManager.IsLastWave;
        startWaveContainer.SetActive(showButton);
    }
    
    private void UpdateResourcesText(int amount)
    {
        if (resourcesText != null)
        {
            resourcesText.text = amount.ToString();
        }
    }
    
    #region Event Handlers
    
    private void OnStartWaveButtonClicked()
    {
        if (_waveManager != null && !_waveManager.IsWaveInProgress)
        {
            _waveManager.StartNextWave();
        }
    }
    
    private void OnWaveStart()
    {
        // Update current wave text
        if (currentWaveText != null)
        {
            currentWaveText.text = (_waveManager.CurrentWaveIndex + 1).ToString();
        }
        
        // Show wave banner
        if (waveBanner != null && waveBannerText != null)
        {
            Wave currentWave = _waveManager.GetCurrentWaveData();
            if (currentWave != null)
            {
                waveBannerText.text = $"Wave {_waveManager.CurrentWaveIndex + 1}: {currentWave.waveName}";
                
                // Update wave name text
                if (waveNameText != null)
                {
                    waveNameText.text = currentWave.waveName;
                }
                
                // Show banner with animation
                ShowWaveBanner();
            }
        }
        
        // Hide timer
        if (timerContainer != null)
        {
            timerContainer.SetActive(false);
        }
        
        // Hide start wave button
        UpdateStartWaveButtonVisibility();
    }
    
    private void OnWaveComplete()
    {
        // Show timer for next wave
        if (_waveManager != null && !_waveManager.IsLastWave)
        {
            _nextWaveTimer = _waveManager.GetTimeBetweenWaves();
            
            if (timerContainer != null)
            {
                timerContainer.SetActive(true);
            }
            
            UpdateTimerText();
        }
        
        // Update start wave button visibility
        UpdateStartWaveButtonVisibility();
    }
    
    private void OnLevelComplete()
    {
        // Hide timer
        if (timerContainer != null)
        {
            timerContainer.SetActive(false);
        }
        
        // Hide start wave button
        if (startWaveContainer != null)
        {
            startWaveContainer.SetActive(false);
        }
        
        // Show level complete message
        if (waveBanner != null && waveBannerText != null)
        {
            waveBannerText.text = "Level Complete!";
            ShowWaveBanner(false); // Don't auto-hide
        }
    }
    
    private void OnWaveProgress(int current, int total)
    {
        // Update enemies remaining
        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text = (total - current).ToString();
        }
        
        // Update progress slider
        if (waveProgressSlider != null && total > 0)
        {
            waveProgressSlider.value = (float)current / total;
        }
    }
    
    #endregion
    
    private void ShowWaveBanner(bool autoHide = true)
    {
        if (waveBanner == null)
            return;
            
        // Stop existing coroutine if running
        if (_bannerCoroutine != null)
        {
            StopCoroutine(_bannerCoroutine);
        }
        
        // Show banner
        waveBanner.SetActive(true);
        
        // Auto-hide after delay if requested
        if (autoHide)
        {
            _bannerCoroutine = StartCoroutine(HideBannerAfterDelay());
        }
    }
    
    private IEnumerator HideBannerAfterDelay()
    {
        yield return new WaitForSeconds(bannerDisplayTime);
        
        if (waveBanner != null)
        {
            waveBanner.SetActive(false);
        }
        
        _bannerCoroutine = null;
    }
}
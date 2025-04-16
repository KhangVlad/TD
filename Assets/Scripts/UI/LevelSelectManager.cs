using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private Button levelButtonPrefab;
    [SerializeField] private Button backButton;
    
    [Header("Level Info Panel")]
    [SerializeField] private GameObject levelInfoPanel;
    [SerializeField] private Image levelThumbnail;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI levelDescriptionText;
    [SerializeField] private TextMeshProUGUI waveCountText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private Button playButton;
    
    private List<LevelDataSO> _availableLevels = new List<LevelDataSO>();
    private LevelDataSO _selectedLevel;
    private int _selectedLevelIndex = -1;
    
    private void Start()
    {
        // Initialize UI
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        
        // Hide level info panel initially
        if (levelInfoPanel != null)
        {
            levelInfoPanel.SetActive(false);
        }
        
        // Load available levels
        LoadLevels();
        
        // Generate level buttons
        GenerateLevelButtons();
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
        
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }
    }
    
    private void LoadLevels()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager instance not found!");
            return;
        }
        
        _availableLevels.Clear();
        
        // Get total level count
        int totalLevels = GameDataManager.Instance.GetTotalLevelCount();
        
        // Load each level
        for (int i = 0; i < totalLevels; i++)
        {
            LevelDataSO level = GameDataManager.Instance.GetLevelData(i);
            if (level != null)
            {
                _availableLevels.Add(level);
            }
        }
        
        Debug.Log($"Loaded {_availableLevels.Count} levels");
    }
    
    private void GenerateLevelButtons()
    {
        if (levelButtonContainer == null || levelButtonPrefab == null)
        {
            Debug.LogError("Level button container or prefab not assigned!");
            return;
        }
        
        // Clear existing buttons
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create button for each level
        for (int i = 0; i < _availableLevels.Count; i++)
        {
            LevelDataSO level = _availableLevels[i];
            Button levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
            
            // Set button text
            TextMeshProUGUI buttonText = levelButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = level.levelName;
            }
            
            // Set button image if available
            Image buttonImage = levelButton.GetComponent<Image>();
            if (buttonImage != null && level.levelThumbnail != null)
            {
                buttonImage.sprite = level.levelThumbnail;
            }
            
            // Set interactable based on unlocked status
            bool isUnlocked = GameDataManager.Instance.IsLevelUnlocked(i);
            levelButton.interactable = isUnlocked;
            
            // Add lock icon if locked
            Transform lockIcon = levelButton.transform.Find("LockIcon");
            if (lockIcon != null)
            {
                lockIcon.gameObject.SetActive(!isUnlocked);
            }
            
            // Add click event
            int levelIndex = i; // Capture for lambda
            levelButton.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
        }
    }
    
    private void OnLevelButtonClicked(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _availableLevels.Count)
            return;
            
        _selectedLevel = _availableLevels[levelIndex];
        _selectedLevelIndex = levelIndex;
        
        // Show level info panel
        ShowLevelInfo(_selectedLevel);
    }
    
    private void ShowLevelInfo(LevelDataSO level)
    {
        if (levelInfoPanel == null || level == null)
            return;
            
        // Set level information
        if (levelNameText != null)
        {
            levelNameText.text = $"Level {level.levelNumber}: {level.levelName}";
        }
        
        if (levelDescriptionText != null)
        {
            levelDescriptionText.text = level.levelDescription;
        }
        
        if (levelThumbnail != null && level.levelThumbnail != null)
        {
            levelThumbnail.sprite = level.levelThumbnail;
        }
        
        if (waveCountText != null)
        {
            waveCountText.text = $"Waves: {level.waves.Count}";
        }
        
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Enemies: {level.GetTotalMonsterCount()}";
        }
        
        if (durationText != null)
        {
            float duration = level.GetApproximateLevelDuration();
            int minutes = Mathf.FloorToInt(duration / 60);
            int seconds = Mathf.FloorToInt(duration % 60);
            durationText.text = $"Duration: ~{minutes}:{seconds:00}";
        }
        
        // Show the panel
        levelInfoPanel.SetActive(true);
    }
    
    private void OnPlayButtonClicked()
    {
        if (_selectedLevel == null || _selectedLevelIndex < 0)
            return;
            
        // Set the current level in GameDataManager
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SetCurrentLevel(_selectedLevelIndex);
        }
        
        // Load the game scene
        SceneManager.LoadScene("GameScene");
    }
    
    private void OnBackButtonClicked()
    {
        // Navigate back to the main menu
        SceneManager.LoadScene("MainMenu");
    }
}
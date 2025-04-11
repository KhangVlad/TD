using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuidingSlot : MonoBehaviour
{
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image towerImage;
    [SerializeField] private TextMeshProUGUI cost;
    private TowerSO towerSO;
    public event Action<TowerSO> OnUpgrade;

    private void Start()
    {
        upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
    }

    private void OnDestroy()
    {
        upgradeButton.onClick.RemoveListener(OnUpgradeButtonClick);
    }

    private void OnUpgradeButtonClick()
    {
        OnUpgrade?.Invoke(towerSO);
    }


    public void Initialize(TowerSO so)
    {
        towerSO = so;
        towerImage.sprite = so.sprite;
        cost.text = so.cost.ToString();
    }
}
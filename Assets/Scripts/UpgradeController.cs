using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;  

public class UpgradeController : MonoBehaviour
{
    [SerializeField] private CarController carController;
    [SerializeField] private Text[] upgradeButtons; // Array of UI Text components for displaying upgrades
    [SerializeField] private LeanButton[] upgradeButtonControls; // Corresponding LeanButtons that trigger the upgrades

    private List<string> possibleUpgrades = new List<string>
    {
        "Engine Boost",  // Increases top speed
        "Advanced Drivetrain", // Improves responsiveness of drifting
        "Turbocharger", // Increases acceleration
        "Reinforced Tires",  // Offers better grip and reduces the impact of slipping
        "High-Performance Brakes", // Reduces stopping distance and improves control when stopping quickly
    };

    void OnEnable()
    {
        GameManager.OnRoundEnd += GenerateRandomUpgrades; 
    }

    void OnDisable()
    {
        GameManager.OnRoundEnd -= GenerateRandomUpgrades; 
    }

    private void Start()
    {
        GenerateRandomUpgrades();
        InitializeButtonIndices();
    }

    private void GenerateRandomUpgrades()
    {
        if (upgradeButtons.Length < 3 || possibleUpgrades.Count < 3)
        {
            Debug.LogError("Not enough UI buttons or upgrades defined.");
            return;
        }

        List<string> selectedUpgrades = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            string upgrade;
            do
            {
                upgrade = possibleUpgrades[UnityEngine.Random.Range(0, possibleUpgrades.Count)];
            }
            while (selectedUpgrades.Contains(upgrade));

            selectedUpgrades.Add(upgrade);
            upgradeButtons[i].text = upgrade;  
        }
    }

    private void InitializeButtonIndices()
    {
        for (int i = 0; i < upgradeButtonControls.Length; i++)
        {
            int index = i; 
            upgradeButtonControls[i].OnClick.RemoveAllListeners(); 
            upgradeButtonControls[i].OnClick.AddListener(() => ApplyUpgrade(index));
        }
    }

    public void ApplyUpgrade(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < upgradeButtons.Length)
        {
            string selectedUpgrade = upgradeButtons[buttonIndex].text;
            Debug.Log($"Applying upgrade: {selectedUpgrade} at index {buttonIndex}");
            carController.ApplyUpgrade(selectedUpgrade);
        }
    }
}

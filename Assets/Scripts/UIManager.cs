using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject upgradeScreen; 
    [SerializeField] private GameObject gameOverScreen;
    private void OnEnable()
    {
        GameManager.OnRoundEndTimed += DisplayUpgrades;
        GameManager.OnRoundStart += HideUpgrades;
        GameManager.OnRoundStart += HideGameOver;
        GameManager.OnGameOverTimed += DisplayGameOver;
    }
    
    private void OnDisable()
    {
        GameManager.OnRoundEndTimed -= DisplayUpgrades;
        GameManager.OnRoundStart -= HideUpgrades;
        GameManager.OnRoundStart -= HideGameOver;
        GameManager.OnGameOverTimed -= DisplayGameOver;
    }
    
    private IEnumerator DisplayUpgrades()
    {
        yield return new WaitForSecondsRealtime(4);
        upgradeScreen.SetActive(true);
    }

    private void HideUpgrades()
    {
        upgradeScreen.SetActive(false);
    }

    private IEnumerator DisplayGameOver()
    {
        yield return new WaitForSecondsRealtime(4);
        gameOverScreen.SetActive(true);
    }
    
    private void HideGameOver()
    {
        gameOverScreen.SetActive(false);
    }
}

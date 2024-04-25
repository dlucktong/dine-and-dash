using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject upgradeScreen; 
    [SerializeField] private GameObject gameOverScreen;
    void OnEnable()
    {
        GameManager.RoundEndTimed += DisplayUpgrades;
        GameManager.RoundStart += HideUpgrades;
        GameManager.RoundStart += HideGameOver;
        GameManager.GameOverTimed += DisplayGameOver;
    }
    
    void OnDisable()
    {
        GameManager.RoundEndTimed -= DisplayUpgrades;
        GameManager.RoundStart -= HideUpgrades;
        GameManager.RoundStart -= HideGameOver;
        GameManager.GameOverTimed -= DisplayGameOver;
    }
    
    IEnumerator DisplayUpgrades()
    {
        yield return new WaitForSecondsRealtime(4);
        upgradeScreen.SetActive(true);
    }

    void HideUpgrades()
    {
        upgradeScreen.SetActive(false);
    }

    IEnumerator DisplayGameOver()
    {
        yield return new WaitForSecondsRealtime(4);
        gameOverScreen.SetActive(true);
    }
    
    void HideGameOver()
    {
        gameOverScreen.SetActive(false);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime;
    
    // Start is called before the first frame update
    public void StartGame()
    {
        StartCoroutine(LoadLevel(1));
    }

    public void StartTutorial()
    {
        StartCoroutine(LoadLevel(2));
    }

    public void ReturnToMenu()
    {
        StartCoroutine(LoadLevel(0));
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSecondsRealtime(transitionTime);
        
        SceneManager.LoadScene(levelIndex);
    }
}

using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.OnGameStart += RemoveTimers;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= RemoveTimers;
    }

    private void RemoveTimers()
    {
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}

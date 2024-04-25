using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.GameStart += RemoveTimers;
    }

    private void OnDisable()
    {
        GameManager.GameStart -= RemoveTimers;
    }

    private void RemoveTimers()
    {
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}

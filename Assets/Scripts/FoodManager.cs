using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public int foodCarrying;

    private void OnEnable()
    {
        GameManager.OnGameStart += ResetFood;
    }
    private void OnDisable()
    {
        GameManager.OnGameStart -= ResetFood;
    }

    public void RemoveTop()
    {
        foodCarrying -= 1;
        Destroy(transform.GetChild(transform.childCount - 1).gameObject);;
    }

    private void ResetFood()
    {
        while(foodCarrying > 0)
        {
            RemoveTop();
        }
    }
}

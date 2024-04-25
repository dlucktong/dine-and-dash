using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public int foodCarrying;

    private void OnEnable()
    {
        GameManager.GameStart += ResetFood;
    }
    private void OnDisable()
    {
        GameManager.GameStart -= ResetFood;
    }

    public void RemoveTop()
    {
        foodCarrying -= 1;
        Destroy(transform.GetChild(transform.childCount - 1).gameObject);;
    }

    public void ResetFood()
    {
        while(foodCarrying > 0)
        {
            RemoveTop();
        }
    }
}

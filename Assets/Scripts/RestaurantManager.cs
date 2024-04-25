using UnityEngine;

public class RestaurantManager : MonoBehaviour
{
    [SerializeField] private FoodManager fm;
    [SerializeField] private GameObject pizzaBox;
    
    private delegate void OnRemoveAll();
    private static event OnRemoveAll RemoveAll;
    private delegate void OnSpawnAll();
    private static event OnSpawnAll SpawnAll;

    private void OnEnable()
    {
        GameManager.GameStart += RemoveAllFood;
        RemoveAll += Remove;
        SpawnAll += SpawnFood;
    }

    private void OnDisable()
    {
        GameManager.GameStart -= RemoveAllFood;
        RemoveAll -= Remove;
        SpawnAll -= SpawnFood;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            while (transform.childCount > 0)
            {
                Vector3 pos = fm.transform.position;
                pos.y += 0.3f * fm.foodCarrying;
                transform.GetChild(0).position = pos;
                transform.GetChild(0).parent = fm.transform;
                fm.foodCarrying += 1;
            }
            RemoveAll?.Invoke();
        }
    }

    public void SpawnFoodAtAllLocations()
    {
        SpawnAll?.Invoke();
    }

    public void SpawnFood()
    {
        Vector3 pos = transform.position;
        pos.y += transform.childCount * 0.3f;
        Instantiate(pizzaBox, pos, Quaternion.identity, transform);
    }

    private void RemoveAllFood()
    {
        RemoveAll?.Invoke();
    }

    private void Remove()
    {
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}

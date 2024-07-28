using System;
using UnityEngine;

public class RestaurantManager : MonoBehaviour
{
    [SerializeField] private FoodManager fm;
    [SerializeField] private GameObject pizzaBox;
    [SerializeField] private Target target;

    private delegate void RemoveAll();

    private static event RemoveAll OnRemoveAll;

    private delegate void SpawnAll();

    private static event SpawnAll OnSpawnAll;


    private void OnEnable()
    {
        GameManager.OnGameStart += RemoveAllFood;
        OnRemoveAll += Remove;
        OnSpawnAll += SpawnFood;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= RemoveAllFood;
        OnRemoveAll -= Remove;
        OnSpawnAll -= SpawnFood;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            while (transform.childCount > 0)
            {
                Vector3 pos = fm.transform.position;
                pos.y += 0.3f * fm.foodCarrying;
                transform.GetChild(0).position = pos;
                transform.GetChild(0).parent = fm.transform;
                fm.foodCarrying += 1;
            }

            OnRemoveAll?.Invoke();
        }
    }

    public void SpawnFoodAtAllLocations()
    {
        OnSpawnAll?.Invoke();
    }

    public void SpawnFood()
    {
        Vector3 pos = transform.position;
        pos.y += transform.childCount * 0.3f;
        Instantiate(pizzaBox, pos, Quaternion.identity, transform);
    }

    private void RemoveAllFood()
    {
        OnRemoveAll?.Invoke();
    }

    private void Remove()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        target.enabled = transform.childCount != 0;
    }
}
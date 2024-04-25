using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject timerDisplay;
    [SerializeField] private Transform timerContainer;
    [SerializeField] private AudioSource chime;
    [SerializeField] private Transform player;
    [SerializeField] private List<Transform> restaurants;

    private float _restaurantDistance = Mathf.Infinity;
    private float _playerDistance;
    
    private void OnEnable()
    {
        GameManager.GameStart += SetAllInactive;
    }
    
    private void OnDisable()
    {
        GameManager.GameStart -= SetAllInactive;
    }

    public void SpawnDropoffLocation()
    {
        GameObject dropoffLocation = transform.GetChild(Random.Range(0, transform.childCount)).gameObject;
        while (dropoffLocation.activeInHierarchy)
        {
            dropoffLocation = transform.GetChild(Random.Range(0, transform.childCount)).gameObject;
        }
        
        foreach (Transform restaurant in restaurants)
        {
            _restaurantDistance = Mathf.Min(_restaurantDistance, Vector3.Distance(restaurant.position, dropoffLocation.transform.position));
        }
        _playerDistance = Vector3.Distance(player.position, dropoffLocation.transform.position);
        
        DeliveryManager deliveryManager = dropoffLocation.GetComponent<DeliveryManager>();
        
        deliveryManager.timerObject = Instantiate(timerDisplay, timerContainer);
        deliveryManager.distanceAdder = (_playerDistance + _restaurantDistance) / 20;
        dropoffLocation.SetActive(true);
        chime.Play();
    }

    private void SetAllInactive()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
            DeliveryManager deliveryManager = transform.GetChild(i).GetComponent<DeliveryManager>();
        
            deliveryManager.timerObject = null;
        }
    }
}

using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public bool hit = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            hit = true;
        }
    }
}

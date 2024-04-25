using UnityEngine;

public class MinimapScript : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private RectTransform minimapContent;
    [SerializeField] private RectTransform minimapView;
    private void FixedUpdate()
    {
        Vector3 playerPos = player.position;
        minimapContent.anchoredPosition = new Vector2(-(playerPos.x - 224) * 786*2f/1360, 
        -(playerPos.z - 168) * 786*2f/1344);
        
        minimapView.rotation = Quaternion.Euler(0,0 ,player.rotation.eulerAngles.y);
        
    }
}
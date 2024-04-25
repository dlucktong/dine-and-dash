using UnityEngine;
using UnityEngine.UI;

public class TimerColor : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image image;
    [SerializeField] private Gradient colors;
    
    // Update is called once per frame
    void Update()
    {
        image.color = colors.Evaluate(slider.value);
    }
}

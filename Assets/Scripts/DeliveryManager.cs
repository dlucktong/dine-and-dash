using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DeliveryManager : MonoBehaviour
{
    [SerializeField] private FoodManager foodManager;
    [SerializeField] private Transform player;
    [SerializeField] private Gradient colors;
    public float timeRemaining;
    public GameObject timerObject;
    public GameObject minimapIconPrefab;
    public Transform minimap;

    private Slider slider;
    private TextMeshProUGUI distanceText;
    private TextMeshProUGUI timeText;
    private Renderer spawnRenderer;
    private Target target;
    private Image icon;
    private GameObject indicator;
    private bool gameOver = false;
    private float totalTime;
    public float distanceAdder = 1;
    [FormerlySerializedAs("_paused")] public bool paused = false;
    private GameManager gm;
    
    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        GameManager.OnPause += Pause;
    }

    private void OnDisable()
    {
        GameManager.OnPause -= Pause;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && foodManager.foodCarrying > 0)
        {
            foodManager.RemoveTop();
            transform.gameObject.SetActive(false);
            Destroy(timerObject);
            Destroy(indicator);
            GameManager.DeliveriesMade += 1;
            Stats.Deliveries += 1;

            int tipIndex = Mathf.FloorToInt(timeRemaining / (totalTime / GameManager.tipValues.Length));

            GameManager.Tips += GameManager.tipValues[tipIndex];
            GameManager.DeliverySound.Play();
        }
    }

    private void OnEnable()
    {
        slider = timerObject.GetComponent<Slider>();
        indicator = Instantiate(minimapIconPrefab, minimap);
        indicator.transform.localPosition = new Vector2((transform.position.x - 224) * 786 / 1360,(transform.position.z - 168) * 786 / 1344);
        icon = indicator.GetComponent<Image>();
        var texts = timerObject.GetComponentsInChildren<TextMeshProUGUI>();
        timeText = texts[0];
        distanceText = texts[1];
        spawnRenderer = transform.GetComponent<Renderer>();
        target = transform.GetComponent<Target>();
        totalTime = GameManager.DeliveryTime + distanceAdder;
        timeRemaining = totalTime;
    }

    private void Update()
    {
        if (!gameOver && !paused)
        {
            if (timeRemaining > 0)
            {
                Vector3 playerPosition = player.position;

                Color currentColor = colors.Evaluate(timeRemaining / totalTime);

                spawnRenderer.material.SetColor("_EmissionColor", currentColor);
                target.TargetColor = currentColor;
                icon.color = currentColor;

                timeRemaining -= Time.deltaTime;

                slider.value = timeRemaining / totalTime;
                timeText.text = "" + Mathf.Round(timeRemaining) + "s";
                distanceText.text = "" + Mathf.Round(Vector3.Distance(playerPosition, transform.position)) + "m";
            }
            else
            {
                StartCoroutine(gm.handleGameOver());
                gameOver = true;
            }
        
        }

    }

    private void Pause()
    {
        paused = !paused;
    }

}

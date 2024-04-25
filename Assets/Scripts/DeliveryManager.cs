using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DeliveryManager : MonoBehaviour
{
    [SerializeField] private FoodManager foodManager;
    [SerializeField] private Transform player;
    [SerializeField] private Gradient colors;
    [SerializeField] private GameManager gameManager;
    public float timeRemaining;
    public GameObject timerObject;
    public GameObject minimapIconPrefab;
    public Transform minimap;

    private Slider _slider;
    private TextMeshProUGUI _distanceText;
    private TextMeshProUGUI _timeText;
    private Renderer _renderer;
    private Target _target;
    private Image _icon;
    private GameObject _indicator;
    private bool _gameOver = false;
    private float _totalTime;
    public float distanceAdder = 1;
    [FormerlySerializedAs("_paused")] public bool paused = false;


    private void Start()
    {
        GameManager.Pause += Pause;
    }

    private void OnDisable()
    {
        GameManager.Pause -= Pause;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null && foodManager.foodCarrying > 0)
        {
            foodManager.RemoveTop();
            transform.gameObject.SetActive(false);
            Destroy(timerObject);
            Destroy(_indicator);
            gameManager.deliveriesMade += 1;
            Stats.Deliveries += 1;

            int tipIndex = Mathf.FloorToInt(timeRemaining / (_totalTime / gameManager.tipValues.Length));

            gameManager.tips += gameManager.tipValues[tipIndex];
            gameManager.deliverySound.Play();
        }
    }

    private void OnEnable()
    {
        _slider = timerObject.GetComponent<Slider>();
        _indicator = Instantiate(minimapIconPrefab, minimap);
        _indicator.transform.localPosition = new Vector2((transform.position.x - 224) * 786 / 1360,(transform.position.z - 168) * 786 / 1344);
        _icon = _indicator.GetComponent<Image>();
        var texts = timerObject.GetComponentsInChildren<TextMeshProUGUI>();
        _timeText = texts[0];
        _distanceText = texts[1];
        _renderer = transform.GetComponent<Renderer>();
        _target = transform.GetComponent<Target>();
        _totalTime = gameManager.deliveryTime + distanceAdder;
        timeRemaining = _totalTime;
    }

    void Update()
    {
        if (!_gameOver && !paused)
        {
            if (timeRemaining > 0)
            {
                Vector3 playerPosition = player.position;

                Color currentColor = colors.Evaluate(timeRemaining / _totalTime);

                _renderer.material.SetColor("_EmissionColor", currentColor);
                _target.TargetColor = currentColor;
                _icon.color = currentColor;

                timeRemaining -= Time.deltaTime;

                _slider.value = timeRemaining / _totalTime;
                _timeText.text = "" + Mathf.Round(timeRemaining) + "s";
                _distanceText.text = "" + Mathf.Round(Vector3.Distance(playerPosition, transform.position)) + "m";
            }
            else
            {
                StartCoroutine(gameManager.handleGameOver());
                _gameOver = true;
            }
        
        }

    }

    private void Pause()
    {
        paused = !paused;
    }

}

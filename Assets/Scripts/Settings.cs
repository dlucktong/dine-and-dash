public class Settings
{
    public float spawnInterval;
    public float deliveriesToday;
    public float deliveryTime;

    public Settings(float interval, float today, float time)
    {
        spawnInterval = interval;
        deliveriesToday = today;
        deliveryTime = time;
    }
}
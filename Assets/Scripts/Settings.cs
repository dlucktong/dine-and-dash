public class Settings
{
    public float SpawnInterval;
    public float DeliveriesToday;
    public float DeliveryTime;

    public Settings(float interval, float today, float time)
    {
        SpawnInterval = interval;
        DeliveriesToday = today;
        DeliveryTime = time;
    }
}
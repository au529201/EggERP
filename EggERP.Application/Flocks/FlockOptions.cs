namespace EggERP.Application.Flocks;

public static class FlockOptions
{
    public static readonly string[] Species = { "Chicken", "Duck", "Quail", "Goose", "Other" };

    public static readonly string[] FlockInReasons = { "Bought", "Given", "Hatched" };
    public static readonly string[] FlockOutReasons = { "Sold", "Died", "Missing", "Given", "Culled" };

    public static readonly string[] EggInReasons = { "Laid", "Bought" };

    // "Sold" is intentionally excluded here — it's reserved for the Sale
    // workflow (Step 2), which will auto-create Out/Sold egg movements.
    public static readonly string[] EggOutReasons = { "Broken", "Stolen", "Missing", "Hatched" };
}
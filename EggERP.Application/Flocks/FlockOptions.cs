namespace EggERP.Application.Flocks;

public static class FlockOptions
{
    // "Bought" and "Sold" are intentionally excluded from these lists.
    // They are never written directly — selecting them on the Add/Remove
    // pages redirects to New Purchase / New Sale instead.
    public static readonly string[] FlockInReasons = { "Moved", "Hatched" };
    public static readonly string[] FlockOutReasons = { "Died", "Missing", "Moved", "Culled" };

    public static readonly string[] EggInReasons = { "Laid", "Moved" };
    public static readonly string[] EggOutReasons = { "Broken", "Stolen", "Missing", "Moved", "Hatched" };
}
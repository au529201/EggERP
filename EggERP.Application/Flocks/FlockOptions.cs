namespace EggERP.Application.Flocks;

// Controlled value lists for Flocks, per MVP decision to avoid a full
// lookup table. If these ever need to become admin-configurable, this
// is the single place to convert into a database-backed list.
public static class FlockOptions
{
    public static readonly string[] BirdTypes = { "Chicken", "Duck", "Quail", "Other" };

    public static readonly string[] Sources = { "Bought", "Hatched", "Given", "Other" };

    public static readonly string[] MovementReasons =
        { "Sold", "Died", "Missing", "Given Away", "Transferred", "Adjustment", "Other" };
}
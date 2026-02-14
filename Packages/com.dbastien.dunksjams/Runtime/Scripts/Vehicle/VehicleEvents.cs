using UnityEngine;

public class VehicleSpawnEvent : GameEvent
{
    public VehicleController Vehicle { get; set; }
}

public class VehicleDestroyEvent : GameEvent
{
    public VehicleController Vehicle { get; set; }
}

public class VehicleImpactEvent : GameEvent
{
    public VehicleController Vehicle { get; set; }
    public Vector3 Point { get; set; }
    public Vector3 Normal { get; set; }
    public float Impulse { get; set; }
    public GroundSurface Surface { get; set; }
}

public class VehicleGearChangeEvent : GameEvent
{
    public VehicleController Vehicle { get; set; }
    public int PreviousGear { get; set; }
    public int NewGear { get; set; }
}

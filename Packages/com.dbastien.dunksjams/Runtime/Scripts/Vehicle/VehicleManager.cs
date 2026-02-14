using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent, SingletonAutoCreate]
public class VehicleManager : SingletonEagerBehaviour<VehicleManager>
{
    readonly List<VehicleController> _vehicles = new();
    VehicleController _activeVehicle;

    public IReadOnlyList<VehicleController> Vehicles => _vehicles;
    public VehicleController ActiveVehicle => _activeVehicle;

    protected override void InitInternal() { }

    public void Register(VehicleController vehicle)
    {
        if (vehicle == null || _vehicles.Contains(vehicle)) return;
        _vehicles.Add(vehicle);

        if (_activeVehicle == null)
            SetActiveVehicle(vehicle);
    }

    public void Unregister(VehicleController vehicle)
    {
        _vehicles.Remove(vehicle);
        if (_activeVehicle == vehicle)
            _activeVehicle = _vehicles.Count > 0 ? _vehicles[0] : null;
    }

    public void SetActiveVehicle(VehicleController vehicle)
    {
        if (vehicle == null || !_vehicles.Contains(vehicle)) return;
        _activeVehicle = vehicle;
    }

    public void CycleVehicle()
    {
        if (_vehicles.Count <= 1) return;
        int idx = _vehicles.IndexOf(_activeVehicle);
        int next = (idx + 1) % _vehicles.Count;
        SetActiveVehicle(_vehicles[next]);
    }

    protected override void OnDestroy()
    {
        _vehicles.Clear();
        base.OnDestroy();
    }
}

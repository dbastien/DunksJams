using System;
using System.Collections.Generic;

//todo: largely untested
public class Inventory<T>
{
    private readonly Dictionary<T, int> _items = new(); // T could be an Item type or ID

    /// <summary>Capacity is slot-based (unique item types), not total quantity.</summary>
    public int Capacity { get; private set; }

    public int ItemCount => _items.Count;

    public event Action<T, int> OnItemAdded, OnItemRemoved;
    public event Action OnInventoryChanged;

    public Inventory(int capacity) => Capacity = capacity;

    public bool AddItem(T item, int quantity = 1)
    {
        if (quantity <= 0 || ItemCount >= Capacity) return false;

        if (!_items.TryAdd(item, quantity))
            _items[item] += quantity;

        OnItemAdded?.Invoke(item, quantity);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(T item, int quantity = 1)
    {
        if (!_items.TryGetValue(item, out int currentQuantity) || quantity <= 0) return false;

        if (currentQuantity <= quantity)
            _items.Remove(item);
        else
            _items[item] -= quantity;

        OnItemRemoved?.Invoke(item, quantity);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetQuantity(T item) => _items.TryGetValue(item, out int quantity) ? quantity : 0;

    public bool ContainsItem(T item) => _items.ContainsKey(item);

    public void Clear()
    {
        _items.Clear();
        OnInventoryChanged?.Invoke();
    }

    public IEnumerable<T> GetAllItems() => _items.Keys;

    public void SetCapacity(int newCapacity)
    {
        Capacity = newCapacity;
        if (ItemCount > Capacity)
            throw new InvalidOperationException("Current items exceed the new capacity.");
    }
}
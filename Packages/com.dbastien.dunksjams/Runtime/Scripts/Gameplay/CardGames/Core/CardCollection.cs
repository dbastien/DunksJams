using System;
using System.Collections;
using System.Collections.Generic;

public class CardCollection<T> : ICollection<T> where T : CardBase
{
    readonly List<T> _cards = new();

    public CardCollection()
    {
    }

    public CardCollection(IEnumerable<T> cards) => _cards.AddRange(cards);

    public IReadOnlyList<T> Cards => _cards;

    public T this[int index] => _cards[index];

    public void Add(T card) => _cards.Add(card);
    public void AddRange(IEnumerable<T> cards) => _cards.AddRange(cards);
    public bool Remove(T card) => _cards.Remove(card);
    public void RemoveAt(int index) => _cards.RemoveAt(index);

    public void Shuffle() => Rand.Shuffle(_cards);

    public T DrawFromTop()
    {
        if (_cards.Count == 0) throw new InvalidOperationException("No cards left.");
        var card = _cards[^1];
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }

    public T PeekTop() => _cards.Count > 0 ? _cards[^1] : throw new InvalidOperationException("No cards left.");

    public int Count => _cards.Count;
    public bool IsReadOnly => false;

    public IEnumerator<T> GetEnumerator() => _cards.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => string.Join(", ", _cards);

    public bool Contains(T item) => _cards.Contains(item);
    public void CopyTo(T[] array, int index) => _cards.CopyTo(array, index);
    public void Clear() => _cards.Clear();
}
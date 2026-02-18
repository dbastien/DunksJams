using System;
using System.Collections.Generic;
using System.Linq;

public sealed class Deck<T> where T : CardBase
{
    public CardCollection<T> DrawPile { get; }
    public CardCollection<T> DiscardPile { get; }
    public bool AutoRecycleDiscard { get; set; }
    public bool KeepTopDiscardOnRecycle { get; set; }

    public Deck(IEnumerable<T> cards = null, bool autoRecycleDiscard = false, bool keepTopDiscardOnRecycle = false)
    {
        DrawPile = new CardCollection<T>(cards ?? Enumerable.Empty<T>());
        DiscardPile = new CardCollection<T>();
        AutoRecycleDiscard = autoRecycleDiscard;
        KeepTopDiscardOnRecycle = keepTopDiscardOnRecycle;
    }

    public int Count => DrawPile.Count;
    public int DiscardCount => DiscardPile.Count;

    public void Shuffle() => DrawPile.Shuffle();

    public T Draw()
    {
        if (DrawPile.Count == 0)
        {
            if (!AutoRecycleDiscard || DiscardPile.Count == 0)
                throw new InvalidOperationException("No cards left in draw pile.");

            RecycleDiscardIntoDraw();
        }

        return DrawPile.DrawFromTop();
    }

    public IEnumerable<T> Draw(int count)
    {
        for (var i = 0; i < count; ++i) yield return Draw();
    }

    public void Discard(T card) => DiscardPile.Add(card);

    public void DiscardRange(IEnumerable<T> cards) => DiscardPile.AddRange(cards);

    public void RecycleDiscardIntoDraw()
    {
        bool keepTop = KeepTopDiscardOnRecycle && DiscardPile.Count > 0;
        T top = keepTop ? DiscardPile.DrawFromTop() : null;

        DrawPile.AddRange(DiscardPile);
        DiscardPile.Clear();
        Shuffle();

        if (keepTop && top != null)
            DiscardPile.Add(top);
    }
}
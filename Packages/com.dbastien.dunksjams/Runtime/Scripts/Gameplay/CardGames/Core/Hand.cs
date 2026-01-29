using System.Collections.Generic;

public class Hand<T> : CardCollection<T> where T : CardBase
{
    public Hand() { }
    public Hand(IEnumerable<T> cards) : base(cards) { }
}

using System.Collections.Generic;
using NUnit.Framework;

public class CombinatoricsTests : TestBase
{
    [Test] public void PermutationWithoutDuplication_Count()
    {
        var result = Combinatorics.PermutationWithoutDuplication(new List<int> { 1, 2, 3 });
        Eq(6, result.Count); // 3! = 6
    }

    [Test] public void PermutationWithDuplication_Count()
    {
        var result = Combinatorics.PermutationWithDuplication(new List<int> { 1, 2 }, 3);
        Eq(8, result.Count); // 2^3 = 8
    }

    [Test] public void CombinationWithoutDuplication_Count()
    {
        var result = Combinatorics.CombinationWithoutDuplication(new List<int> { 1, 2, 3, 4 }, 2);
        Eq(6, result.Count); // C(4,2) = 6
    }

    [Test] public void CombinationWithDuplication_Count()
    {
        var result = Combinatorics.CombinationWithDuplication(new List<int> { 1, 2, 3 }, 2);
        Eq(6, result.Count); // C(3+2-1,2) = 6
    }

    [Test] public void CartesianProduct_Count()
    {
        var sets = new List<List<int>> { new() { 1, 2 }, new() { 3, 4 }, new() { 5 } };
        var result = Combinatorics.CartesianProduct(sets);
        Eq(4, result.Count); // 2 * 2 * 1 = 4
    }

    [Test] public void PermutationWithoutDuplication_ContainsAllElements()
    {
        var input = new List<int> { 1, 2, 3 };
        var result = Combinatorics.PermutationWithoutDuplication(input);
        foreach (var perm in result)
        {
            H.Count(perm, 3);
            H.Contains(perm, 1);
            H.Contains(perm, 2);
            H.Contains(perm, 3);
        }
    }
}

using System;
using System.Collections.Generic;

public static class Combinatorics
{
    // order matters, elements can repeat - e.g. dice rolls, passwords with character repetition
    public static List<List<T>> PermutationWithDuplication<T>(List<T> set, int k)
    {
        int n = set.Count, resultCount = n.Pow(k);
        List<List<T>> result = new(resultCount);
        var indices = new int[k];

        for (var r = 0; r < resultCount; ++r)
        {
            result.Add(CreateList(set, indices, k));

            for (var i = k - 1; i >= 0; --i)
            {
                if (++indices[i] < n) break;
                else indices[i] = 0;
            }
        }

        return result;
    }

    // order matters, no repeats - e.g. shuffling, anagrams
    public static List<List<T>> PermutationWithoutDuplication<T>(List<T> set)
    {
        List<List<T>> result = new();
        Recurse(set, 0, result, (s, i) => { (s[0], s[i]) = (s[i], s[0]); });
        return result;
    }

    // order doesn't matter, no repeats - e.g. lottery numbers, team selection
    public static List<List<T>> CombinationWithoutDuplication<T>(List<T> set, int k) => GetCombinations(set, k, false);

    // order doesn't matter, elements can repeat - e.g. choosing items with duplicates allowed
    public static List<List<T>> CombinationWithDuplication<T>(List<T> set, int k) => GetCombinations(set, k, true);

    // all combinations of elements across sets - e.g. all configurations from multiple input sets
    public static List<List<T>> CartesianProduct<T>(List<List<T>> sets)
    {
        List<List<T>> result = new();

        void CartesianHelper(List<T> current, int depth)
        {
            if (depth == sets.Count)
            {
                result.Add(new List<T>(current)); // Capture product
                return;
            }

            foreach (var item in sets[depth])
            {
                current.Add(item);
                CartesianHelper(current, depth + 1);
                current.RemoveAt(current.Count - 1); // Backtrack
            }
        }

        CartesianHelper(new List<T>(), 0);
        return result;
    }

    // all subsets of a set - e.g., all possible combinations of features in machine learning
    public static List<List<T>> PowerSet<T>(List<T> set)
    {
        List<List<T>> result = new();
        var powerSetCount = 2.Pow(set.Count);
        for (var i = 0; i < powerSetCount; ++i)
        {
            List<T> subset = new();
            for (var j = 0; j < set.Count; ++j)
            {
                if ((i & (1 << j)) != 0)
                    subset.Add(set[j]);
            }

            result.Add(subset);
        }

        return result;
    }

    // create a list from indices for permutation/combination building
    static List<T> CreateList<T>(List<T> set, int[] indices, int k)
    {
        List<T> result = new(k);
        for (var i = 0; i < k; ++i) result.Add(set[indices[i]]);
        return result;
    }

    // General combinations (with or without repetition)
    static List<List<T>> GetCombinations<T>(List<T> set, int k, bool allowRepetition)
    {
        List<List<T>> result = new();

        void Combine(List<T> current, int start)
        {
            if (current.Count == k)
            {
                result.Add(new List<T>(current)); // add combination to result
                return;
            }

            for (var i = start; i < set.Count; ++i)
            {
                current.Add(set[i]);
                Combine(current, allowRepetition ? i : i + 1);
                current.RemoveAt(current.Count - 1); // Backtrack
            }
        }

        Combine(new List<T>(k), 0);
        return result;
    }

    // Generic recursion for permutations (allows swapping and backtracking)
    static void Recurse<T>(List<T> set, int start, List<List<T>> resultSet, Action<List<T>, int> swap)
    {
        if (start >= set.Count)
        {
            resultSet.Add(new List<T>(set)); // add current permutation
            return;
        }

        for (var i = start; i < set.Count; ++i)
        {
            swap(set, i);
            Recurse(set, start + 1, resultSet, swap);
            swap(set, i); // Backtrack (swap back)
        }
    }

    // K-permutations without repetition e.g - drawing a specific number of winners from a pool without replacement 
    public static List<List<T>> KPermutationWithoutRepetition<T>(List<T> set, int k)
    {
        var result = new List<List<T>>();

        void Permute(List<T> current, bool[] used)
        {
            if (current.Count == k)
            {
                result.Add(new List<T>(current));
                return;
            }

            for (var i = 0; i < set.Count; ++i)
            {
                if (used[i]) continue;
                used[i] = true;
                current.Add(set[i]);
                Permute(current, used);
                current.RemoveAt(current.Count - 1);
                used[i] = false;
            }
        }

        Permute(new List<T>(k), new bool[set.Count]);
        return result;
    }

    // Combinations with custom constraints (e.g., selecting items that meet a specific condition)
    public static List<List<T>> CombinationWithConstraints<T>(List<T> set, int k, Func<List<T>, bool> constraint)
    {
        List<List<T>> result = new();

        void Combine(List<T> current, int start)
        {
            if (current.Count == k)
            {
                if (constraint == null || constraint(current))
                    result.Add(new List<T>(current));
                return;
            }

            for (var i = start; i < set.Count; ++i)
            {
                current.Add(set[i]);
                Combine(current, i + 1);
                current.RemoveAt(current.Count - 1);
            }
        }

        Combine(new List<T>(k), 0);
        return result;
    }
}
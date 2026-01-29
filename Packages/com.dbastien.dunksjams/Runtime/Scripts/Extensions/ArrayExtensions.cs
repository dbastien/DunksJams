using System;

public static class ArrayExtensions
{
    public static void Fill(this bool[] a, bool val) 
    { 
        if (!val) Array.Clear(a, 0, a.Length);
        else Array.Fill(a, true);
    }

    public static void Fill<T>(this T[] a, T val) => Array.Fill(a, val);
 }
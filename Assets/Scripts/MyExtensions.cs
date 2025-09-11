
using System.Collections.Generic;
using UnityEngine;

public static class MyExtensions
{
    private static readonly System.Random rng = new System.Random();

    //
    // Fisher - Yates shuffle
    //
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    //
    // Choose a random element of any list
    //
    public static T RandomChoice<T>(this IList<T> list)
    {
        int rand = rng.Next(0, list.Count);
        return list[rand];
    }

    ///
    // Shortcut for adding items to a dictionary of lists
    //
    public static void AddToListDict<T, U>(this Dictionary<U, List<T>> dict, U key, T element)
    {
        if (dict.ContainsKey(key))
            dict[key].Add(element);
        else
            dict.Add(key, new List<T>() { element });
    }
}
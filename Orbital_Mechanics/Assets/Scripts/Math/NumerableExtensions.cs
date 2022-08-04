using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sim.Math
{
    public static class NumerableExtensions
    {
        public static IEnumerator<T> RepeatIndefinitely<T>(this IEnumerable<T> source)
        {
            while (true)
                foreach (var item in source)
                    yield return item;
        }
    }
}

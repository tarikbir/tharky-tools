using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UnityEngine;

namespace TTools
{
    public static class TToolsExtensions
    {
        private static readonly StringBuilder _sb = new();

        #region GameObject
        /// <summary>Gets a component or if the component is non-existent, creates one and adds it onto the game object.</summary>
        /* By adammyhre (git-amend) */
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (!component) component = gameObject.AddComponent<T>();
            return component;
        }

        /// <summary>Allows Unity objects to be used for null propogation by returning null if the object is <null>.</summary>
        /* By adammyhre (git-amend) */
        public static T OrNull<T>(this T obj) where T : UnityEngine.Object => obj ? obj : null;
        #endregion

        #region Transform
        /// <summary>Resets the transform back to its default values.</summary>
        public static void Reset(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>Destroys all children of a parent. (Sounds horrible, I know)</summary>
        /* By Tarodev */
        public static void DestroyChildren(this Transform parent)
        {
            for (int i = parent.childCount; i > 0; --i)
                GameObject.Destroy(parent.GetChild(0).gameObject);
        }

        /// <summary>Destroys all children of a parent immediately. (Yuck)</summary>
        /* By Tarodev */
        public static void DestroyChildrenImmediate(this Transform parent)
        {
            for (int i = parent.childCount; i > 0; --i)
                GameObject.DestroyImmediate(parent.GetChild(0).gameObject);
        }

        /// <summary>Operate on children of a transform. From last to first.</summary>
        /* By adammyhre (git-amend) */
        public static void ForEveryChild(this Transform parent, System.Action<Transform> action)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                action(parent.GetChild(i));
            }
        }

        /// <summary>
        /// Retrieves all the children of a given Transform.
        /// </summary>
        /// <remarks>
        /// This method can be used with LINQ to perform operations on all child Transforms. For example,
        /// you could use it to find all children with a specific tag, to disable all children, etc.
        /// Transform implements IEnumerable and the GetEnumerator method which returns an IEnumerator of all its children.
        /// </remarks>
        /// <param name="parent">The Transform to retrieve children from.</param>
        /// <returns>An IEnumerable&lt;Transform&gt; containing all the child Transforms of the parent.</returns>    
        /* By adammyhre (git-amend) */
        public static IEnumerable<Transform> Children(this Transform parent)
        {
            foreach (Transform child in parent)
            {
                yield return child;
            }
        }
        #endregion

        #region Vectors
        /// <summary>
        /// Returns a Boolean indicating whether the current Vector3 is in a given range from another Vector3
        /// </summary>
        /// <param name="current">The current Vector3 position</param>
        /// <param name="target">The Vector3 position to compare against</param>
        /// <param name="range">The range value to compare against</param>
        /// <returns>True if the current Vector3 is in the given range from the target Vector3, false otherwise</returns>
        /* By adammyhre (git-amend) */
        public static bool IsInRangeOf(this Vector3 current, Vector3 target, float range)
        {
            return (current - target).sqrMagnitude <= range * range;
        }
        #endregion

        #region Arrays
        /// <summary>Converts a list to string with a Comma Delimited List of Values.</summary>
        /* By Jason Storey */
        public static string ToReadable<T>(this IEnumerable<T> source)
        {
            if (source == null) return "null";
            var s = "[";
            s = source.Aggregate(s, (res, x) => res + x + ", ");
            return $"{s.Substring(0, s.Length - 2)}]";
        }

        /// <summary>Joins all the elements of the list into a string separated by the given separator string. Not thread-safe (uses static SB).</summary>
        /* By Jason Storey */
        public static string ToReadable<T>(this IList<T> list, string separator)
        {
            if (list == null) return string.Empty;
            _sb.Clear();
            for (var i = 0; i < list.Count - 1; i++)
            {
                _sb.Append(list[i]);
                _sb.Append(separator);
            }

            _sb.Append(list[list.Count - 1]);
            return _sb.ToString();
        }

        /// <summary>'Contains' logic for arrays</summary>
        public static bool Contains<T>(this T[] array, T item)
        {
            bool hasItem = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(item))
                {
                    hasItem = true;
                    break;
                }
            }
            return hasItem;
        }

        /// <summary>Randomly picks from an array. Uses UnityEngine.Random</summary>
        public static T Random<T>(this IList<T>[] array)
        {
            int randomIndex = UnityEngine.Random.Range(0, array.Length);
            return array.ElementAt(randomIndex).FirstOrDefault();
        }

        /// <summary>Allows to enumerate foreach loops like Kotlin.</summary>
        /* By Nick Chapsas */
        public static CustomIntEnumerator GetEnumerator(this Range range)
        {
            return new CustomIntEnumerator(range);
        }

        /// <summary>Allows to enumerate foreach loops like Kotlin.</summary>
        /* By Nick Chapsas */
        public static CustomIntEnumerator GetEnumerator(this int number)
        {
            return new CustomIntEnumerator(new Range(0, number));
        }
        #endregion

        #region Text
        public static string ToRoman(this int number)
        {
            if (number < 0 || number > 3999)
            {
                Debug.LogWarning("Cannot return roman numerals for: " + number.ToString() + ". Value must be between 1 and 3999");
                return string.Empty;
            }
            if (number < 1)
                return string.Empty;
            if (number >= 1000)
                return "M" + ToRoman(number - 1000);
            if (number >= 900)
                return "CM" + ToRoman(number - 900);
            if (number >= 500)
                return "D" + ToRoman(number - 500);
            if (number >= 400)
                return "CD" + ToRoman(number - 400);
            if (number >= 100)
                return "C" + ToRoman(number - 100);
            if (number >= 90)
                return "XC" + ToRoman(number - 90);
            if (number >= 50)
                return "L" + ToRoman(number - 50);
            if (number >= 40)
                return "XL" + ToRoman(number - 40);
            if (number >= 10)
                return "X" + ToRoman(number - 10);
            if (number >= 9)
                return "IX" + ToRoman(number - 9);
            if (number >= 5)
                return "V" + ToRoman(number - 5);
            if (number >= 4)
                return "IV" + ToRoman(number - 4);
            return number >= 1 ? "I" + ToRoman(number - 1) : string.Empty;
        }
        #endregion

        #region Color
        // By Jason Storey
        public static string ToHex(this Color color) => $"#{ColorUtility.ToHtmlStringRGBA(color)}";

        // By Jason Storey
        public static Color WithAlphaAt(this Color col, float a) => new Color(col.r, col.g, col.b, a);
        #endregion

        #region Other
        /// <summary>Calculate percentage of given chance. Uses Random.Next().</summary>
        public static bool PercentChanceTo(this System.Random random, int chance)
        {
            return random.Next(1, 100) <= chance;
        }

        /// <summary>
        /// Converts an IEnumerator<T> to an IEnumerable<T>.
        /// </summary>
        /// <param name="e">An instance of IEnumerator<T>.</param>
        /// <returns>An IEnumerable<T> with the same elements as the input instance.</returns>
        /* By adammyhre (git-amend) */
        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
        #endregion
    }

    /* By Nick Chapsas */
    public ref struct CustomIntEnumerator
    {
        private int _current;
        private readonly int _end;

        public CustomIntEnumerator(Range range)
        {
            if (range.End.IsFromEnd)
            {
                throw new NotSupportedException("Does not support endless range.");
            }

            _current = range.Start.Value - 1;
            _end = range.End.Value;
        }

        public int Current => _current;

        public bool MoveNext()
        {
            _current++;
            return _current <= _end;
        }
    }
}

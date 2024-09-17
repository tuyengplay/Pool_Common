using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace TuyenGameCore
{
    public static class CollectionUtil
    {
        private static readonly System.Random intRandom = new System.Random();
        private static readonly object intRandomLock = new object();
        public static int RandomInt(int minNum, int maxNum)
        {
            int result;
            lock (intRandomLock)
            {
                result = intRandom.Next(minNum, maxNum);
            }
            return result;
        }

        #region Dictionary
        public static void Set<K, V>(this IDictionary<K, V> dictionary, K key, V value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }
            dictionary.Add(key, value);
        }

        public static V Get<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            return default;
        }

        public static V GetAndAdd<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            if (!dictionary.ContainsKey(key))
            {
                V v = Activator.CreateInstance<V>();
                dictionary.Add(key, v);
                return v;
            }
            return dictionary[key];
        }

        public static Dictionary<N, M> Unsort<N, M>(this IDictionary<N, M> dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }
            int[] array = new int[dictionary.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
            for (int j = array.Length - 1; j > 0; j--)
            {
                int num = RandomInt(0, j);
                array[num] += array[j];
                array[j] = array[num] - array[j];
                array[num] -= array[j];
            }
            Dictionary<N, M> dictionary2 = new Dictionary<N, M>();
            for (int k = 0; k < array.Length; k++)
            {
                dictionary2.Add(dictionary.Keys.ElementAt(array[k]), dictionary.Values.ElementAt(array[k]));
            }
            return dictionary2;
        }

        public static Dictionary<N, M> RemoveAll<N, M>(this IDictionary<N, M> dictionary, Func<KeyValuePair<N, M>, bool> condition)
        {
            if (dictionary == null)
            {
                return null;
            }
            Dictionary<N, M> dictionary2 = new Dictionary<N, M>();
            foreach (KeyValuePair<N, M> arg in dictionary)
            {
                if (!condition(arg))
                {
                    dictionary2.Add(arg.Key, arg.Value);
                }
            }
            return dictionary2;
        }

        public static void AddPairs<N, M>(this IDictionary<N, M> original, IDictionary<N, M> source)
        {
            foreach (N key in source.Keys)
            {
                original.Set(key, source[key]);
            }
        }
        #endregion

        public static T GetRandom<T>(this ICollection<T> collection)
        {
            if (collection == null || collection.Count == 0)
            {
                return default;
            }
            return collection.ElementAtOrDefault(RandomInt(0, collection.Count));
        }

        public static T GetRandom<T>(this IEnumerable<T> collection)
        {
            if (collection == null)
            {
                return default;
            }
            T[] array = collection.ToArray<T>();
            if (array.Length == 0)
            {
                return default;
            }
            return array[RandomInt(0, array.Length)];
        }

        public static List<T> GetRandom<T>(this ICollection<T> collection, int count)
        {
            if (collection == null || collection.Count == 0 || count <= 0)
            {
                return new List<T>();
            }
            if (count == 1)
            {
                return new List<T> { collection.GetRandom<T>() };
            }
            if (count == collection.Count)
            {
                return new List<T>(collection);
            }
            bool[] array = new bool[collection.Count];
            int num = 0;
            while (num < count)
            {
                int num2 = RandomInt(0, collection.Count);
                if (!array[num2])
                {
                    array[num2] = true;
                    num++;
                }
            }
            List<T> list = new List<T>(count);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i])
                {
                    list.Add(collection.ElementAt(i));
                }
            }
            return list;
        }

        public static List<T> GetRandom<T>(this IEnumerable<T> collection, int count)
        {
            if (collection == null)
            {
                return new List<T>();
            }
            if (count == 1)
            {
                return new List<T> { collection.GetRandom<T>() };
            }
            return collection.ToArray<T>().GetRandom(count);
        }

        public static ICollection<T> Unsort<T>(this ICollection<T> collection)
        {
            if (collection == null)
            {
                return null;
            }
            int[] array = new int[collection.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i;
            }
            for (int j = array.Length - 1; j > 0; j--)
            {
                int num = RandomInt(0, j);
                array[num] += array[j];
                array[j] = array[num] - array[j];
                array[num] -= array[j];
            }
            List<T> list = new List<T>();
            for (int k = 0; k < array.Length; k++)
            {
                list.Add(collection.ElementAt(array[k]));
            }
            return list;
        }

        public static IList<T> Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
            return ts;
        }

        public static int Count<T>(this ICollection<T> collection, Func<T, bool> filter)
        {
            if (collection == null)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                if (filter(collection.ElementAt(i)))
                {
                    num++;
                }
            }
            return num;
        }

        public static T GetMin<T>(this IEnumerable<T> collection, Func<T, int> filter)
        {
            if (collection == null)
            {
                return default;
            }
            int num = int.MaxValue;
            T result = default;
            foreach (T t in collection)
            {
                int num2 = filter(t);
                if (num2 < num)
                {
                    num = num2;
                    result = t;
                }
            }
            return result;
        }

        public static T GetMin<T>(this IEnumerable<T> collection, Func<T, float> filter)
        {
            if (collection == null)
            {
                return default;
            }
            float num = float.MaxValue;
            T result = default;
            foreach (T t in collection)
            {
                float num2 = filter(t);
                if (num2 < num)
                {
                    num = num2;
                    result = t;
                }
            }
            return result;
        }

        public static T GetMax<T>(this IEnumerable<T> collection, Func<T, int> filter)
        {
            if (collection == null)
            {
                return default;
            }
            int num = int.MinValue;
            T result = default;
            foreach (T t in collection)
            {
                int num2 = filter(t);
                if (num2 > num)
                {
                    num = num2;
                    result = t;
                }
            }
            return result;
        }

        public static T GetMax<T>(this IEnumerable<T> collection, Func<T, float> filter)
        {
            if (collection == null)
            {
                return default;
            }
            float num = -2.1474836E+09f;
            T result = default;
            foreach (T t in collection)
            {
                float num2 = filter(t);
                if (num2 > num)
                {
                    num = num2;
                    result = t;
                }
            }
            return result;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> function)
        {
            if (collection == null)
            {
                return;
            }
            foreach (T obj in collection)
            {
                function(obj);
            }
        }

        public static bool Contains<T>(this IEnumerable<T> source, Func<T, bool> function)
        {
            foreach (T arg in source)
            {
                if (function(arg))
                {
                    return true;
                }
            }
            return false;
        }

        public static T Get<T>(this ICollection<T> collection, int index)
        {
            if (collection == null)
            {
                throw new NullReferenceException("Collection is null");
            }
            if (collection.Count == 0 || index < 0 || collection.Count - 1 < index)
            {
                return default;
            }
            return collection.ElementAt(index);
        }

        public static ICollection<T> Swap<T>(this ICollection<T> collection, int from, int to)
        {
            if (collection == null)
            {
                throw new NullReferenceException("Collection is null");
            }
            if (from < 0 || from >= collection.Count || to < 0 || to >= collection.Count)
            {
                throw new IndexOutOfRangeException();
            }
            List<T> list = new List<T>();
            for (int i = 0; i < collection.Count; i++)
            {
                if (i == from)
                {
                    list.Add(collection.ElementAt(to));
                }
                else if (i == to)
                {
                    list.Add(collection.ElementAt(from));
                }
                else
                {
                    list.Add(collection.ElementAt(i));
                }
            }
            return list;
        }

        public static Dictionary<N, M> ToDictionary<N, M>(this IEnumerable<KeyValuePair<N, M>> collection)
        {
            Dictionary<N, M> dictionary = new Dictionary<N, M>();
            foreach (KeyValuePair<N, M> keyValuePair in collection)
            {
                dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return dictionary;
        }

        public static string Join(this IEnumerable<string> values, string separator)
        {
            string text = "";
            int num = 0;
            foreach (string str in values)
            {
                if (num++ > 0)
                {
                    text += separator;
                }
                text += str;
            }
            return text;
        }

        public static bool IsNullOrEmpty(this ICollection collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static List<T> ToList<T>(this IEnumerator<T> enumerator)
        {
            List<T> list = new List<T>();
            while (enumerator.MoveNext())
            {
                T item = enumerator.Current;
                list.Add(item);
            }
            try
            {
                enumerator.Reset();
            }
            catch { }
            return list;
        }

        public static List<T> ToList<T>(this Func<IEnumerator<T>> enumeratorFunc)
        {
            List<T> list = new List<T>();
            IEnumerator<T> enumerator = enumeratorFunc();
            while (enumerator.MoveNext())
            {
                T item = enumerator.Current;
                list.Add(item);
            }
            return list;
        }

        public static IEnumerator<T> Collect<T>(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T)
                {
                    yield return (T)(enumerator.Current);
                }
                else if (enumerator.Current is IEnumerator)
                {
                    IEnumerator subKeys = Collect<T>(enumerator.Current as IEnumerator);
                    while (subKeys.MoveNext())
                    {
                        object obj = subKeys.Current;
                        yield return (T)(obj);
                    }
                }
            }
            yield break;
        }

        public static int IndexOf<T>(this T[] array, T value)
        {
            if (array == null || array.Length == 0)
            {
                throw new NullReferenceException("Array is null or empty");
            }
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }
        public static bool CheckInCam(this Transform _transform, Camera _camera)
        {
            Vector3 screenPoint = _camera.WorldToViewportPoint(_transform.position);
            return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1;
        }
        public static bool CheckInCam(this Transform _transform)
        {
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(_transform.position);
            return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1;
        }
        public static bool CheckInCam(this Renderer _render, Camera _camera)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_camera);
            return GeometryUtility.TestPlanesAABB(planes, _render.bounds);
        }
        public static bool CheckInCam(this Renderer _render)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            return GeometryUtility.TestPlanesAABB(planes, _render.bounds);
        }
        public static bool CheckInCam(this RectTransform _rectTransform, Canvas _canvas)
        {
            Vector3[] corners = new Vector3[4];
            _rectTransform.GetWorldCorners(corners);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.GetComponent<RectTransform>(), corners[0], _canvas.worldCamera, out Vector2 localPoint);
            return _rectTransform.rect.Contains(localPoint);
        }
        public static bool IsPointerOverUI()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }
        public static Sprite ConvertTexture2DToSprite(this Texture2D _texture, float _pivotX = 0.5f, float _pivotY = 0.5f)
        {
            Rect rect = new Rect(0, 0, _texture.width, _texture.height);

            Vector2 pivot = new Vector2(_pivotX, _pivotY);

            Sprite sprite = Sprite.Create(_texture, rect, pivot);

            return sprite;
        }
    }
}
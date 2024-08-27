using System.Collections.Generic;
using UnityEngine;
namespace GameCore.Pool
{
    public static class ManagerPool
    {
        public static Dictionary<GameObject, ControlPool> Links = new Dictionary<GameObject, ControlPool>();
        #region SpawnComponent
        public static T Spawn<T>(T prefab, float _timeLive = 0.0f) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            GameObject clone = Spawn(prefab.gameObject, _timeLive);
            return clone != null ? clone.GetComponent<T>() : null;
        }
        public static T Spawn<T>(T prefab, Transform parent, bool worldPositionStays = false, float _timeLive = 0.0f) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            GameObject clone = Spawn(prefab.gameObject, parent, worldPositionStays, _timeLive);
            return clone != null ? clone.GetComponent<T>() : null;
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null, float _timeLive = 0.0f)
            where T : Component
        {
            if (prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            var clone = Spawn(prefab.gameObject, position, rotation, parent, _timeLive);
            return clone != null ? clone.GetComponent<T>() : null;
        }
        #endregion SpawnComponent
        #region SpawnObject
        public static GameObject Spawn(GameObject prefab, float _timeLive = 0.0f)
        {
            if (prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            var transform = prefab.transform;
            return Spawn(prefab, transform.localPosition, transform.localRotation, transform.localScale, null, false, _timeLive);
        }
        public static GameObject Spawn(GameObject _prefab, Transform _parent, bool _worldPositionStays = false, float _timeLive = 0.0f)
        {
            if (_prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            Transform transform = _prefab.transform;
            if (_parent != null && _worldPositionStays == true)
            {
                return Spawn(_prefab, _prefab.transform.position, Quaternion.identity, Vector3.one, _parent, _worldPositionStays, _timeLive);
            }
            return Spawn(_prefab, transform.localPosition, transform.localRotation, transform.localScale, _parent, false, _timeLive);
        }
        public static GameObject Spawn(GameObject _prefab, Vector3 _position, Quaternion _rotation, Transform _parent = null, float _time = 0.0f)
        {
            if (_prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            if (_parent != null)
            {
                _position = _parent.InverseTransformPoint(_position);
                _rotation = Quaternion.Inverse(_parent.rotation) * _rotation;
            }
            return Spawn(_prefab, _position, _rotation, _prefab.transform.localScale, _parent, false, _time);
        }
        private static GameObject Spawn(GameObject _prefab, Vector3 _location, Quaternion _rotation, Vector3 _scale, Transform _parent, bool _worldPositionStays, float _time)
        {
            if (_prefab != null)
            {
                ControlPool pool = default;
                if (ControlPool.TryFindPoolByPrefab(_prefab, ref pool) == false)
                {
                    pool = new GameObject($"PoolObject_{_prefab.name}_Control").AddComponent<ControlPool>();
                    pool.Prefab = _prefab;
                }
                GameObject clone = default;
                if (pool.TrySpawn(ref clone, _location, _rotation, _scale, _parent, _worldPositionStays, _time) == true)
                {
                    if (Links.ContainsKey(clone))
                    {
                        Links.Remove(clone);
                    }
                    Links.Add(clone, pool);
                    return clone;
                }
            }
            else
            {
                Debug.LogError("Null Prefabs");
            }
            return default;
        }
        #endregion SpawnObject
        public static void Despawn(Component clone, float delay = 0.0f)
        {
            if (clone != null)
            {
                Despawn(clone.gameObject, delay);
            }
        }
        private static void Despawn(GameObject clone, float delay = 0.0f)
        {
            if (clone != null)
            {
                ControlPool pool = default;

                if (Links.TryGetValue(clone, out pool) == true)
                {
                    Links.Remove(clone);

                    pool.Despawn(clone, delay);
                }
                else
                {
                    Object.Destroy(clone, delay);
                }
            }
            else
            {
                Debug.LogWarning("You're attempting to despawn a null gameObject.", clone);
            }
        }
        public static void KillAll()
        {
            ControlPool.KillALl();
        }
        public static void DespawnAll()
        {
            ControlPool.DespawnAll();
        }
        public static void DespawnAllKey(this GameObject _prefab, NotificationType _type)
        {
            ControlPool pool = default;
            if (ControlPool.TryFindPoolByPrefab(_prefab, ref pool) == true)
            {
                pool.DespawnAll_Local();
            }
        }
        public static void SetSendMessage(this GameObject _prefab, NotificationType _type)
        {
            ControlPool pool = default;
            if (ControlPool.TryFindPoolByPrefab(_prefab, ref pool) == false)
            {
                pool = new GameObject($"PoolObject_{_prefab.name}_Control").AddComponent<ControlPool>();
                pool.Prefab = _prefab;
            }
            pool.notification = _type;
        }
    }
    public enum NotificationType
    {
        None,
        SendMessage,
        BroadcastMessage,
        IPoolable,
        BroadcastIPoolable
    }
}

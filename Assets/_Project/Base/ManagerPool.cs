using System.Collections.Generic;
using UnityEngine;
namespace TuyenGameCore.Poolable
{
    public static class ManagerPool
    {
        public static Dictionary<GameObject, ControlPool> Links = new Dictionary<GameObject, ControlPool>();
        #region SpawnComponent
        public static T Spawn<T>(T _prefab) where T : Component
        {
            if (_prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            GameObject clone = Spawn(_prefab.gameObject);
            return clone != null ? clone.GetComponent<T>() : null;
        }
        public static T Spawn<T>(T _prefab, Transform _parent, bool _worldPositionStays = false) where T : Component
        {
            if (_prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            GameObject clone = Spawn(_prefab.gameObject, _parent, _worldPositionStays);
            return clone != null ? clone.GetComponent<T>() : null;
        }

        public static T Spawn<T>(T _prefab, Vector3 _position, Quaternion _rotation, Transform _parent = null)
            where T : Component
        {
            if (_prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            var clone = Spawn(_prefab.gameObject, _position, _rotation, _parent);
            return clone != null ? clone.GetComponent<T>() : null;
        }
        #endregion SpawnComponent
        #region SpawnObject
        public static GameObject Spawn(GameObject _prefab)
        {
            if (_prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            var transform = _prefab.transform;
            return Spawn(_prefab, transform.localPosition, transform.localRotation, transform.localScale, null, false);
        }
        public static GameObject Spawn(GameObject _prefab, Transform _parent, bool _worldPositionStays = false)
        {
            if (_prefab == null)
            {
                Debug.LogError("Null Prefabs");
                return null;
            }
            Transform transform = _prefab.transform;
            if (_parent != null && _worldPositionStays == true)
            {
                return Spawn(_prefab, _prefab.transform.position, Quaternion.identity, Vector3.one, _parent, _worldPositionStays);
            }
            return Spawn(_prefab, transform.localPosition, transform.localRotation, transform.localScale, _parent, false);
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
            return Spawn(_prefab, _position, _rotation, _prefab.transform.localScale, _parent, false);
        }
        private static GameObject Spawn(GameObject _prefab, Vector3 _location, Quaternion _rotation, Vector3 _scale, Transform _parent, bool _worldPositionStays)
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
                if (pool.TrySpawn(ref clone, _location, _rotation, _scale, _parent, _worldPositionStays) == true)
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
        public static void Despawn(Component _clone, float _delay = 0.0f)
        {
            if (_clone != null)
            {
                Despawn(_clone.gameObject, _delay);
            }
        }
        private static void Despawn(GameObject _clone, float _delay = 0.0f)
        {
            ControlPool pool = default;
            if (Links.TryGetValue(_clone, out pool) == true)
            {
                Links.Remove(_clone);

                pool.Despawn(_clone, _delay);
            }
            else
            {
                Object.Destroy(_clone, _delay);
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
        SendMessage,
        BroadcastMessage,
        IPoolable,
        BroadcastIPoolable
    }
}

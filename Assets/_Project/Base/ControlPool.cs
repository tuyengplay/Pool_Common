using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TuyenGameCore.Poolable
{
    public interface I_Pool
    {
        void OnSpawn();

        void OnDespawn();
    }

    [System.Serializable]
    internal class Delay
    {
        internal GameObject Clone;
        internal float Life;
    }

    public class ControlPool : MonoBehaviour
    {
        private GameObject prefab;

        internal GameObject Prefab
        {
            set
            {
                if (value != prefab)
                {
                    UnregisterPrefab();
                    prefab = value;
                    RegisterPrefab();
                }
            }
            get { return prefab; }
        }

        private List<GameObject> spawnedClones = new List<GameObject>();
        private List<GameObject> despawnedClones = new List<GameObject>();

        private Transform deactivatedChild;

        private Transform DeactivatedChild
        {
            get
            {
                if (deactivatedChild == null)
                {
                    GameObject child = new GameObject("Despawned");

                    child.SetActive(false);

                    deactivatedChild = child.transform;

                    deactivatedChild.SetParent(transform, false);
                }

                return deactivatedChild;
            }
        }

        internal NotificationType notification = NotificationType.IPoolable;
        private List<I_Pool> listIpool = new List<I_Pool>();
        private List<Delay> delays = new List<Delay>();

        private void UnregisterPrefab()
        {
            if (Equals(prefab, null) == true)
            {
                return;
            }

            ControlPool existingPool = default;

            if (PrefabMap.TryGetValue(prefab, out existingPool) == true && existingPool == this)
            {
                PrefabMap.Remove(prefab);
                //Co the them Destroy
            }
        }

        private void RegisterPrefab()
        {
            if (prefab != null)
            {
                ControlPool existingPool = default;

                if (PrefabMap.TryGetValue(prefab, out existingPool) == true)
                {
                    Debug.LogWarning("You have multiple pools managing the same prefab (" + prefab.name + ").", existingPool);
                }
                else
                {
                    PrefabMap.Add(prefab, this);
                }
            }
        }

        private void DespawnNow(GameObject _clone)
        {
            despawnedClones.Add(_clone);
            spawnedClones.Remove(_clone);
            if (transform != null && this != null)
            {
                _clone.transform.SetParent(DeactivatedChild, false);
            }

            InvokeOnDespawn(_clone);
        }

        internal void Despawn(GameObject _clone, float _timeLive)
        {
            if (_timeLive > 0)
            {
                Delay delay = new Delay();
                delay.Clone = _clone;
                delay.Life = _timeLive;
                delays.Add(delay);
            }
            else
            {
                DespawnNow(_clone);
            }
        }

        internal void DespawnAll_Local()
        {
            for (int i = spawnedClones.Count - 1; i >= 0; i--)
            {
                try
                {
                    DespawnNow(spawnedClones[i]);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    throw;
                }
            }
        }

        internal bool TrySpawn(ref GameObject _clone, Vector3 _location, Quaternion _rotation, Vector3 _scale, Transform _parent, bool _worldPositionStays)
        {
            if (prefab != null)
            {
                for (int i = despawnedClones.Count - 1; i >= 0; i--)
                {
                    _clone = despawnedClones[i];

                    despawnedClones.RemoveAt(i);

                    if (_clone != null)
                    {
                        SpawnClone(_clone, _location, _rotation, _scale, _parent, _worldPositionStays);
                        return true;
                    }
                }

                _clone = CreateClone(_location, _rotation, _scale, _parent, _worldPositionStays);
                spawnedClones.Add(_clone);
                InvokeOnSpawn(_clone);
                return true;
            }

            return false;
        }

        private void SpawnClone(GameObject _clone, Vector3 _location, Quaternion _rotaion, Vector3 _scale, Transform _parent, bool _worldPositionStays)
        {
            spawnedClones.Add(_clone);
            Transform cloneTransform = _clone.transform;

            cloneTransform.SetParent(null, false);

            cloneTransform.position = _location;
            cloneTransform.localRotation = _rotaion;
            cloneTransform.localScale = _scale;

            cloneTransform.SetParent(_parent, _worldPositionStays);

            if (_parent == null)
            {
                SceneManager.MoveGameObjectToScene(_clone, SceneManager.GetActiveScene());
            }

            InvokeOnSpawn(_clone);
        }

        private GameObject CreateClone(Vector3 _localPosition, Quaternion _localRotation, Vector3 _localScale, Transform _parent, bool _worldPositionStays)
        {
            GameObject clone = DoInstantiate(prefab, _localPosition, _localRotation, _localScale, _parent, _worldPositionStays);
            clone.name = prefab.name;
            return clone;
        }

        private GameObject DoInstantiate(GameObject _prefab, Vector3 _location, Quaternion _rotation, Vector3 _scale, Transform _parent, bool _worldPositionStays)
        {
            if (_worldPositionStays == true)
            {
                return Instantiate(_prefab, _parent, true);
            }
            else
            {
                GameObject clone = Instantiate(_prefab, _location, _rotation, _parent);

                clone.transform.localPosition = _location;
                clone.transform.localRotation = _rotation;
                clone.transform.localScale = _scale;

                return clone;
            }
        }

        private void InvokeOnSpawn(GameObject _clone)
        {
            switch (notification)
            {
                case NotificationType.IPoolable:
                    _clone.GetComponents(listIpool);
                    for (var i = listIpool.Count - 1; i >= 0; i--)
                    {
                        listIpool[i].OnSpawn();
                    }

                    break;
                case NotificationType.BroadcastIPoolable:
                    _clone.GetComponentsInChildren(listIpool);
                    for (var i = listIpool.Count - 1; i >= 0; i--)
                    {
                        listIpool[i].OnSpawn();
                    }

                    break;
                case NotificationType.SendMessage:
                    _clone.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
                    break;
                case NotificationType.BroadcastMessage:
                    _clone.BroadcastMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
                    break;
            }
        }

        private void InvokeOnDespawn(GameObject _clone)
        {
            switch (notification)
            {
                case NotificationType.IPoolable:
                    _clone.GetComponents(listIpool);
                    for (var i = listIpool.Count - 1; i >= 0; i--)
                    {
                        listIpool[i].OnDespawn();
                    }

                    break;
                case NotificationType.BroadcastIPoolable:
                    _clone.GetComponentsInChildren(listIpool);
                    for (var i = listIpool.Count - 1; i >= 0; i--)
                    {
                        listIpool[i].OnDespawn();
                    }

                    break;
                case NotificationType.SendMessage:
                    _clone.SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
                    break;
                case NotificationType.BroadcastMessage:
                    _clone.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
                    break;
            }
        }

        private void Update()
        {
            for (var i = delays.Count - 1; i >= 0; i--)
            {
                var delay = delays[i];

                delay.Life -= Time.deltaTime;

                if (delay.Life > 0.0f)
                {
                    continue;
                }

                delays.RemoveAt(i);

                if (delay.Clone != null)
                {
                    DespawnNow(delay.Clone);
                }
            }
        }

        private void OnDestroy()
        {
            DespawnAll_Local();
            if (PrefabMap.ContainsKey(prefab))
            {
                PrefabMap.Remove(prefab);
            }
        }

        #region Static

        private static Dictionary<GameObject, ControlPool> PrefabMap = new Dictionary<GameObject, ControlPool>();

        internal static bool TryFindPoolByPrefab(GameObject _prefab, ref ControlPool _foundPool)
        {
            return PrefabMap.TryGetValue(_prefab, out _foundPool);
        }

        internal static void KillALl()
        {
            foreach (KeyValuePair<GameObject, ControlPool> item in PrefabMap)
            {
                if (item.Value != null)
                {
                    item.Value.DespawnAll_Local();
                    DestroyImmediate(item.Value.gameObject);
                }
            }

            PrefabMap.Clear();
        }

        internal static void DespawnAll()
        {
            foreach (KeyValuePair<GameObject, ControlPool> item in PrefabMap)
            {
                item.Value.DespawnAll_Local();
            }
        }

        #endregion Static
    }
}
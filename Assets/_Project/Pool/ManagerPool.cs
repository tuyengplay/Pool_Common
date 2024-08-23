using UnityEngine;

public static class ManagerPool
{
    #region SpawnComponent
    public static T Spawn<T>(T prefab) where T : Component
    {
        if (prefab == null)
        {
            Debug.LogError("Null Prefabs");
            return null;
        }
        var clone = Spawn(prefab.gameObject);
        return clone != null ? clone.GetComponent<T>() : null;
    }
    public static T Spawn<T>(T prefab, Transform parent, bool worldPositionStays = false) where T : Component
    {
        if (prefab == null)
        {
            Debug.LogError("Null Prefabs");
            return null;
        }
        GameObject clone = Spawn(prefab.gameObject, parent, worldPositionStays);
        return clone != null ? clone.GetComponent<T>() : null;
    }

    public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        if (prefab == null)
        {
            Debug.LogError("Null Prefabs");
            return null;
        }
        var clone = Spawn(prefab.gameObject, position, rotation, parent);
        return clone != null ? clone.GetComponent<T>() : null;
    }
    #endregion SpawnComponent
    #region SpawnObject
    public static GameObject Spawn(GameObject prefab, float _timeLive = 0f)
    {
        if (prefab == null)
        {
            Debug.LogError("Null Prefabs");
            return null;
        }
        var transform = prefab.transform;
        return Spawn(prefab, transform.localPosition, transform.localRotation, transform.localScale, null, false, _timeLive);
    }
    public static GameObject Spawn(GameObject _prefab, Transform _parent, bool _worldPositionStays = false, float _timeLive = 0f)
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
    public static GameObject Spawn(GameObject _prefab, Vector3 _position, Quaternion _rotation, Transform _parent = null, float _timeLive = 0)
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
        return Spawn(_prefab, _position, _rotation, _prefab.transform.localScale, _parent, false, _timeLive);
    }
    private static GameObject Spawn(GameObject _prefab, Vector3 _location, Quaternion _rotation, Vector3 _scale, Transform _parent, bool _worldPositionStays, float _timeLive)
    {
        if (_prefab == null)
        {
            ControlPool pool = default;
            if (ControlPool.TryFindPoolByPrefab(_prefab, ref pool) == false)
            {
                pool = new GameObject($"PoolObject_{_prefab.name}_Control").AddComponent<ControlPool>();
                pool.Prefab = _prefab;
            }
            GameObject clone = default;
            if (pool.TrySpawn(ref clone, _location, _rotation, _scale, _parent, _worldPositionStays, _timeLive) == true)
            { }
        }
        else
        {
            Debug.LogError("Null Prefabs");
        }
        return default;
    }
    #endregion SpawnObject
}

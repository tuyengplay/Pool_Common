using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlPool : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    public GameObject Prefab
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
    private List<GameObject> despawnedClones = new List<GameObject>();
    private List<Delay> delayKill = new List<Delay>();

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
    public bool TrySpawn(ref GameObject clone, Vector3 _location, Quaternion _rotation, Vector3 _scale, Transform _parent, bool _worldPositionStays, float _timeLive)
    {
        if (prefab != null)
        {
            for (var i = despawnedClones.Count - 1; i >= 0; i--)
            {
                clone = despawnedClones[i];

                despawnedClones.RemoveAt(i);

                if (clone != null)
                {
                    SpawnClone(clone, _location, _rotation, _scale, _parent, _worldPositionStays);
                    if (_timeLive > 0)
                    {

                    }
                    return true;
                }
            }
        }

        return false;
    }
    private void SpawnClone(GameObject _clone, Vector3 _location, Quaternion _rotaion, Vector3 _scale, Transform _parent, bool _worldPositionStays)
    {

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
    private GameObject DoInstantiate(GameObject _prefab, Vector3 _location, Quaternion _rotation, Vector3 _scale, Transform _parent, bool _worldPositionStays)
    {
        if (_worldPositionStays == true)
        {
            return Instantiate(_prefab, _parent, true);
        }
        else
        {
            var clone = Instantiate(_prefab, _location, _rotation, _parent);

            clone.transform.localPosition = _location;
            clone.transform.localRotation = _rotation;
            clone.transform.localScale = _scale;

            return clone;
        }
    }
    private void InvokeOnSpawn(GameObject _clone)
    { }
    private void InvokeOnDespawn(GameObject _clone)
    { }
    private void Update()
    {
        for (var i = delayKill.Count - 1; i >= 0; i--)
        {
            Delay delay = delayKill[i];

            delay.life -= Time.deltaTime;

            if (delay.life > 0.0f)
            {
                continue;
            }

            delayKill.RemoveAt(i);

            if (delay.clone != null)
            {
                delay.clone.SendMessage("OnKill", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    #region Static
    private static Dictionary<GameObject, ControlPool> PrefabMap = new Dictionary<GameObject, ControlPool>();
    public static bool TryFindPoolByPrefab(GameObject prefab, ref ControlPool foundPool)
    {
        return PrefabMap.TryGetValue(prefab, out foundPool);
    }
    #endregion Static
    [System.Serializable]
    public class Delay
    {
        public GameObject clone;
        public float life;
    }
}

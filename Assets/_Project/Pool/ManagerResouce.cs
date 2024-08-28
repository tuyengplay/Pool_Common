using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerResouce : MonoBehaviour
{
    private static ManagerResouce poolResource;
    public static void GetPrefab(string _nameObject, Action<GameObject> _result, string _pathPrefabs = "Prefabs")
    {
        if (poolResource == null)
        {
            poolResource = new GameObject("Pool_Resource").AddComponent<ManagerResouce>();
        }
        GameObject result = default;
        if (poolResource.PrefabsMap.TryGetValue(_nameObject, out result))
        {
            _result?.Invoke(result);
            return;
        }
        poolResource.StartCoroutine(poolResource.FindPrefab(_nameObject, (_resultResource) =>
        {
            if (_resultResource != null)
            {
                poolResource.PrefabsMap.Add(_nameObject, _resultResource);
            }
            _result?.Invoke(_resultResource);
        }, _pathPrefabs));
    }
    public static void GetObject(string _pathPrefabs, string _nameObject, Action<UnityEngine.Object> _result)
    {
        if (poolResource == null)
        {
            poolResource = new GameObject("Pool_Resource").AddComponent<ManagerResouce>();
        }
        UnityEngine.Object result = default;
        if (poolResource.ObjectsMap.TryGetValue(_nameObject, out result))
        {
            _result?.Invoke(result);
            return;
        }
        poolResource.StartCoroutine(poolResource.FindObject(_pathPrefabs, _nameObject, (_resultResource) =>
        {
            if (_resultResource != null)
            {
                poolResource.ObjectsMap.Add(_nameObject, _resultResource);
            }
            _result?.Invoke(_resultResource);
        }));
    }
    private Dictionary<string, GameObject> PrefabsMap = new Dictionary<string, GameObject>();
    private Dictionary<string, UnityEngine.Object> ObjectsMap = new Dictionary<string, UnityEngine.Object>();
    public IEnumerator FindPrefab(string _nameObject, Action<GameObject> _result, string _pathPrefabs)
    {
        ResourceRequest request = Resources.LoadAsync($"{_pathPrefabs}/{_nameObject}");
        yield return new WaitUntil(() => request.isDone);
        _result?.Invoke(request.asset as GameObject);
        yield break;
    }
    public IEnumerator FindObject(string _pathPrefabs, string _nameObject, Action<UnityEngine.Object> _result)
    {
        ResourceRequest request = Resources.LoadAsync($"{_pathPrefabs}/{_nameObject}");
        yield return new WaitUntil(() => request.isDone);
        _result?.Invoke(request.asset);
        yield break;
    }
}

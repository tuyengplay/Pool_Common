using GameCore.Pool;
using UnityEngine;

public class InputM : MonoBehaviour
{
    [SerializeField]
    private GameObject prefabs;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Aaa a = ManagerPool.Spawn<Aaa>(prefabs.GetComponent<Aaa>(), 5);
            //GameObject a = ManagerPool.Spawn(prefabs);
            Debug.Log(a.ToString());
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            ManagerResouce.GetPrefab("Hello", (result) =>
            {
                ManagerPool.Spawn(result);
            });
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ManagerResouce.GetPrefab("Hello_A", (result) =>
            {
                ManagerPool.Spawn(result);
            });
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ManagerPool.DespawnAll();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ManagerPool.KillAll();
        }
    }
    private void OnDisable()
    {
        ManagerPool.KillAll();
    }
}

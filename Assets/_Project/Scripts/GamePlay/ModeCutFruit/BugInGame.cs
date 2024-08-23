using GamePlay;
using UnityEngine;

public class BugInGame : MonoBehaviour
{
    private float speed = 1f;
    private Vector3 moveDirection;
    private int layerItem;

    private bool isDeath;
    private Fruit fruitCurrent;
    private Fruit FruitCurrent
    {
        get => fruitCurrent;
        set
        {
            if (fruitCurrent != null)
            {
                fruitCurrent.RemoveBug(this);
            }
            fruitCurrent = value;
            if (fruitCurrent != null)
            {
                fruitCurrent.AddBug(this);
            }
        }
    }
    private void Start()
    {
        ChangeDirection();
        layerItem = LayerMask.GetMask("Item");
        isDeath = false;
    }
    private void Update()
    {
        if (isDeath)
        {
            return;
        }
        Vector2 newPosition = transform.position + moveDirection * speed * Time.deltaTime;

        if (IsInsideLeaf(newPosition))
        {
            transform.position = newPosition;
        }
        else
        {
            ChangeDirection();
        }
    }
    private void ChangeDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0).normalized;
    }
    private bool IsInsideLeaf(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, 5);
        if (hit.collider == null)
        {
            return false;
        }
        else
        {
            Fruit fruit = hit.collider.gameObject.GetComponent<Fruit>();
            if (fruit != null && fruit.CheckObject(this) == false)
            {
                FruitCurrent = fruit;
            }
            return true;
        }
    }
    public void OnDeath()
    {
        isDeath = true;
        gameObject.AddComponent<Rigidbody2D>();
        CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
        circle.radius = 0.08f;
        circle.isTrigger = false;
    }
}

using System.Collections.Generic;
using UnityEngine;
namespace GamePlay
{
    public class Fruit : MonoBehaviour
    {
        private List<BugInGame> bugs = new List<BugInGame>();
        public bool CheckObject(BugInGame _bug)
        {
            return bugs.Contains(_bug);
        }
        public bool IsNoBug
        {
            get => bugs.Count < 1;
        }
        public void AddBug(BugInGame _bug)
        {
            bugs.Add(_bug);
        }
        public void RemoveBug(BugInGame _bug)
        {
            bugs.Remove(_bug);
        }
        public void DropObject()
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            Rigidbody2D rig = gameObject.GetComponent<Rigidbody2D>();
            rig.bodyType = RigidbodyType2D.Dynamic;
            foreach (BugInGame bug in bugs)
            {
                bug.OnDeath();
            }
            Destroy(gameObject, 0.1f);
        }
        public float Acreage()
        {
            PolygonCollider2D poly = gameObject.GetComponent<PolygonCollider2D>();
            return ModeCutFruit.CalculatePolygonArea(poly.points);
        }
    }
}

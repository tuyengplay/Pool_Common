using GameCore.Slicer2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GamePlay
{
    public class ModeCutFruit : BaseLevel
    {
        [SerializeField]
        private LineRenderer lineCut;
        private Vector3 posBegin;
        private bool isDrawCut;
        private PolygonCollider2D polygon;
        private float acreage;
        private float percentInOne;
        [SerializeField, Range(3, 20)]
        private int needCut = 10;
        private float fNeedCut;
        private float fNeedCutCurrent;
        [SerializeField]
        private LayerMask layerCut;
        private List<SpriteSlicer2DSliceInfo> dataCut = new List<SpriteSlicer2DSliceInfo>();
        public override void OnSetUp(object[] parametters)
        {
            base.OnSetUp(parametters);
            acreage = CalculatePolygonArea(polygon.points);
            percentInOne = acreage / 25f;
            fNeedCut = percentInOne * needCut;
            isDrawCut = false;
        }
        public override void OnInputBegin(Vector3 _posInput)
        {
            posBegin = _posInput;
            lineCut.positionCount = 2;
            lineCut.SetPosition(0, posBegin);
            lineCut.SetPosition(1, posBegin);
            isDrawCut = true;
        }
        public override void OnInputMove(Vector3 _posInput)
        {
            if (isDrawCut)
            {
                lineCut.SetPosition(1, _posInput);
            }
        }
        public override void OnInputEnd(Vector3 _posInput)
        {
            dataCut.Clear();
            if (isDrawCut)
            {
                SpriteSlicer2D.SliceAllSprites(posBegin, _posInput, true, ref dataCut, layerCut);
                if (dataCut.Count > 0)
                {
                    GameObject objOne = dataCut[0].ChildObjects[0];
                    GameObject objTwo = dataCut[0].ChildObjects[1];
                    Fruit fruitOne = objOne.AddComponent<Fruit>();
                    Fruit fruitTwo = objTwo.AddComponent<Fruit>();
                    StartCoroutine(IE_WaitForFrame(fruitOne, fruitTwo));
                }
            }
            isDrawCut = false;
            lineCut.positionCount = 0;
        }
        private IEnumerator IE_WaitForFrame(Fruit _fruitOne, Fruit _fruitTwo)
        {
            float acrOne = _fruitOne.Acreage();
            float acrTwo = _fruitTwo.Acreage();
            yield return null;
            if (_fruitOne.IsNoBug)
            {
                _fruitOne.DropObject();
                fNeedCutCurrent += acrOne;
                int count = (int)Mathf.Clamp(acrOne / percentInOne, 1, 25);
                for (int i = 0; i < count; i++)
                {
                    Debug.Log("FX");
                }
                if (fNeedCutCurrent >= fNeedCut)
                {
                    IsEndGame = true;
                    OnEnLevel(true);
                }
                yield break;
            }
            if (_fruitTwo.IsNoBug)
            {
                _fruitTwo.DropObject();
                fNeedCutCurrent += acrTwo;
                int count = (int)Mathf.Clamp(acrTwo / percentInOne, 1, 25);
                for (int i = 0; i < count; i++)
                {
                    Debug.Log("FX");
                }
                if (fNeedCutCurrent >= fNeedCut)
                {
                    IsEndGame = true;
                    OnEnLevel(true);
                }
                yield break;
            }
            if (acrOne > acrTwo)
            {
                _fruitOne.DropObject();
            }
            else
            {
                _fruitTwo.DropObject();
            }
            IsEndGame = true;
            OnEnLevel(false);
            yield break;
        }
        public static float CalculatePolygonArea(Vector2[] vertices)
        {
            float area = 0;
            int j = vertices.Length - 1;

            for (int i = 0; i < vertices.Length; i++)
            {
                area += (vertices[j].x + vertices[i].x) * (vertices[j].y - vertices[i].y);
                j = i;
            }

            return Mathf.Abs(area / 2);
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            polygon = gameObject.GetComponentInChildren<PolygonCollider2D>();
            lineCut = gameObject.GetComponentInChildren<LineRenderer>();
            lineCut.startWidth = 0.2f;
            lineCut.endWidth = 0.2f;
        }
#endif
    }
}

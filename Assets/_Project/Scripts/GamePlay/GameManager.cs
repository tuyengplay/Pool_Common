using UnityEngine;
namespace GamePlay
{
    public class GameManager : MonoBehaviour
    {
        private int layerZone;
        private Vector3 posTouch;
        [SerializeField]
        private BaseLevel levelCurrent;
        //Mobile
        private Touch touch;
        //
        private void Start()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Debug.Log("______Emulator______");
            }
            else
            {
                Debug.Log("______NO Emulator______");
            }
            levelCurrent.OnSetUp(new object[] { });
        }

        private void Update()
        {
            if (Input.touchCount < 1 || levelCurrent == null || levelCurrent.IsEndGame)
            {
                return;
            }
            touch = Input.GetTouch(0);
            posTouch = Camera.main.ScreenToWorldPoint(touch.position);
            posTouch.z = 0;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    {
                        levelCurrent.OnInputBegin(posTouch);
                    }
                    break;
                case TouchPhase.Moved:
                    {
                        levelCurrent.OnInputMove(posTouch);
                    }
                    break;
                case TouchPhase.Stationary:
                    {
                        levelCurrent.OnInputStationary(posTouch);
                    }
                    break;
                default:
                    {
                        levelCurrent.OnInputEnd(posTouch);
                    }
                    break;
            }
        }
    }
}

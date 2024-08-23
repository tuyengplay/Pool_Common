using UnityEngine;
namespace GamePlay
{
    public abstract class BaseLevel : MonoBehaviour
    {
        public bool IsEndGame
        {
            get;
            protected set;
        }
        public virtual void OnSetUp(object[] parametters)
        {
            IsEndGame = false;
        }
        public virtual void OnStartLevel()
        {
            IsEndGame = false;
        }
        public abstract void OnInputBegin(Vector3 _posInput);
        public virtual void OnInputMove(Vector3 _posInput) { }
        public virtual void OnInputStationary(Vector3 _posInput) { }
        public virtual void OnInputEnd(Vector3 _posInput) { }
        public virtual void OnEnLevel(bool _isWin)
        {
            IsEndGame = true;
            if (_isWin)
            {
                Debug.Log("Win");
            }
            else
            {
                Debug.Log("Lose");
            }
        }
    }
}

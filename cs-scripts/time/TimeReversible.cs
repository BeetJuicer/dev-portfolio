using UnityEngine;

namespace CommandPattern
{
    public abstract class TimeReversible : MonoBehaviour
    {
        protected bool isReversing;
        public void Reverse()
        {
            isReversing = true;
        }
        public void StopReversing()
        {
            isReversing = false;
        }
    }
}
using UnityEngine;

namespace zc.util
{
    public static partial class Util
    {
        public static Vector2 ClampVector(this Vector2 inOriginal, float inClampValue = 0.5f)
        {
            return new Vector2(Mathf.Clamp(inOriginal.x, -inClampValue, inClampValue), Mathf.Clamp(inOriginal.y, -inClampValue, inClampValue));
        }
        
    }
}

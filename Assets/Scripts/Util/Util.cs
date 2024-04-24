using UnityEngine;

namespace zc.util
{
    public static partial class Util
    {
        public static void SetActive(this Component inComponent, bool inActive)
        {
            inComponent.gameObject.SetActive(inActive);
        }
    }
}

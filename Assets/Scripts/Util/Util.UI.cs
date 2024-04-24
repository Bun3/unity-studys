using UnityEngine;

namespace zc.util
{
    public static partial class Util
    {
        public static void ScreenPointToClampedWorldPoint(RectTransform inOuter, RectTransform inInner, Vector2 inScreenPoint, out Vector2 outWorldPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(inOuter, inScreenPoint, null, out var localPoint_);

            outWorldPoint = inOuter.TransformPoint(GetClampedLocal(localPoint_, inOuter, inInner));
        }
        
        public static void ScreenPointToClampedLocalPoint(RectTransform inOuter, RectTransform inInner, Vector2 inScreenPoint, out Vector2 outLocalPoint)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(inOuter, inScreenPoint, null, out outLocalPoint);
            
            outLocalPoint = GetClampedLocal(outLocalPoint, inOuter, inInner);
        }
        
        public static Vector2 GetClampedLocal(Vector2 inLocalVector, RectTransform inOuter, RectTransform inInner)
        {
            var outerRect_ = inOuter.rect;
            var innerRect_ = inInner.rect;
            
            var xMin_ = outerRect_.xMin + innerRect_.width / 2;
            var xMax_ = outerRect_.xMax - innerRect_.width / 2;
            var yMin_ = outerRect_.yMin + innerRect_.height / 2;
            var yMax_ = outerRect_.yMax - innerRect_.height / 2;

            return new Vector2(Mathf.Clamp(inLocalVector.x, xMin_, xMax_), Mathf.Clamp(inLocalVector.y, yMin_, yMax_));
        }
        
    }
}

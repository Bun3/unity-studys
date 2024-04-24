using UnityEngine;
using UnityEngine.EventSystems;

using zc.util;
using zc.system;

namespace zc.input
{
    [DefaultExecutionOrder(-100)]
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform rtSafeBound;
        [SerializeField] private RectTransform rtArea;
        [SerializeField] private RectTransform rtKnob;
        
        [SerializeField] private float movementRange = 100f;
        [SerializeField] private bool normalizedToRange = true;
        
        private Vector2 downPosition;
        
        private void Start()
        {
            rtArea.SetActive(false);
        }

        public void OnPointerDown(PointerEventData inEventData)
        {
            rtArea.SetActive(true);

            Util.ScreenPointToClampedLocalPoint(rtSafeBound, rtArea, inEventData.position, out var pos_);
            rtArea.anchoredPosition = downPosition = pos_;
            
            MoveKnob(inEventData.position);
        }
        
        public void OnDrag(PointerEventData inEventData)
        {
            MoveKnob(inEventData.position);
        }
        
        private void MoveKnob(Vector2 inPointerScreenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rtSafeBound, inPointerScreenPosition, null, out var pos_);
            
            var delta_ = Vector2.ClampMagnitude(pos_ - downPosition, movementRange);
            delta_ = normalizedToRange ? delta_.normalized * movementRange : delta_;
            
            rtKnob.anchoredPosition = delta_;

            //safety
            PlayerInputSystem.instance.joystickInput = delta_.normalized;
        }

        public void OnPointerUp(PointerEventData inEventData)
        {
            PlayerInputSystem.instance.joystickInput = Vector2.zero;
            
            rtArea.SetActive(false);
        }


    }
}

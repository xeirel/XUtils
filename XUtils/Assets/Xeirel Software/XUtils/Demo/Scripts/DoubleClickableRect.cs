using UnityEngine;
using UnityEngine.EventSystems;

namespace XUtils.Samples
{
    public class DoubleClickableRect : UIDoubleClickHandler
    {
        private void Start()
        {
            SetDoubleClickThreshold(0.4f);
        }
        protected override void HandleDoubleClick(PointerEventData eventData)
        {
            GetComponentInParent<XUIUtilsSamples>().OnSampleDoubleClick();
        }
    }
}

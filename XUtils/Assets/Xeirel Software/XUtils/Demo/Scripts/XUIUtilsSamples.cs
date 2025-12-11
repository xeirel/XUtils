using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XUtils.StringUtils;
using XUtils.UIUtils;

namespace XUtils.Samples
{
    public class XUIUtilsSamples : MonoBehaviour
    {
        [Header("Text Morphing")]
        public TMP_Text MorphText;
        public Slider MorphSlider;
        public Toggle MorphRandomizeToggle;
        public string startString = "Hello World!";
        public string endString = "Goodbye World :(";

        [Header("UI Hover Utility")]
        public GameObject TargetHoverObject;
        public TMP_Text HoverStateText;

        [Header("Double Click Handler")]
        private int clickCount = 0;
        public TMP_Text DoubleClickState;

        [Header("Json Colors Example")]
        public TMP_Text JsonText;

        [Header("Colorized Money Example")]
        public TMP_Text MoneyText;

        [Header("Stack Trace Test")]
        public TMP_Text StackTraceText;
        public TMP_Text StackTraceCallerText;

        private void Start()
        {
            SimulateError();
        }
        private void SimulateError()
        {
            try
            {
                throw new System.Exception("Simulated Exception for Stack Trace Test");
            }
            catch (System.Exception ex)
            {
                StackTraceText.text = ex.ToString();
                StackTraceCallerText.text = "Caller: " + ex.StackTrace.ExtractCallerFromStack();
                throw;
            }
        }
        private void Update()
        {
            // Text Morphing Sample
            MorphText.text = MorphRandomizeToggle.isOn
                ? XString.MorphStringRandom(startString, endString, MorphSlider.value)
                : XString.MorphString(startString, endString, MorphSlider.value);

            // UI Hover Utility Sample
            HoverStateText.text = XUIHoverUtility.CurrentHovered == TargetHoverObject
                ? $"Hovering for {XUIHoverUtility.SecondsSinceHoverEnter.ToString("F1")}s".Italic()
                : "Not hovering over target.".Bold();

            // Json Colors Sample
            JsonText.text = XString.ColorizeJson("{\r\n  \"title\": \"Example\",\r\n  \"count\": 42,\r\n  \"active\": false,\r\n  \"value\": 3.14,\r\n  \"tags\": [\"alpha\", \"beta\"],\r\n  \"info\": {\r\n    \"id\": 7\r\n  }\r\n}\r\n");

            // Colorized Money Sample
            MoneyText.text = $"You have {3293524.ToSmartString(useThousandSeparator: true)}$ in your bank account.".GiveMoneyColor();
        }
        public void OnSampleDoubleClick()
        {
            clickCount++;
            DoubleClickState.text = $"Double Clicked {clickCount} times!";
        }
    }
}

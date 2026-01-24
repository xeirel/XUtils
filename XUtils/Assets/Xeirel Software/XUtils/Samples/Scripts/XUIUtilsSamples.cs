using TMPro;
using UnityEngine;
using XUtils.UIUtils;
using UnityEngine.UI;
using XUtils.StringUtils;

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
            //SimulateError();
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
        public void BasicMessageDialog()
        {
            XModalDialog.ShowMessage("This is a basic message dialog.", "Basic Message");
        }
        public void BasicYesNoDialog()
        {
            XModalDialog.ShowYesNoCancel("This is a basic Yes / No / Cancel dialog. You can cancel dialogs with 'Escape' Key/Input System Action if defined.", "Do you want to proceed?").OnResult(result =>
            {
                XModalDialog.ShowMessage($"You selected: {result.Result.ToString()}", "Result");
            });
        }
        public void BasicInputDialog()
        {
            XModalDialog.CreateDialog()
                .WithTitle("Custom Input Dialog")
                .WithMessage("Please enter your name below:")
                .AddInputField("str_username", "Your name here...", "") // Input field with key "str_username"
                .AddButton("Submit", ModalDialogResult.Custom)
                .AddButton("Cancel", ModalDialogResult.Cancel)
                .OnResult(result =>
                {
                    if (result.Result == ModalDialogResult.Cancel)
                        return;

                    string username = result.GetInput("str_username");
                    if (string.IsNullOrWhiteSpace(username))
                    {
                        XModalDialog.ShowMessage("Name cannot be empty. Please try again.", "Input Error").OnResult((Result) => { BasicInputDialog(); });
                        return;
                    }

                    XModalDialog.ShowMessage($"Hello {username}", "Result");
                }).Show(); // Do not forget to call Show() at the end!
        }
    }
}

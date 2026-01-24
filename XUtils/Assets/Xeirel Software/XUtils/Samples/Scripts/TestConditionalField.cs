using UnityEngine;
using XUtils.EditorUtils;

namespace XUtils.Samples
{
    public class TestConditionalField : MonoBehaviour
    {
        [Header("Conditional Field")]
        public bool ShowAdvancedSettings = false;
        [ConditionalField(nameof(ShowAdvancedSettings))] public bool ShowFPS = false;

        public TestEnum MyTestEnum;
        [ConditionalField(nameof(MyTestEnum), typeof(TestEnum), TestEnum.OptionB)]
        public string AdvancedOptionForB;


        public enum TestEnum
        {
            OptionA,
            OptionB
        }
    }
}
using System;
using UnityEditor;
using UnityEngine;

// Usage:

//  public bool ShowAdvancedSettings = false;
//  [ConditionalField(nameof(ShowAdvancedSettings))] public bool ShowFPS = false;

//  public TestEnum MyTestEnum;
//  [ConditionalField(nameof(MyTestEnum), typeof(TestEnum), TestEnum.OptionB)]
//  public string AdvancedOptionForB;

namespace XUtils.EditorUtils
{
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public string ConditionField { get; private set; }
        public bool Inverse { get; private set; }

        public Type EnumType { get; private set; }
        public object EnumValue { get; private set; }

        public ConditionalFieldAttribute(string conditionField, bool inverse = false)
        {
            ConditionField = conditionField;
            Inverse = inverse;
        }

        public ConditionalFieldAttribute(string conditionField, Type enumType, object enumValue)
        {
            ConditionField = conditionField;
            EnumType = enumType;
            EnumValue = enumValue;
            Inverse = false;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
    public class ConditionalFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShouldDraw(property))
                EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShouldDraw(property) ? EditorGUI.GetPropertyHeight(property, label, true) : 0;
        }

        private bool ShouldDraw(SerializedProperty property)
        {
            ConditionalFieldAttribute cond = (ConditionalFieldAttribute)attribute;
            SerializedProperty conditionProp = property.serializedObject.FindProperty(cond.ConditionField);

            if (conditionProp == null)
                return true;

            bool enabled = true;

            if (conditionProp.propertyType == SerializedPropertyType.Boolean)
            {
                enabled = conditionProp.boolValue;
                if (cond.Inverse)
                    enabled = !enabled;
            }
            else if (conditionProp.propertyType == SerializedPropertyType.Enum && cond.EnumType != null)
            {
                int currentEnumValue = conditionProp.enumValueIndex;
                int targetEnumValue = Convert.ToInt32(cond.EnumValue);
                enabled = currentEnumValue == targetEnumValue;
            }

            return enabled;
        }
    }
#endif
}

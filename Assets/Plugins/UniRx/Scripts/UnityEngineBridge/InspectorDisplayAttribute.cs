using UnityEngine;
namespace EranCore.UniRx
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class InspectorDisplayAttribute : PropertyAttribute
    {
        public string FieldName { get; private set; }
        public bool NotifyPropertyChanged { get; private set; }

        public InspectorDisplayAttribute(string fieldName = "value", bool notifyPropertyChanged = true)
        {
            FieldName = fieldName;
            NotifyPropertyChanged = notifyPropertyChanged;
        }
    }

    /// <summary>
    /// Enables multiline input field for StringReactiveProperty. Default line is 3.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class MultilineReactivePropertyAttribute : PropertyAttribute
    {
        public int Lines { get; private set; }

        public MultilineReactivePropertyAttribute()
        {
            Lines = 3;
        }

        public MultilineReactivePropertyAttribute(int lines)
        {
            Lines = lines;
        }
    }

    /// <summary>
    /// Enables range input field for Int/FloatReactiveProperty.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class RangeReactivePropertyAttribute : PropertyAttribute
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public RangeReactivePropertyAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
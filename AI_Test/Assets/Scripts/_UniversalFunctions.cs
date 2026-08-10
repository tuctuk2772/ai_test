using UnityEngine;

namespace UniversalFunctions
{
    public static class _UniversalFunctions
    {
        public static float ConvertRangeNewValue(float oldMin, float oldMax, float newMin, float newMax, float oldValue)
        {
            float oldRange = oldMax - oldMin;
            float newRange = newMax - newMin;

            return (((oldValue - oldMin) * newRange) / oldRange) + newMin;
        }
    }
}
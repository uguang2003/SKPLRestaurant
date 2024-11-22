//
// FPEMinMaxRangeAttribute.cs
//
// Use a FPEMinMaxRange class to replace twin float range values (eg: float minSpeed, maxSpeed; becomes FPEMinMaxRange speed)
// Apply a [FPEMinMaxRange( minLimit, maxLimit )] attribute to a FPEMinMaxRange instance to control the limits and to show a
// slider in the inspector
//

using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Whilefun.FPEKit
{

    public class FPEMinMaxRangeAttribute : PropertyAttribute
    {

        public float minLimit = 0.0f;
        public float maxLimit = 0.0f;

        public FPEMinMaxRangeAttribute(float minLimit, float maxLimit)
        {
            this.minLimit = minLimit;
            this.maxLimit = maxLimit;
        }

    }

    [Serializable]
    public class FPEMinMaxRange
    {

        public float minValue = 0.0f;
        public float maxValue = 0.0f;

        public float GetRandomValue()
        {
            return Random.Range(minValue, maxValue);
        }

    }

}

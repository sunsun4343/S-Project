using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATJS
{
    public class GVector3
    {
        Vector3 _value0;
        Vector3 _value1;

        public Vector3 value
        {
            get
            {
                if (ATJSManager.Instance.bufferCursor)
                {
                    return _value0;
                }
                else
                {
                    return _value1;
                }
            }

            set
            {
                if (ATJSManager.Instance.bufferCursor)
                {
                    _value0 = value;
                }
                else
                {
                    _value1 = value;
                }
            }
        }

    }
}
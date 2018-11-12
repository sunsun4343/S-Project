using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ATJS
{
    public class Job
    {
        public byte priority;
        public bool isInfinity;

        public delegate void JobAction();
        public JobAction jobAction;

        public void Work()
        {
            if (jobAction != null)
            {
                jobAction();
            }
        }
    }
}


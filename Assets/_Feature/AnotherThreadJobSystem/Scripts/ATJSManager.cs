using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ATJS
{
    public class ATJSManager : SingletonMono<ATJSManager>
    {
        private Thread _updateThread;
        private bool _isThreadAlive = true;
        private bool _isUpdateOn = true;

        private bool _pause;

        private bool _bufferCursor;
        public bool bufferCursor { get { return _bufferCursor; } }

        private List<Job> _topPriorityJobList = new List<Job>();
        private List<Job> _otherPriorityJobList = new List<Job>();

        protected virtual void Awake()
        {
            ATJSManager temp = ATJSManager.Instance;

            _pause = false;
        }

        protected virtual void Start()
        {
            _updateThread = new Thread(thread_entry);
            _updateThread.Start();
        }

        private void thread_entry()
        {
            Thread_Init();

            while (_isThreadAlive)
            {
                if (_isUpdateOn)
                {
                    _isUpdateOn = false;

                    Update_TopPriority();
                }

                Update_OtherPriority();
            }
        }

        protected virtual void LateUpdate()
        {
            if (_bufferCursor)
            {

            }
            else
            {

            }

            _isUpdateOn = true;
            _bufferCursor = !_bufferCursor;

        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _isThreadAlive = false;
            if (_updateThread != null)
            {
                _updateThread.Abort();
            }
        }

        private void Update_TopPriority()
        {
            for (int i = 0; i < _topPriorityJobList.Count; i++)
            {
                _topPriorityJobList[i].Work();
            }
        }

        private void Update_OtherPriority()
        {
            
            for (int i = 0; i < _otherPriorityJobList.Count; i++)
            {
                _otherPriorityJobList[i].Work();
            }
        }

        protected virtual void Thread_Init()
        {

        }

        protected void AddTopPriorityJob(Job job)
        {
            _topPriorityJobList.Add(job);
        }

        public void Pause()
        {
            _pause = true;
        }

        public void Resume()
        {
            _pause = false;
        }



    }
}


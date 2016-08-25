using SocialPoint.Base;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SocialPoint.Threading
{
    public sealed class Task
    {
        Action _workAction;
        Action _completionAction;

        public bool isCancelled { get; private set; }
        public bool haveCompletionAction { get; private set; }

        public Task(Action workAction, Action completionAction)
        {
            _workAction = workAction;
            _completionAction = completionAction;
            isCancelled = false;
            haveCompletionAction = completionAction != null;
        }

        public Task(Action workAction) : this(workAction, null)
        {
           
        }

        internal void Run()
        {
            _workAction();
        }

        internal void OnFinished()
        {
            if(!isCancelled && _completionAction != null)
            {
                _completionAction();
            }
        }

        public void Cancel()
        {
            isCancelled = true;
        }
    }

    public sealed class TaskDispatcher
    {
        int _numRunningTasks;
        int _maxSimultaneousTasks;
        Queue<Task> _pendingTasks;
        Queue<Task> _completedTasks;

        public TaskDispatcher(int maxSimultaneousTasks = 1)
        {
            _numRunningTasks = 0;
            _maxSimultaneousTasks = maxSimultaneousTasks;
            _pendingTasks = new Queue<Task>();
            _completedTasks = new Queue<Task>();
        }

        public void Dispatch(Task task)
        {
            lock(_pendingTasks)
            {
                if(_numRunningTasks < _maxSimultaneousTasks)
                {
                    ++_numRunningTasks;
                    ThreadPool.QueueUserWorkItem(RunTask, task);
                }
                else
                {
                    _pendingTasks.Enqueue(task);
                }
            }
        }

        void RunTask(object obj)
        {
            var task = (Task)obj;

            try
            {
                task.Run();
            }
            catch(Exception e)
            {           
                Log.x(e);
            }
            finally
            {
                if(!task.isCancelled && task.haveCompletionAction)
                {
                    lock(_completedTasks)
                    {
                        _completedTasks.Enqueue(task);
                    }
                }

                lock(_pendingTasks)
                {
                    --_numRunningTasks;

                    if(_pendingTasks.Count > 0 && _numRunningTasks < _maxSimultaneousTasks)
                    {
                        bool nextTaskFound = false;
                        while(_pendingTasks.Count > 0 && !nextTaskFound)
                        {
                            Task nextTask = _pendingTasks.Dequeue();
                            if(!nextTask.isCancelled)
                            {
                                ++_numRunningTasks;
                                ThreadPool.QueueUserWorkItem(RunTask, nextTask);
                                nextTaskFound = true;
                            }
                        }
                    }
                }
            } 
        }

        public void Update()
        {
            if(_completedTasks.Count > 0)
            {
                Queue<Task> completedTaskCopy = null;

                lock(_completedTasks)
                {
                    completedTaskCopy = new Queue<Task>(_completedTasks);
                    _completedTasks.Clear();
                }

                Task task = null;

                while(completedTaskCopy.Count > 0)
                {
                    task = completedTaskCopy.Dequeue();
                    task.OnFinished();
                }
            }
        }
    }
}

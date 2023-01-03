/// CoroutineTaskManager.cs
///
/// This is a convenient coroutine API for Unity.
///
/// Example usage:
///   IEnumerator MyAwesomeTask()
///   {
///       while(true) 
///       {
///           // ...
///           yield return null;
////      }
///   }
///
///   IEnumerator TaskKiller(float delay, Task t)
///   {
///       yield return new WaitForSeconds(delay);
///       t.Stop();
///   }
///
///   // From anywhere
///   CoroutineTask my_task = new CoroutineTask(MyAwesomeTask());
///   new CoroutineTask(TaskKiller(5, my_task));
///
/// The code above will schedule MyAwesomeTask() and keep it running
/// concurrently until either it terminates on its own, or 5 seconds elapses
/// and triggers the TaskKiller Task that was created.
///
/// Note that to facilitate this API's behavior, a "CoroutineTaskManager" GameObject is
/// created lazily on first use of the Task API and placed in the scene root
/// with the internal TaskManager component attached. All coroutine dispatch
/// for Tasks is done through this component.
/// 
/// TODO : (maybe) Add editor support?
/// NOTE : <see cref="BXFW.Tweening.BXTween"/> does not use this task manager. Instead it uses it's own task manager.

using UnityEngine;
using System.Collections;

namespace BXFW
{
    /// <summary>
    /// A CoroutineTask object represents a coroutine. Tasks can be started, paused, and stopped.
    /// <br>NOTE : It is an error to attempt to start a task that has been stopped or which has naturally terminated.</br>
    /// </summary>
    public class CoroutineTask
    {
        /// Returns true if and only if the coroutine is running. Paused tasks
        /// are considered to be running.
        public bool Running
        {
            get
            {
                return task.Running;
            }
        }

        /// Returns true if and only if the coroutine is currently paused.
        public bool Paused
        {
            get
            {
                return task.Paused;
            }
        }

        /// <summary>Delegate for termination subscribers. Manual is true if and only if
        /// the coroutine was stopped with an explicit call to Stop().</summary>
        public delegate void FinishedHandler(bool manual);

        /// <summary>Termination event. Triggered when the coroutine completes execution.</summary>
        public event FinishedHandler Finished;

        /// <summary>
        /// Creates a new Task object for the given coroutine.
        ///
        /// If autoStart is true (default) the task is automatically started
        /// upon construction.
        /// </summary>
        public CoroutineTask(IEnumerator c, bool autoStart = true)
        {
            task = TaskManager.CreateTask(c);
            task.Finished += TaskFinished;

            if (autoStart)
                Start();
        }

        /// <summary> Begins execution of the coroutine. </summary>
        public void Start()
        {
            task.Start();
        }

        /// <summary> Discontinues execution of the coroutine at its next <see langword="yield"/>. </summary>
        public void Stop()
        {
            task.Stop();
        }

        /// <summary> Pauses the coroutine at it's next yield. (Stores the <see cref="IEnumerator.Current"/> variable)
        public void Pause()
        {
            task.Pause();
        }

        /// <summary> Unpauses the task. (If it's paused) </summary>
        public void Unpause()
        {
            task.UnPause();
        }

        private void TaskFinished(bool manual)
        {
            FinishedHandler handler = Finished;

            if (handler != null)
                handler(manual);
        }

        private readonly TaskManager.TaskState task;
    }

    /// <summary>
    /// Manages all created <see cref="CoroutineTask"/>'s.
    /// </summary>
    internal class TaskManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton <see cref="MonoBehaviour"/> variable to run coroutines over.
        /// </summary>
        private static TaskManager instance;

        /// <summary>
        /// Object that represents state of the current coroutine task assigned.
        /// </summary>
        public class TaskState
        {
            public bool Running
            {
                get
                {
                    return running;
                }
            }

            public bool Paused
            {
                get
                {
                    return paused;
                }
            }

            public delegate void FinishedHandler(bool manual);
            public event FinishedHandler Finished;

            private IEnumerator coroutine;
            private bool running;
            private bool paused;
            private bool stopped;

            public TaskState(IEnumerator c)
            {
                coroutine = c;
            }

            public void Pause()
            {
                if (paused)
                {
                    Debug.LogWarning("[CoroutineTaskManager::TaskState::UnPause] Coroutine running is paused.");
                    return;
                }

                paused = true;
            }

            public void UnPause()
            {
                if (!paused)
                {
                    Debug.LogWarning("[CoroutineTaskManager::TaskState::UnPause] Coroutine running isn't paused.");
                    return;
                }

                paused = false;
            }

            public void Start()
            {
                if (running)
                {
                    Debug.LogWarning("[CoroutineTaskManager::TaskState::Start] Coroutine is already running.");
                    return;
                }

                running = true;
                instance.StartCoroutine(CallWrapper());
            }

            public void Stop()
            {
                if (!running && stopped)
                {
                    Debug.LogError("[CoroutineTaskManager::TaskState::Stop] Coroutine is already stopped.");
                    return;
                }

                stopped = true;
                running = false;
            }

            private IEnumerator CallWrapper()
            {
                yield return null;
                IEnumerator e = coroutine;

                while (running)
                {
                    if (paused)
                        yield return null;
                    else
                    {
                        if (e != null && e.MoveNext())
                        {
                            yield return e.Current;
                        }
                        else
                        {
                            running = false;
                        }
                    }
                }

                FinishedHandler handler = Finished;
                if (handler != null)
                    handler(stopped);
            }
        }

        /// <summary>
        /// Constructs a <see cref="TaskState"/> object.
        /// <br>Initilazes <see cref="TaskManager"/> if it isn't initilazed.</br>
        /// </summary>
        /// <param name="coroutine"></param>
        /// <returns></returns>
        public static TaskState CreateTask(IEnumerator coroutine)
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CoroutineTaskManager");
                instance = go.AddComponent<TaskManager>();
                DontDestroyOnLoad(instance);
            }

            return new TaskState(coroutine);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SocialPoint.Utils
{
    public sealed class PriorityCoroutineActionTests : MonoBehaviour
    {
        PriorityCoroutineAction _queue;
        int prioMinusCount = 0;
        int prioNormalCount = 0;
        int prioHighCount = 0;

        void Start()
        {

            var runner = this.gameObject.AddComponent<UnityUpdateRunner>();

            _queue = new PriorityCoroutineAction(runner);
            _queue.Add(-10, () => CountToOne(-10));
            IncreasePrio(-10);
            _queue.Add(-10, () => CountToTwo(-10));
            IncreasePrio(-10);
            _queue.Add(-10, () => CountToTwo(-10));
            IncreasePrio(-10);
            _queue.Add(0, () => CountToOne(0));
            IncreasePrio(0);
            _queue.Add(0, () => CountToOne(0));
            IncreasePrio(0);
            _queue.Add(0, () => CountToThree(0));
            IncreasePrio(0);
            _queue.Add(10, () => CountToOne(10));
            IncreasePrio(10);
            _queue.Add(10, () => CountToThree(10));
            IncreasePrio(10);
            _queue.Run();
        }

        public IEnumerator CountToOne(int prio)
        {
            yield return new WaitForSeconds(0.5f);
            print("Counted to one");
            DecreasePrio(prio);
        }

        public IEnumerator CountToTwo(int prio)
        {
            yield return new WaitForSeconds(1f);
            print("Counted to two");
            DecreasePrio(prio);
        }

        public IEnumerator CountToThree(int prio)
        {
            yield return new WaitForSeconds(1.5f);
            print("Counted to three");
            DecreasePrio(prio);
        }

        void IncreasePrio(int prio)
        {
            switch(prio)
            {
            case -10:
                prioMinusCount++;
                break;
            case 0:
                prioNormalCount++;
                break;
            case 10:
                prioHighCount++;
                break;
            }
        }

        void DecreasePrio(int prio)
        {
            switch(prio)
            {
            case -10:
                if(prioHighCount > 0 || prioNormalCount > 0)
                    IntegrationTest.Fail("not all higher prio done before");
                prioMinusCount--;
                break;
            case 0:
                if(prioHighCount > 0)
                    IntegrationTest.Fail("not all higher prio done before");
                prioNormalCount--;
                break;
            case 10:
                prioHighCount--;
                break;
            }

            if(prioHighCount == 0 && prioMinusCount == 0 && prioNormalCount == 0)
                IntegrationTest.Pass();
        }
    }
}


using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public class UpdateCoroutineRunner : ICoroutineRunner, IUpdateable
    {
        public sealed class InvalidYieldStatementException : System.Exception
        {
            public InvalidYieldStatementException(string message) : base(message)
            {
            }
        }

        class CoroutineNode
        {
            public bool Finished;
            public CoroutineNode WaitForCoroutine;

            public IEnumerator Enumerator{ get; private set; }

            public CoroutineNode(IEnumerator enumerator)
            {
                Enumerator = enumerator;
            }
        }

        readonly LinkedList<CoroutineNode> _listCoroutines;

        public UpdateCoroutineRunner(IUpdateScheduler scheduler)
        {
            _listCoroutines = new LinkedList<CoroutineNode>();
            scheduler.Add(this);
        }

        public IEnumerator StartCoroutine(IEnumerator enumerator)
        {
            if(enumerator == null)
            {
                return null;
            }
            // create coroutine node and run until we reach first yield
            var coroutine = new CoroutineNode(enumerator);
            AddCoroutine(coroutine);
            return coroutine.Enumerator;
        }

        public void StopCoroutine(IEnumerator handler)
        {
            var coroutine = GetCoroutineWithHandler(handler);
            if(coroutine != null)
            {
                RemoveCoroutine(coroutine);
            }
        }

        public void Update()
        {
            UpdateAllCoroutines();
        }

        void UpdateAllCoroutines()
        {
            LinkedListNode<CoroutineNode> coroutine = _listCoroutines.First;
            while(coroutine != null)
            {
                //Store listNext before coroutine finishes and is removed from the list
                LinkedListNode<CoroutineNode> listNext = coroutine.Next;

                if(coroutine.Value.WaitForCoroutine != null && coroutine.Value.WaitForCoroutine.Finished)
                {
                    coroutine.Value.WaitForCoroutine = null;
                    UpdateCoroutine(coroutine);
                }
                else if(coroutine.Value.WaitForCoroutine == null)
                {
                    UpdateCoroutine(coroutine);
                }
                coroutine = listNext;
            }
        }

        void UpdateCoroutine(LinkedListNode<CoroutineNode> coroutine)
        {
            IEnumerator enumerator = coroutine.Value.Enumerator;
            if(coroutine.Value.Enumerator.MoveNext())
            {
                var anotherEnumerator = enumerator.Current as IEnumerator;
                if(anotherEnumerator != null)
                {
                    var coroutineNode = new CoroutineNode(anotherEnumerator);
                    AddCoroutine(coroutineNode);
                    coroutine.Value.WaitForCoroutine = coroutineNode;
                }
                else if(enumerator.Current == null)
                {
                    //Do nothing. Continue in next update
                }
                else
                {
                    throw new InvalidYieldStatementException(string.Format("[UpdateCoroutineRunner] Cannot process this yield command: {0}", enumerator.Current.GetType().Name));
                }
            }
            else
            {
                //Coroutine finished
                coroutine.Value.Finished = true;
                RemoveCoroutine(coroutine);
            }
        }

        void AddCoroutine(CoroutineNode coroutine)
        {
            _listCoroutines.AddFirst(coroutine);
        }

        void RemoveCoroutine(LinkedListNode<CoroutineNode> coroutine)
        {
            _listCoroutines.Remove(coroutine);
        }

        LinkedListNode<CoroutineNode> GetCoroutineWithHandler(IEnumerator handler)
        {
            var node = _listCoroutines.First;
            while(node != null)
            {
                if(node.Value.Enumerator == handler)
                {
                    return node;
                }
                else
                {
                    node = node.Next;
                }
            }
            return null;
        }
    }
}
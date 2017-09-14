using System.Collections;

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

        class UpdateCoroutineEnumerator : IEnumerator
        {
            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {

            }

            public object Current
            {
                get
                {
                    return null;
                }
            }
        }

        class CoroutineNode
        {
            public CoroutineNode ListPrevious;
            public CoroutineNode ListNext;
            public bool Finished;
            public CoroutineNode WaitForCoroutine;

            public IEnumerator Enumerator{ get; private set; }

            public UpdateCoroutineEnumerator Handler{ get; private set; }

            public CoroutineNode(IEnumerator enumerator)
            {
                Handler = new UpdateCoroutineEnumerator();
                Enumerator = enumerator;
            }
        }

        CoroutineNode FirstCoroutine;

        public UpdateCoroutineRunner(IUpdateScheduler scheduler)
        {
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
            return coroutine.Handler;
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
            CoroutineNode coroutine = this.FirstCoroutine;
            while(coroutine != null)
            {
                //Store listNext before coroutine finishes and is removed from the list
                CoroutineNode listNext = coroutine.ListNext;

                if(coroutine.WaitForCoroutine != null && coroutine.WaitForCoroutine.Finished)
                {
                    coroutine.WaitForCoroutine = null;
                    UpdateCoroutine(coroutine);
                }
                else if(coroutine.WaitForCoroutine == null)
                {
                    UpdateCoroutine(coroutine);
                }
                coroutine = listNext;
            }
        }

        void UpdateCoroutine(CoroutineNode coroutine)
        {
            IEnumerator enumerator = coroutine.Enumerator;
            if(coroutine.Enumerator.MoveNext())
            {
                var anotherEnumerator = enumerator.Current as IEnumerator;
                if(anotherEnumerator != null)
                {
                    var coroutineNode = new CoroutineNode(anotherEnumerator);
                    AddCoroutine(coroutineNode);
                    coroutine.WaitForCoroutine = coroutineNode;
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
                coroutine.Finished = true;
                RemoveCoroutine(coroutine);
            }
        }

        void AddCoroutine(CoroutineNode coroutine)
        {
            if(FirstCoroutine != null)
            {
                coroutine.ListNext = FirstCoroutine;
                FirstCoroutine.ListPrevious = coroutine;
            }
            FirstCoroutine = coroutine;
        }

        void RemoveCoroutine(CoroutineNode coroutine)
        {
            if(FirstCoroutine == coroutine)
            {
                // remove first
                FirstCoroutine = coroutine.ListNext;
            }
            else
            {
                // not head of list
                if(coroutine.ListNext != null)
                {
                    // remove between
                    coroutine.ListPrevious.ListNext = coroutine.ListNext;
                    coroutine.ListNext.ListPrevious = coroutine.ListPrevious;
                }
                else if(coroutine.ListPrevious != null)
                {
                    // and listNext is null
                    coroutine.ListPrevious.ListNext = null;
                    // remove last
                }
            }
            coroutine.ListPrevious = null;
            coroutine.ListNext = null;
        }

        CoroutineNode GetCoroutineWithHandler(IEnumerator handler)
        {
            var node = FirstCoroutine;
            while(node != null)
            {
                if(node.Handler == handler)
                {
                    return node;
                }
                else
                {
                    node = node.ListNext;
                }
            }
            return null;
        }
    }
}
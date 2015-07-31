using System;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public class PetitionList
    {
        public List<Petition> Children { get; internal set; }
        
        public PetitionList()
        {
            Children = new List<Petition>();
        }

        public bool Cancel(bool val = true)
        {
            Children.RemoveAll(petition => {
                petition.Cancel();
                return !petition.Active;
            });

            return Children.Count == 0;
        }

        public Petition Add()
        {
            Children.Add(new Petition());
            return Children[(Children.Count - 1)];
        }

    }

    public class Petition : IDisposable
    {
        public enum State
        {
            Pending,
            Processing,
            Cancelled,
            Finished
        }
        
        private State state = State.Pending;
        private Error error = new Error();

        public Error Error
        {
            get
            {
                return error;
            }
            internal set
            {
                error = value;
                if(Parent != null)
                {
                    Parent.Error = error;
                }
            }
        }

        public Petition Parent { get; internal set; }

        public Petition Child { get; internal set; }

        public void Start()
        {
            state = State.Processing;
            if(Parent != null && Parent.state == State.Pending)
            {
                Parent.Start();
            }
        }

        public void Finish()
        {
            if(state == State.Finished)
            {
                return;
            }
            state = State.Finished;
            if(Child != null)
            {
                Child.Cancel();
            }

            if(Parent != null)
            {
                Parent.Finish();
            }
        }

        public void Restart()
        {
            if(state == State.Cancelled)
            {
                Finish();
            }
            else if(state == State.Processing)
            {
                state = State.Pending;
            }
        }

        public virtual bool Cancel()
        {
            if(state == State.Cancelled)
            {
                return false;
            }
            bool finishing = false;
            if(state == State.Pending)
            {
                finishing = true;
            }
            else if(state == State.Processing)
            {
                state = State.Cancelled;
            }
            if(Child != null)
            {
                Child.Cancel();
            }
            if(Child != null && Child.state == State.Finished)
            {
                finishing = true;
            }
            if(finishing)
            {
                Finish();
            }
            return finishing;
        }

        public bool HasError
        {
            get
            {
                if(Error != null)
                {
                    return Error.HasError;
                }
                else
                {
                    return false;
                }
            }
        }
       
        public bool Active
        {
            get
            {
                return state == State.Pending || state == State.Processing;
            }
        }

        public bool Processing
        {
            get
            {
                return state == State.Processing;
            }
        }

        public bool Pending
        {
            get
            {
                return state == State.Pending;
            }
        }

        public bool Cancelled
        {
            get
            {
                return state == State.Cancelled;
            }
        }

        public bool Finished
        {
            get
            {
                return state == State.Finished;
            }
        }

        public void Dispose()
        {
            Cancel();
        }

    }

}
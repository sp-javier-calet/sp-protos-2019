using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Class that processes TLEvents in every Update cycle.
    /// </summary>
    /// TLEventManager is responsible for registering and dispatching the events of a TLWindow in every Update cycle.
	public sealed class TLEventManager
	{
        private List<TLAbstractEvent> _events;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SocialPoint.Tool.Shared.TLGUI.TLEventManager"/>
        /// supress events.
        /// When events are supressed, they won't be sent to this manager.
        /// </summary>
        /// <value><c>true</c> if supress events; otherwise, <c>false</c>.</value>
        public bool SupressEvents { get; set; }

		public TLEventManager()
		{
            _events = new List<TLAbstractEvent>();
		}

        /// <summary>
        /// Register an event to be processed in the next Update cycle.
        /// </summary>
        public void AddEvent( TLAbstractEvent e )
		{
            if (!SupressEvents)
            {
    			if ( !_events.Contains( e ) ) {
    				_events.Add( e );
                }
            }
		}

        /// <summary>
        /// (Internal Use Only)Dispatch all the events registered for this Update cycle(the one this method was called from).
        /// </summary>
		public void ProcessEvents()
		{
			for ( int i = 0; i < _events.Count; i++ ) {
                ProcessEvent(_events[i]);
			}

			_events.Clear();
		}

        void ProcessEvent(TLAbstractEvent e)
        {
            e.doAction();

            for ( int i = 0; i < e.connectedEvents.Count; i++ ) {
                if (e.connectedEvents[i] != null) {
                    //Propagate its parameters if any
                    e.propagate(e.connectedEvents[i]);

                    ProcessEvent(e.connectedEvents[i]);
                }
            }
        }
	}
}

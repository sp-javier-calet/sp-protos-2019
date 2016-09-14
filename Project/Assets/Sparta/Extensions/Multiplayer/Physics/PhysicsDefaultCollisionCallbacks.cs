using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using System;

namespace SocialPoint.Multiplayer
{
    public class PhysicsDefaultCollisionCallbacks : ICollisionCallbackEventHandler
    {
        public class PersistentManifoldList
        {
            public List<PersistentManifold> Manifolds = new List<PersistentManifold>();
        }

        protected CollisionObject _collisionObject;

        Dictionary<CollisionObject, PersistentManifoldList> _otherObjs2ManifoldMap = new Dictionary<CollisionObject, PersistentManifoldList>();
        List<PersistentManifoldList> _newContacts = new List<PersistentManifoldList>();
        List<CollisionObject> _objectsToRemove = new List<CollisionObject>();

        public PhysicsDefaultCollisionCallbacks(CollisionObject collisionObject)
        {
            _collisionObject = collisionObject;
        }

        public void OnVisitPersistentManifold(PersistentManifold pm)
        {
            CollisionObject other;
            if(pm.Body0 == _collisionObject)
            {
                other = pm.Body1;
            }
            else
            {
                other = pm.Body0;
            }
            PersistentManifoldList pml;
            if(!_otherObjs2ManifoldMap.TryGetValue(other, out pml))
            {
                //todo get from object pool
                pml = new PersistentManifoldList();
                _newContacts.Add(pml);
            }
            pml.Manifolds.Add(pm);
        }

        public void OnFinishedVisitingManifolds()
        {
            _objectsToRemove.Clear();
            foreach(CollisionObject co in _otherObjs2ManifoldMap.Keys)
            {
                PersistentManifoldList pml = _otherObjs2ManifoldMap[co];
                if(pml.Manifolds.Count > 0)
                {
                    OnCollisionStay(co, pml);
                }
                else
                {
                    OnCollisionExit(co);
                    _objectsToRemove.Add(co);
                }
            }

            for(int i = 0; i < _objectsToRemove.Count; i++)
            {
                _otherObjs2ManifoldMap.Remove(_objectsToRemove[i]);
            }
            _objectsToRemove.Clear();


            for(int i = 0; i < _newContacts.Count; i++)
            {
                PersistentManifoldList pml = _newContacts[i];
                CollisionObject other;
                if(pml.Manifolds[0].Body0 == _collisionObject)
                {
                    other = pml.Manifolds[0].Body1;
                }
                else
                {
                    other = pml.Manifolds[0].Body0;
                }
                _otherObjs2ManifoldMap.Add(other, pml);
                OnCollisionEnter(other, pml);
            }
            _newContacts.Clear();

            foreach(CollisionObject co in _otherObjs2ManifoldMap.Keys)
            {
                PersistentManifoldList pml = _otherObjs2ManifoldMap[co];
                pml.Manifolds.Clear();
            }
        }

        /// <summary>
        ///Beware of creating, destroying, adding or removing bullet objects inside these functions. Doing so can alter the list of collisions and ContactManifolds 
        ///that are being iteratated over
        ///(comodification). This can result in infinite loops, null pointer exceptions, out of sequence Enter,Stay,Exit, etc... A good way to handle this sitution is 
        ///to collect the information in these callbacks then override "OnFinishedVisitingManifolds" like:
        ///
        /// public override void OnFinishedVisitingManifolds(){
        ///     base.OnFinishedVistingManifolds(); //don't omit this it does the callbacks
        ///     do my Instantiation and deletion here.
        /// }
        /// </summary>

        public virtual void OnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
        {
        }

        public virtual void OnCollisionStay(CollisionObject other, PersistentManifoldList manifoldList)
        {
        }

        public virtual void OnCollisionExit(CollisionObject other)
        {
        }
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using System;

namespace SocialPoint.Multiplayer
{
    public class BCollisionCallbacksDefault : PhysicsCollisionObject.BICollisionCallbackEventHandler
    {
        public class PersistentManifoldList
        {
            public List<PersistentManifold> manifolds = new List<PersistentManifold>();
        }

        public BCollisionCallbacksDefault(CollisionObject collisionObject)
        {
            myCollisionObject = collisionObject;
        }

        CollisionObject myCollisionObject;
        Dictionary<CollisionObject, PersistentManifoldList> otherObjs2ManifoldMap = new Dictionary<CollisionObject, PersistentManifoldList>();
        List<PersistentManifoldList> newContacts = new List<PersistentManifoldList>();
        List<CollisionObject> objectsToRemove = new List<CollisionObject>();

        public void OnVisitPersistentManifold(PersistentManifold pm)
        {
            CollisionObject other;
            if(pm.Body0 == myCollisionObject)
            {
                other = pm.Body1;
            }
            else
            {
                other = pm.Body0;
            }
            PersistentManifoldList pml;
            if(!otherObjs2ManifoldMap.TryGetValue(other, out pml))
            {
                //todo get from object pool
                pml = new PersistentManifoldList();
                newContacts.Add(pml);
            }
            pml.manifolds.Add(pm);
        }

        public void OnFinishedVisitingManifolds()
        {
            objectsToRemove.Clear();
            foreach(CollisionObject co in otherObjs2ManifoldMap.Keys)
            {
                PersistentManifoldList pml = otherObjs2ManifoldMap[co];
                if(pml.manifolds.Count > 0)
                {
                    BOnCollisionStay(co, pml);
                }
                else
                {
                    BOnCollisionExit(co);
                    objectsToRemove.Add(co);
                }
            }

            for(int i = 0; i < objectsToRemove.Count; i++)
            {
                otherObjs2ManifoldMap.Remove(objectsToRemove[i]);
            }
            objectsToRemove.Clear();


            for(int i = 0; i < newContacts.Count; i++)
            {
                PersistentManifoldList pml = newContacts[i];
                CollisionObject other;
                if(pml.manifolds[0].Body0 == myCollisionObject)
                {
                    other = pml.manifolds[0].Body1;
                }
                else
                {
                    other = pml.manifolds[0].Body0;
                }
                otherObjs2ManifoldMap.Add(other, pml);
                BOnCollisionEnter(other, pml);
            }
            newContacts.Clear();

            foreach(CollisionObject co in otherObjs2ManifoldMap.Keys)
            {
                PersistentManifoldList pml = otherObjs2ManifoldMap[co];
                pml.manifolds.Clear();
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

        public virtual void BOnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
        {
            UnityEngine.Debug.Log("*** TEST Collision Enter");
        }

        public virtual void BOnCollisionStay(CollisionObject other, PersistentManifoldList manifoldList)
        {
            UnityEngine.Debug.Log("*** TEST Collision Stay");
        }

        public virtual void BOnCollisionExit(CollisionObject other)
        {
            UnityEngine.Debug.Log("*** TEST Collision Exit");
        }
    }
}

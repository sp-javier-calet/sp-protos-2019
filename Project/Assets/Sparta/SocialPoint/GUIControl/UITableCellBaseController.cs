using UnityEngine;
using System.Collections;

namespace SocialPoint.GUIControl
{
    public abstract class UITableBaseCellController<T> : MonoBehaviour
    {
        /// <summary>
        /// TableView will cache unused cells and reuse them according to their
        /// reuse identifier. Override this to add custom cache grouping logic.
        /// </summary>
        public virtual string ReuseIdentifier
        { 
            get
            { 
                return this.GetType().Name; 
            } 
        }

        public abstract void UpdateData(T data);
    }
}

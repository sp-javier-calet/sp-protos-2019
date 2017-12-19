using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SocialPoint.Utils
{
    public struct QuadTreeSize
    {
        public QuadTreeSize(double width, double height)
        {
            if(width < 0 || height < 0)
                throw new ArgumentException("Width and Height must be non-negative.");

            this.width = width;
            this.height = height;
        }

        public bool IsEmpty
        {
            get
            {
                return (double.IsNegativeInfinity(width) &&
                double.IsNegativeInfinity(height));
            }
        }

        public double Height
        {
            get { return height; }
            set
            {
                if(IsEmpty)
                    throw new InvalidOperationException("Cannot modify this property on the Empty Size.");

                if(value < 0)
                    throw new ArgumentException("height must be non-negative.");

                height = value;
            }
        }

        public double Width
        {
            get { return width; }
            set
            {
                if(IsEmpty)
                    throw new InvalidOperationException("Cannot modify this property on the Empty Size.");

                if(value < 0)
                    throw new ArgumentException("width must be non-negative.");

                width = value;
            }
        }

        double width;
        double height;
    }

    public struct QuadTreeRect
    {
        public QuadTreeRect(double x, double y, double width, double height)
        {
            if(width < 0 || height < 0)
                throw new ArgumentException("width and height must be non-negative.");
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public QuadTreeRect(QuadTreePoint location, QuadTreeSize size)
        {
            x = location.X;
            y = location.Y;
            width = size.Width;
            height = size.Height;
        }

        public bool Contains(QuadTreeRect rect)
        {
            if(rect.Left < Left ||
               rect.Right > Right)
                return false;

            if(rect.Top < Top ||
               rect.Bottom > Bottom)
                return false;

            return true;
        }

        public bool Contains(double x, double y)
        {
            if(x < Left || x > Right)
                return false;
            if(y < Top || y > Bottom)
                return false;

            return true;
        }

        public bool IntersectsWith(QuadTreeRect rect)
        {
            return !((Left >= rect.Right) || (Right <= rect.Left) ||
            (Top >= rect.Bottom) || (Bottom <= rect.Top));
        }

        public bool Contains(QuadTreePoint point)
        {
            return Contains(point.X, point.Y);
        }


        public bool IsEmpty
        {
            get
            {
                return (double.IsPositiveInfinity(x) &&
                double.IsPositiveInfinity(y) &&
                double.IsNegativeInfinity(width) &&
                double.IsNegativeInfinity(height));
            }
        }

        public double X
        {
            get { return x; }
            set
            {
                if(IsEmpty)
                    throw new InvalidOperationException("Cannot modify this property on the Empty Rect.");

                x = value;
            }
        }

        public double Y
        {
            get { return y; }
            set
            {
                if(IsEmpty)
                    throw new InvalidOperationException("Cannot modify this property on the Empty Rect.");

                y = value;
            }
        }

        public double Width
        {
            get { return width; }
            set
            {
                if(IsEmpty)
                    throw new InvalidOperationException("Cannot modify this property on the Empty Rect.");

                if(value < 0)
                    throw new ArgumentException("width must be non-negative.");

                width = value;
            }
        }

        public double Height
        {
            get { return height; }
            set
            {
                if(IsEmpty)
                    throw new InvalidOperationException("Cannot modify this property on the Empty Rect.");

                if(value < 0)
                    throw new ArgumentException("height must be non-negative.");

                height = value;
            }
        }


        public double Left
        {
            get { return x; }
        }

        public double Top
        {
            get { return y; }
        }

        public double Right
        {
            get { return x + width; }
        }

        public double Bottom
        {
            get { return y + height; }
        }

        public QuadTreePoint TopLeft
        {
            get { return new QuadTreePoint(Left, Top); }
        }

        public QuadTreePoint TopRight
        {
            get { return new QuadTreePoint(Right, Top); }
        }

        public QuadTreePoint BottomLeft
        {
            get { return new QuadTreePoint(Left, Bottom); }
        }

        public QuadTreePoint BottomRight
        {
            get { return new QuadTreePoint(Right, Bottom); }
        }

        double x;
        double y;
        double width;
        double height;
    }

    public struct QuadTreePoint
    {
        public QuadTreePoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double X
        {
            get { return x; }
            set { x = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        double x;
        double y;
    }

    public sealed class QuadTree<T> where T : class, IQuadObject
    {
        readonly bool sort;
        readonly QuadTreeSize minLeafSize;
        readonly int maxObjectsPerLeaf;
        QuadTreeNode root;
        Dictionary<T, QuadTreeNode> objectToNodeLookup = new Dictionary<T, QuadTreeNode>();
        Dictionary<T, int> objectSortOrder = new Dictionary<T, int>();

        public QuadTreeNode Root { get { return root; } }

        object syncLock = new object();
        int objectSortId;

        public QuadTree(QuadTreeSize minLeafSize, int maxObjectsPerLeaf)
        {
            this.minLeafSize = minLeafSize;
            this.maxObjectsPerLeaf = maxObjectsPerLeaf;
        }

        public int GetSortOrder(T quadObject)
        {
            lock(objectSortOrder)
            {
                return !objectSortOrder.ContainsKey(quadObject) ? -1 : objectSortOrder[quadObject];
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="minLeafSize">The smallest size a leaf will split into</param>
        /// <param name="maxObjectsPerLeaf">Maximum number of objects per leaf before it forces a split into sub quadrants</param>
        /// <param name="sort">Whether or not queries will return objects in the order in which they were added</param>
        public QuadTree(QuadTreeSize minLeafSize, int maxObjectsPerLeaf, bool sort)
            : this(minLeafSize, maxObjectsPerLeaf)
        {
            this.sort = sort;
        }

        public void Insert(T quadObject)
        {
            lock(syncLock)
            {
                if(sort & !objectSortOrder.ContainsKey(quadObject))
                {
                    objectSortOrder.Add(quadObject, objectSortId++);
                }

                QuadTreeRect bounds = quadObject.Bounds;
                if(root == null)
                {
                    var rootSize = new QuadTreeSize(Math.Ceiling(bounds.Width / minLeafSize.Width),
                                       Math.Ceiling(bounds.Height / minLeafSize.Height));
                    double multiplier = Math.Max(rootSize.Width, rootSize.Height);
                    rootSize = new QuadTreeSize(minLeafSize.Width * multiplier, minLeafSize.Height * multiplier);

                    var center = new QuadTreePoint(bounds.X, bounds.Y);
                    var rootOrigin = new QuadTreePoint(center.X, center.Y);

                    root = new QuadTreeNode(new QuadTreeRect(rootOrigin, rootSize));
                }

                while(!root.Bounds.Contains(bounds))
                {
                    ExpandRoot(bounds);
                }

                InsertNodeObject(root, quadObject);
            }
        }

        public List<T> Query(QuadTreeRect bounds)
        {
            lock(syncLock)
            {
                var results = new List<T>();
                if(root != null)
                    Query(bounds, root, results);
                if(sort)
                    results.Sort((a, b) => objectSortOrder[a].CompareTo(objectSortOrder[b]));
                return results;
            }
        }

        void Query(QuadTreeRect bounds, QuadTreeNode node, List<T> results)
        {
            lock(syncLock)
            {
                if(node == null)
                    return;

                if(bounds.IntersectsWith(node.Bounds))
                {
                    for(int i = 0, nodeObjectsCount = node.Objects.Count; i < nodeObjectsCount; i++)
                    {
                        T quadObject = node.Objects[i];
                        if(bounds.IntersectsWith(quadObject.Bounds))
                            results.Add(quadObject);
                    }

                    for(int i = 0, nodeNodesCount = node.Nodes.Count; i < nodeNodesCount; i++)
                    {
                        QuadTreeNode childNode = node.Nodes[i];
                        Query(bounds, childNode, results);
                    }
                }
            }
        }

        void ExpandRoot(QuadTreeRect newChildBounds)
        {
            lock(syncLock)
            {
                bool isNorth = root.Bounds.Y < newChildBounds.Y;
                bool isWest = root.Bounds.X < newChildBounds.X;

                Direction rootDirection;
                if(isNorth)
                {
                    rootDirection = isWest ? Direction.NW : Direction.NE;
                }
                else
                {
                    rootDirection = isWest ? Direction.SW : Direction.SE;
                }

                double newX = (rootDirection == Direction.NW || rootDirection == Direction.SW)
                    ? root.Bounds.X
                    : root.Bounds.X - root.Bounds.Width;
                double newY = (rootDirection == Direction.NW || rootDirection == Direction.NE)
                    ? root.Bounds.Y
                    : root.Bounds.Y - root.Bounds.Height;
                var newRootBounds = new QuadTreeRect(newX, newY, root.Bounds.Width * 2, root.Bounds.Height * 2);
                var newRoot = new QuadTreeNode(newRootBounds);
                SetupChildNodes(newRoot);
                newRoot[rootDirection] = root;
                root = newRoot;
            }
        }

        void InsertNodeObject(QuadTreeNode node, T quadObject)
        {
            lock(syncLock)
            {
                if(!node.Bounds.Contains(quadObject.Bounds))
                    throw new Exception("This should not happen, child does not fit within node bounds");

                if(!node.HasChildNodes && node.Objects.Count + 1 > maxObjectsPerLeaf)
                {
                    SetupChildNodes(node);

                    var childObjects = new List<T>(node.Objects);
                    var childrenToRelocate = new List<T>();

                    for(int i = 0, childObjectsCount = childObjects.Count; i < childObjectsCount; i++)
                    {
                        T childObject = childObjects[i];
                        for(int j = 0, nodeNodesCount = node.Nodes.Count; j < nodeNodesCount; j++)
                        {
                            QuadTreeNode childNode = node.Nodes[j];
                            if(childNode == null)
                                continue;
                            if(childNode.Bounds.Contains(childObject.Bounds))
                            {
                                childrenToRelocate.Add(childObject);
                            }
                        }
                    }

                    for(int i = 0, childrenToRelocateCount = childrenToRelocate.Count; i < childrenToRelocateCount; i++)
                    {
                        T childObject = childrenToRelocate[i];
                        RemoveQuadObjectFromNode(childObject);
                        InsertNodeObject(node, childObject);
                    }
                }

                for(int i = 0, nodeNodesCount = node.Nodes.Count; i < nodeNodesCount; i++)
                {
                    QuadTreeNode childNode = node.Nodes[i];
                    if(childNode != null)
                    {
                        if(childNode.Bounds.Contains(quadObject.Bounds))
                        {
                            InsertNodeObject(childNode, quadObject);
                            return;
                        }
                    }
                }

                AddQuadObjectToNode(node, quadObject);
            }
        }

        void ClearQuadObjectsFromNode(QuadTreeNode node)
        {
            lock(syncLock)
            {
                var quadObjects = new List<T>(node.Objects);
                for(int i = 0, quadObjectsCount = quadObjects.Count; i < quadObjectsCount; i++)
                {
                    T quadObject = quadObjects[i];
                    RemoveQuadObjectFromNode(quadObject);
                }
            }
        }

        void RemoveQuadObjectFromNode(T quadObject)
        {
            lock(syncLock)
            {
                QuadTreeNode node = objectToNodeLookup[quadObject];
                node.quadObjects.Remove(quadObject);
                objectToNodeLookup.Remove(quadObject);
                quadObject.BoundsChanged -= OnObjectBoundsChanged;
            }
        }

        void AddQuadObjectToNode(QuadTreeNode node, T quadObject)
        {
            lock(syncLock)
            {
                node.quadObjects.Add(quadObject);
                objectToNodeLookup.Add(quadObject, node);
                quadObject.BoundsChanged += OnObjectBoundsChanged;
            }
        }

        void OnObjectBoundsChanged(object sender, EventArgs e)
        {
            lock(syncLock)
            {
                var quadObject = sender as T;
                if(quadObject != null)
                {
                    QuadTreeNode node = objectToNodeLookup[quadObject];
                    if(!node.Bounds.Contains(quadObject.Bounds) || node.HasChildNodes)
                    {
                        RemoveQuadObjectFromNode(quadObject);
                        Insert(quadObject);
                        if(node.Parent != null)
                        {
                            CheckChildNodes(node.Parent);
                        }
                    }
                }
            }
        }

        void SetupChildNodes(QuadTreeNode node)
        {
            lock(syncLock)
            {
                if(minLeafSize.Width <= node.Bounds.Width / 2 && minLeafSize.Height <= node.Bounds.Height / 2)
                {
                    node[Direction.NW] = new QuadTreeNode(node.Bounds.X, node.Bounds.Y, node.Bounds.Width / 2,
                        node.Bounds.Height / 2);
                    node[Direction.NE] = new QuadTreeNode(node.Bounds.X + node.Bounds.Width / 2, node.Bounds.Y,
                        node.Bounds.Width / 2,
                        node.Bounds.Height / 2);
                    node[Direction.SW] = new QuadTreeNode(node.Bounds.X, node.Bounds.Y + node.Bounds.Height / 2,
                        node.Bounds.Width / 2,
                        node.Bounds.Height / 2);
                    node[Direction.SE] = new QuadTreeNode(node.Bounds.X + node.Bounds.Width / 2,
                        node.Bounds.Y + node.Bounds.Height / 2,
                        node.Bounds.Width / 2, node.Bounds.Height / 2);

                }
            }
        }

        public void Remove(T quadObject)
        {
            lock(syncLock)
            {
                if(sort && objectSortOrder.ContainsKey(quadObject))
                {
                    objectSortOrder.Remove(quadObject);
                }

                if(!objectToNodeLookup.ContainsKey(quadObject))
                    throw new KeyNotFoundException("QuadObject not found in dictionary for removal");

                QuadTreeNode containingNode = objectToNodeLookup[quadObject];
                RemoveQuadObjectFromNode(quadObject);

                if(containingNode.Parent != null)
                    CheckChildNodes(containingNode.Parent);
            }
        }



        void CheckChildNodes(QuadTreeNode node)
        {
            lock(syncLock)
            {
                if(GetObjectCount(node) <= maxObjectsPerLeaf)
                {
                    // Move child objects into this node, and delete sub nodes
                    List<T> subChildObjects = GetChildObjects(node);
                    for(int i = 0, subChildObjectsCount = subChildObjects.Count; i < subChildObjectsCount; i++)
                    {
                        T childObject = subChildObjects[i];
                        if(!node.Objects.Contains(childObject))
                        {
                            RemoveQuadObjectFromNode(childObject);
                            AddQuadObjectToNode(node, childObject);
                        }
                    }
                    if(node[Direction.NW] != null)
                    {
                        node[Direction.NW].Parent = null;
                        node[Direction.NW] = null;
                    }
                    if(node[Direction.NE] != null)
                    {
                        node[Direction.NE].Parent = null;
                        node[Direction.NE] = null;
                    }
                    if(node[Direction.SW] != null)
                    {
                        node[Direction.SW].Parent = null;
                        node[Direction.SW] = null;
                    }
                    if(node[Direction.SE] != null)
                    {
                        node[Direction.SE].Parent = null;
                        node[Direction.SE] = null;
                    }

                    if(node.Parent != null)
                        CheckChildNodes(node.Parent);
                    else
                    {
                        // Its the root node, see if we're down to one quadrant, with none in local storage - if so, ditch the other three
                        int numQuadrantsWithObjects = 0;
                        QuadTreeNode nodeWithObjects = null;
                        for(int i = 0, nodeNodesCount = node.Nodes.Count; i < nodeNodesCount; i++)
                        {
                            QuadTreeNode childNode = node.Nodes[i];
                            if(childNode != null && GetObjectCount(childNode) > 0)
                            {
                                numQuadrantsWithObjects++;
                                nodeWithObjects = childNode;
                                if(numQuadrantsWithObjects > 1)
                                    break;
                            }
                        }
                        if(numQuadrantsWithObjects == 1)
                        {
                            for(int i = 0, nodeNodesCount = node.Nodes.Count; i < nodeNodesCount; i++)
                            {
                                QuadTreeNode childNode = node.Nodes[i];
                                if(childNode != nodeWithObjects)
                                    childNode.Parent = null;
                            }
                            root = nodeWithObjects;
                        }
                    }
                }
            }
        }


        List<T> GetChildObjects(QuadTreeNode node)
        {
            lock(syncLock)
            {
                var results = new List<T>();
                results.AddRange(node.quadObjects);
                for(int i = 0, nodeNodesCount = node.Nodes.Count; i < nodeNodesCount; i++)
                {
                    QuadTreeNode childNode = node.Nodes[i];
                    if(childNode != null)
                        results.AddRange(GetChildObjects(childNode));
                }
                return results;
            }
        }

        public int GetObjectCount()
        {
            lock(syncLock)
            {
                if(root == null)
                    return 0;
                int count = GetObjectCount(root);
                return count;
            }
        }

        int GetObjectCount(QuadTreeNode node)
        {
            lock(syncLock)
            {
                int count = node.Objects.Count;
                for(int i = 0, nodeNodesCount = node.Nodes.Count; i < nodeNodesCount; i++)
                {
                    QuadTreeNode childNode = node.Nodes[i];
                    if(childNode != null)
                    {
                        count += GetObjectCount(childNode);
                    }
                }
                return count;
            }
        }

        public int GetNodeCount()
        {
            lock(syncLock)
            {
                if(root == null)
                    return 0;
                int count = GetNodeCount(root, 1);
                return count;
            }
        }

        int GetNodeCount(QuadTreeNode node, int count)
        {
            lock(syncLock)
            {
                if(node == null)
                    return count;

                for(int i = 0, nodeNodesCount = node.Nodes.Count; i < nodeNodesCount; i++)
                {
                    QuadTreeNode childNode = node.Nodes[i];
                    if(childNode != null)
                        count++;
                }
                return count;
            }
        }

        public List<QuadTreeNode> GetAllNodes()
        {
            lock(syncLock)
            {
                var results = new List<QuadTreeNode>();
                if(root != null)
                {
                    results.Add(root);
                    GetChildNodes(root, results);
                }
                return results;
            }
        }

        void GetChildNodes(QuadTreeNode node, ICollection<QuadTreeNode> results)
        {
            lock(syncLock)
            {
                for(int i = 0, nodeNodesCount = node.Nodes.Count; i < nodeNodesCount; i++)
                {
                    QuadTreeNode childNode = node.Nodes[i];
                    if(childNode != null)
                    {
                        results.Add(childNode);
                        GetChildNodes(childNode, results);
                    }
                }
            }
        }

        public sealed class QuadTreeNode
        {
            static int _id;
            public readonly int ID = _id++;

            public QuadTreeNode Parent { get; internal set; }

            readonly QuadTreeNode[] _nodes = new QuadTreeNode[4];

            public QuadTreeNode this[Direction direction]
            {
                get
                {
                    switch(direction)
                    {
                    case Direction.NW:
                        return _nodes[0];
                    case Direction.NE:
                        return _nodes[1];
                    case Direction.SW:
                        return _nodes[2];
                    case Direction.SE:
                        return _nodes[3];
                    default:
                        return null;
                    }
                }
                set
                {
                    switch(direction)
                    {
                    case Direction.NW:
                        _nodes[0] = value;
                        break;
                    case Direction.NE:
                        _nodes[1] = value;
                        break;
                    case Direction.SW:
                        _nodes[2] = value;
                        break;
                    case Direction.SE:
                        _nodes[3] = value;
                        break;
                    }
                    if(value != null)
                        value.Parent = this;
                }
            }

            public ReadOnlyCollection<QuadTreeNode> Nodes;

            internal List<T> quadObjects = new List<T>();
            public ReadOnlyCollection<T> Objects;

            public QuadTreeRect Bounds { get; internal set; }

            public bool HasChildNodes
            {
                get
                {
                    return _nodes[0] != null;
                }
            }

            public QuadTreeNode(QuadTreeRect bounds)
            {
                Bounds = bounds;
                Nodes = new ReadOnlyCollection<QuadTreeNode>(_nodes);
                Objects = new ReadOnlyCollection<T>(quadObjects);
            }

            public QuadTreeNode(double x, double y, double width, double height)
                : this(new QuadTreeRect(x, y, width, height))
            {

            }
        }

        public void Draw()
        {
#if UNITY_5_3_OR_NEWER
            for(int i = 0, maxCount = GetAllNodes().Count; i < maxCount; i++)
            {
                var node = GetAllNodes()[i];
                var botomLeft = new UnityEngine.Vector3((float)node.Bounds.X, 0, (float)node.Bounds.Y);
                var botomRight = new UnityEngine.Vector3((float)(node.Bounds.X + node.Bounds.Width), 0, (float)node.Bounds.Y);
                var topLeft = new UnityEngine.Vector3((float)(node.Bounds.X), 0, (float)(node.Bounds.Y + node.Bounds.Height));
                var topRight = new UnityEngine.Vector3((float)(node.Bounds.X + node.Bounds.Width), 0, (float)(node.Bounds.Y + node.Bounds.Height));
                UnityEngine.Debug.DrawLine(botomLeft, botomRight, UnityEngine.Color.red);
                UnityEngine.Debug.DrawLine(botomLeft, topLeft, UnityEngine.Color.red);
                UnityEngine.Debug.DrawLine(topLeft, topRight, UnityEngine.Color.red);
                UnityEngine.Debug.DrawLine(topRight, botomRight, UnityEngine.Color.red);
            }
#endif
        }

    }

    public enum Direction
    {
        NW = 0,
        NE = 1,
        SW = 2,
        SE = 3
    }

}

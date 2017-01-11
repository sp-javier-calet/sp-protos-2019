namespace SocialPoint.Geometry
{
    public partial struct Vector
    {
        #region Static readonly variables

        /// <summary>
        /// A vector with components (0,0,0);
        /// </summary>
        public static readonly Vector Zero;

        /// <summary>
        /// A vector with components (1,0,0);
        /// </summary>
        public static readonly Vector Left;

        /// <summary>
        /// A vector with components (-1,0,0);
        /// </summary>
        public static readonly Vector Right;

        /// <summary>
        /// A vector with components (0,1,0);
        /// </summary>
        public static readonly Vector Up;

        /// <summary>
        /// A vector with components (0,-1,0);
        /// </summary>
        public static readonly Vector Down;

        /// <summary>
        /// A vector with components (0,0,1);
        /// </summary>
        public static readonly Vector Backward;

        /// <summary>
        /// A vector with components (0,0,-1);
        /// </summary>
        public static readonly Vector Forward;

        /// <summary>
        /// A vector with components (1,1,1);
        /// </summary>
        public static readonly Vector One;

        static Vector()
        {
            One = new Vector(1, 1, 1);
            Zero = new Vector(0, 0, 0);
            Left = new Vector(1, 0, 0);
            Right = new Vector(-1, 0, 0);
            Up = new Vector(0, 1, 0);
            Down = new Vector(0, -1, 0);
            Backward = new Vector(0, 0, 1);
            Forward = new Vector(0, 0, -1);
        }

        #endregion

        public float X;
        public float Y;
        public float Z;

        public Vector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector)) return false;
            Vector other = (Vector)obj;

            return (((X == other.X) && (Y == other.Y)) && (Z == other.Z));
        }

        public static bool operator ==(Vector value1, Vector value2)
        {
            return (((value1.X == value2.X) && (value1.Y == value2.Y)) && (value1.Z == value2.Z));
        }

        public static bool operator !=(Vector value1, Vector value2)
        {
            if ((value1.X == value2.X) && (value1.Y == value2.Y))
            {
                return (value1.Z != value2.Z);
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1},{2}]", X, Y, Z);
        }
    }

    // Unity Vector3 adapter
    public partial struct Vector
    {
        public static Vector Convert(UnityEngine.Vector3 v)
        {
            return new Vector(v.x, v.y, v.z);
        }

        public static implicit operator Vector(UnityEngine.Vector3 v)
        {
            return new Vector(v.x, v.y, v.z);
        }

        public static implicit operator UnityEngine.Vector3(Vector v)
        {
            return new UnityEngine.Vector3(v.X, v.Y, v.Z);
        }
    }

    // Unity Vector2 adapter
    public partial struct Vector
    {
        public static Vector Convert(UnityEngine.Vector2 v)
        {
            return new Vector(v.x, v.y, 0);
        }

        public static implicit operator Vector(UnityEngine.Vector2 v)
        {
            return new Vector(v.x, v.y, 0);
        }

        public static implicit operator UnityEngine.Vector2(Vector v)
        {
            return new UnityEngine.Vector2(v.X, v.Y);
        }
    }
}

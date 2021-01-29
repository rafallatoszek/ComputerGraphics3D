using System;

namespace gk
{
	public class Vector3
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public float Lenght { get => (float)Math.Sqrt(Dot(this)); }

		public Vector3(float x, float y, float z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		public static Vector3 operator +(Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
		}

		public static Vector3 operator -(Vector3 v)
		{
			return new Vector3(-v.X, -v.Y, -v.Z);
		}

		public static Vector3 operator -(Vector3 v1, Vector3 v2)
		{
			return v1 + (-v2);
		}

		public static Vector3 operator /(Vector3 v1, float s2)
		{
			return new Vector3(v1.X / s2, v1.Y / s2, v1.Z / s2);
		}

		public static float Distance(Vector3 v1, Vector3 v2)
		{
			return (float)Math.Sqrt(
				   (v1.X - v2.X) * (v1.X - v2.X) +
				   (v1.Y - v2.Y) * (v1.Y - v2.Y) +
				   (v1.Z - v2.Z) * (v1.Z - v2.Z)
			   );
		}

		public float Distance(Vector3 other)
		{
			return Distance(this, other);
		}

		public float Dot(Vector3 v)
		{
			return X * v.X + Y * v.Y + Z * v.Z;
		}

		public Vector3 Cross(Vector3 v1)
		{
			float x = Y * v1.Z - Z * v1.Y;
			float y = Z * v1.X - X * v1.Z;
			float z = X * v1.Y - Y * v1.X;
			return new Vector3(x, y, z);
		}

		public Vector3 Normal()
		{
			return this / Lenght;
		}

	}
}

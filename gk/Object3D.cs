using SFML.Graphics;
using System.Collections.Generic;

namespace gk
{
	class Object3D
	{

		public List<Vector3> Vertices = new List<Vector3>();

		public List<Plane> Planes = new List<Plane>();

		public Color Color { get; set; }

        public Object3D(List<Vector3> vertices, List<Plane> planes, Color color)
        {
			Vertices = vertices;
			Planes = planes;
			Color = color;
        }

		public void AddPlanes(List<int[]> ids)
		{
			for (int i = 0; i < ids.Count; i++) {
				Planes.Add(new Plane(ids[i]));
			}
		}

		public Vector3 getCenterVector()
        {
			Vector3 center = new Vector3(0, 0, 0);
            for (int i = 0; i < Vertices.Count; i++)
            {
				center += Vertices[i];
            }
			center /= Vertices.Count;
			return center;
        }
	}

	class Plane
	{
		public int[] FieldVertices { get; private set; }

		public Plane(int[] ids)
		{
			FieldVertices = ids;
		}
	}
}

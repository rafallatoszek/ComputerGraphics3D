using System;
using SFML.Window;
using SFML.Graphics;
using MathNet.Numerics.LinearAlgebra;
using SFML.System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace gk
{
	class Program
	{
		static InputType inputType;
		static readonly Vector3 lightPos = new Vector3(-10, -10, -10).Normal();

		static void Main(string[] args)
		{
			Vector2f windowSize = new Vector2f(1080, 800);
			RenderWindow renderWindow = new RenderWindow(new VideoMode((uint)Math.Round(windowSize.X), (uint)Math.Round(windowSize.Y)), "Projekt Grafika");
			float speed = 50f;

			Object3D cube = CreateObject3D("objects/cube1.obj", 10, 0, 0, 40, Color.Blue);
			Object3D cube2 = CreateObject3D("objects/cube2.obj", 10, 40, 0, 30, Color.Red);
			Object3D sphere = CreateObject3D("objects/sphere.obj", 8, 20, -30, 40, Color.Green);
			List<Object3D> objects3D = new List<Object3D>() { cube, cube2, sphere };

			Clock clock = new Clock();
			renderWindow.SetFramerateLimit(60);
			renderWindow.KeyPressed += OnKeyPressed;

			while (renderWindow.IsOpen)
			{
				renderWindow.DispatchEvents();
				Time time = clock.ElapsedTime;
				renderWindow.Clear();

				List<ConvexShape> convexShapes = new List<ConvexShape>();

				foreach (Object3D object3D in objects3D)
				{
					for (int i = 0; i < object3D.Vertices.Count; i++)
					{
						if (inputType == InputType.Forward)
						{
							object3D.Vertices[i] = Move(object3D.Vertices[i], 0, 0, -speed * time.AsSeconds());
						}
						if (inputType == InputType.Backward)
						{
							object3D.Vertices[i] = Move(object3D.Vertices[i], 0, 0, speed * time.AsSeconds());
						}
						if (inputType == InputType.Left)
						{
							object3D.Vertices[i] = Move(object3D.Vertices[i], speed * time.AsSeconds(), 0, 0);
						}
						if (inputType == InputType.Right)
						{
							object3D.Vertices[i] = Move(object3D.Vertices[i], -speed * time.AsSeconds(), 0, 0);
						}
						if (inputType == InputType.Up)
						{
							object3D.Vertices[i] = Move(object3D.Vertices[i], 0, speed * time.AsSeconds(), 0);
						}
						if (inputType == InputType.Down)
						{
							object3D.Vertices[i] = Move(object3D.Vertices[i], 0, -speed * time.AsSeconds(), 0);
						}
						if (inputType == InputType.RotateLeft)
						{
							object3D.Vertices[i] = RotateInY(object3D.Vertices[i], -speed * time.AsSeconds());
						}
						if (inputType == InputType.RotateRight)
						{
							object3D.Vertices[i] = RotateInY(object3D.Vertices[i], speed * time.AsSeconds());
						}
						if (inputType == InputType.RotateUp)
						{
							object3D.Vertices[i] = RotateInX(object3D.Vertices[i], speed * time.AsSeconds());
						}
						if (inputType == InputType.RotateDown)
						{
							object3D.Vertices[i] = RotateInX(object3D.Vertices[i], -speed * time.AsSeconds());
						}
						if (inputType == InputType.RotateFallLeft)
						{
							object3D.Vertices[i] = RotateInZ(object3D.Vertices[i], -speed * time.AsSeconds());
						}
						if (inputType == InputType.RotateFallRight)
						{
							object3D.Vertices[i] = RotateInZ(object3D.Vertices[i], speed * time.AsSeconds());
						}
					}
				}
				convexShapes = CreateConvexShapes(windowSize, objects3D);

				if (time.AsSeconds() > (1 / 60f))
				{
					clock.Restart();
				}

				for (int i = convexShapes.Count - 1; i >= 0; i--)
				{
					renderWindow.Draw(convexShapes[i]);
				}
				inputType = InputType.None;
				renderWindow.Display();
			}
		}

		static Object3D CreateObject3D(string path, int size, int x, int y, int z, Color color)
        {
			StreamReader sr = new StreamReader(path);
			List<Vector3> vertexes = new List<Vector3>();
			List<Plane> planes = new List<Plane>();
			string line;
			while ((line = sr.ReadLine()) != null)
			{
				HandleLine(line, size, x, y, z, vertexes, planes);
			}
			Object3D object3D = new Object3D(vertexes, planes, color);
			return object3D;
		}

		static void HandleLine(string line, int size, int x, int y, int z, List<Vector3> vertexes, List<Plane> planes)
        {
			string[] columns = line.Split(" ");
			if (line[0] == 'v') {
				vertexes.Add(new Vector3(size * Converter.StringToFloat(columns[1]) + x,
					size * Converter.StringToFloat(columns[2]) + y,
					size * Converter.StringToFloat(columns[3]) + z));
			} else if (line[0] == 'f') {
				AddPlane(planes, columns);
			}
		}

		static void AddPlane(List<Plane> planes, string[] columns)
        {
			if (columns.Length == 4) {
				int[] ids = new int[3] { int.Parse(columns[1]) - 1, int.Parse(columns[2]) - 1, int.Parse(columns[3]) - 1 };
				planes.Add(new Plane(ids));
			} else {
				int[] ids = new int[4] { int.Parse(columns[1]) - 1, int.Parse(columns[2]) - 1, int.Parse(columns[3]) - 1, int.Parse(columns[4]) - 1 };
				planes.Add(new Plane(ids));
			}
		}

		static List<ConvexShape> CreateConvexShapes(Vector2f windowSize, List<Object3D> objects3D)
		{
			List<ConvexShape> convexShapes = new List<ConvexShape>();
			List<float> planeDistances = new List<float>();

			for (int k = 0; k < objects3D.Count; k++)
			{
				List<Vector3> object3DPoints = objects3D[k].Vertices;
				Vector2f[] processedObject3DPoints = new Vector2f[objects3D[k].Vertices.Count];

				for (int i = 0; i < object3DPoints.Count; i++)
				{
					(float tempX, float tempY) = PerspectiveProjection(object3DPoints[i], 600);
					processedObject3DPoints[i] = new Vector2f(tempX + windowSize.X / 2, tempY + windowSize.Y / 2);
				}

				for (int i = 0; i < objects3D[k].Planes.Count; i++)
				{
					ConvexShape convex = new ConvexShape();
					Vector3[] plane = new Vector3[objects3D[k].Planes[i].FieldVertices.Length];
					convex.SetPointCount((uint)objects3D[k].Planes[i].FieldVertices.Length);

					for (uint j = 0; j < objects3D[k].Planes[i].FieldVertices.Length; j++)
					{
						convex.SetPoint(j, processedObject3DPoints[objects3D[k].Planes[i].FieldVertices[j]]);
						plane[j] = objects3D[k].Vertices[objects3D[k].Planes[i].FieldVertices[j]];
					}
					planeDistances.Add(GetDistanceToPlane(plane));

					Vector3 center = objects3D[k].getCenterVector();
					convex.FillColor = FlatShading( objects3D[k].Vertices[objects3D[k].Planes[i].FieldVertices[0]],
													objects3D[k].Vertices[objects3D[k].Planes[i].FieldVertices[1]],
													objects3D[k].Vertices[objects3D[k].Planes[i].FieldVertices[2]],
													center, objects3D[k].Color);
					convexShapes.Add(convex);
				}
			}
			return PainterAlgorithm(convexShapes, planeDistances);
		}

		static List<ConvexShape> PainterAlgorithm(List<ConvexShape> convexShapes, List<float> planeDistances)
        {
			var combined = convexShapes.Zip(planeDistances, (convex, distance) => new { convex, distance });
			var orderedConvexDistance = combined.OrderBy(x => x.distance);
			var orderedConvexDistanceWithoutConvexBehind = orderedConvexDistance.Where(x => x.distance > 10);
			List<ConvexShape> cs = orderedConvexDistanceWithoutConvexBehind.Select(x => x.convex).ToList();
			return cs;
		}

		static Color FlatShading(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 center, Color color)
		{
			Vector3 normal = (v3 - v1).Cross(v2 - v1).Normal();
			float x = normal.X * center.X + normal.Y * center.Y + normal.Z * center.Z;
			if (x > 0) {
				normal = -normal;
			}
			float dp = normal.X * lightPos.X + normal.Y * lightPos.Y + normal.Z * lightPos.Z;
			dp = Math.Max(25, dp * 255);

			return new Color((byte)Math.Min(color.R, Convert.ToInt32(dp)),
							 (byte)Math.Min(color.G, Convert.ToInt32(dp)),
							 (byte)Math.Min(color.B, Convert.ToInt32(dp)));
		}

		static float GetDistanceToPlane(Vector3[] plane)
        {
			Vector3 myPosition = new Vector3(0, 0, 0);
            Vector3 mean = new Vector3(0, 0 ,0);
            for (int i = 0; i < plane.Length; i++) {
				mean += plane[i];
			}
			mean /= plane.Length;
			return myPosition.Distance(mean);
		}

		static Vector3 Move(Vector3 vec3, float deltaX, float deltaY, float deltaZ)
		{
			Matrix<float> m = Matrix<float>.Build.Dense(4, 4);

			m[0, 0] = 1;		//	a = [x(i) y(i) z(i) 1] *
			m[1, 1] = 1;		//	[	1	0	0	X
			m[2, 2] = 1;        //		0	1	0	Y
			m[0, 3] = deltaX;   //      0	0	1	Z
			m[1, 3] = deltaY;   //      0	0	0	1]
			m[2, 3] = deltaZ;
			m[3, 3] = 1;

			float[,] x = { { vec3.X }, { vec3.Y }, { vec3.Z }, { 1 } };
			Matrix<float> m2 = Matrix<float>.Build.DenseOfArray(x);

			var a = m.Multiply(m2); //a = M * [x(i) y(i) z(i) 1]';

			return new Vector3(a[0, 0], a[1, 0], a[2, 0]);
		}

		static Vector3 RotateInY(Vector3 vec3, float angle)
		{
			angle = (float)(angle * Math.PI / 180f);
			Matrix<float> m = Matrix<float>.Build.Dense(4, 4);

			m[0, 0] = (float)Math.Cos(angle);   //	a = [x(i) y(i) z(i) 1] *
			m[0, 2] = (float)-Math.Sin(angle);  //	[	cos(a)	0	-sin(a)	0
			m[1, 1] = 1;                        //		0		1	0		0
			m[2, 0] = (float)Math.Sin(angle);   //      sin(a)	0	cos(a)	0
			m[2, 2] = (float)Math.Cos(angle);   //      0		0	0		1]
			m[3, 3] = 1;

			float[,] x = { { vec3.X }, { vec3.Y } , { vec3.Z } , { 1 }};
			Matrix<float> m2 = Matrix<float>.Build.DenseOfArray(x);

			var a = m.Multiply(m2); //a = M * [x(i) y(i) z(i) 1]';

			return new Vector3(a[0, 0], a[1, 0], a[2, 0]);
		}

		static Vector3 RotateInX(Vector3 vec3, float angle)
		{
			angle = (float)(angle * Math.PI / 180f);
			Matrix<float> m = Matrix<float>.Build.Dense(4, 4);

			m[0, 0] = 1;						//	a = [x(i) y(i) z(i) 1] *
			m[1, 1] = (float)Math.Cos(angle);   //	[	1	0		0		0         
			m[1, 2] = (float)Math.Sin(angle);   //		0   cos(a)	sin(a)	0
			m[2, 1] = (float)-Math.Sin(angle);  //      0	-sin(a)	cos(a)	0
			m[2, 2] = (float)Math.Cos(angle);	//      0	0		0		1]
			m[3, 3] = 1;

			float[,] x = { { vec3.X }, { vec3.Y }, { vec3.Z }, { 1 } };
			Matrix<float> m2 = Matrix<float>.Build.DenseOfArray(x);

			var a = m.Multiply(m2); //a = M * [x(i) y(i) z(i) 1]';

			return new Vector3(a[0, 0], a[1, 0], a[2, 0]);
		}

		static Vector3 RotateInZ(Vector3 vec3, float angle)
		{
			angle = (float)(angle * Math.PI / 180f);
			Matrix<float> m = Matrix<float>.Build.Dense(4, 4);

			m[0, 0] = (float)Math.Cos(angle);   //	a = [x(i) y(i) z(i) 1] *
			m[0, 1] = (float)Math.Sin(angle);	//	[	cos(a)	sin(a)	0	0
			m[1, 0] = (float)-Math.Sin(angle);  //		-sin(a)	cos(a)	0	0
			m[1, 1] = (float)Math.Cos(angle);   //      0		0		1	0
			m[2, 2] = 1;						//      0		0		0	1]
			m[3, 3] = 1;

			float[,] x = { { vec3.X }, { vec3.Y }, { vec3.Z }, { 1 } };
			Matrix<float> m2 = Matrix<float>.Build.DenseOfArray(x);

			var a = m.Multiply(m2); //a = M * [x(i) y(i) z(i) 1]';

			return new Vector3(a[0, 0], a[1, 0], a[2, 0]);
		}

		static void OnKeyPressed(object sender, KeyEventArgs e)
		{
            switch (e.Code)
            {
                case Keyboard.Key.A:
					inputType = InputType.Left;
					break;
                case Keyboard.Key.D:
					inputType = InputType.Right;
					break;
                case Keyboard.Key.E:
					inputType = InputType.Down;
					break;
                case Keyboard.Key.I:
					inputType = InputType.RotateUp;
					break;
                case Keyboard.Key.J:
					inputType = InputType.RotateLeft;
					break;
                case Keyboard.Key.K:
					inputType = InputType.RotateDown;
					break;
                case Keyboard.Key.L:
					inputType = InputType.RotateRight;
					break;
                case Keyboard.Key.O:
					inputType = InputType.RotateFallRight;
					break;
                case Keyboard.Key.Q:
					inputType = InputType.Up;
					break;
                case Keyboard.Key.S:
					inputType = InputType.Backward;
					break;
                case Keyboard.Key.U:
					inputType = InputType.RotateFallLeft;
					break;
                case Keyboard.Key.W:
					inputType = InputType.Forward;
					break;
                default:
                    break;
            }
		}

		static (float, float) PerspectiveProjection(Vector3 vec3, float d)
		{
			Matrix<float> m = Matrix<float>.Build.Dense(4, 4);

			m[0, 0] = 1;		//M = [	1	0	0	0 ;
			m[1, 1] = 1;		//		0	1	0	0 ;
			m[3, 2] = 1 / d;	//		0	0	0	0 ;
			m[2, 2] = 1;		//		0	0	1/d 1 ]

			float[,] x = { { vec3.X }, { vec3.Y }, { vec3.Z }, { 1 } };
			Matrix<float> m2 = Matrix<float>.Build.DenseOfArray(x);
			
			var a = m.Multiply(m2); //a = M * [x(i) y(i) z(i) 1]';

			var resX = a[0, 0] * d / vec3.Z; //normalizacja
			var resY = a[1, 0] * d / vec3.Z;
			return (resX, resY);
		}
	}
}

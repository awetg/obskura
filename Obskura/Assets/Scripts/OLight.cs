//OLight.cs
/* Cast a 2D light mesh.
 * Written by Manuel Furia, 
 * expanding on the public domain example https://github.com/unitycoder/2DShadow
 * 
 * NOTE: I refactored several core parts of the original algorithm using LINQ,
 * 		 and corrected some conceptual error.
 * 		 The code has been extended to handle different light cone angles and colors.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OLight : MonoBehaviour {



	List<Segment2D> segments = new List<Segment2D>(); //Segments of the walls or light blocking objects
	List<Vector3> vertices = new List<Vector3>(); //Vertices of the walls or light blocking objects
	List<Intersection> intersects = new List<Intersection>(); //Interceptions between rays and segments
	List<float> vertAngles = new List<float>(); //Angles of the vertices

	//Mesh that represents the area affected by the light
	//NOTE: present as a global variable for caching purposes
	Mesh lightMesh;

	//Mesh filter that will pass the generate mesh to the renderer
	MeshFilter meshFilter;

	//
	bool computed = false;

	//Exposing parameters to the inspector
	public Vector2 Position;
	public float ConeAngle = 6.30F;
	public float Direction = 3.14F;
	public bool Mouse = false;
	public bool Static = false;
	//public Color Color = new Color(1.0F, 1.0F, 1.0F);

	//Initialize the mesh and the vertices
	void Awake() 
	{
		lightMesh = new Mesh();
		meshFilter = GetComponent<MeshFilter>();

		CollectVertices ();
	}


	void Update () 
	{
		if (Static) {
			Mouse = false;
			if (computed)
				return;
		}

		// Get the light position
		Vector3 pos = Mouse ? Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,10))
			: new Vector3(Position.x, Position.y, 0);
		
		const float delta = 0.00001f;

		// Set the current position in the shader
		Material mat = GetComponent<Renderer>().material;
		mat.SetVector ("_Origin", new Vector4(pos.x, pos.y, pos.z, 0));

		// Move light to position
		transform.position = pos;


		// For every vertices, get its angle with respect to the x-axis
		// Additionally, add two other rays to hit also objects behind the corner
		vertAngles.Clear();
		for(var i=0;i<vertices.Count;i++)
		{
			float angle = Mathf.Atan2(vertices[i].y-pos.y,vertices[i].x-pos.x);

			//Ignore the vertices outside the light cone
			if (ConeAngle > Mathf.PI * 2 || isInsideLightCone(angle, Direction, ConeAngle)) {
				vertAngles.Add (angle - delta);
				vertAngles.Add (angle);
				vertAngles.Add (angle + delta);
			}

		}
		// Cast two rays for the edge of the cone:
		if (ConeAngle < Mathf.PI * 2) {
			float minAngle = (Direction - (ConeAngle / 2));
			float maxAngle = (Direction + (ConeAngle / 2));


			//Normalize the angles to the range (-PI, PI]
			//(same range as Mathf.Atan2)
			minAngle = minAngle <= Mathf.PI ? minAngle : minAngle - Mathf.PI * 2;
			maxAngle = maxAngle <= Mathf.PI ? maxAngle : maxAngle - Mathf.PI * 2;


			//Add them in the list of rays to cast
			vertAngles.Add(minAngle - delta);
			vertAngles.Add(minAngle);
			vertAngles.Add(minAngle + delta);
			vertAngles.Add (maxAngle - delta);
			vertAngles.Add (maxAngle);
			vertAngles.Add (maxAngle + delta);
		}

		// Cast rays to all the angles
		intersects.Clear();
		for(var j=0;j<vertAngles.Count;j++)
		{
			float angle = vertAngles[j];

			// Calculate dx & dy from angle
			float dx = Mathf.Cos(angle);
			float dy = Mathf.Sin(angle);

			// Ray from light position in the direction of angle
			Ray2D ray = new Ray2D(new Vector2(pos.x,pos.y), new Vector2(pos.x+dx,pos.y+dy));

			// Get all the possible intersections between the ray and the segments
			var allIntersct = 
				segments.Select (s => getIntersection (ray, s));

			// Find the nearest intersection
			Intersection nearestIntersection = 
				allIntersct.Where (x => x.v != null).Min();

			/*
			bool found = false;

			for(int i=0;i<segments.Count;i++)
			{
				Intersection intersect = getIntersection(ray,segments[i]);

				if(intersect.v==null) continue;

				if(!found || intersect.angle<nearestInersection.angle)
				{
					found = true;
					nearestInersection=intersect;
				}
			}  // for segments*/

			// Intersect angle
			//if(nearestInersection==null) continue;

			//Set the intercept angle for the valid interception we found
			nearestIntersection.angle = angle;

			// Add to list of intersects
			intersects.Add(nearestIntersection);

		}


		// Sort intersects by angle
		intersects.Sort((x, y) =>{ return Comparer<float?>.Default.Compare(x.angle, y.angle); });


		// Create mesh objetcs
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		verts.Clear();
		tris.Clear();

		// Put the first vertex in the light position
		verts.Add(transform.InverseTransformPoint(transform.position));

		for(var i=0;i<intersects.Count;i++)
		{
			if (intersects [i].v != null) {
				verts.Add (transform.InverseTransformPoint ((Vector3)intersects [i].v));
				verts.Add (transform.InverseTransformPoint ((Vector3)intersects [(i + 1) % intersects.Count].v));
			}
		}


		// Triangles
		for(var i=0;i<verts.Count+1;i++)
		{
			tris.Add((i+1) % verts.Count);
			tris.Add((i) % verts.Count);
			tris.Add(0);
		}

		// Build mesh
		lightMesh.Clear();
		lightMesh.vertices = verts.ToArray();
		lightMesh.triangles = tris.ToArray();
		//lightMesh.RecalculateNormals(); // FIXME: no need if no lights..or just assign fixed value..

		meshFilter.mesh = lightMesh;
		computed = true;

	}




	void CollectVertices ()
	{
		//Clear the vertices list, since it might not be the first time
		//the vertices are collected (immagine moving walls).
		vertices.Clear (); 

		// Collect all the game objects that are supposed to react to light
		GameObject[] gos = GameObject.FindGameObjectsWithTag("Wall");

		// Get all the vertices
		foreach (GameObject go in gos)
		{
			Mesh goMesh = go.GetComponent<MeshFilter>().mesh;
			int[] tris = goMesh.triangles;

			var uniqueTris = tris.Distinct ().ToArray();
				//new List<int>();
			//uniqueTris.Clear();

			// Collect unique tris
			/*for (int i = 0; i < tris.Length; i++) 
			{

				if (!uniqueTris.Contains(tris[i])) 
				{
					uniqueTris.Add(tris[i]);
				}
			} // for tris*/


			// Calculate pseudoangles (much faster than angles)
			List<PseudoAngleLocationTuple> psAngLocTuples = new List<PseudoAngleLocationTuple>();
			for (int n=0;n<uniqueTris.Length;n++)
			{
				float x = goMesh.vertices[uniqueTris[n]].x;
				float y = goMesh.vertices[uniqueTris[n]].y;

				// Sort by angle without calculating the angle
				// http://stackoverflow.com/questions/16542042/fastest-way-to-sort-vectors-by-angle-without-actually-computing-that-angle
				float psAng = copysign(1-x/(Mathf.Abs (x)+Mathf.Abs(y)),y);

				PseudoAngleLocationTuple psLoc = new PseudoAngleLocationTuple();
				psLoc.pAngle = psAng;
				psLoc.point = goMesh.vertices[uniqueTris[n]];
				psAngLocTuples.Add(psLoc);
			}

			// Sort by pseudoangle
			psAngLocTuples.Sort();

			// Get sorted list of points
			List<Vector3> sortedVerts = psAngLocTuples.Select(x => x.point).ToList();
				//new List<Vector3>();
			//uniqueTris.Clear();
			/*for (int n=0; n < psAngLocTuples.Count; n++)
			{
				sortedVerts.Add(psAngLocTuples[n].point);
			}*/

			// Add world borders, necessary to prevent misbehaviour if the world is not closed
			const int safetyRange = 1000;
			Camera cam = Camera.main;
			Vector3 b1 = cam.ScreenToWorldPoint(new Vector3(-safetyRange, -safetyRange, Camera.main.nearClipPlane + 0.1f+safetyRange)); // bottom left
			Vector3 b2 = cam.ScreenToWorldPoint(new Vector3(-safetyRange, cam.pixelHeight+safetyRange, cam.nearClipPlane + 0.1f+safetyRange)); // top left
			Vector3 b3 = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth+safetyRange, cam.pixelHeight, cam.nearClipPlane + 0.1f+safetyRange)); // top right
			Vector3 b4 = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth+safetyRange, -safetyRange, cam.nearClipPlane + 0.1f+safetyRange)); // bottom right

			// Transform world borders into vertices
			Segment2D seg1 = new Segment2D();
			seg1.a = new Vector2(b1.x,b1.y);
			seg1.b = new Vector2(b2.x,b2.y);
			segments.Add(seg1);

			seg1.a = new Vector2(b2.x,b2.y);
			seg1.b = new Vector2(b3.x,b3.y);
			segments.Add(seg1);

			seg1.a = new Vector2(b3.x,b3.y);
			seg1.b = new Vector2(b4.x,b4.y);
			segments.Add(seg1);

			seg1.a = new Vector2(b4.x,b4.y);
			seg1.b = new Vector2(b1.x,b1.y);
			segments.Add(seg1);

			// Connect the vertices with segment
			// Since they are sorted by angle, they will be connected counter-clockwise
			for (int n=0;n < sortedVerts.Count;n++)
			{
				// Segment start
				Vector3 wPos1 = go.transform.TransformPoint(sortedVerts[n]);

				// Segment end
				Vector3 wPos2 = go.transform.TransformPoint(sortedVerts[(n+1) % sortedVerts.Count]);

				vertices.Add(wPos1);

				Segment2D seg = new Segment2D();
				seg.a = new Vector2(wPos1.x,wPos1.y);
				seg.b = new Vector2(wPos2.x, wPos2.y);
				segments.Add(seg);
			}
		}
	}




	// Find intersection of ray and segment
	// http://ncase.me/sight-and-light/
	Intersection getIntersection(Ray2D ray, Segment2D segment)
	{
		Intersection res = new Intersection();

		// Ray in parametric form: Point + Delta*T1
		float r_px = ray.a.x;
		float r_py = ray.a.y;
		float r_dx = ray.b.x-ray.a.x;
		float r_dy = ray.b.y-ray.a.y;

		// Segment in parametric form: Point + Delta*T2
		float s_px = segment.a.x;
		float s_py = segment.a.y;
		float s_dx = segment.b.x-segment.a.x;
		float s_dy = segment.b.y-segment.a.y;

		// Get the magnitudes
		var r_mag = Mathf.Sqrt(r_dx*r_dx+r_dy*r_dy);
		var s_mag = Mathf.Sqrt(s_dx*s_dx+s_dy*s_dy);

		// Check if they are parallel
		if(r_dx/r_mag==s_dx/s_mag && r_dy/r_mag==s_dy/s_mag) // Unit vectors are the same
		{
			return res; 
		}

		// SOLVE FOR T1 & T2
		// r_px+r_dx*T1 = s_px+s_dx*T2 && r_py+r_dy*T1 = s_py+s_dy*T2
		// ==> T1 = (s_px+s_dx*T2-r_px)/r_dx = (s_py+s_dy*T2-r_py)/r_dy
		// ==> s_px*r_dy + s_dx*T2*r_dy - r_px*r_dy = s_py*r_dx + s_dy*T2*r_dx - r_py*r_dx
		// ==> T2 = (r_dx*(s_py-r_py) + r_dy*(r_px-s_px))/(s_dx*r_dy - s_dy*r_dx)
		var T2 = (r_dx*(s_py-r_py) + r_dy*(r_px-s_px))/(s_dx*r_dy - s_dy*r_dx);
		var T1 = (s_px+s_dx*T2-r_px)/r_dx;

		// If the following conditions are not true, there is no valid interception
		// (meaning the interception is outside the segment)
		// res.v and res.param will be null
		if(T1<0) return res;
		if(T2<0 || T2>1) return res;

		//Found interception
		res.v = new Vector3(r_px+r_dx*T1, r_py+r_dy*T1, 0);
		res.param = T1;

		return res;

	}



	/*
	 * Helper Functions
	 */

	// a = a * sgn(b)
	// http://stackoverflow.com/a/1905142
	float copysign(float a,float b)
	{
		return (a*Mathf.Sign(b));
	}

	float mod(float a, float n) {
		return a - Mathf.Floor (a / n) * n;
	}

	bool isInsideLightCone(float angle, float coneDirection, float coneAngle) {
		float diff = angle - coneDirection;

		diff = mod ((diff + Mathf.PI), Mathf.PI * 2) - Mathf.PI;


		if (Mathf.Abs(diff) < coneAngle / 2)
			return true;
		return false;
	}

	/// <summary>
	/// Intersection in parametric form, centered in the origin (v*param).
	/// </summary>
	struct Intersection : System.IComparable<Intersection>
	{
		public float? angle {get;set;}
		public float? param {get;set;}
		public Vector3? v { get; set;}

		public int CompareTo(Intersection that) {
			return this.param.Value.CompareTo(that.param.Value);
		}
	}

	struct PseudoAngleLocationTuple : System.IComparable<PseudoAngleLocationTuple>
	{
		public float pAngle {get;set;}
		public Vector3 point {get;set;}

		public int CompareTo(PseudoAngleLocationTuple that) { 
			return this.pAngle.CompareTo(that.pAngle); 
		}
	}

	//Ray from A to B
	struct Ray2D
	{
		public Vector2 a {get; set;}
		public Vector2 b {get; set;}

		public Ray2D(Vector2 _a, Vector2 _b)
		{
			this.a = _a;
			this.b = _b;
		}

	}

	//Segment from a to b
	struct Segment2D
	{
		public Vector2 a {get; set;}
		public Vector2 b {get; set;}

		public Segment2D(Vector2 _a, Vector2 _b)
		{
			this.a = _a;
			this.b = _b;
		}

	}

}

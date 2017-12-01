using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Geometry {

	static List<Segment2D> segments = new List<Segment2D>(); //Segments of the walls or light blocking objects
	static List<Vector3> vertices = new List<Vector3>(); //Vertices of the walls or light blocking objects
	static List<Polygon2D> walls = new List<Polygon2D>(); //Walls as polygon (for collision checking)

	public static void CollectVertices() {
		CollectVertices (new string[1] {"Wall"});
	}
		
	public static List<Vector3> GetVertices() {
		return vertices;
	}

	public static List<Segment2D> GetSegments() {
		return segments;
	}

	public static void CollectVertices(string[] tags)
	{
		//Clear the vertices list, since it might not be the first time
		//the vertices are collected (immagine moving walls).
		vertices.Clear (); 

		// Collect all the game objects that are supposed to react to light
		var gos = tags.SelectMany(tag => GameObject.FindGameObjectsWithTag(tag));
		//GameObject[] gos = GameObject.FindGameObjectsWithTag("Wall");

		// Get all the vertices
		foreach (GameObject go in gos)
		{
			Mesh goMesh = go.GetComponent<MeshFilter>().sharedMesh;
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

			if (sortedVerts.Count () <= 0)
				continue;

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

			List<Segment2D> wall = new List<Segment2D> ();

			//To calculate a point outside the wall
			const float margin = 1;
			float maxX = sortedVerts[0].x + margin, maxY = sortedVerts[0].y + margin;

			// Connect the vertices with segment
			// Since they are sorted by angle, they will be connected counter-clockwise
			for (int n=0;n < sortedVerts.Count;n++)
			{
				if (sortedVerts [n].x >= maxX)
					maxX = sortedVerts [n].x + margin;

				if (sortedVerts [n].y >= maxY)
					maxX = sortedVerts [n].y + margin;

				// Segment start
				Vector3 wPos1 = go.transform.TransformPoint(sortedVerts[n]);

				// Segment end
				Vector3 wPos2 = go.transform.TransformPoint(sortedVerts[(n+1) % sortedVerts.Count]);

				vertices.Add(wPos1);

				Segment2D seg = new Segment2D();
				seg.a = new Vector2(wPos1.x,wPos1.y);
				seg.b = new Vector2(wPos2.x, wPos2.y);
				segments.Add(seg); //Add the segment to the global list of segments
				wall.Add (seg); //Add the segment to the wall polygon
			}

			walls.Add (new Polygon2D (_segments : wall, _outside : new Vector2 (maxX, maxY)));
			
		}
	}

	public static bool IsPointInAWall(Vector2 p){
		return IsPointInAWall(new Vector3(p.x, p.y, 0));
	}

	public static bool IsPointInAWall(Vector3 p){
		return walls.Exists (w => PolygonContainsPoint (w, p));
	}

	public static bool IsSquareInAWall(Vector3 p, float size){
		float halfSide = size / 2;
		List<Vector2> vertices = new List<Vector2> (4);
		vertices.Add (p.x + halfSide, p.y + halfSide);
		vertices.Add (p.x + halfSide, p.y - halfSide);
		vertices.Add (p.x - halfSide, p.y + halfSide);
		vertices.Add (p.x - halfSide, p.y - halfSide);
		return vertices.Exists(v => IsPointInAWall(v));
	}

	public static bool IsRectangleInAWall(Vector3 p, float w, float h){
		float hw = w / 2, hh = h / 2;
		List<Vector2> vertices = new List<Vector2> (4);
		vertices.Add (p.x + hw, p.y + hh);
		vertices.Add (p.x + hw, p.y - hh);
		vertices.Add (p.x - hw, p.y + hh);
		vertices.Add (p.x - hw, p.y - hh);
		return vertices.Exists(v => IsPointInAWall(v));
	}

	public static bool IsInLineOfSight(Vector2 a, Vector2 b){
		return IsInLineOfSight (new Vector3 (a.x, a.y, 0), new Vector3 (b.x, b.y, 0));
	}

	/// <summary>
	/// Determines if there is a direct line of sight between two points.
	/// </summary>
	/// <returns><c>true</c> if there is a direct line of sight from a to b; otherwise, <c>false</c>.</returns>
	/// <param name="a">Point A.</param>
	/// <param name="b">Point B.</param>
	public static bool IsInLineOfSight(Vector3 a, Vector3 b){
		Ray2D ray = new Ray2D(a, b);

		return segments.TrueForAll(s => {
			var intersect = GetIntersection(ray, s);
			return intersect.v == null || intersect.param > 1.0;}
		);
	}

	/// <summary>
	/// Check if a polygon contains the point.
	/// Only xy plane is considered.
	/// </summary>
	/// <returns><c>true</c>, if point was contained, <c>false</c> otherwise.</returns>
	/// <param name="polygon">Polygon object.</param>
	/// <param name="p">Point</param>
	public static bool PolygonContainsPoint (Polygon2D polygon, Vector3 p)  { 
		//Cast a ray from the outside the polygon to the point to test
		Ray2D ray = new Ray2D(new Vector3(polygon.outside.x, polygon.outside.y, 0), p);
		//Get all the intersections between the segments if the light poligon and the casted ray
		var allIntersct = polygon.segments.Select (s => Geometry.GetIntersection (ray, s));
		// Find the number of intersections until the point (param < 1.0F)
		var intersectCount = allIntersct.Where (x => x.v != null && x.param <= 1.0F).Count();

		if (intersectCount % 2 != 0)
			return true;
		else
			return false;
	}


	// Find intersection of ray and segment
	// http://ncase.me/sight-and-light/
	public static Intersection GetIntersection(Ray2D ray, Segment2D segment)
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


	// a = a * sgn(b)
	// http://stackoverflow.com/a/1905142
	static float copysign(float a,float b)
	{
		return (a*Mathf.Sign(b));
	}
}

//Below helper structs used by Geometry and lights

/// <summary>
/// Intersection in parametric form, centered in the origin (v*param).
/// </summary>
public struct Intersection : System.IComparable<Intersection>
{
	public float? angle {get;set;}
	public float? param {get;set;}
	public Vector3? v { get; set;}

	public int CompareTo(Intersection that) {
		return this.param.Value.CompareTo(that.param.Value);
	}
}

public struct PseudoAngleLocationTuple : System.IComparable<PseudoAngleLocationTuple>
{
	public float pAngle {get;set;}
	public Vector3 point {get;set;}

	public int CompareTo(PseudoAngleLocationTuple that) { 
		return this.pAngle.CompareTo(that.pAngle); 
	}
}

//Ray from A to B
public struct Ray2D
{
	public Vector2 a {get; set;}
	public Vector2 b {get; set;}

	public Ray2D(Vector2 _a, Vector2 _b)
	{
		this.a = _a;
		this.b = _b;
	}

	public Ray2D(Vector3 _a, Vector3 _b)
	{
		this.a = new Vector2(_a.x, _a.y);
		this.b = new Vector2(_b.x, _b.y);
	}
}

//Segment from a to b
public struct Segment2D
{
	public Vector2 a {get; set;}
	public Vector2 b {get; set;}

	public Segment2D(Vector2 _a, Vector2 _b)
	{
		this.a = _a;
		this.b = _b;
	}

}

public struct Polygon2D
{
	public List<Segment2D> segments;
	public Vector2 outside;

	public Polygon2D(List<Segment2D> _segments, Vector2 _outside)
	{
		this.segments = _segments;
		this.outside = _outside;
	}

}
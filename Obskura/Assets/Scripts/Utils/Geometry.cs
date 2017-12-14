//Written by Manuel
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// World geometry utils.
/// </summary>
public static class Geometry {

	//To calculate a point outside the wall
	const float margin = 1;
	//When to consider two near vertices the same one
	const float verticesResolution = 0.01f;

	static List<Segment2D> segments = new List<Segment2D>(); //Segments of the walls or light blocking objects
	static List<Segment2D> dynSegments = new List<Segment2D>();
	static List<Vector3> vertices = new List<Vector3>(); //Vertices of the walls or light blocking objects
	static List<Vector3> dynVertices = new List<Vector3>(); 
	static List<Polygon2D> walls = new List<Polygon2D>(); //Walls as polygon (for collision checking)
	static List<Polygon2D> dynWalls = new List<Polygon2D>(); //Walls as polygon (for collision checking)

	//Segments longer than this will be cut in pieces
	//NOTE: This is an optimization to allow the lights to be local:
	//		A light will always be able to take into account only its surroundings and
	//		still have all the vertices of a segment inside its influence if the segments has a maximum length
	private static float segMaxLength = 8.0F;

	/// <summary>
	/// Get the maximum length of one segment, after which it will be broken in additional ones
	/// </summary>
	public static float MaxSegmentLength {
		get{
			return segMaxLength;
		}
	}

	/// <summary>
	/// Clear all the cachedd segments, verticesand walls.
	/// </summary>
	public static void Clear(){
		segments.Clear ();
		dynSegments.Clear();
		vertices.Clear();
		dynVertices.Clear();
		walls.Clear();
		dynWalls.Clear ();
	}

	/// <summary>
	/// Collects the vertices of the walls.
	/// </summary>
	public static void CollectVertices() {
		CollectVertices (new string[1] {"Wall"});
	}
		
	/// <summary>
	/// Returns the cached vertices
	/// </summary>
	/// <returns>The vertices.</returns>
	public static List<Vector3> GetVertices(bool dyn = false) {
		if (dyn)
			return dynVertices;
		else
			return vertices;
	}

	/// <summary>
	/// Returns the cached segments
	/// </summary>
	/// <returns>The segments.</returns>
	public static List<Segment2D> GetSegments(bool dyn = false) {
		if (dyn)
			return dynSegments;
		else
			return segments;
	}

	/// <summary>
	/// Returns the cached vertices within a range from a point
	/// </summary>
	/// <returns>The vertices.</returns>
	public static List<Vector3> GetVertices(Vector2 pos, float range = 8.0F) {
		List<Vector3> borderVertices = GetSquareCornersVertices (pos, range);

		var dynVs = dynVertices.Where (v => (Mathf.Abs (pos.x - v.x) < range) && (Mathf.Abs (pos.y - v.y) < range));

		return vertices.Where(v => (Mathf.Abs(pos.x - v.x) < range) && (Mathf.Abs(pos.y - v.y) < range))
			.Concat(borderVertices).Concat(dynVs).ToList();

	}

	/// <summary>
	/// Returns the cached segments within a range from a point
	/// </summary>
	/// <returns>The segments.</returns>
	public static List<Segment2D> GetSegments(Vector2 pos, float range = 8.0F) {
		List<Segment2D> borderSegments = GetSquareCornersSegments (pos, range); //GetSquareBorderSegments (pos, range);

		var dynSegs = dynSegments.Where (s => 
			(Mathf.Abs (pos.x - s.a.x) < range) && (Mathf.Abs (pos.y - s.a.y) < range) &&
			(Mathf.Abs (pos.x - s.b.x) < range) && (Mathf.Abs (pos.y - s.b.y) < range));

		return segments.Where(s => 
			(Mathf.Abs(pos.x - s.a.x) < range) && (Mathf.Abs(pos.y - s.a.y) < range) &&
			(Mathf.Abs(pos.x - s.b.x) < range) && (Mathf.Abs(pos.y - s.b.y) < range)
		).Concat(borderSegments).Concat(dynSegs).ToList();

	}

	/// <summary>
	/// Get the cached walls within a range
	/// </summary>
	/// <returns>The walls.</returns>
	/// <param name="pos">Position.</param>
	/// <param name="range">Range.</param>
	public static List<Polygon2D> GetWalls(Vector2 pos, float range = 8.0F) {
		var dynWs = dynWalls.Where (w => 
			w.segments.Exists (s => 
				((Mathf.Abs (pos.x - s.a.x) < range) && (Mathf.Abs (pos.y - s.a.y) < range) ||
		         (Mathf.Abs (pos.x - s.b.x) < range) && (Mathf.Abs (pos.y - s.b.y) < range))));

		return walls.Where(w => 
			w.segments.Exists(s => 
				((Mathf.Abs(pos.x - s.a.x) < range) && (Mathf.Abs(pos.y - s.a.y) < range) ||
				 (Mathf.Abs(pos.x - s.b.x) < range) && (Mathf.Abs(pos.y - s.b.y) < range)))
		).Concat(dynWalls).ToList();
	}

	/// <summary>
	/// Collects the vertices, segments and walls into the cache.
	/// </summary>
	/// <param name="tags">Tags.</param>
	/// <param name="dyn">If set to <c>true</c> dyn.</param>
	/// <param name="worldBorders">If set to <c>true</c> world borders.</param>
	/// <param name="maxSegLength">Max seg length.</param>
	public static void CollectVertices(string[] tags, bool dyn = false, bool worldBorders = true, float maxSegLength = 8.0F)
	{
		if (!dyn) {
			segMaxLength = maxSegLength;
		}
		//Debug.Log ("Collecting vertices");
		//Clear the vertices list, since it might not be the first time
		//the vertices are collected (imagine moving walls).
		if (dyn) {
			dynVertices.Clear ();
			dynSegments.Clear ();
		} else {
			vertices.Clear (); 
			segments.Clear ();
		}

		// Collect all the game objects that are supposed to react to light
		//var gos = tags.SelectMany(tag => GameObject.FindGameObjectsWithTag(tag));
		var gos = tags.SelectMany(tag => Tags.GameObjectsWithTag(tag));
		//GameObject[] gos = GameObject.FindGameObjectsWithTag("Wall");

		List<Polygon2D> polygons = new List<Polygon2D> ();

		// Get all the vertices
		foreach (GameObject go in gos)
		{
			Mesh goMesh = go.GetComponent<MeshFilter>().sharedMesh;

			// Get sorted (clockwise) list of points
			List<Vector3> sortedVerts = GetSortedVerticesFromMesh(goMesh).Select(v => go.transform.TransformPoint(v)).ToList();

			if (sortedVerts.Count () <= 0)
				continue;

			Polygon2D poly = GetPolyFromSortedVertices (sortedVerts);

			polygons.Add (poly);

			if (dyn) {
				//Create dynamic walls, segments and vertices
				dynWalls.Add (poly);
				dynSegments.AddRange (poly.segments);
				dynVertices.AddRange (sortedVerts);
			} else {
				// Create walls, segments and vertices
				walls.Add(poly);
				segments.AddRange (poly.segments);
				vertices.AddRange (sortedVerts);
			}
		}

		if (!dyn) CutSegmentsToPieces (maxSegLength);

		if (worldBorders) {
			// Add world borders, necessary to prevent misbehaviour if the world is not closed
			Polygon2D worldPoly = GetWorldBordersPoly ();

			segments.AddRange (worldPoly.segments);
		}


	}

	/// <summary>
	/// Determines if a point is in a wall within a certain range
	/// </summary>
	public static bool IsPointInAWall(Vector2 p, float range = 8.0F){
		return IsPointInAWall(new Vector3(p.x, p.y, 0), segMaxLength);
	}

	/// <summary>
	/// Determines if a point is in a wall within a certain range
	/// </summary>
	public static bool IsPointInAWall(Vector3 p, float range = 8.0F){
		return GetWalls(p, segMaxLength).Exists (w => PolygonContainsPoint (w, p));
	}

	/// <summary>
	/// Determines if any of the verttices of a square are in a wall within a certain range
	/// </summary>
	public static bool IsSquareInAWall(Vector3 p, float size, float range = 8.0F){
		float halfSide = size / 2;
		List<Vector2> vertices = new List<Vector2> (4);
		vertices.Add (new Vector2(p.x + halfSide, p.y + halfSide));
		vertices.Add (new Vector2(p.x + halfSide, p.y - halfSide));
		vertices.Add (new Vector2(p.x - halfSide, p.y + halfSide));
		vertices.Add (new Vector2(p.x - halfSide, p.y - halfSide));
		return vertices.Exists(v => IsPointInAWall(v, segMaxLength));
	}

	/// <summary>
	/// Determines if any of the vertices of a rectangle are in a wall
	/// </summary>
	public static bool IsRectangleInAWall(Vector3 p, float w, float h){
		float hw = w / 2, hh = h / 2;
		List<Vector2> vertices = new List<Vector2> (4);
		vertices.Add (new Vector2(p.x + hw, p.y + hh));
		vertices.Add (new Vector2(p.x + hw, p.y - hh));
		vertices.Add (new Vector2(p.x - hw, p.y + hh));
		vertices.Add (new Vector2(p.x - hw, p.y - hh));
		return vertices.Exists(v => IsPointInAWall(v));
	}

	/// <summary>
	/// Is there a direct line of sight between a and b?
	/// </summary>
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
		//If there is an odd number of intersection we are inside the polygon, if it's even we are outside
		if (intersectCount % 2 != 0)
			return true;
		else
			return false;
	}
		
	/// <summary>
	/// Returns a list of collidables with a certain tag, intersected by a ray.
	/// </summary>
	/// <returns>The actors intersecting ray with tags.</returns>
	public static List<ICollidableActor2D> GetActorsIntersectingRayWithTags(Vector2 a, Vector2 b, List<string> tags, bool stopAtWalls = true){
		Ray2D ray = new Ray2D (a, b);

		//Get all the object with the tag
		var gos = Tags.GameObjectsWithTags (tags).Where(g => g.GetComponent<ICollidableActor2D>() != null)
			.Select(g => g.GetComponent<ICollidableActor2D>()).Cast<ICollidableActor2D>();

		//Should we stop the ray at the first wall?
		if (stopAtWalls) {
			var allIntersct = 
				segments.Select (s => Geometry.GetIntersection (ray, s)).Where (x => x.v != null);

			if (allIntersct.Count () > 0) {
				// Find the nearest intersection
				Intersection nearestIntersection = allIntersct.Min();

				//Owerwrite the end of the ray with the position of the nearest wall intersection
				ray.b = new Vector2(nearestIntersection.v.Value.x, nearestIntersection.v.Value.y);
			}

		}

		float dist = (ray.a - ray.b).magnitude;

		//Check if the ray intesects any of the segments between the vertices of the square of the size of the actor
		return gos.Where (g => {
			Vector2 pos = g.GetPosition();
			float size = g.GetSize() / 2;
			float actorDist = (ray.a - pos).magnitude;
			bool isBehindWall = stopAtWalls && actorDist > dist; 
			List<Vector3> vertices = new List<Vector3>();
			vertices.Add(new Vector3(pos.x + size, pos.y + size, 0));
			vertices.Add(new Vector3(pos.x + size, pos.y - size, 0));
			vertices.Add(new Vector3(pos.x - size, pos.y - size, 0));
			vertices.Add(new Vector3(pos.x - size, pos.y + size, 0));
			return (!isBehindWall) && RayIntersectsWithAny(ray, GetSegmentsFromSortedVertices(vertices));
		}).ToList();

	}

	/// <summary>
	/// Gets the first intersection of a ray with a wall.
	/// </summary>
	/// <returns>The first intersection.</returns>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	/// <param name="maxDistance">Max distance.</param>
	public static Intersection GetFirstIntersection(Vector2 a, Vector2 b, float maxDistance){
		Ray2D ray = new Ray2D (a, b);

		//Find the first viable intersection
		Intersection first = new Intersection(null, null, null);
		//bool isThereAnIntersection = false;
		for (int i = 0; i < segments.Count; i++) {
			Intersection tmp = Geometry.GetIntersection (ray, segments [i]);
			if (tmp.v != null) {
				first = tmp;
				//isThereAnIntersection = true;
				break;
			}
		}
			

		//Find the nearest interception by computing a minimum
		Intersection nearestIntersection = first;
		foreach (Segment2D s in Geometry.GetSegments(a, maxDistance)) {
			Intersection tmpIntersect = Geometry.GetIntersection(ray, s);
			if (tmpIntersect.param != null && tmpIntersect.v != null &&
				tmpIntersect.param.Value < nearestIntersection.param.Value)

				nearestIntersection = tmpIntersect;
		}

		return nearestIntersection;
	}

	public static List<Vector3> GetSortedVerticesFromMesh(Mesh mesh) {

		int[] tris = mesh.triangles;

		var uniqueTris = tris.Distinct ().ToArray();

		// Calculate pseudoangles (much faster than angles)
		List<PseudoAngleLocationTuple> psAngLocTuples = new List<PseudoAngleLocationTuple>();
		for (int n=0;n<uniqueTris.Length;n++)
		{
			float x = mesh.vertices[uniqueTris[n]].x;
			float y = mesh.vertices[uniqueTris[n]].y;

			// Sort by angle without calculating the angle
			// http://stackoverflow.com/questions/16542042/fastest-way-to-sort-vectors-by-angle-without-actually-computing-that-angle
			float psAng = copysign(1-x/(Mathf.Abs (x)+Mathf.Abs(y)),y);

			PseudoAngleLocationTuple psLoc = new PseudoAngleLocationTuple();
			psLoc.pAngle = psAng;
			psLoc.point = mesh.vertices[uniqueTris[n]];

			bool isThereSimilarOne = false;

			foreach (PseudoAngleLocationTuple p in psAngLocTuples) {
				//L1 distance is good enough to recognize similar vertices, saves us from a Sqrt
				if (Mathf.Abs (p.point.x - x) < verticesResolution &&
				    Mathf.Abs (p.point.y - y) < verticesResolution) {
					isThereSimilarOne = true;
				}

			}

			if (!isThereSimilarOne)
				psAngLocTuples.Add(psLoc);
		}

		// Sort by pseudoangle
		psAngLocTuples.Sort();

		// Get sorted list of points
		List<Vector3> sortedVerts = psAngLocTuples.Select(x => x.point).ToList();

		return sortedVerts;
	}

	public static Polygon2D GetWorldBordersPoly(){
		List<Segment2D> segs = new List<Segment2D> ();

		// Add world borders, necessary to prevent misbehaviour if the world is not closed
		const int safetyRange = 1000;

		Camera cam = Camera.main;
		Vector3 b1 = cam.ScreenToWorldPoint(new Vector3(-safetyRange, -safetyRange, Camera.main.nearClipPlane + 0.1f+safetyRange)); // bottom left
		Vector3 b2 = cam.ScreenToWorldPoint(new Vector3(-safetyRange, cam.pixelHeight+safetyRange, cam.nearClipPlane + 0.1f+safetyRange)); // top left
		Vector3 b3 = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth+safetyRange, cam.pixelHeight, cam.nearClipPlane + 0.1f+safetyRange)); // top right
		Vector3 b4 = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth+safetyRange, -safetyRange, cam.nearClipPlane + 0.1f+safetyRange)); // bottom right

		float max = (safetyRange * 2) + cam.pixelHeight + cam.pixelHeight;

		// Transform world borders into vertices
		Segment2D seg1 = new Segment2D();
		seg1.a = new Vector2(b1.x,b1.y);
		seg1.b = new Vector2(b2.x,b2.y);
		segs.Add(seg1);

		seg1.a = new Vector2(b2.x,b2.y);
		seg1.b = new Vector2(b3.x,b3.y);
		segs.Add(seg1);

		seg1.a = new Vector2(b3.x,b3.y);
		seg1.b = new Vector2(b4.x,b4.y);
		segs.Add(seg1);

		seg1.a = new Vector2(b4.x,b4.y);
		seg1.b = new Vector2(b1.x,b1.y);
		segs.Add(seg1);

		return new Polygon2D (_segments : segs, _outside : new Vector2 (max, max), _filled : false);
	}

	/// <summary>
	/// Gets a polygon from clockwise vertices.
	/// Note: Remember to apply gameObject.transform.TransformPoint if the vertices if from a mesh
	/// </summary>
	/// <returns>The poly from clockwise vertices.</returns>
	/// <param name="sortedVerts">Sorted verts in world coordinates.</param>
	public static Polygon2D GetPolyFromSortedVertices(List<Vector3> sortedVerts){
		
		List<Segment2D> segs = new List<Segment2D> ();

		float maxX = sortedVerts[0].x + margin, maxY = sortedVerts[0].y + margin;

		for (int n=0;n < sortedVerts.Count;n++)
		{
			if (sortedVerts [n].x >= maxX)
				maxX = sortedVerts [n].x + margin;

			if (sortedVerts [n].y >= maxY)
				maxX = sortedVerts [n].y + margin;

			// Segment start
			Vector3 wPos1 = sortedVerts[n];

			// Segment end
			Vector3 wPos2 = sortedVerts[(n+1) % sortedVerts.Count];

			vertices.Add(wPos1);

			Segment2D seg = new Segment2D();
			seg.a = new Vector2(wPos1.x,wPos1.y);
			seg.b = new Vector2(wPos2.x, wPos2.y);
			segs.Add (seg); //Add the segment to the wall polygon
		}

		return new Polygon2D (_segments : segs, _outside : new Vector2 (maxX, maxY));

	}

	/// <summary>
	/// Gets the segments of a polygon from clockwise vertices.
	/// Note: Remember to apply gameObject.transform.TransformPoint if the vertices if from a mesh
	/// </summary>
	/// <returns>The segments from clockwise vertices.</returns>
	/// <param name="verts">Sorted verts in world coordinates.</param>
	public static List<Segment2D> GetSegmentsFromSortedVertices(List<Vector3> verts){
		List<Segment2D> res = new List<Segment2D> ();
		for (int i=0;i < verts.Count;i++)
		{
			// Segment start
			Vector3 wPos1 = verts[i];

			// Segment end
			Vector3 wPos2 = verts[(i+1) % verts.Count];

			Segment2D seg = new Segment2D();
			seg.a = new Vector2(wPos1.x,wPos1.y);
			seg.b = new Vector2(wPos2.x, wPos2.y);
			res.Add(seg);
		}
		return res;
	}

	/// <summary>
	/// Does the ray intersect with any of this segments?
	/// </summary>
	/// <returns><c>true</c>, if intersects with any was rayed, <c>false</c> otherwise.</returns>
	/// <param name="ray">Ray.</param>
	/// <param name="segments">Segments.</param>
	public static bool RayIntersectsWithAny(Ray2D ray, List<Segment2D> segments){
		return segments.Exists (s => GetIntersection (ray, s).v != null);
	}

	// Find intersection of ray and segment, adapted from the javascript tutorial
	// http://ncase.me/sight-and-light/
	/// <summary>
	/// Find intersection between a ray and a segment.
	/// </summary>
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

	/// <summary>
	/// Gets the vertices of a square
	/// </summary>
	public static List<Vector3> GetSquareCornersVertices(Vector2 center, float size) {
		float x = center.x, y = center.y;
		Vector3 b1 = new Vector3(x-size, y-size, 0); // bottom left
		Vector3 b2 = new Vector3(x-size, y+size, 0); // top left
		Vector3 b3 = new Vector3(x+size, y+size, 0); // top right
		Vector3 b4 = new Vector3(x+size, y-size, 0); // bottom right

		return new List<Vector3> { b1, b2, b3, b4 };
	} 

	/// <summary>
	/// Gets the sides (segments) of a square.
	/// </summary>
	public static  List<Segment2D> GetSquareCornersSegments(Vector2 center, float size) {
		return GetSegmentsFromSortedVertices(GetSquareCornersVertices(center, size));
	}
		
	/// <summary>
	/// Cuts the segments to pieces of maximum length maxLength.
	/// This is an optimization to allow lights to not care about far away objects
	/// WARNING: Call this procedure only once after having acquired the global segments and vertices, every additional call is going to slow down the frame rate
	/// </summary>
	/// <param name="maxLength">Max length.</param>
	private static void CutSegmentsToPieces(float maxLength) {
		List<Segment2D> resSegs = new List<Segment2D>(segments.Count * 2);
		List<Vector3> resVects = new List<Vector3> (vertices.Count * 2);

		//For each segment in the cache
		foreach (Segment2D s in segments) {
			Vector2 diff = s.b - s.a;
			float dist = diff.magnitude; //Calculate its length

			if (dist > maxLength) { //If the length is greater than the maximum
				int num = (int)(dist / maxLength); //calculate the number of pieces
				Vector2 oldv = s.a;

				//Generate segments and vertices composing the pieces
				for (int i = 1; i <= num; i++) {
					Vector2 newv = oldv + (diff * (maxLength / dist));
					resSegs.Add (new Segment2D (oldv, newv));
					resVects.Add (new Vector3 (newv.x, newv.y, -1.0F));
					oldv = newv;
				}
				//Add them to the result
				resSegs.Add (new Segment2D (oldv, s.b));

			} else {
				resSegs.Add (s);
			}
		}
		//Update the cache
		segments.Clear ();
		segments.AddRange(resSegs);
		vertices.AddRange (resVects);
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
	public float? angle;
	public float? param;
	public Vector3? v;

	public int CompareTo(Intersection that) {
		return this.param.Value.CompareTo(that.param.Value);
	}

	public Intersection(Vector3? _v, float? _angle, float? _param)
	{
		this.angle = _angle;
		this.param = _param;
		this.v = _v;
	}

}

/// <summary>
/// Location and its pseudoangle (useful to order points by angle efficiently)
/// </summary>
public struct PseudoAngleLocationTuple : System.IComparable<PseudoAngleLocationTuple>
{
	public float pAngle {get;set;}
	public Vector3 point {get;set;}

	public int CompareTo(PseudoAngleLocationTuple that) { 
		return this.pAngle.CompareTo(that.pAngle); 
	}
}

/// <summary>
/// Ray from point A to point B
/// </summary>
public struct Ray2D
{
	public Vector2 a;
	public Vector2 b;

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

/// <summary>
/// Segment from point a to point b
/// </summary>
public struct Segment2D
{
	public Vector2 a;
	public Vector2 b;

	public Segment2D(Vector2 _a, Vector2 _b)
	{
		this.a = _a;
		this.b = _b;
	}

}

/// <summary>
/// Poligon: Set of segments and definition of inside/outside.
/// </summary>
public struct Polygon2D
{
	public List<Segment2D> segments;
	public Vector2 outside;
	public bool filled;

	public Polygon2D(List<Segment2D> _segments, Vector2 _outside, bool _filled = true)
	{
		this.segments = _segments;
		this.outside = _outside;
		this.filled = _filled;
	}

}
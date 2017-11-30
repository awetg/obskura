using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Geometry : MonoBehaviour {

	List<Segment2D> segments = new List<Segment2D>(); //Segments of the walls or light blocking objects
	List<Vector3> vertices = new List<Vector3>(); //Vertices of the walls or light blocking objects

	public void CollectVertices() {
		CollectVertices (new string[1] {"Wall"});
	}

	public void CollectVertices(string[] tags)
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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	// a = a * sgn(b)
	// http://stackoverflow.com/a/1905142
	float copysign(float a,float b)
	{
		return (a*Mathf.Sign(b));
	}
}

//Below helper structs used by Geometry and lights

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

	public Ray2D(Vector3 _a, Vector3 _b)
	{
		this.a = new Vector2(_a.x, _a.y);
		this.b = new Vector2(_b.x, _b.y);
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
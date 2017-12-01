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

	List<Intersection> intersects = new List<Intersection>(); //Interceptions between rays and segments
	List<float> vertAngles = new List<float>(); //Angles of the vertices

	//Segments of the last calculates light, used to compute if a point is in the light
	List<Segment2D> lightSegments = new List<Segment2D>();

	//Mesh that represents the area affected by the light
	//NOTE: present as a global variable for caching purposes
	Mesh lightMesh;

	//Mesh filter that will pass the generate mesh to the renderer
	MeshFilter meshFilter;

	//
	bool computed = false;
	//bool debugFlag = false;

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
	}

	/// <summary>
	/// Refreshs the light mesh (for static lights).
	/// Call when the shadown casting objects in the map move or change.
	/// </summary>
	public void RefreshLight(){

		List<Segment2D> segments = Geometry.GetSegments (); //Segments of the walls or light blocking objects
		List<Vector3> vertices = Geometry.GetVertices(); //Vertices of the walls or light blocking objects


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
				segments.Select (s => Geometry.GetIntersection (ray, s));

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

		//Build the segments of the logical polygon used to detect if an object is lit
		lightSegments.Clear ();

		for (int i=0;i < verts.Count;i++)
		{
			// Segment start
			Vector3 wPos1 = transform.TransformPoint(verts[i]);

			// Segment end
			Vector3 wPos2 = transform.TransformPoint(verts[(i+1) % verts.Count]);

			Segment2D seg = new Segment2D();
			seg.a = new Vector2(wPos1.x,wPos1.y);
			seg.b = new Vector2(wPos2.x, wPos2.y);
			lightSegments.Add(seg);
		}

		// Triangles of the mesh
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

		//DEBUG
		/*Vector2 mpos = Camera.main.ScreenToWorldPoint (new Vector2 (Input.mousePosition.x, Input.mousePosition.y));

		if (PointIsInLight (mpos) != debugFlag) {
			debugFlag = !debugFlag;
			Debug.Log(debugFlag);
		}*/
	}

	void Update () 
	{
		if (Static) {
			Mouse = false;
			if (computed)
				return;
		}

		RefreshLight ();


	}

	/// <summary>
	/// Check if a point is in the light
	/// </summary>
	/// <returns><c>true</c>, if p is in light, <c>false</c> otherwise.</returns>
	/// <param name="p">Point (Vector2)</param>
	public bool PointIsInLight(Vector2 p) {
		return PointIsInLight (new Vector3 (p.x, p.y, 0));
	}

	/// <summary>
	/// Check if a point is in the light
	/// </summary>
	/// <returns><c>true</c>, if p is in light, <c>false</c> otherwise.</returns>
	/// <param name="p">Point (Vector3)</param>
	public bool PointIsInLight(Vector3 p) {
		if (lightSegments.Count == 0)
			return false;

		return ContainsPoint (lightSegments, p);
	}

	/// <summary>
	/// Check if a polygon contains the point.
	/// Only xy plane is considered.
	/// </summary>
	/// <returns><c>true</c>, if point was contained, <c>false</c> otherwise.</returns>
	/// <param name="polySegments">Segments of the polygon.</param>
	/// <param name="p">Point</param>
	bool ContainsPoint (List<Segment2D> polySegments, Vector3 p)  { 
		//Cast a ray from thee light to the point to test
		Ray2D ray = new Ray2D(transform.position, p);
		//Get all the intersections between the segments if the light poligon and the casted ray
		var allIntersct = polySegments.Select (s => Geometry.GetIntersection (ray, s));
		// Find the number of intersections until the point (param < 1.0F)
		var intersectCount = allIntersct.Where (x => x.v != null && x.param <= 1.0F).Count();

		/*   DEBUG
		allIntersct.ToList().Where (x => x.v != null && x.param < 1.0F).ToList().ForEach(i =>
			//Debug.DrawRay(i.v.Value, new Vector3(0.1F,0.1F,0.1F), (intersectCount % 2 == 0) ? Color.green : Color.red, 10000.0F, false)
			Debug.DrawLine (new Vector3(ray.a.x, ray.a.y, -20), new Vector3(ray.b.x, ray.b.y, -20), (intersectCount % 2 == 0) ? Color.green : Color.red, duration : 10000.0F ,depthTest : false)
		);
		Debug.Log (intersectCount);
		*/

		if (intersectCount % 2 == 0)
			return true;
		else
			return false;
	}





	/*
	 * Helper Functions
	 */


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
}

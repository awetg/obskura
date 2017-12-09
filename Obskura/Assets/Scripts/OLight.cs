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

	const float defaultIntensity = 1.0F;
	const string damageTag = "Enemy";
	public float maximumDistanceFactor = 1.6F;

	//Exposing parameters to the inspector
	public Vector2 Position;
	public float ConeAngle = 6.30F;
	public float Direction = 3.14F;
	public bool Mouse = false;
	public bool Static = false;
	public Color LightColor = new Color(1.0F, 1.0F, 1.0F);
	public float Intensity = defaultIntensity;
	public float DimmingDistance = 1.0F;
	public float DamagePerSecond = defaultIntensity * 10F;
	public bool IsStrong = false;
	public bool IsOn = true;
	public float FireProbabilityPerSecond = 0F;

	private bool isEnabled = true;

	//public Color Color = new Color(1.0F, 1.0F, 1.0F);

	//Initialize the mesh and the vertices
	void Awake() 
	{
		lightMesh = new Mesh();
		meshFilter = GetComponent<MeshFilter>();
	}

	void Start() {
		isEnabled = IsOn;
		Position = transform.position;
	}

	int debugVertC = 0;
	int debugSegC = 0;
	int debugVertCA = 0;

	/// <summary>
	/// Refreshs the light mesh (for static lights).
	/// Call when the shadown casting objects in the map move or change.
	/// This method is called in OLight.Update for dynamic lights.
	/// </summary>
	public void RefreshLight(){

		bool conical = ConeAngle < Mathf.PI * 2;
		float maxRange = DimmingDistance * maximumDistanceFactor + Geometry.MaxSegmentLength + 0.1F;

		// Get the light position
		Vector3 pos = Mouse ? Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,10))
			: new Vector3(Position.x, Position.y, 0);

		List<Segment2D> segments = Geometry.GetSegments(new Vector2(pos.x, pos.y), range : maxRange); //Segments of the walls or light blocking objects
		List<Vector3> vertices = Geometry.GetVertices(new Vector2(pos.x, pos.y), range : maxRange); //Vertices of the walls or light blocking objects

		if (segments.Count != debugSegC) {
			debugSegC = segments.Count;
			Debug.Log ("Segments: " + debugSegC);
		}

		if (vertices.Count != debugVertC) {
			debugVertC = vertices.Count;
			Debug.Log ("Segments: " + debugVertC);
		}

		const float delta = 0.00001f;

		// Set the current position in the shader
		Material mat = GetComponent<Renderer>().material;
		mat.SetVector ("_Origin", new Vector4(pos.x, pos.y, pos.z, 0));
		mat.SetFloat ("_Intensity", Intensity / 10.0F);
		mat.SetFloat ("_Dist", DimmingDistance);
		mat.SetColor ("_Color", LightColor);

		// Move light to position
		transform.position = pos;

		//To be used for conical light, minimum and maximum angles in the cone
		float minAngle = 0;
		float maxAngle = 0;

		if (conical) {
			minAngle = (Direction - (ConeAngle / 2));
			maxAngle = (Direction + (ConeAngle / 2));
			//Normalize the angles to the range (-PI, PI]
			//(same range as Mathf.Atan2)
			minAngle = minAngle <= Mathf.PI ? minAngle : minAngle - Mathf.PI * 2;
			maxAngle = maxAngle <= Mathf.PI ? maxAngle : maxAngle - Mathf.PI * 2;
		}

		// For every vertices, get its angle with respect to the x-axis
		// Additionally, add two other rays to hit also objects behind the corner
		vertAngles.Clear();
		for(var i=0;i<vertices.Count;i++)
		{
			float angle = Mathf.Atan2(vertices[i].y-pos.y,vertices[i].x-pos.x);

			if (conical && isInsideLightCone (-Mathf.PI, Direction, ConeAngle) && angle <= maxAngle) {
				angle += 2 * Mathf.PI;
				//Debug.Log ("In singularity");
			}

			//Ignore the vertices outside the light cone
			if (ConeAngle > Mathf.PI * 2 || isInsideLightCone(angle, Direction, ConeAngle)) {
				vertAngles.Add (angle - delta);
				vertAngles.Add (angle);
				vertAngles.Add (angle + delta);
			}

		}
		// if conical, Cast two rays for the edge of the cone:
		if (conical) {

			//Add them in the list of rays to cast
			//vertAngles.Add(minAngle - delta);
			vertAngles.Add(minAngle);
			//vertAngles.Add(minAngle + delta);
			//vertAngles.Add (maxAngle - delta);

			if (isInsideLightCone (-Mathf.PI, Direction, ConeAngle)) {
				vertAngles.Add (maxAngle + 2 * Mathf.PI);
				//Debug.Log ("In singularity");
			} else {
				vertAngles.Add (maxAngle);
			}
			//vertAngles.Add (maxAngle + delta);
		}

		/*if (vertAngles.Count != debugVertCA) {
			debugVertCA = vertAngles.Count;
			Debug.Log ("Angles: " + debugVertCA);
		}*/

		//Sort the angles in ascending order (counter-clockwise)
		vertAngles.Sort ();

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

			/* Note: The profiler shows that this is really slow
			//      I will replace it with a loop that finds the min while creating the intersections

			// Get all the possible intersections between the ray and the segments
			
			//var allIntersct = 
			//	segments.Select (s => Geometry.GetIntersection (ray, s));
			
			// Find the nearest intersection
			//Intersection nearestIntersection = 
			
			//	allIntersct.Where (x => x.v != null).Min();
			
			*/

			//Find the first viable intersection
			Intersection first = new Intersection(null, null, null);
			bool isThereAnIntersection = false;
			for (int i = 0; i < segments.Count; i++) {
				Intersection tmp = Geometry.GetIntersection (ray, segments [i]);
				if (tmp.v != null) {
					first = tmp;
					isThereAnIntersection = true;
					break;
				}
			}

			//If there is no viable intersection, continue with next angle
			if (!isThereAnIntersection) continue;

			//Find the nearest interception by computing a minimum
			Intersection nearestIntersection = first;
			foreach (Segment2D s in segments) {
				Intersection tmpIntersect = Geometry.GetIntersection(ray, s);
				if (tmpIntersect.param != null && tmpIntersect.v != null &&
					tmpIntersect.param.Value < nearestIntersection.param.Value)

					nearestIntersection = tmpIntersect;
			}

			//Set the intercept angle for the valid interception we found
			nearestIntersection.angle = angle;

			// Add to list of intersects
			intersects.Add(nearestIntersection);

		}
			
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
				//verts.Add (transform.InverseTransformPoint ((Vector3)intersects [(i + 1) % intersects.Count].v));
			}
		}

		//Not enough vertices to build a light polygon
		if ((conical && verts.Count < 3) || (!conical && verts.Count < 4))
			return;

		//Build the segments of the logical polygon used to detect if an object is lit
		lightSegments.Clear ();

		for (int i=1;i < verts.Count - 1;i++)
		{
			// Segment start
			Vector3 wPos1 = transform.TransformPoint(verts[i]);

			// Segment end
			Vector3 wPos2 = transform.TransformPoint(verts[(i+1) % verts.Count]);

			Segment2D seg = new Segment2D();
			seg.a = new Vector2(wPos1.x,wPos1.y);
			seg.b = new Vector2(wPos2.x, wPos2.y);

			Debug.DrawLine (seg.a, seg.b, Color.red, 0.1F);
			lightSegments.Add(seg);
		}

		Vector3 firstVert = transform.TransformPoint(verts[1]);
		Vector3 lastVert = transform.TransformPoint(verts[verts.Count - 1]);
		Vector3 center = transform.TransformPoint(verts[0]);

		if (conical) {
			//Connect the center with the first and the last vertices
			Segment2D seg1 = new Segment2D();
			Segment2D seg2 = new Segment2D();
			seg1.a = lastVert;
			seg1.b = center;
			seg2.a = center;
			seg2.b = firstVert;
			lightSegments.Add (seg1);
			lightSegments.Add (seg2);
			//Debug.DrawLine (seg1.a, seg1.b, Color.red, 10F);
			//Debug.DrawLine (seg2.a, seg2.b, Color.red, 10F);

		} else {
			//Discard the center and connect the first and the last vertices
			Segment2D seg = new Segment2D();
			seg.a = lastVert;
			seg.b = firstVert;
			lightSegments.Add (seg);
			//Debug.DrawLine (seg.a, seg.b, Color.red, 10F);
		}


		// Triangles of the mesh (except the last)
		for(var i=1;i<verts.Count-1;i++)
		{
			tris.Add((i+1));
			tris.Add((i));
			tris.Add(0);
		}

		// Add the last triangle
		tris.Add(1);
		tris.Add(verts.Count-1);
		tris.Add(0);


		/* // Triangles of the mesh
		for(var i=0;i<verts.Count+1;i++)
		{
			tris.Add((i+1) % verts.Count);
			tris.Add((i) % verts.Count);
			tris.Add(0);
		}*/

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
		if (!IsOn) {
			if (isEnabled) {
				GetComponent<Renderer> ().enabled = false;
				isEnabled = false;
			}
			return;
		}

		if (IsOn && !isEnabled) {
			GetComponent<Renderer> ().enabled = true;
			isEnabled = true;
		}

		InflictDamages ();

		SearchForPlayer ();

		if (Static) {
			Mouse = false;
			if (computed)
				return;
		}
			

		RefreshLight ();

	}

	void FixedUpdate(){
		if (!IsOn)
			return;
		
		//Vector3 pos = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 10));
		//PointIsInLight (pos);

		
		//InflictDamages ();
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
		var maxSegDist = (float) polySegments.Max (s => Mathf.Max (s.a.magnitude, s.b.magnitude)) * 2;
		var outside = new Vector3 (maxSegDist, maxSegDist + 0.01F * maxSegDist, 0);
		//We need to displace the starting point slightly, to make it be inside even in case of conic light
		//var direction = new Vector3(Mathf.Cos(Direction), Mathf.Sin(Direction)) * 0.1F;
		//Cast a ray from the light to the point to test
		//Ray2D ray = new Ray2D(transform.position, p);
		Ray2D ray = new Ray2D(outside, p);
		//float dist = ((transform.position) - p).magnitude;
		//Get all the intersections between the segments if the light poligon and the casted ray
		var allIntersct = polySegments.Select (s => Geometry.GetIntersection (ray, s));
		// Find the number of intersections until the point (param < 1.0F)
		var intersectCount = allIntersct.Where (x => x.v != null && x.param <= 1.0F).Count();

		/*
		allIntersct.ToList().Where (x => x.v != null && x.param < 1.0F).ToList().ForEach(i =>
			//Debug.DrawRay(i.v.Value, new Vector3(0.1F,0.1F,0.1F), (intersectCount % 2 == 0) ? Color.green : Color.red, 10000.0F, false)
			Debug.DrawLine (new Vector3(ray.a.x, ray.a.y, -20), new Vector3(ray.b.x, ray.b.y, -20), (intersectCount % 2 != 0) ? Color.green : Color.red, duration : 10000.0F ,depthTest : false)
		);
		Debug.Log (intersectCount);*/


		if (intersectCount % 2 != 0)
			return true;
		else
			return false;
	}


	void InflictDamages(){

		if (DamagePerSecond <= 0f)
			return;

		var objs = Tags.CachedGameObjectsWithTagInRange (damageTag, transform.position, DimmingDistance * maximumDistanceFactor);

		foreach (GameObject obj in objs) {
			Enemy enemy = obj.GetComponent<Enemy>();

			if (enemy != null && enemy.transform != null) {
				Vector2 enemyPos = new Vector2 (enemy.transform.position.x, enemy.transform.position.y);

				if (!PointIsInLight (enemyPos))
					return;

				Vector2 lightPos = new Vector2 (transform.position.x, transform.position.y);
				Vector2 diff = enemyPos - lightPos;
				float dist = diff.magnitude;
				float decay = 1F;

				if (dist > DimmingDistance) {
					decay = 1F / dist;
				}

				float damage = DamagePerSecond * Time.deltaTime * decay;

				//Debug.Log ("Inflicted Damage " + damage + " to " + enemy.name);

				enemy.GetDamaged (damage);
			}
		}
	}

	void SearchForPlayer(){

		if (!IsStrong)
			return;

		var objs = Tags.CachedGameObjectsWithTagInRange ("Player", transform.position, DimmingDistance * maximumDistanceFactor);

		foreach (GameObject obj in objs) {
			Player player = obj.GetComponent<Player>();

			if (player != null && player.transform != null) {
				Vector2 playerPos = new Vector2 (player.transform.position.x, player.transform.position.y);

				if (!PointIsInLight (playerPos))
					return;

				Vector2 lightPos = new Vector2 (transform.position.x, transform.position.y);
				Vector2 diff = playerPos - lightPos;
				float dist = diff.magnitude;

				if (dist < DimmingDistance)
					player.SetInStrongLight ();
			}
		}
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

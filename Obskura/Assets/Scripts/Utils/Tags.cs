using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Tags {

	static Dictionary<string,  List<GameObject>> cache = new Dictionary<string, List<GameObject>>();

	public static void CacheTag(string tag){
		if (cache.ContainsKey (tag)) {
			cache [tag] = GameObjectsWithTag (tag);
		} else {
			cache.Add(tag, GameObjectsWithTag(tag));
		}
	}

	public static void CacheAdd(GameObject obj){
		var pattern = "(([A-Z])([a-z]*))";
		var matches = Regex.Matches (obj.tag, pattern);

		foreach (Match m in matches) {
			string tag = m.Value;

			if (cache.ContainsKey (tag))
				cache [tag].Add (obj);
			else
				cache.Add (tag, new List<GameObject> { obj });
		}
	}

	public static void CacheRemove(GameObject obj){
		var pattern = "(([A-Z])([a-z]*))";
		var matches = Regex.Matches (obj.tag, pattern);

		foreach (Match m in matches) {
			string tag = m.Value;

			if (cache.ContainsKey (tag))
				cache [tag].Remove (obj);
		}
	}

	public static List<GameObject> CachedGameObjectsWithTag(string tag){
		if (cache.ContainsKey(tag)){
			var objs = cache[tag];
			return objs.Where (obj => obj.tag.Contains (tag)).ToList();
		}
		return new List<GameObject> { };
	}

	public static List<GameObject> CachedGameObjectsWithTagInRange(string tag, Vector2 center, float range){
		List<GameObject> res = new List<GameObject> ();
		if (cache.ContainsKey(tag)){
			foreach (GameObject obj in cache[tag]){
				var diff = new Vector2(obj.transform.position.x, obj.transform.transform.position.y) - center;
				var dist = diff.magnitude;
				if (dist <= range)
					res.Add (obj);
			}
		}
		return res;
	}

	// *** NOTE ***
	//The methods below are more generic and don't require a cache, but they are too slow to be used in Update()

	static IEnumerable<GameObject> GetAllGameObjectRecursive(GameObject root, int maxDepth) {
		List<GameObject> objs = new List<GameObject>();
		foreach (Transform tr in root.transform)
		{
			if (tr != null)
				objs.Add(tr.gameObject);
		}
			
		//Note: This should never happen, but if it does, it will prevent unity from crashing
		if (maxDepth <= 0)
			throw new UnityException ("Infinite recursion in GetAllGameObjectRecursive");


		//var objs = root.GetComponentsInChildren<Transform> ().Select (t => t.gameObject);

		if (objs.Count () <= 0) {
			return new List<GameObject>{ root };
		} else {
			var children = objs.SelectMany (o => GetAllGameObjectRecursive (o, maxDepth-1));
			return (new List<GameObject>{ root }).Concat (children);
		}
	}

	/// <summary>
	/// Gets all game objects in the current scene.
	/// Note: Slow, don't use in update.
	/// </summary>
	/// <returns>The all game objects.</returns>
	public static List<GameObject> GetAllGameObjects(){
		var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
		var roots = scene.GetRootGameObjects();
		return roots.SelectMany (r => GetAllGameObjectRecursive (r, maxDepth : 100)).ToList();
	}

	/// <summary>
	/// Get all gameobjects with a specific tag, in the current scene
	/// Note: Slow, don't use in update.
	/// </summary>
	/// <returns>The objects with tag.</returns>
	/// <param name="tag">Tag.</param>
	public static List<GameObject> GameObjectsWithTag(string tag){
		var objs = GetAllGameObjects ();
		return objs.Where (obj => obj.tag.Contains (tag)).ToList();
	}

	/// <summary>
	/// Get all gameobjects with all the tags specificed, in the current scene
	/// Note: Slow, don't use in update.
	/// </summary>
	/// <returns>The objects with tags.</returns>
	/// <param name="tags">Tags.</param>
	public static List<GameObject> GameObjectsWithTags(List<string> tags){
		var objs = GetAllGameObjects ();
		return objs.Where (obj => tags.TrueForAll(tag => obj.tag.Contains(tag))).ToList();
	}

	/// <summary>
	/// Get all gameobjects with one or more of the tags specificed, in the current scene
	/// Note: Slow, don't use in update.
	/// </summary>
	/// <returns>The objects with tags.</returns>
	/// <param name="tags">Tags.</param>
	public static List<GameObject> GameObjectsWithOneOf(List<string> tags){
		var objs = GetAllGameObjects ();
		return objs.Where (obj => tags.Exists(tag => obj.tag.Contains(tag))).ToList();
	}

	/// <summary>
	/// Get all gameobjects with one or more of the tags specificed, in the current scene
	/// Note: Slow, don't use in update.
	/// </summary>
	/// <returns>The objects with tags.</returns>
	/// <param name="tags">Tags.</param>
	public static List<GameObject> GameObjectsWithOneOf(string[] tags){
		return GameObjectsWithOneOf(tags.ToList());
	}
}

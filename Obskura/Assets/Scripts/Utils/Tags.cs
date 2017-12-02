using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Tags {

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

	public static List<GameObject> GetAllGameObjects(){
		var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
		var roots = scene.GetRootGameObjects();
		return roots.SelectMany (r => GetAllGameObjectRecursive (r, maxDepth : 100)).ToList();
	}

	public static List<GameObject> GameObjectsWithTag(string tag){
		var objs = GetAllGameObjects ();
		return objs.Where (obj => obj.tag.Contains (tag)).ToList();
	}

	public static List<GameObject> GameObjectsWithTags(List<string> tags){
		var objs = GetAllGameObjects ();
		return objs.Where (obj => tags.TrueForAll(tag => obj.tag.Contains(tag))).ToList();
	}

	public static List<GameObject> GameObjectsWithOneOf(List<string> tags){
		var objs = GetAllGameObjects ();
		return objs.Where (obj => tags.Exists(tag => obj.tag.Contains(tag))).ToList();
	}

	public static List<GameObject> GameObjectsWithOneOf(string[] tags){
		return GameObjectsWithOneOf(tags.ToList());
	}
}

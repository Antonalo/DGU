using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CopyComponents : MonoBehaviour
{
	public GameObject SourceGameObject;

	public GameObject TargetGameObjec;

	public List<Type> excludeTypes;

	private void Awake()
	{
		excludeTypes = new List<Type>();
		excludeTypes.Add(typeof(Transform));
		excludeTypes.Add(typeof(MeshFilter));
		excludeTypes.Add(typeof(MeshRenderer));
	}

	public void copy()
	{
	}
}

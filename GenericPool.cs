using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericPool<T> where T : MonoBehaviour
{
	private readonly Stack<T> _freeObjects;

	private readonly T _prefab;

	public GenericPool(T prefab, bool dontUseOriginal = false)
	{
		if (prefab == null)
		{
			throw new NullReferenceException("Generic pool received a null as a prefab");
		}
		prefab.gameObject.SetActive(value: false);
		_prefab = prefab;
		_freeObjects = new Stack<T>();
		if (!dontUseOriginal)
		{
			Store(prefab);
		}
	}

	private T InstantiateNew()
	{
		return UnityEngine.Object.Instantiate(_prefab);
	}

	public T GetResource()
	{
		return InstantiateNew();
	}

	public void Store(T obj)
	{
		obj.gameObject.SetActive(value: false);
		_freeObjects.Push(obj);
	}

	public void Cleanup()
	{
		_freeObjects.Clear();
	}
}

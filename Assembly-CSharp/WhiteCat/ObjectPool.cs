using System;
using System.Collections.Generic;
using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Object Pool")]
public class ObjectPool : BaseBehaviour
{
	[SerializeField]
	[Editable(true, false)]
	private GameObject _original;

	[SerializeField]
	[Editable(true, false)]
	private Transform _parent;

	[SerializeField]
	[Editable(true, false)]
	private int _quantity;

	[NonSerialized]
	private List<GameObject> _list;

	public int quantity => _quantity;

	public void AddObjects(int quantity)
	{
		while (quantity > 0)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_original);
			gameObject.SetActive(value: false);
			gameObject.transform.parent = _parent;
			gameObject.AddComponent<ObjectRecycler>().objectPool = this;
			_list.Add(gameObject);
			quantity--;
		}
		_quantity = _list.Count;
	}

	public GameObject TakeOut()
	{
		if (_quantity == 0)
		{
			AddObjects(1);
			Debug.Log("The ObjectPool is empty, a new GameObject had been created immediately.");
		}
		GameObject gameObject = _list[_quantity - 1];
		_list.RemoveAt(_quantity - 1);
		_quantity--;
		gameObject.SetActive(value: true);
		return gameObject;
	}

	public static void Recycle(GameObject go)
	{
		ObjectPool objectPool = go.GetComponent<ObjectRecycler>().objectPool;
		go.SetActive(value: false);
		go.transform.parent = objectPool._parent;
		objectPool._quantity++;
		objectPool._list.Add(go);
	}

	private void Awake()
	{
		_list = new List<GameObject>((_quantity >= 1) ? _quantity : 4);
		AddObjects(_quantity);
	}
}

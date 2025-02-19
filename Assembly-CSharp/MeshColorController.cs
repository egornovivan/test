using UnityEngine;

public class MeshColorController : MonoBehaviour
{
	public Color mColor;

	public bool mUpdate;

	private void Start()
	{
		Resetcolor();
	}

	private void Update()
	{
		if (mUpdate)
		{
			mUpdate = false;
			Resetcolor();
		}
	}

	private void Resetcolor()
	{
		MeshRenderer component = GetComponent<MeshRenderer>();
		if ((bool)component)
		{
			component.material = new Material(component.material);
			component.material.color = mColor;
		}
	}
}

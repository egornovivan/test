using UnityEngine;

namespace WhiteCat.Internal;

public abstract class TweenMaterialProperty : TweenBase
{
	[SerializeField]
	private Material _refMaterial;

	public Renderer renderer;

	public int materialIndex;

	public bool useSharedMaterial;

	[SerializeField]
	private string _propertyName;

	private int _propertyID = -1;

	public Material material
	{
		get
		{
			if ((bool)_refMaterial)
			{
				return _refMaterial;
			}
			if ((bool)renderer)
			{
				Material[] array = ((!useSharedMaterial && Application.isPlaying) ? renderer.materials : renderer.sharedMaterials);
				if (materialIndex < 0)
				{
					materialIndex = 0;
				}
				if (materialIndex >= array.Length)
				{
					materialIndex = array.Length - 1;
				}
				return array[materialIndex];
			}
			return null;
		}
		set
		{
			_refMaterial = value;
		}
	}

	public string propertyName
	{
		get
		{
			return _propertyName;
		}
		set
		{
			_propertyName = value;
			_propertyID = -1;
		}
	}

	public int propertyID => (_propertyID != -1) ? _propertyID : (_propertyID = Shader.PropertyToID(_propertyName));
}

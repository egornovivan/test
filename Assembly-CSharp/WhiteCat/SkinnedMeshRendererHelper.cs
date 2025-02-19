using UnityEngine;

namespace WhiteCat;

public class SkinnedMeshRendererHelper : MonoBehaviour, ISerializationCallbackReceiver
{
	[SerializeField]
	private Transform[] _bones;

	[SerializeField]
	private SkinnedMeshRenderer _renderer;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if ((bool)_renderer)
		{
			_bones = _renderer.bones;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if ((bool)_renderer)
		{
			_renderer.bones = _bones;
		}
	}

	private void Awake()
	{
		_renderer = GetComponent<SkinnedMeshRenderer>();
	}
}

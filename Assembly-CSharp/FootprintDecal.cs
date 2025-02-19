using UnityEngine;

public class FootprintDecal : MonoBehaviour
{
	private const int MaxLifeTime = 420;

	private const int FadeOutCnt = 300;

	[HideInInspector]
	public int _lifetime;

	private Renderer _r;

	public void Reset(Vector3 pos, Quaternion rot)
	{
		base.transform.position = pos;
		base.transform.rotation = rot;
		_lifetime = 420;
		_r.enabled = true;
	}

	private void Start()
	{
		_r = base.gameObject.GetComponent<Renderer>();
		_lifetime = 420;
		_r.enabled = true;
	}

	public void UpdateDecal()
	{
		if (_lifetime > 0)
		{
			_lifetime--;
			if (VFVoxelTerrain.self.Voxels.SafeRead((int)base.transform.position.x, (int)base.transform.position.y, (int)base.transform.position.z).Volume <= 64)
			{
				_lifetime = 0;
			}
			if (_lifetime <= 0)
			{
				_r.material.color = new Color(1f, 1f, 1f, 0f);
				_r.enabled = false;
			}
			else if (_lifetime > 300)
			{
				_r.material.color = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				_r.material.color = new Color(1f, 1f, 1f, (float)_lifetime / 300f);
			}
		}
	}
}

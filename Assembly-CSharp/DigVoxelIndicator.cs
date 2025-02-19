using UnityEngine;

public class DigVoxelIndicator : MonoBehaviour
{
	public IntVector3 m_Center = InvalidPos;

	public float m_Radius = 0.3f;

	public int m_AddHeight;

	public Gradient m_ActiveColors;

	public Gradient m_UnActiveColors;

	private Material _LineMaterial;

	public bool show;

	private bool m_FindVoxel;

	private int minVol = 1;

	public int volAdder = 10;

	public float m_Contrast = 0.75f;

	private BSMath.DrawTarget m_DTar;

	private float shrink = 0.2f;

	private ViewBounds mViewBounds;

	public static IntVector3 InvalidPos => new IntVector3(-100, -100, -100);

	public bool disEnable { get; set; }

	public Vector3 digPos => m_DTar.snapto;

	public bool active => disEnable && m_FindVoxel;

	public Bounds bounds
	{
		get
		{
			Vector3 size = 2f * (m_Radius + shrink) * Vector3.one;
			size.y = 1f;
			return new Bounds(m_Center, size);
		}
	}

	private void OnDestroy()
	{
		if (null != mViewBounds)
		{
			ViewBoundsMgr.Instance.Recycle(mViewBounds);
		}
	}

	private void GetCenterVoxel()
	{
		for (int i = minVol; i < 128 + volAdder; i += volAdder)
		{
			if (BSMath.RayCastDrawTarget(PeCamera.mouseRay, BuildingMan.Voxels, out m_DTar, i, true))
			{
				m_Center = new IntVector3(m_DTar.snapto);
				if (CheckVoxel(m_Center))
				{
					m_FindVoxel = true;
					return;
				}
			}
		}
		if (CheckVoxel(PeCamera.mouseRay.origin - BuildingMan.Voxels.Offset))
		{
			m_Center = new IntVector3(PeCamera.mouseRay.origin - BuildingMan.Voxels.Offset);
			m_FindVoxel = true;
			m_DTar = default(BSMath.DrawTarget);
			m_DTar.snapto = m_Center;
		}
		else
		{
			m_FindVoxel = false;
			m_Center = InvalidPos;
		}
	}

	private bool CheckVoxel(IntVector3 pos)
	{
		if (VFVoxelTerrain.self.Voxels.SafeRead(pos.x, pos.y, pos.z).Volume < 128)
		{
			for (int i = 0; i < 3; i++)
			{
				for (int j = -1; j <= 1; j += 2)
				{
					IntVector3 intVector = pos + new IntVector3(j * ((i == 0) ? 1 : 0), j * ((i == 1) ? 1 : 0), j * ((i == 2) ? 1 : 0));
					if (VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z).Volume >= 128)
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	private void Update()
	{
		if (show && base.enabled && !BSInput.s_MouseOnUI)
		{
			GetCenterVoxel();
			UpdateViewBounds();
		}
		else if (null != mViewBounds)
		{
			ViewBoundsMgr.Instance.Recycle(mViewBounds);
			mViewBounds = null;
		}
	}

	private void UpdateViewBounds()
	{
		if (null == mViewBounds)
		{
			mViewBounds = ViewBoundsMgr.Instance.Get();
		}
		float num = Mathf.RoundToInt(m_Radius);
		float num2 = 0f - num + (float)Mathf.FloorToInt(m_Radius * 2f);
		Vector3 vector = new Vector3((float)m_Center.x - num - 0.5f, (float)m_Center.y - 0.5f, (float)m_Center.z - num - 0.5f);
		Vector3 vector2 = new Vector3((float)m_Center.x + num2 + 0.5f, (float)(m_Center.y + m_AddHeight) + 0.5f, (float)m_Center.z + num2 + 0.5f);
		vector -= shrink * Vector3.one;
		vector2 += shrink * Vector3.one;
		mViewBounds.SetSize(vector2 - vector);
		mViewBounds.SetPos(vector);
		mViewBounds.SetColor((!active) ? m_UnActiveColors.Evaluate(1f) : m_ActiveColors.Evaluate(1f));
	}
}

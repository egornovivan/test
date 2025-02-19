using UnityEngine;

public class BuildOpItem : GLBehaviour
{
	public BuildingGui_N.OpType mType;

	public int mItemID;

	private bool mDrawGL;

	private bool mSelected;

	public bool Selected => mSelected;

	private void Awake()
	{
		m_Material = new Material(Shader.Find("Lines/Colored Blended"));
		m_Material.hideFlags = HideFlags.HideAndDontSave;
		m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		GlobalGLs.AddGL(this);
	}

	private void OnMouseUpAsButton()
	{
		BuildingGui_N.Instance.OnBuildOpItemSel(this);
	}

	private void OnMouseOver()
	{
		mDrawGL = true;
	}

	public void SetActive(bool active)
	{
		mSelected = active;
	}

	public override void OnGL()
	{
		if (null != GetComponent<Collider>() && (mDrawGL || mSelected))
		{
			mDrawGL = false;
			Vector3[] array = new Vector3[8]
			{
				new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.min.y, GetComponent<Collider>().bounds.min.z),
				new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.min.y, GetComponent<Collider>().bounds.min.z),
				new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.max.y, GetComponent<Collider>().bounds.min.z),
				new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.max.y, GetComponent<Collider>().bounds.min.z),
				new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.min.y, GetComponent<Collider>().bounds.max.z),
				new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.min.y, GetComponent<Collider>().bounds.max.z),
				new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.max.y, GetComponent<Collider>().bounds.max.z),
				new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.max.y, GetComponent<Collider>().bounds.max.z)
			};
			GL.PushMatrix();
			m_Material.SetPass(0);
			GL.Begin(1);
			GL.Color(Color.yellow);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.End();
			GL.Begin(7);
			if (mSelected)
			{
				GL.Color(new Color(0f, 0f, 0.2f, 0.5f));
			}
			else
			{
				GL.Color(new Color(0f, 0.2f, 0f, 0.5f));
			}
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[1].x, array[1].y, array[1].z);
			GL.Vertex3(array[5].x, array[5].y, array[5].z);
			GL.Vertex3(array[7].x, array[7].y, array[7].z);
			GL.Vertex3(array[3].x, array[3].y, array[3].z);
			GL.Vertex3(array[0].x, array[0].y, array[0].z);
			GL.Vertex3(array[4].x, array[4].y, array[4].z);
			GL.Vertex3(array[6].x, array[6].y, array[6].z);
			GL.Vertex3(array[2].x, array[2].y, array[2].z);
			GL.End();
			GL.PopMatrix();
		}
	}
}

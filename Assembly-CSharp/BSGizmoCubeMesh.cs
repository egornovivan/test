using UnityEngine;

public class BSGizmoCubeMesh : GLBehaviour
{
	private MeshFilter m_MeshFilter;

	private Mesh m_GizmoMesh;

	private MeshRenderer m_Renderer;

	public float m_VoxelSize = 0.01f;

	private IntVector3 m_CubeSize = new IntVector3(1, 1, 1);

	public float m_Shrink = 0.03f;

	public Transform m_ShapeGizmo;

	public Color color
	{
		get
		{
			return m_Renderer.material.color;
		}
		set
		{
			m_Renderer.material.SetColor("_MainColor", value);
		}
	}

	public IntVector3 CubeSize
	{
		get
		{
			return m_CubeSize;
		}
		set
		{
			m_CubeSize = new IntVector3(value);
			Update();
		}
	}

	private void Start()
	{
		m_MeshFilter = GetComponent<MeshFilter>();
		m_Renderer = GetComponent<MeshRenderer>();
		m_Renderer.material = Object.Instantiate(m_Renderer.material);
		m_GizmoMesh = m_MeshFilter.mesh;
	}

	private void Update()
	{
		m_GizmoMesh.Clear();
		Vector3[] array = new Vector3[36];
		Vector3[] array2 = new Vector3[36];
		Vector2[] array3 = new Vector2[36];
		int[] array4 = new int[36];
		Vector3 vector = m_VoxelSize * m_CubeSize.ToVector3();
		Vector3 vector2 = new Vector3(m_VoxelSize * m_Shrink, m_VoxelSize * m_Shrink, m_VoxelSize * m_Shrink);
		Vector3 vector3 = vector - vector2;
		ref Vector3 reference = ref array[2];
		reference = new Vector3(vector2.x, vector2.y, vector2.z);
		ref Vector3 reference2 = ref array[1];
		reference2 = new Vector3(vector2.x, vector3.y, vector2.z);
		ref Vector3 reference3 = ref array[0];
		reference3 = new Vector3(vector2.x, vector3.y, vector3.z);
		ref Vector3 reference4 = ref array[5];
		reference4 = new Vector3(vector2.x, vector3.y, vector3.z);
		ref Vector3 reference5 = ref array[4];
		reference5 = new Vector3(vector2.x, vector2.y, vector3.z);
		ref Vector3 reference6 = ref array[3];
		reference6 = new Vector3(vector2.x, vector2.y, vector2.z);
		ref Vector3 reference7 = ref array[6];
		reference7 = new Vector3(vector3.x, vector2.y, vector2.z);
		ref Vector3 reference8 = ref array[7];
		reference8 = new Vector3(vector3.x, vector3.y, vector2.z);
		ref Vector3 reference9 = ref array[8];
		reference9 = new Vector3(vector3.x, vector3.y, vector3.z);
		ref Vector3 reference10 = ref array[9];
		reference10 = new Vector3(vector3.x, vector3.y, vector3.z);
		ref Vector3 reference11 = ref array[10];
		reference11 = new Vector3(vector3.x, vector2.y, vector3.z);
		ref Vector3 reference12 = ref array[11];
		reference12 = new Vector3(vector3.x, vector2.y, vector2.z);
		ref Vector3 reference13 = ref array[12];
		reference13 = new Vector3(vector2.x, vector2.y, vector2.z);
		ref Vector3 reference14 = ref array[13];
		reference14 = new Vector3(vector3.x, vector2.y, vector2.z);
		ref Vector3 reference15 = ref array[14];
		reference15 = new Vector3(vector3.x, vector2.y, vector3.z);
		ref Vector3 reference16 = ref array[15];
		reference16 = new Vector3(vector3.x, vector2.y, vector3.z);
		ref Vector3 reference17 = ref array[16];
		reference17 = new Vector3(vector2.x, vector2.y, vector3.z);
		ref Vector3 reference18 = ref array[17];
		reference18 = new Vector3(vector2.x, vector2.y, vector2.z);
		ref Vector3 reference19 = ref array[20];
		reference19 = new Vector3(vector2.x, vector3.y, vector2.z);
		ref Vector3 reference20 = ref array[19];
		reference20 = new Vector3(vector3.x, vector3.y, vector2.z);
		ref Vector3 reference21 = ref array[18];
		reference21 = new Vector3(vector3.x, vector3.y, vector3.z);
		ref Vector3 reference22 = ref array[23];
		reference22 = new Vector3(vector3.x, vector3.y, vector3.z);
		ref Vector3 reference23 = ref array[22];
		reference23 = new Vector3(vector2.x, vector3.y, vector3.z);
		ref Vector3 reference24 = ref array[21];
		reference24 = new Vector3(vector2.x, vector3.y, vector2.z);
		ref Vector3 reference25 = ref array[24];
		reference25 = new Vector3(vector2.x, vector2.y, vector2.z);
		ref Vector3 reference26 = ref array[25];
		reference26 = new Vector3(vector2.x, vector3.y, vector2.z);
		ref Vector3 reference27 = ref array[26];
		reference27 = new Vector3(vector3.x, vector3.y, vector2.z);
		ref Vector3 reference28 = ref array[27];
		reference28 = new Vector3(vector3.x, vector3.y, vector2.z);
		ref Vector3 reference29 = ref array[28];
		reference29 = new Vector3(vector3.x, vector2.y, vector2.z);
		ref Vector3 reference30 = ref array[29];
		reference30 = new Vector3(vector2.x, vector2.y, vector2.z);
		ref Vector3 reference31 = ref array[32];
		reference31 = new Vector3(vector2.x, vector2.y, vector3.z);
		ref Vector3 reference32 = ref array[31];
		reference32 = new Vector3(vector2.x, vector3.y, vector3.z);
		ref Vector3 reference33 = ref array[30];
		reference33 = new Vector3(vector3.x, vector3.y, vector3.z);
		ref Vector3 reference34 = ref array[35];
		reference34 = new Vector3(vector3.x, vector3.y, vector3.z);
		ref Vector3 reference35 = ref array[34];
		reference35 = new Vector3(vector3.x, vector2.y, vector3.z);
		ref Vector3 reference36 = ref array[33];
		reference36 = new Vector3(vector2.x, vector2.y, vector3.z);
		ref Vector3 reference37 = ref array2[0];
		reference37 = Vector3.left;
		ref Vector3 reference38 = ref array2[1];
		reference38 = Vector3.left;
		ref Vector3 reference39 = ref array2[2];
		reference39 = Vector3.left;
		ref Vector3 reference40 = ref array2[3];
		reference40 = Vector3.left;
		ref Vector3 reference41 = ref array2[4];
		reference41 = Vector3.left;
		ref Vector3 reference42 = ref array2[5];
		reference42 = Vector3.left;
		ref Vector3 reference43 = ref array2[6];
		reference43 = Vector3.right;
		ref Vector3 reference44 = ref array2[7];
		reference44 = Vector3.right;
		ref Vector3 reference45 = ref array2[8];
		reference45 = Vector3.right;
		ref Vector3 reference46 = ref array2[9];
		reference46 = Vector3.right;
		ref Vector3 reference47 = ref array2[10];
		reference47 = Vector3.right;
		ref Vector3 reference48 = ref array2[11];
		reference48 = Vector3.right;
		ref Vector3 reference49 = ref array2[12];
		reference49 = Vector3.down;
		ref Vector3 reference50 = ref array2[13];
		reference50 = Vector3.down;
		ref Vector3 reference51 = ref array2[14];
		reference51 = Vector3.down;
		ref Vector3 reference52 = ref array2[15];
		reference52 = Vector3.down;
		ref Vector3 reference53 = ref array2[16];
		reference53 = Vector3.down;
		ref Vector3 reference54 = ref array2[17];
		reference54 = Vector3.down;
		ref Vector3 reference55 = ref array2[18];
		reference55 = Vector3.up;
		ref Vector3 reference56 = ref array2[19];
		reference56 = Vector3.up;
		ref Vector3 reference57 = ref array2[20];
		reference57 = Vector3.up;
		ref Vector3 reference58 = ref array2[21];
		reference58 = Vector3.up;
		ref Vector3 reference59 = ref array2[22];
		reference59 = Vector3.up;
		ref Vector3 reference60 = ref array2[23];
		reference60 = Vector3.up;
		ref Vector3 reference61 = ref array2[24];
		reference61 = Vector3.back;
		ref Vector3 reference62 = ref array2[25];
		reference62 = Vector3.back;
		ref Vector3 reference63 = ref array2[26];
		reference63 = Vector3.back;
		ref Vector3 reference64 = ref array2[27];
		reference64 = Vector3.back;
		ref Vector3 reference65 = ref array2[28];
		reference65 = Vector3.back;
		ref Vector3 reference66 = ref array2[29];
		reference66 = Vector3.back;
		ref Vector3 reference67 = ref array2[30];
		reference67 = Vector3.forward;
		ref Vector3 reference68 = ref array2[31];
		reference68 = Vector3.forward;
		ref Vector3 reference69 = ref array2[32];
		reference69 = Vector3.forward;
		ref Vector3 reference70 = ref array2[33];
		reference70 = Vector3.forward;
		ref Vector3 reference71 = ref array2[34];
		reference71 = Vector3.forward;
		ref Vector3 reference72 = ref array2[35];
		reference72 = Vector3.forward;
		for (int i = 0; i < 6; i++)
		{
			float x = 1f;
			float y = 1f;
			switch (i)
			{
			case 0:
				y = m_CubeSize.y;
				x = m_CubeSize.z;
				break;
			case 1:
				x = m_CubeSize.y;
				y = m_CubeSize.z;
				break;
			case 2:
				x = m_CubeSize.x;
				y = m_CubeSize.z;
				break;
			case 3:
				y = m_CubeSize.x;
				x = m_CubeSize.z;
				break;
			case 4:
				x = m_CubeSize.y;
				y = m_CubeSize.x;
				break;
			case 5:
				y = m_CubeSize.y;
				x = m_CubeSize.x;
				break;
			}
			ref Vector2 reference73 = ref array3[i * 6];
			reference73 = new Vector2(0f, 0f);
			ref Vector2 reference74 = ref array3[i * 6 + 1];
			reference74 = new Vector2(x, 0f);
			ref Vector2 reference75 = ref array3[i * 6 + 2];
			reference75 = new Vector2(x, y);
			ref Vector2 reference76 = ref array3[i * 6 + 3];
			reference76 = new Vector2(x, y);
			ref Vector2 reference77 = ref array3[i * 6 + 4];
			reference77 = new Vector2(0f, y);
			ref Vector2 reference78 = ref array3[i * 6 + 5];
			reference78 = new Vector2(0f, 0f);
			for (int j = 0; j < 6; j++)
			{
				int num = i * 6 + j;
				array4[num] = num;
			}
		}
		m_GizmoMesh.vertices = array;
		m_GizmoMesh.normals = array2;
		m_GizmoMesh.uv = array3;
		m_GizmoMesh.SetTriangles(array4, 0);
		if (m_ShapeGizmo != null)
		{
			m_ShapeGizmo.transform.localScale = m_CubeSize.ToVector3() * m_VoxelSize;
			m_ShapeGizmo.transform.localPosition = m_CubeSize.ToVector3() * m_VoxelSize * 0.5f;
			m_ShapeGizmo.GetComponentsInChildren<Renderer>(includeInactive: true)[0].material.renderQueue = 3000;
		}
	}

	public override void OnGL()
	{
		Vector3 vector = m_VoxelSize * m_CubeSize.ToVector3() * 0.5f + base.transform.position;
		Vector3 vector2 = (m_CubeSize.ToVector3() - Vector3.one) * m_VoxelSize * 0.5f;
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector3 = Vector3.down * vector2.y;
			if ((i & 1) == 0)
			{
				vector3 += Vector3.right * vector2.x;
			}
			else
			{
				vector3 -= Vector3.right * vector2.x;
			}
			if ((i & 2) == 0)
			{
				vector3 += Vector3.forward * vector2.z;
			}
			else
			{
				vector3 -= Vector3.forward * vector2.z;
			}
			Vector3 vector4 = vector + vector3;
			float t = Time.time / 1.5f - Mathf.Floor(Time.time / 1.5f);
			if (Physics.Raycast(new Ray(vector4, Vector3.down), out var hitInfo, 100f, VCConfig.s_EditorLayerMask))
			{
				GL.Begin(1);
				GL.Color(new Color(0.5f, 0.8f, 1f, 0.4f));
				GL.Vertex(vector4);
				GL.Color(new Color(0.5f, 0.8f, 1f, 1f));
				GL.Vertex(hitInfo.point);
				GL.Vertex(vector4);
				GL.Vertex(Vector3.Lerp(vector4, hitInfo.point, t));
				GL.Vertex(hitInfo.point + Vector3.forward * m_VoxelSize * 0.4f);
				GL.Vertex(hitInfo.point - Vector3.forward * m_VoxelSize * 0.4f);
				GL.Vertex(hitInfo.point + Vector3.right * m_VoxelSize * 0.4f);
				GL.Vertex(hitInfo.point - Vector3.right * m_VoxelSize * 0.4f);
				GL.End();
			}
		}
	}
}

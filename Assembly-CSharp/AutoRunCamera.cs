using UnityEngine;

public class AutoRunCamera : MonoBehaviour
{
	public enum AutoRunState
	{
		eIdle,
		eMove,
		eWaitFillVegetation
	}

	public enum MoveDir
	{
		eRight,
		eLeft,
		eUp
	}

	public float m_moveSpd = 25f;

	public float m_waitSec = 30f;

	private Vector3 m_offset = new Vector3(0f, 30f, 0f);

	public Vector3 m_curPos = default(Vector3);

	public int m_curCol;

	public int m_curRow;

	public float m_distX;

	public float m_distZ;

	private VoxelEditor m_voxelEditor;

	public float m_elps;

	public float m_moveStep = 256f;

	public float m_terrainSize = 24576f;

	private int m_maxRow;

	private int m_maxCol;

	public MoveDir m_eMoveDir;

	public AutoRunState m_eRS;

	private void Start()
	{
		m_voxelEditor = GameObject.Find("Voxel Terrain").GetComponent<VoxelEditor>();
	}

	private void Update()
	{
		if (m_eRS == AutoRunState.eIdle && Input.GetKeyDown(KeyCode.RightArrow))
		{
			m_curCol = (int)(base.transform.position.x / m_moveStep);
			m_curRow = (int)(base.transform.position.z / m_moveStep);
			m_curCol = Mathf.Clamp(m_curCol, 0, m_maxCol);
			m_curRow = Mathf.Clamp(m_curRow, 0, m_maxRow);
			if (m_curRow == m_maxRow && m_curCol == m_maxCol)
			{
				Debug.Log("AutoRunCamera::reach end point !!!");
			}
			else
			{
				m_curPos.x = (float)m_curCol * m_moveStep;
				m_curPos.z = (float)m_curRow * m_moveStep;
				m_curPos.y = base.transform.position.y;
				base.transform.position = m_curPos;
				if (m_curCol == m_maxCol)
				{
					m_eMoveDir = MoveDir.eUp;
					m_distZ = m_curPos.z + m_moveStep;
				}
				else
				{
					m_eMoveDir = MoveDir.eRight;
					m_distX = m_curPos.x + m_moveStep;
				}
				m_eRS = AutoRunState.eMove;
			}
		}
		if (m_eRS == AutoRunState.eMove)
		{
			m_curPos = base.transform.position;
			float num = m_moveSpd * Time.deltaTime;
			if (m_eMoveDir == MoveDir.eRight)
			{
				m_curPos.x += num;
				if (m_curPos.x >= m_distX || m_curPos.x >= m_terrainSize)
				{
					m_curPos.x = m_distX;
					m_eRS = AutoRunState.eWaitFillVegetation;
					m_curCol++;
				}
			}
			else if (m_eMoveDir == MoveDir.eLeft)
			{
				m_curPos.x -= num;
				if (m_curPos.x <= m_distX || m_curPos.x <= 0f)
				{
					m_curPos.x = m_distX;
					m_eRS = AutoRunState.eWaitFillVegetation;
					m_curCol--;
				}
			}
			else if (m_eMoveDir == MoveDir.eUp)
			{
				m_curPos.z += num;
				if (m_curPos.z >= m_distZ)
				{
					m_curPos.z = m_distZ;
					m_eRS = AutoRunState.eWaitFillVegetation;
					m_curRow++;
				}
			}
			m_curPos.y = 1600f;
			Ray ray = new Ray(m_curPos, new Vector3(0f, -1f, 0f));
			if (Physics.Raycast(ray, out var hitInfo, m_curPos.y))
			{
				m_curPos = hitInfo.point;
				m_curPos.y += m_offset.y;
				base.transform.position = m_curPos;
			}
		}
		if (m_eRS != AutoRunState.eWaitFillVegetation || VFVoxelTerrain.self.IsInGenerating)
		{
			return;
		}
		m_elps += Time.deltaTime;
		if (!(m_elps >= 6f))
		{
			return;
		}
		m_voxelEditor.AutoFillVegetation();
		m_elps = 0f;
		if (m_curCol == m_maxCol && m_curRow == m_maxRow)
		{
			m_eRS = AutoRunState.eIdle;
			return;
		}
		if (m_eMoveDir == MoveDir.eRight)
		{
			if (m_curCol == m_maxCol)
			{
				m_eMoveDir = MoveDir.eUp;
				m_distZ = m_curPos.z + m_moveStep;
			}
			else
			{
				m_distX = m_curPos.x + m_moveStep;
			}
		}
		else if (m_eMoveDir == MoveDir.eLeft)
		{
			if (m_curCol == 0)
			{
				m_eMoveDir = MoveDir.eUp;
				m_distZ = m_curPos.z + m_moveStep;
			}
			else
			{
				m_distX = m_curPos.x - m_moveStep;
			}
		}
		else if (m_eMoveDir == MoveDir.eUp)
		{
			if (m_curCol == 0)
			{
				m_eMoveDir = MoveDir.eRight;
				m_distX = m_curPos.x + m_moveStep;
			}
			else if (m_curCol == m_maxCol)
			{
				m_eMoveDir = MoveDir.eLeft;
				m_distX = m_curPos.x - m_moveStep;
			}
		}
		m_eRS = AutoRunState.eMove;
	}
}

using System;
using Pathea;
using Pathea.Effect;
using PETools;
using UnityEngine;
using WhiteCat;

public class MousePickableLootItem : MonoBehaviour
{
	public enum MoveState
	{
		Drop,
		Stay,
		Loot
	}

	public Renderer m_sphere;

	public Renderer m_Line;

	public UISprite m_Icon;

	public Transform m_IconRoot;

	public Transform m_LineRoot;

	private static readonly float FetchSqrDis = 0.25f;

	private static readonly float IconEnableSqrDis = 100f;

	public float stayMinTime = 1f;

	public float fadeSpeed = 1f;

	public int effectID;

	public int lootSoundID;

	public int getItemSoundID;

	private AudioController m_LootAudio;

	private LootItemData m_Data;

	private MoveState m_MoveState;

	private Transform m_Target;

	private Action<int> m_EndFunc;

	private Vector3 m_MoveDir;

	private float m_Speed;

	private Vector3 m_Velocity;

	public float m_MaxSpeed = 20f;

	public float m_Acceleration = 10f;

	public float m_RotateSpeed = 5f;

	public float selfRotateSpeed = 360f;

	private float m_StartStayTime;

	private Color m_DefaultCol;

	public LootItemData data => m_Data;

	private void Awake()
	{
		m_sphere.material = UnityEngine.Object.Instantiate(m_sphere.material);
		m_DefaultCol = m_sphere.material.GetColor("_Color");
	}

	public void SetData(LootItemData data)
	{
		m_Data = data;
		if (m_Data != null && m_Data.itemObj != null && m_Data.itemObj.protoData.icon != null)
		{
			m_Icon.spriteName = m_Data.itemObj.protoData.icon[0];
			base.transform.position = m_Data.position;
			base.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), Vector3.up);
			m_Icon.alpha = 0f;
			m_LineRoot.localScale = new Vector3(m_LineRoot.localScale.x, UnityEngine.Random.Range(0.4f, 0.65f), m_LineRoot.localScale.z);
			UpdateColor();
		}
	}

	public void SetMoveState(MoveState state, Action<int> moveEndFunc = null, Transform lootTrans = null)
	{
		m_EndFunc = moveEndFunc;
		m_Target = lootTrans;
		switch (state)
		{
		case MoveState.Drop:
			InitMoveDrop();
			break;
		case MoveState.Loot:
			break;
		case MoveState.Stay:
			InitStay();
			break;
		}
	}

	private void Update()
	{
		UpdateRotate();
		UpdateMoveState();
	}

	private void UpdateRotate()
	{
		m_IconRoot.rotation = Quaternion.AngleAxis(selfRotateSpeed * Time.deltaTime, Vector3.up) * m_IconRoot.rotation;
		m_sphere.transform.rotation = PEUtil.MainCamTransform.rotation;
		m_LineRoot.eulerAngles = new Vector3(0f, PEUtil.MainCamTransform.eulerAngles.y, 0f);
	}

	private void UpdateMoveState()
	{
		switch (m_MoveState)
		{
		case MoveState.Drop:
			UpdateDrop();
			break;
		case MoveState.Loot:
			UpdateLoot();
			break;
		case MoveState.Stay:
			UpdateStay();
			break;
		}
	}

	private void InitStay()
	{
		m_MoveState = MoveState.Stay;
		m_Line.enabled = true;
		m_Icon.enabled = true;
		m_Icon.alpha = 0f;
		m_StartStayTime = Time.time;
	}

	private void InitMoveDrop()
	{
		m_MoveState = MoveState.Drop;
		m_Line.enabled = false;
		m_Icon.enabled = false;
		m_Speed = m_MaxSpeed * UnityEngine.Random.Range(0.2f, 0.5f);
		m_MoveDir = Vector3.Slerp(UnityEngine.Random.onUnitSphere, Vector3.up, 0.7f);
		m_Velocity = m_MoveDir * m_Speed;
		base.transform.position += Vector3.up;
	}

	private void InitMoveLoot()
	{
		if (!(null == m_Target))
		{
			m_Line.enabled = false;
			m_Icon.enabled = false;
			m_MoveState = MoveState.Loot;
			Vector3 vector = m_Target.position - base.transform.position;
			m_MoveDir = Vector3.Slerp(UnityEngine.Random.onUnitSphere, -vector.normalized, 0.5f);
			if (m_MoveDir.y < 0f)
			{
				m_MoveDir.y = 0f - m_MoveDir.y;
			}
			m_Speed = m_MaxSpeed * UnityEngine.Random.Range(0.2f, 0.5f);
			if (null == m_LootAudio)
			{
				m_LootAudio = AudioManager.instance.Create(base.transform.position, lootSoundID, base.transform, isPlay: false, isDelete: false);
			}
			if (null != m_LootAudio)
			{
				m_LootAudio.PlayAudio(0.3f);
			}
		}
	}

	private void UpdateDrop()
	{
		if (!Physics.Raycast(base.transform.position + Vector3.up, Vector3.down, 50f, GameConfig.SceneLayer))
		{
			InitStay();
			return;
		}
		m_Velocity += Physics.gravity * Time.deltaTime;
		float maxDistance = m_Velocity.magnitude * Time.deltaTime;
		if (Physics.Raycast(base.transform.position, m_Velocity.normalized, maxDistance, GameConfig.SceneLayer))
		{
			InitStay();
			return;
		}
		base.transform.position += m_Velocity * Time.deltaTime;
		data.position = base.transform.position;
	}

	private void UpdateLoot()
	{
		if (null == m_Target)
		{
			OnEndLoot();
			return;
		}
		m_Speed = Mathf.Clamp(m_Speed + m_Acceleration * Time.deltaTime, 0f, m_MaxSpeed);
		Vector3 vector = m_Target.position - base.transform.position;
		m_MoveDir = Vector3.Slerp(m_MoveDir, vector.normalized, m_RotateSpeed * Time.deltaTime).normalized;
		m_Velocity = m_MoveDir * m_Speed;
		Vector3 position = base.transform.position;
		Vector3 vector2 = base.transform.position + m_Velocity * Time.deltaTime;
		base.transform.position = vector2;
		Vector3 vector3 = Utility.ClosestPoint(position, vector2, m_Target.position);
		if (Vector3.SqrMagnitude(vector3 - m_Target.position) < FetchSqrDis)
		{
			AudioManager.instance.Create(base.transform.position, getItemSoundID);
			base.transform.position = vector3;
			OnEndLoot();
		}
	}

	private void UpdateStay()
	{
		if (null != PeSingleton<MainPlayer>.Instance.entity)
		{
			float num = ((!(Vector3.SqrMagnitude(PeSingleton<MainPlayer>.Instance.entity.position - base.transform.position) < IconEnableSqrDis)) ? (-1f) : 1f);
			m_Icon.alpha = Mathf.Clamp01(m_Icon.alpha + num * fadeSpeed * Time.deltaTime);
		}
		if (Time.time - m_StartStayTime > stayMinTime)
		{
			InitMoveLoot();
		}
	}

	private void OnEndLoot()
	{
		Singleton<EffectBuilder>.Instance.Register(effectID, null, base.transform.position, base.transform.rotation, m_Target);
		if (null != m_LootAudio)
		{
			m_LootAudio.StopAudio();
		}
		m_MoveState = MoveState.Stay;
		if (m_EndFunc != null)
		{
			m_EndFunc(data.id);
		}
	}

	private void UpdateColor()
	{
		Color color = m_DefaultCol;
		if (!m_Data.itemObj.protoData.isFormula)
		{
			color = m_Data.itemObj.protoData.color;
		}
		m_sphere.material.SetColor("_Color", color);
	}
}

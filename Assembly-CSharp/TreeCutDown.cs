using System;
using Pathea.Effect;
using UnityEngine;

public class TreeCutDown : MonoBehaviour
{
	private const int scaling = 30;

	private const float particalDelayTime = 2f;

	[SerializeField]
	private AnimationCurve animaCur;

	private float startTime;

	private float downTime;

	private Vector3 downDirection;

	private Vector3[] footsPos;

	private float height;

	private int radius;

	private bool lockRotate;

	private bool lockDown = true;

	private bool lockPartical;

	private static AnimationCurve anim;

	private static AnimationCurve Anim
	{
		get
		{
			if (anim == null)
			{
				anim = new AnimationCurve(((GameObject)Resources.Load("Prefab/Item/Other/CutDownTreeCurve")).GetComponent<TreeCutDown>().animaCur.keys);
			}
			return anim;
		}
	}

	public void SetDirection(Vector3 casterPos, float height, float radius, Vector3[] footsPos = null)
	{
		if (footsPos == null)
		{
			Vector3 vector = base.transform.position - casterPos;
			vector = new Vector3(vector.x, 0f, vector.z);
			Vector3 vector2 = Vector3.Cross(vector.normalized, Vector3.up);
			Vector3 normalized = (vector + vector2).normalized;
			downDirection = Vector3.Cross(Vector3.up, normalized).normalized;
		}
		else if (footsPos.Length == 2)
		{
			Vector3 lhs = footsPos[0] - footsPos[1];
			Vector3 lhs2 = Vector3.Cross(lhs, Vector3.up);
			if (Vector3.Dot(lhs2, casterPos - (footsPos[0] / 2f + footsPos[1] / 2f)) > 0f)
			{
				downDirection = new Vector3(0f - lhs.x, 0f, 0f - lhs.z).normalized;
			}
			else
			{
				downDirection = new Vector3(lhs.x, 0f, lhs.z).normalized;
			}
			radius /= 2f;
			this.footsPos = footsPos;
		}
		int num = (int)Math.Ceiling(radius);
		this.radius = ((radius >= 7f) ? 8 : ((radius >= 5f) ? 6 : ((!(radius >= 3f)) ? 2 : 4)));
		this.height = height;
		animaCur = Anim;
		startTime = Time.time;
	}

	private void PlayRootPartical()
	{
		if (!lockPartical && Time.time - startTime > 2f)
		{
			if (footsPos == null)
			{
				Physics.Raycast(base.transform.position + Vector3.up * 10f, Vector3.down, out var hitInfo, 30f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
				Singleton<EffectBuilder>.Instance.Register(179 + radius / 2, null, ((!(hitInfo.collider == null)) ? hitInfo.point : base.transform.position) + new Vector3(0f, -0.2f, 0f), Quaternion.identity);
			}
			else
			{
				Physics.Raycast(footsPos[0] + Vector3.up * 10f, Vector3.down, out var hitInfo2, 30f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
				Physics.Raycast(footsPos[1] + Vector3.up * 10f, Vector3.down, out var hitInfo3, 30f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
				Singleton<EffectBuilder>.Instance.Register(179 + radius / 2, null, ((!(hitInfo2.collider == null)) ? hitInfo2.point : footsPos[0]) + new Vector3(0f, -0.2f, 0f), Quaternion.identity);
				Singleton<EffectBuilder>.Instance.Register(179 + radius / 2, null, ((!(hitInfo3.collider == null)) ? hitInfo3.point : footsPos[1]) + new Vector3(0f, -0.2f, 0f), Quaternion.identity);
			}
			AudioManager.instance.Create(base.transform.position, (radius < 5) ? 947 : ((radius >= 7) ? 939 : 945));
			lockPartical = true;
		}
	}

	private void Update()
	{
		if (downDirection == Vector3.zero)
		{
			return;
		}
		PlayRootPartical();
		if (!lockRotate)
		{
			base.transform.Rotate(downDirection, Time.deltaTime * 30f * animaCur.Evaluate(Time.time - startTime));
			Vector3 normalized = Vector3.Cross(downDirection, base.transform.up).normalized;
			Physics.Raycast(base.transform.position + base.transform.up * height, normalized, out var hitInfo, height * 0.2f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
			Physics.Raycast(base.transform.position + base.transform.up * height * 0.5f, normalized, out var hitInfo2, height * 0.1f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
			if (hitInfo.collider != null || hitInfo2.collider != null)
			{
				if (hitInfo.collider != null)
				{
					Singleton<EffectBuilder>.Instance.Register(175 + radius / 2, null, hitInfo.point + new Vector3(0f, 1f, 0f), Quaternion.identity);
				}
				else
				{
					Singleton<EffectBuilder>.Instance.Register(175 + radius / 2, null, hitInfo2.point + new Vector3(0f, 1f, 0f), Quaternion.identity);
				}
				AudioManager.instance.Create(base.transform.position, (radius < 5) ? 948 : ((radius >= 7) ? 940 : 946));
				lockRotate = true;
				UnityEngine.Object.Destroy(base.gameObject, 10f);
				downTime = Time.time;
			}
			if (Time.time - startTime > 6f)
			{
				lockRotate = true;
				UnityEngine.Object.Destroy(base.gameObject, 10f);
				downTime = Time.time;
			}
		}
		else if (!lockDown)
		{
			base.transform.Translate(Vector3.down * 0.01f * radius * Time.deltaTime * 30f, Space.World);
		}
		else if (Time.time - downTime > 1f)
		{
			lockDown = false;
		}
		else
		{
			base.transform.Rotate(downDirection, Time.deltaTime * 30f * animaCur.Evaluate(Time.time - downTime + 7f));
		}
	}
}

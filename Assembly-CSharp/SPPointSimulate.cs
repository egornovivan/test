using AiAsset;
using UnityEngine;

public class SPPointSimulate : SPPointMovable
{
	private float damage;

	private float minInterval;

	private float maxInterval;

	private float radius;

	private float mDamage;

	private float mHp;

	private float mMaxHp;

	public bool isDamage => hpPercent > float.Epsilon && (base.clone == null || !base.clone.activeSelf);

	public float hpPercent
	{
		get
		{
			return mHp / mMaxHp;
		}
		set
		{
			mHp = Mathf.Clamp(mMaxHp * value, 0f, mMaxHp);
		}
	}

	public override void Init(IntVector4 idx, Transform parent = null, int spid = 0, int pathid = 0, bool isActive = true, bool revisePos = true, bool isBoss = false, bool isErode = true, bool isDelete = true, SimplexNoise noise = null, AssetReq.ReqFinishDelegate onSpawned = null, CommonInterface common = null)
	{
		base.Init(idx, parent, spid, pathid, isActive, revisePos, isBoss, isErode, isDelete, noise, onSpawned, common);
		if (pathid > 0)
		{
			AiDataBlock aIDataBase = AiDataBlock.GetAIDataBase(pathid);
			if (aIDataBase != null)
			{
				mDamage = aIDataBase.damageSimulate;
				mMaxHp = aIDataBase.maxHpSimulate;
				mHp = mMaxHp;
			}
		}
		if (spid > 0)
		{
			AISpawnPath spawnPath = AISpawnPath.GetSpawnPath(spid);
			if (spawnPath != null)
			{
				mDamage = spawnPath.damage;
				mMaxHp = spawnPath.maxHp;
				mHp = mMaxHp;
			}
		}
	}

	protected override void OnSpawnedChild(GameObject obj)
	{
		base.OnSpawnedChild(obj);
		AiObject component = obj.GetComponent<AiObject>();
		if (component != null)
		{
			component.lifePercent = hpPercent;
		}
	}

	protected override void OnSpawned(GameObject obj)
	{
		base.OnSpawned(obj);
		if (base.aiObject != null)
		{
			base.aiObject.lifePercent = hpPercent;
		}
	}

	public void SetData(float argDamage, float argMinInterval, float argMaxInterval, float argRadius)
	{
		damage = argDamage;
		minInterval = argMinInterval;
		maxInterval = argMaxInterval;
		radius = argRadius;
	}

	public void ApplyDamage(float value)
	{
		if (base.aiObject != null)
		{
			hpPercent = base.aiObject.lifePercent;
		}
		mHp = Mathf.Clamp(mHp - value, 0f, mMaxHp);
		if (base.aiObject != null)
		{
			base.aiObject.lifePercent = hpPercent;
		}
		if (hpPercent < float.Epsilon)
		{
			Delete();
		}
	}

	public new void Start()
	{
		base.Start();
	}
}

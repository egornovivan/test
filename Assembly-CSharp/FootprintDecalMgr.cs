using System.Collections.Generic;
using NaturalResAsset;
using Pathea;
using Pathea.Effect;
using UnityEngine;

public class FootprintDecalMgr : MonoLikeSingleton<FootprintDecalMgr>
{
	private List<FootprintDecalMan> _reqDecals = new List<FootprintDecalMan>();

	private int _layerToFootprintTerra = 12;

	private int _layerToFootprintMetal = 11;

	private int _layerMaskToFootprint;

	private int _lastSoundID;

	private static readonly int[] _metalClipIds = new int[4] { 4097, 4098, 4099, 4100 };

	private static readonly int[] _stoneClipIds = new int[4] { 4501, 4502, 4503, 4504 };

	private int[] _nonTerClipIds = _metalClipIds;

	protected override void OnInit()
	{
		_layerMaskToFootprint = 0;
		_layerToFootprintTerra = 12;
		_layerToFootprintMetal = -1;
		_nonTerClipIds = _metalClipIds;
		if (PeGameMgr.IsAdventure)
		{
			if (RandomDungenMgr.Instance != null && RandomDungenMgrData.dungeonBaseData != null && RandomDungenMgrData.InDungeon)
			{
				DungeonType dungeonType = RandomDunGenUtil.GetDungeonType();
				if (dungeonType == DungeonType.Iron || dungeonType == DungeonType.Cave)
				{
					_layerToFootprintTerra = -1;
					_layerToFootprintMetal = 11;
					_nonTerClipIds = _stoneClipIds;
				}
			}
		}
		else if (PeGameMgr.IsStory)
		{
			if ((PeGameMgr.IsSingle || PeGameMgr.IsTutorial) && SingleGameStory.curType != 0)
			{
				_layerToFootprintTerra = -1;
				_layerToFootprintMetal = 11;
			}
			if (PeGameMgr.IsMulti && PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId != 0)
			{
				_layerToFootprintTerra = -1;
				_layerToFootprintMetal = 11;
			}
		}
		if (_layerToFootprintTerra >= 0)
		{
			_layerMaskToFootprint |= 1 << _layerToFootprintTerra;
		}
		if (_layerToFootprintMetal >= 0)
		{
			_layerMaskToFootprint |= 1 << _layerToFootprintMetal;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void Update()
	{
		int count = _reqDecals.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			if (_reqDecals[num] == null)
			{
				_reqDecals.RemoveAt(num);
			}
			else
			{
				Update(_reqDecals[num]);
			}
		}
	}

	public void Register(FootprintDecalMan reqDecal)
	{
		_reqDecals.Add(reqDecal);
	}

	public void Unregister(FootprintDecalMan reqDecal)
	{
		_reqDecals.Remove(reqDecal);
	}

	private void Update(FootprintDecalMan reqDecal)
	{
		float num = 0f;
		bool flag = false;
		bool flag2 = reqDecal._ctrlr.velocity.magnitude > reqDecal._thresVelOfMove;
		if (flag2)
		{
			Vector3 velocity = reqDecal._ctrlr.velocity;
			velocity.y = 0f;
			Vector3 lhs = reqDecal._lrFoot[0].position - reqDecal._lrFoot[1].position;
			lhs.y = 0f;
			num = Vector3.Dot(lhs, velocity.normalized);
			if ((reqDecal._curFoot == 0 && reqDecal._fpLastLRFootDistance > 0f && num < reqDecal._fpLastLRFootDistance) || (reqDecal._curFoot == 1 && reqDecal._fpLastLRFootDistance < 0f && num > reqDecal._fpLastLRFootDistance))
			{
				flag = true;
			}
			reqDecal._fpbFootInMove[0] = (reqDecal._fpbFootInMove[1] = true);
			reqDecal._fpbPlayerInMove = true;
		}
		else
		{
			if (reqDecal._fpbPlayerInMove)
			{
				ref Vector3 reference = ref reqDecal._fpLastFootsPos[0];
				reference = reqDecal._lrFoot[0].position;
				ref Vector3 reference2 = ref reqDecal._fpLastFootsPos[1];
				reference2 = reqDecal._lrFoot[1].position;
				reqDecal._fpbPlayerInMove = false;
			}
			float num2 = Vector3.Magnitude(reqDecal._lrFoot[reqDecal._curFoot].position - reqDecal._fpLastFootsPos[reqDecal._curFoot]);
			if (reqDecal._fpbFootInMove[reqDecal._curFoot])
			{
				if (num2 < 0.02f)
				{
					reqDecal._fpbFootInMove[reqDecal._curFoot] = false;
				}
				ref Vector3 reference3 = ref reqDecal._fpLastFootsPos[reqDecal._curFoot];
				reference3 = reqDecal._lrFoot[reqDecal._curFoot].position;
			}
			else if (num2 > 0.04f)
			{
				reqDecal._fpbFootInMove[reqDecal._curFoot] = true;
				flag = true;
			}
		}
		if (flag || reqDecal._ctrlr.fallGround)
		{
			Transform transform = reqDecal._lrFoot[reqDecal._curFoot];
			Ray ray = new Ray(transform.position + Vector3.up * reqDecal._rayLength * 0.5f, Vector3.down);
			if (Physics.Raycast(ray, out var hitInfo, reqDecal._rayLength, (1 << _layerToFootprintTerra) | (1 << _layerToFootprintMetal)))
			{
				Vector3 vector = hitInfo.point + hitInfo.normal * 0.02f;
				int layer = hitInfo.transform.gameObject.layer;
				if (layer == _layerToFootprintTerra)
				{
					Vector3 toDirection = Quaternion.Euler(0f, 90f, 0f) * transform.forward;
					toDirection.y = 0f;
					int num3 = reqDecal._curFpIdx[reqDecal._curFoot];
					Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, hitInfo.normal) * Quaternion.FromToRotation(Vector3.forward, toDirection);
					if (reqDecal._fpGoUpdates[reqDecal._curFoot, num3] == null)
					{
						GameObject gameObject = Object.Instantiate(reqDecal._fpSeedGoLR, vector, quaternion) as GameObject;
						gameObject.transform.parent = reqDecal.FootPrintsParent;
						reqDecal._fpGoUpdates[reqDecal._curFoot, num3] = gameObject.GetComponent<FootprintDecal>();
					}
					else
					{
						reqDecal._fpGoUpdates[reqDecal._curFoot, num3].Reset(vector, quaternion);
					}
					reqDecal._curFpIdx[reqDecal._curFoot] = (num3 + 1) % reqDecal._fpGoUpdates.GetLength(1);
				}
				if (flag2 || reqDecal._ctrlr.fallGround)
				{
					int num4 = 0;
					if (layer == _layerToFootprintMetal)
					{
						if (reqDecal._mmc == null || !reqDecal._mmc.GetMaskState(PEActionMask.SwordAttack))
						{
							int num5 = Random.Range(0, _nonTerClipIds.Length);
							num4 = _nonTerClipIds[num5];
							if (_lastSoundID == num4)
							{
								num5 += Random.Range(1, _nonTerClipIds.Length);
								if (num5 >= _nonTerClipIds.Length)
								{
									num5 -= _nonTerClipIds.Length;
								}
								num4 = _nonTerClipIds[num5];
							}
							AudioManager.instance.Create(hitInfo.point, num4);
							_lastSoundID = num4;
						}
					}
					else
					{
						int vType = 8;
						if (layer == 12)
						{
							vType = VFVoxelTerrain.self.Voxels.SafeRead((int)hitInfo.point.x, (int)hitInfo.point.y, (int)hitInfo.point.z).Type;
						}
						NaturalRes terrainResData = NaturalRes.GetTerrainResData(vType);
						if (terrainResData != null)
						{
							if (terrainResData.mGroundEffectID > 0)
							{
								Singleton<EffectBuilder>.Instance.Register(terrainResData.mGroundEffectID, null, reqDecal.transform);
							}
							if (terrainResData.mGroundSoundIDs != null && terrainResData.mGroundSoundIDs.Length > 0 && (reqDecal._mmc == null || !reqDecal._mmc.GetMaskState(PEActionMask.SwordAttack)))
							{
								if (terrainResData.mGroundSoundIDs.Length > 1)
								{
									int num6 = Random.Range(0, terrainResData.mGroundSoundIDs.Length);
									num4 = terrainResData.mGroundSoundIDs[num6];
									if (_lastSoundID == num4)
									{
										num6 += Random.Range(1, terrainResData.mGroundSoundIDs.Length);
										if (num6 >= terrainResData.mGroundSoundIDs.Length)
										{
											num6 -= terrainResData.mGroundSoundIDs.Length;
										}
										num4 = terrainResData.mGroundSoundIDs[num6];
									}
								}
								else
								{
									num4 = terrainResData.mGroundSoundIDs[0];
								}
								AudioManager.instance.Create(vector, num4);
								_lastSoundID = num4;
							}
						}
					}
				}
			}
			reqDecal._curFoot = (reqDecal._curFoot + 1) & 1;
		}
		reqDecal._fpLastLRFootDistance = num;
	}
}

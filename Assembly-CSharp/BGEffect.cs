using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.Effect;
using PETools;
using UnityEngine;

public class BGEffect : MonoBehaviour
{
	private static BGEffect mInstance;

	public float audioRate;

	public float effectRate;

	private GameObject mAudioPoint;

	private GameObject mEffectPoint;

	private List<AudioController> envSounds = new List<AudioController>();

	private List<GameObject> envEffects = new List<GameObject>();

	private List<IntVector2> exists = new List<IntVector2>();

	public static BGEffect Instance => mInstance;

	internal virtual int GetMapID(Vector3 position)
	{
		return -1;
	}

	private IEnumerator Start()
	{
		mInstance = this;
		while (LODOctreeMan.self == null)
		{
			yield return null;
		}
		LODOctreeMan.self.AttachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
		if (mEffectPoint == null)
		{
			mEffectPoint = new GameObject("Effect");
			mEffectPoint.transform.parent = base.transform;
			mEffectPoint.transform.localPosition = Vector3.zero;
		}
		if (mAudioPoint == null)
		{
			mAudioPoint = new GameObject("Audio");
			mAudioPoint.transform.parent = base.transform;
			mAudioPoint.transform.localPosition = Vector3.zero;
		}
	}

	private void Update()
	{
		PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
		if (!(mainPlayer != null))
		{
			return;
		}
		for (int num = envSounds.Count - 1; num >= 0; num--)
		{
			AudioController audioController = envSounds[num];
			if (!(audioController == null))
			{
				float num2 = PEUtil.SqrMagnitude(mainPlayer.position, audioController.transform.position);
				float num3 = audioController.mAudio.maxDistance * 0.97f;
				if (num2 < num3 * num3)
				{
					if (!audioController.mAudio.loop)
					{
						audioController.autoDel = true;
						envSounds.RemoveAt(num);
					}
					audioController.PlayAudio(1f);
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (LODOctreeMan.self != null)
		{
			LODOctreeMan.self.DetachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
		}
	}

	private void OnTerrainColliderCreated(IntVector4 node)
	{
		if (node.w == 0)
		{
			IntVector2 item = new IntVector2(node.x, node.z);
			if (!exists.Contains(item))
			{
				SetupEnvironmentAudio(node);
				SetupEnvironmentEffect(node);
				exists.Add(item);
			}
		}
	}

	private void OnTerrainColliderDestroy(IntVector4 node)
	{
		if (node.w != 0)
		{
			return;
		}
		IntVector2 item = new IntVector2(node.x, node.z);
		if (!exists.Contains(item))
		{
			return;
		}
		List<AudioController> list = envSounds.FindAll((AudioController ret) => Match(ret, node));
		foreach (AudioController item2 in list)
		{
			envSounds.Remove(item2);
			item2.Delete();
		}
		List<GameObject> list2 = envEffects.FindAll((GameObject ret) => MatchEffect(ret, node));
		foreach (GameObject item3 in list2)
		{
			envEffects.Remove(item3);
			Object.Destroy(item3);
		}
		exists.Remove(item);
	}

	private void SetupEnvironmentEffect(IntVector4 node)
	{
		if (Random.value < effectRate)
		{
			SetupEffect(node);
		}
	}

	private void OnEffectSpawned(GameObject effect)
	{
		if (effect != null && !envEffects.Contains(effect))
		{
			envEffects.Add(effect);
		}
	}

	private int CalculateType(Vector3 pos)
	{
		return AiUtil.CheckPositionInCave(pos, 128f, AiUtil.groundedLayer) ? 2 : ((!GameConfig.IsNight) ? 1 : 0);
	}

	private void SetupEffect(IntVector4 node)
	{
		Vector3 vector = node.ToVector3();
		vector += new Vector3(Random.Range(0f, 32 << node.w), 0f, Random.Range(0f, 32 << node.w));
		int num = 32 << node.w;
		if (!Physics.Raycast(vector + Vector3.up * num, Vector3.down, out var hitInfo, num, GameConfig.GroundLayer))
		{
			return;
		}
		int num2 = -1;
		int num3 = -1;
		if (PEUtil.GetWaterSurfaceHeight(hitInfo.point, out var waterHeight))
		{
			if (Random.value < 0.3f)
			{
				num2 = 0;
				vector = new Vector3(hitInfo.point.x, waterHeight, hitInfo.point.z);
			}
			else
			{
				num2 = 1;
				vector = hitInfo.point + Vector3.up * Random.Range(0f, waterHeight - hitInfo.point.y);
			}
		}
		else
		{
			num2 = 2;
			vector = hitInfo.point + Vector3.up * Random.Range(0.5f, 5f);
		}
		num3 = CalculateType(vector);
		int envEffectID = AISpawnDataStory.GetEnvEffectID(GetMapID(vector), num2, num3);
		Transform parent = ((!(mEffectPoint != null)) ? null : mEffectPoint.transform);
		EffectBuilder.EffectRequest effectRequest = Singleton<EffectBuilder>.Instance.Register(envEffectID, null, vector, Quaternion.identity, parent);
		effectRequest.SpawnEvent += OnEffectSpawned;
	}

	private void SetupEnvironmentAudio(IntVector4 node)
	{
		if (Random.value < audioRate)
		{
			SetupAudioController(node);
		}
	}

	private void SetupAudioController(IntVector4 node)
	{
		Vector3 vector = node.ToVector3();
		vector += new Vector3(Random.Range(0f, 32 << node.w), 0f, Random.Range(0f, 32 << node.w));
		int num = 32 << node.w;
		if (Physics.Raycast(vector + Vector3.up * num, Vector3.down, out var hitInfo, num, GameConfig.GroundLayer))
		{
			vector = ((!PEUtil.GetWaterSurfaceHeight(hitInfo.point, out var waterHeight)) ? (hitInfo.point + Vector3.up * Random.Range(0.5f, 5f)) : (hitInfo.point + Vector3.up * Random.Range(0f, waterHeight - hitInfo.point.y)));
			int envMusicID = AISpawnDataStory.GetEnvMusicID(GetMapID(vector));
			Transform parent = ((!(mAudioPoint != null)) ? null : mAudioPoint.transform);
			AudioController item = AudioManager.instance.Create(vector, envMusicID, parent, isPlay: false, isDelete: false);
			envSounds.Add(item);
		}
	}

	private bool Match(AudioController ac, IntVector4 node)
	{
		if (ac == null)
		{
			return false;
		}
		float num = ac.transform.position.x - (float)node.x;
		float num2 = ac.transform.position.z - (float)node.z;
		return num >= float.Epsilon && num <= (float)(32 << node.w) && num2 >= float.Epsilon && num2 <= (float)(32 << node.w) && ac.transform.position.y >= (float)node.y;
	}

	private bool MatchEffect(GameObject ac, IntVector4 node)
	{
		if (ac == null)
		{
			return false;
		}
		float num = ac.transform.position.x - (float)node.x;
		float num2 = ac.transform.position.z - (float)node.z;
		return num >= float.Epsilon && num <= (float)(32 << node.w) && num2 >= float.Epsilon && num2 <= (float)(32 << node.w) && ac.transform.position.y >= (float)node.y;
	}
}

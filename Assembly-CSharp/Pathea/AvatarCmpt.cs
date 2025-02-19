using System;
using System.Collections;
using System.IO;
using AppearBlendShape;
using CustomCharactor;
using PETools;
using UnityEngine;

namespace Pathea;

public class AvatarCmpt : BiologyViewCmpt
{
	private const int VERSION_0000 = 0;

	private const int CURRENT_VERSION = 0;

	private const string MaleModelPath = "Model/PlayerModel/Male";

	private const string FemaleModelPath = "Model/PlayerModel/Female";

	private const string MaleClothes01Path = "Model/PlayerModel/male01";

	private const string FemaleClothes01Path = "Model/PlayerModel/female01";

	private const string MaleModelPrefabPath = "Prefab/PlayerPrefab/MaleModel";

	private const string FemaleModelPrefabPath = "Prefab/PlayerPrefab/FemaleModel";

	private const string MaleAnimatorPrefabPath = "Prefab/PlayerPrefab/MaleM";

	private const string FemaleAnimatorPrefabPath = "Prefab/PlayerPrefab/FemaleM";

	private static GameObject s_preloadMaleAnim;

	private static GameObject s_preloadFemaleAnim;

	private AppearData _appearData;

	private AvatarData _nudeAvatarData;

	private AvatarData _clothedAvatarData = new AvatarData();

	private AvatarData _curAvatarData;

	private bool _isDirty;

	private bool _asynUpdatingSmr;

	private bool NotCustomizable => null == _nudeAvatarData;

	public AppearData apperaData => _appearData;

	public static void CachePrefab()
	{
		AssetsLoader.Instance.LoadPrefabImm("Model/PlayerModel/Male", bIntoCache: true);
		AssetsLoader.Instance.LoadPrefabImm("Model/PlayerModel/Female", bIntoCache: true);
		AssetsLoader.Instance.LoadPrefabImm("Model/PlayerModel/male01", bIntoCache: true);
		AssetsLoader.Instance.LoadPrefabImm("Model/PlayerModel/female01", bIntoCache: true);
		AssetsLoader.Instance.LoadPrefabImm("Prefab/PlayerPrefab/MaleModel", bIntoCache: true);
		AssetsLoader.Instance.LoadPrefabImm("Prefab/PlayerPrefab/FemaleModel", bIntoCache: true);
		s_preloadMaleAnim = UnityEngine.Object.Instantiate(AssetsLoader.Instance.LoadPrefabImm("Prefab/PlayerPrefab/MaleModel")) as GameObject;
		s_preloadMaleAnim.transform.parent = AssetsLoader.Instance.transform;
		s_preloadMaleAnim.SetActive(value: false);
		s_preloadFemaleAnim = UnityEngine.Object.Instantiate(AssetsLoader.Instance.LoadPrefabImm("Prefab/PlayerPrefab/FemaleModel")) as GameObject;
		s_preloadFemaleAnim.transform.parent = AssetsLoader.Instance.transform;
		s_preloadFemaleAnim.SetActive(value: false);
	}

	public bool HasData()
	{
		return null != _appearData;
	}

	public void SetData(AppearData appearData, AvatarData nudeAvatarData)
	{
		_appearData = appearData;
		_nudeAvatarData = nudeAvatarData;
		if (_nudeAvatarData != null)
		{
			LodCmpt lodCmpt = base.Entity.lodCmpt;
			lodCmpt.onConstruct = (Action<PeEntity>)Delegate.Combine(lodCmpt.onConstruct, (Action<PeEntity>)delegate(PeEntity e)
			{
				e.StartCoroutine(PreLoad());
			});
		}
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		int num = r.ReadInt32();
		if (num > 0)
		{
			Debug.LogError("version error");
			return;
		}
		byte[] array = PETools.Serialize.ReadBytes(r);
		byte[] array2 = PETools.Serialize.ReadBytes(r);
		if (array != null && array.Length > 0 && array2 != null && array2.Length > 0)
		{
			AppearData appearData = new AppearData();
			appearData.Deserialize(array);
			AvatarData avatarData = new AvatarData();
			avatarData.Deserialize(array2);
			SetData(appearData, avatarData);
		}
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		w.Write(0);
		byte[] buff = null;
		if (_appearData != null)
		{
			buff = _appearData.Serialize();
		}
		PETools.Serialize.WriteBytes(buff, w);
		byte[] buff2 = null;
		if (_nudeAvatarData != null)
		{
			buff2 = _nudeAvatarData.Serialize();
		}
		PETools.Serialize.WriteBytes(buff2, w);
	}

	public override void AddPart(int partMask, string info)
	{
		foreach (AvatarData.ESlot item in AvatarData.GetSlot(partMask))
		{
			if (!SystemSettingData.Instance.HideHeadgear || (item != AvatarData.ESlot.HairB && item != 0 && item != AvatarData.ESlot.HairT))
			{
				_clothedAvatarData.SetPart(item, info);
			}
		}
		_isDirty = true;
	}

	public override void RemovePart(int partMask)
	{
		foreach (AvatarData.ESlot item in AvatarData.GetSlot(partMask))
		{
			_clothedAvatarData.SetPart(item, null);
		}
		_isDirty = true;
	}

	private IEnumerator PreLoad()
	{
		_curAvatarData = AvatarData.Merge(_clothedAvatarData, _nudeAvatarData);
		foreach (string partInfo in _curAvatarData)
		{
			if (!string.IsNullOrEmpty(partInfo))
			{
				string strModelFilePath = CustomPartInfo.GetModelFilePath(partInfo);
				AssetsLoader.Instance.AddReq(new AssetReq(strModelFilePath));
				yield return new WaitForSeconds(0.2f);
			}
		}
	}

	private string GetSkeleton()
	{
		CommonCmpt cmpt = base.Entity.GetCmpt<CommonCmpt>();
		return (!(null != cmpt) || cmpt.sex != PeSex.Male) ? "Prefab/PlayerPrefab/FemaleModel" : "Prefab/PlayerPrefab/MaleModel";
	}

	protected override void OnBoneLoad(GameObject obj)
	{
		_coroutineMeshProc = UpdateSmrAsync;
		base.OnBoneLoad(obj);
	}

	protected override void OnBoneLoadSync(GameObject modelObject)
	{
		base.OnBoneLoadSync(modelObject);
		_curAvatarData = AvatarData.Merge(_clothedAvatarData, _nudeAvatarData);
		SkinnedMeshRenderer skinnedMeshRenderer = AppearBuilder.Build(modelObject, _appearData, _curAvatarData);
		bool flag = skinnedMeshRenderer.enabled;
		skinnedMeshRenderer.enabled = true;
	}

	public override void Build()
	{
		if (string.IsNullOrEmpty(base.prefabPath) || base.prefabPath.Equals("0"))
		{
			SetViewPath(GetSkeleton());
			ResetAvatarInfo();
		}
		base.Build();
	}

	private IEnumerator UpdateSmrAsync()
	{
		_asynUpdatingSmr = true;
		_curAvatarData = AvatarData.Merge(_clothedAvatarData, _nudeAvatarData);
		IEnumerator eBuildAsync = AppearBuilder.BuildAsync(base.modelTrans.gameObject, _appearData, _curAvatarData);
		while (_asynUpdatingSmr && eBuildAsync.MoveNext())
		{
			yield return 0;
		}
		if (_asynUpdatingSmr)
		{
			SkinnedMeshRenderer smr = AppearBuilder.GetTargetSmr(base.modelTrans.gameObject);
			bool oldState = smr.enabled;
			smr.enabled = true;
			SkinnedMeshRenderer clonedSmr = AppearBuilder.CloneSmr(monoRagdollCtrlr.gameObject, smr);
			clonedSmr.enabled = true;
			monoRagdollCtrlr.SmrBuild(clonedSmr);
			smr.enabled = oldState;
		}
	}

	public void UpdateSmr()
	{
		if (!NotCustomizable && !(null == base.modelTrans) && !(null == monoRagdollCtrlr))
		{
			_asynUpdatingSmr = false;
			_curAvatarData = AvatarData.Merge(_clothedAvatarData, _nudeAvatarData);
			SkinnedMeshRenderer skinnedMeshRenderer = AppearBuilder.Build(base.modelTrans.gameObject, _appearData, _curAvatarData);
			bool flag = skinnedMeshRenderer.enabled;
			skinnedMeshRenderer.enabled = true;
			SkinnedMeshRenderer skinnedMeshRenderer2 = AppearBuilder.CloneSmr(monoRagdollCtrlr.gameObject, skinnedMeshRenderer);
			skinnedMeshRenderer2.enabled = true;
			monoRagdollCtrlr.SmrBuild(skinnedMeshRenderer2);
			skinnedMeshRenderer.enabled = flag;
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (_isDirty)
		{
			AvatarData avatarData = AvatarData.Merge(_clothedAvatarData, _nudeAvatarData);
			if (_curAvatarData != avatarData)
			{
				_curAvatarData = avatarData;
				UpdateSmr();
			}
			_isDirty = false;
		}
	}

	public override GameObject CloneModel()
	{
		if (string.IsNullOrEmpty(base.prefabPath))
		{
			SetViewPath(GetSkeleton());
		}
		return base.CloneModel();
	}

	private void ResetAvatarInfo()
	{
		if (null != base.Entity.commonCmpt && (_nudeAvatarData == null || _nudeAvatarData.IsInvalid()))
		{
			_nudeAvatarData = ((base.Entity.commonCmpt.sex != PeSex.Male) ? CustomCharactor.CustomData.DefaultFemale().nudeAvatarData : CustomCharactor.CustomData.DefaultMale().nudeAvatarData);
		}
	}
}

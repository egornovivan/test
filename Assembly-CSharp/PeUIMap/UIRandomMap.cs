using Pathea;
using PeMap;
using UnityEngine;

namespace PeUIMap;

public class UIRandomMap : UIMap
{
	[SerializeField]
	private UITexture mMapTex;

	private Texture2D mTex2d;

	private Material mTexMat;

	[SerializeField]
	private Color colUnknow;

	[SerializeField]
	private Color colDefault;

	[SerializeField]
	private Color colGrassLand;

	[SerializeField]
	private Color colForest;

	[SerializeField]
	private Color colDesert;

	[SerializeField]
	private Color colRedstone;

	[SerializeField]
	private Color colRainforest;

	[SerializeField]
	private Color colSea;

	[SerializeField]
	private Color colMountain;

	[SerializeField]
	private Color colSwamp;

	[SerializeField]
	private Color colCrater;

	private float offsetScale = 1f;

	[SerializeField]
	private UISprite mSelectMask;

	private int mSelectIndex = -1;

	private MaskTile mSelectMaskTile;

	private Vector2 mapSize => RandomMapConfig.Instance.MapSize;

	private float mapPerPixel => mapSize.x / (base.texSize * mScale);

	private int mTexLen => PeSingleton<MaskTile.Mgr>.Instance.mNumPerSide;

	public override void OnCreate()
	{
		base.OnCreate();
		InitMapTex();
	}

	protected override void InitWindow()
	{
		base.InitWindow();
		mScale = 1f;
		mScaleMin = 1f;
		mMapPosMin = Vector3.zero;
		ChangeScale(0f);
		mScaleMin = 1f * (float)Screen.width / base.texSize;
		mMapPosMin.x = (base.texSize - (float)Screen.width) / 2f;
		mMapPosMin.y = (base.texSize - (float)Screen.height) / 2f;
		mSelectMask.enabled = false;
		mMapPos = GetInitPos();
		mMapPos.x = Mathf.Clamp(mMapPos.x, 0f - mMapPosMin.x, mMapPosMin.x);
		mMapPos.y = Mathf.Clamp(mMapPos.y, 0f - mMapPosMin.y, mMapPosMin.y);
		mMapWnd.transform.localPosition = new Vector3(mMapPos.x, mMapPos.y, -10f);
		mMeatSprite.gameObject.SetActive(value: false);
		mMoneySprite.gameObject.SetActive(value: true);
	}

	protected override void OpenWarpWnd(UIMapLabel label)
	{
		mMeatSprite.gameObject.SetActive(value: false);
		mMoneySprite.gameObject.SetActive(value: true);
		base.OpenWarpWnd(label);
	}

	protected override Vector3 GetUIPos(Vector3 worldPos)
	{
		Vector3 zero = Vector3.zero;
		zero.x = worldPos.x * (base.texSize / mapSize.x);
		zero.y = worldPos.z * (base.texSize / mapSize.y);
		zero.z = -1f;
		return zero;
	}

	protected override Vector3 GetInitPos()
	{
		if (GameUI.Instance.mMainPlayer != null)
		{
			Vector3 position = GameUI.Instance.mMainPlayer.position;
			Vector3 zero = Vector3.zero;
			zero.x = (0f - position.x) * (base.texSize / mapSize.x);
			zero.y = (0f - position.z) * (base.texSize / mapSize.y);
			zero.z = -1f;
			return zero;
		}
		return Vector3.zero;
	}

	protected override float ConvetMToPx(float m)
	{
		return base.texSize / mapSize.x * m;
	}

	protected override Vector3 GetWorldPos(Vector2 mousePos)
	{
		Vector3 zero = Vector3.zero;
		zero.x = (mousePos.x - (float)(Screen.width / 2) - mMapPos.x) * mapPerPixel;
		zero.z = (mousePos.y - (float)(Screen.height / 2) - mMapPos.y) * mapPerPixel;
		zero.y = 0f;
		return zero;
	}

	private void InitMapTex()
	{
		mTex2d = new Texture2D(mTexLen, mTexLen);
		mTex2d.filterMode = FilterMode.Point;
		for (int i = 0; i < mTexLen; i++)
		{
			for (int j = 0; j < mTexLen; j++)
			{
				mTex2d.SetPixel(i, j, colUnknow);
			}
		}
		mTex2d.Apply();
		mMapTex.mainTexture = mTex2d;
		mTexMat = mMapTex.material;
		PeSingleton<MaskTile.Mgr>.Instance.eventor.Subscribe(ReflashMaskTile);
		int mLength = PeSingleton<MaskTile.Mgr>.Instance.mLength;
		int num = PeSingleton<MaskTile.Mgr>.Instance.mNumPerSide * 128;
		offsetScale = (float)num / (float)mLength;
		float num2 = base.texSize * offsetScale;
		mMapTex.transform.localScale = new Vector3(num2, num2, 1f);
	}

	private Color GetMaskTileColor(MaskTileType type)
	{
		return type switch
		{
			MaskTileType.GrassLand => colGrassLand, 
			MaskTileType.Forest => colForest, 
			MaskTileType.Desert => colDesert, 
			MaskTileType.Redstone => colRedstone, 
			MaskTileType.Rainforest => colRainforest, 
			MaskTileType.Sea => colSea, 
			MaskTileType.Mountain => colMountain, 
			MaskTileType.Swamp => colSwamp, 
			MaskTileType.Crater => colCrater, 
			_ => colDefault, 
		};
	}

	private void ReflashMaskTile(object sender, MaskTile.Mgr.Args args)
	{
		if (args.add)
		{
			Color maskTileColor = GetMaskTileColor((MaskTileType)PeSingleton<MaskTile.Mgr>.Instance.Get(args.index).type);
			int forceGroup = PeSingleton<MaskTile.Mgr>.Instance.Get(args.index).forceGroup;
			if (forceGroup == -1)
			{
				mTex2d.SetPixel(args.index % mTexLen, args.index / mTexLen, maskTileColor);
			}
			else
			{
				Color32 forceColor = Singleton<ForceSetting>.Instance.GetForceColor(forceGroup);
				Color color = new Color((float)(int)forceColor.r / 255f, (float)(int)forceColor.g / 255f, (float)(int)forceColor.b / 255f, (float)(int)forceColor.a / 255f);
				mTex2d.SetPixel(args.index % mTexLen, args.index / mTexLen, color);
			}
		}
		else
		{
			mTex2d.SetPixel(args.index % mTexLen, args.index / mTexLen, colUnknow);
		}
		mTex2d.Apply();
	}

	protected override void UIMapBg_OnClick()
	{
		base.UIMapBg_OnClick();
		if (Input.GetMouseButtonUp(0))
		{
			OnSelectMask();
		}
	}

	private void OnSelectMask()
	{
		Vector3 worldPos = GetWorldPos(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
		mSelectIndex = PeSingleton<MaskTile.Mgr>.Instance.GetMapIndex(worldPos);
		MaskTile maskTile = PeSingleton<MaskTile.Mgr>.Instance.Get(mSelectIndex);
		if (maskTile == mSelectMaskTile)
		{
			mSelectMaskTile = null;
			mSelectMask.enabled = false;
			mSelectIndex = -1;
		}
		else
		{
			mSelectMaskTile = maskTile;
			mSelectMask.enabled = true;
			float num = base.texSize / (float)mTexLen;
			mSelectMask.transform.localScale = new Vector3(num, num, 1f);
			float x = ((float)(mSelectIndex % mTexLen - mTexLen / 2) * base.texSize / (float)mTexLen + 0.5f * num) * offsetScale;
			float y = ((float)(mSelectIndex / mTexLen - mTexLen / 2) * base.texSize / (float)mTexLen + 0.5f * num) * offsetScale;
			mSelectMask.transform.localPosition = new Vector3(x, y, 0f);
		}
		mWarpWnd.SetActive(value: false);
	}

	private void OnLockBtn()
	{
		if (mSelectIndex != -1 && mSelectMaskTile != null && null != PlayerNetwork.mainPlayer)
		{
			ServerAdministrator.RequestLockArea(mSelectIndex);
		}
	}

	private void OnUnLockBtn()
	{
		if (mSelectIndex != -1 && mSelectMaskTile != null && null != PlayerNetwork.mainPlayer)
		{
			ServerAdministrator.RequestUnLockArea(mSelectIndex);
		}
	}

	private void OnResetBtn()
	{
		if (mSelectIndex != -1 && mSelectMaskTile != null && null != PlayerNetwork.mainPlayer)
		{
			ServerAdministrator.RequestClearVoxelData(mSelectIndex);
		}
	}

	private void OnResetAllBtn()
	{
		if (mSelectIndex != -1 && null != PlayerNetwork.mainPlayer)
		{
			ServerAdministrator.RequestClearAllVoxelData();
		}
	}
}

using System;
using Pathea;
using UnityEngine;

namespace WhiteCat;

public class Bone2DObjects : MonoBehaviour
{
	private const float lightDamping = 5f;

	private static int[] groupFirst = new int[5] { 0, 1, 4, 12, 0 };

	private static int[] groupLast = new int[5] { 0, 3, 11, 15, 15 };

	private static int[] arrayIndexToBoneGroup = new int[16]
	{
		0, 1, 1, 1, 2, 2, 2, 2, 2, 2,
		2, 2, 3, 3, 3, 3
	};

	private static int[] arrayIndexToBoneIndex = new int[16]
	{
		0, 0, 1, 2, 0, 1, 2, 3, 4, 5,
		6, 7, 0, 1, 2, 3
	};

	private static Color outlineColor1 = new Color(1f, 0.1f, 0f, 1f);

	private static Color outlineColor2 = new Color(1f, 0.9f, 0f, 1f);

	private static Color normalLightColor = new Color(1f, 1f, 1f, 1f);

	private static Color darkLightColor = new Color(0.3f, 0.25f, 0.4f, 1f);

	private static Vector2 viewSize = new Vector2(310f, 480f);

	private static Vector3 minPosition = new Vector3(-0.2f, -0.2f, -0.2f);

	private static Vector3 maxPosition = new Vector3(0.2f, 0.2f, 0.2f);

	private static Vector3 minScale = new Vector3(0.25f, 0.25f, 0.25f);

	private static Vector3 maxScale = new Vector3(2f, 2f, 2f);

	[SerializeField]
	private UIBoneFocus[] _boneUIs;

	[SerializeField]
	private TweenInterpolator _tools;

	[SerializeField]
	private ArmorToolTip _toolTip;

	private Camera _uiCamera;

	private Camera _viewCamera;

	private Light _light;

	private ArmorBones _armorBones;

	private PlayerArmorCmpt _armorCmpt;

	private PeViewController _vc;

	private ArmorType _activeGroup = ArmorType.None;

	private int _highlightIndex = -1;

	private int _mouseDownIndex = -1;

	private bool _armorsVisible;

	private bool _focused;

	private bool _showHandles;

	private bool _isDecoration;

	private Transform _target;

	private OutlineObject _outline;

	private float _outlineStartTime;

	private Vector2 _lastRightButtonDownPosition;

	private int highlightIndex
	{
		set
		{
			if (_highlightIndex != value)
			{
				if (_highlightIndex >= 0)
				{
					_boneUIs[_highlightIndex].highlight = false;
				}
				_highlightIndex = value;
				if (_highlightIndex >= 0)
				{
					_boneUIs[_highlightIndex].highlight = true;
				}
			}
		}
	}

	public bool focused
	{
		set
		{
			if (_focused == value)
			{
				return;
			}
			_focused = value;
			_mouseDownIndex = -1;
			if (value)
			{
				_boneUIs[_highlightIndex].enabled = true;
				_tools.transform.SetParent(_boneUIs[_highlightIndex].transform, worldPositionStays: false);
				_tools.speed = 1f;
				_tools.isPlaying = true;
				BoneNode boneNode = _armorBones.nodes(_highlightIndex);
				SetOutLine((!boneNode.normal) ? boneNode.decoration : boneNode.normal);
			}
			else
			{
				if (_highlightIndex >= 0)
				{
					_boneUIs[_highlightIndex].enabled = false;
				}
				_tools.speed = -1f;
				_tools.isPlaying = true;
				SetOutLine(null);
			}
		}
	}

	private bool isDecoration
	{
		set
		{
			if (_highlightIndex >= 0)
			{
				BoneNode boneNode = _armorBones.nodes(_highlightIndex);
				if ((bool)boneNode.decoration && (bool)boneNode.normal)
				{
					_isDecoration = value;
					SetOutLine((!_isDecoration) ? boneNode.normal : boneNode.decoration);
				}
				else
				{
					_isDecoration = boneNode.decoration;
					SetOutLine((!_isDecoration) ? boneNode.normal : boneNode.decoration);
				}
			}
		}
	}

	public bool dragWindowEnabled
	{
		set
		{
			GameUI.Instance.mUIPlayerInfoCtrl.dragObject.enabled = value;
		}
	}

	private void SetOutLine(Transform target)
	{
		if (!target)
		{
			if ((bool)_outline)
			{
				UnityEngine.Object.Destroy(_outline);
			}
			_outline = null;
			return;
		}
		if ((bool)_outline)
		{
			if (_outline.transform == target)
			{
				return;
			}
			UnityEngine.Object.Destroy(_outline);
		}
		_outline = target.gameObject.AddComponent<OutlineObject>();
		_outline.color = outlineColor1;
		_outlineStartTime = 0f;
	}

	private void SetMoveHandle(Transform target)
	{
		_target = null;
		if ((bool)_vc)
		{
			_target = target;
			_showHandles = target;
			_vc.moveHandle.gameObject.SetActive(_showHandles);
			_vc.moveHandle.Targets.Clear();
			if (_showHandles)
			{
				_vc.moveHandle.Targets.Add(target);
			}
			SetOutLine(target);
		}
	}

	private void SetScaleHandle(Transform target)
	{
		_target = null;
		if ((bool)_vc)
		{
			_target = target;
			_showHandles = target;
			_vc.scaleHandle.gameObject.SetActive(_showHandles);
			_vc.scaleHandle.Targets.Clear();
			if (_showHandles)
			{
				_vc.scaleHandle.Targets.Add(target);
			}
			SetOutLine(target);
		}
	}

	private void SetRotateHandle(Transform target)
	{
		_target = null;
		if ((bool)_vc)
		{
			_target = target;
			_showHandles = target;
			_vc.rotateHandle.gameObject.SetActive(_showHandles);
			_vc.rotateHandle.Targets.Clear();
			if (_showHandles)
			{
				_vc.rotateHandle.Targets.Add(target);
			}
			SetOutLine(target);
		}
	}

	private void OnMoveHandle(Vector3 position)
	{
		if ((bool)_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
		{
			Vector3 localPosition = _target.localPosition;
			localPosition.x = Mathf.Clamp(localPosition.x, minPosition.x, maxPosition.x);
			localPosition.y = Mathf.Clamp(localPosition.y, minPosition.y, maxPosition.y);
			localPosition.z = Mathf.Clamp(localPosition.z, minPosition.z, maxPosition.z);
			_target.localPosition = localPosition;
			_armorCmpt.SetArmorPartPosition(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration, localPosition);
		}
	}

	private void OnRotateHandle(Quaternion rotation)
	{
		if ((bool)_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
		{
			Quaternion localRotation = _target.localRotation;
			_armorCmpt.SetArmorPartRotation(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration, localRotation);
		}
	}

	private void OnScaleHandle(Vector3 scale)
	{
		if ((bool)_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
		{
			Vector3 localScale = _target.localScale;
			localScale.x = Mathf.Clamp(localScale.x, minScale.x, maxScale.x);
			localScale.y = Mathf.Clamp(localScale.y, minScale.y, maxScale.y);
			if (localScale.z > 0f)
			{
				localScale.z = Mathf.Clamp(localScale.z, minScale.z, maxScale.z);
			}
			else
			{
				localScale.z = Mathf.Clamp(localScale.z, 0f - maxScale.z, 0f - minScale.z);
			}
			_target.localScale = localScale;
			_armorCmpt.SetArmorPartScale(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration, localScale);
		}
	}

	public bool GetHoverBone(out int boneGroup, out int boneIndex)
	{
		if (_highlightIndex >= 0)
		{
			boneGroup = arrayIndexToBoneGroup[_highlightIndex];
			boneIndex = arrayIndexToBoneIndex[_highlightIndex];
			return true;
		}
		boneGroup = -1;
		boneIndex = -1;
		return false;
	}

	private void OnEnable()
	{
		HideAll();
	}

	private void OnDisable()
	{
		HideAll();
		if ((bool)_light)
		{
			_light.color = normalLightColor;
		}
	}

	public void Init(PeViewController viewController, GameObject viewModel, PlayerArmorCmpt armorCmpt)
	{
		if (_vc != viewController)
		{
			_vc = viewController;
			_vc.moveHandle.OnTargetMove += OnMoveHandle;
			_vc.rotateHandle.OnTargetRotate += OnRotateHandle;
			_vc.scaleHandle.OnTargetScale += OnScaleHandle;
			_vc.moveHandle.OnBeginTargetMove += delegate
			{
				dragWindowEnabled = false;
			};
			_vc.rotateHandle.OnBeginTargetRotate += delegate
			{
				dragWindowEnabled = false;
			};
			_vc.scaleHandle.OnBeginTargetScale += delegate
			{
				dragWindowEnabled = false;
			};
			_vc.moveHandle.OnEndTargetMove += delegate
			{
				dragWindowEnabled = true;
			};
			_vc.rotateHandle.OnEndTargetRotate += delegate
			{
				dragWindowEnabled = true;
			};
			_vc.scaleHandle.OnEndTargetScale += delegate
			{
				dragWindowEnabled = true;
			};
			if (PeGameMgr.IsMulti)
			{
				_vc.moveHandle.OnEndTargetMove += delegate
				{
					if ((bool)_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
					{
						_armorCmpt.C2S_SyncArmorPartPosition(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration, _target.localPosition);
					}
				};
				_vc.rotateHandle.OnEndTargetRotate += delegate
				{
					if ((bool)_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
					{
						_armorCmpt.C2S_SyncArmorPartRotation(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration, _target.localRotation);
					}
				};
				_vc.scaleHandle.OnEndTargetScale += delegate
				{
					if ((bool)_target && _highlightIndex >= 0 && !_armorCmpt.hasRequest)
					{
						_armorCmpt.C2S_SyncArmorPartScale(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration, _target.localScale);
					}
				};
			}
		}
		_viewCamera = viewController.viewCam;
		_light = _viewCamera.GetComponentInParent<Light>();
		_armorBones = viewModel.GetComponent<ArmorBones>();
		_armorCmpt = armorCmpt;
	}

	public void HideAll()
	{
		SetMoveHandle(null);
		SetScaleHandle(null);
		SetRotateHandle(null);
		focused = false;
		SetArmorsVisible(visible: false);
		SetActiveGroup(ArmorType.None);
		highlightIndex = -1;
	}

	public void SetActiveGroup(ArmorType group)
	{
		if (group == _activeGroup)
		{
			return;
		}
		if (_activeGroup != ArmorType.None)
		{
			for (int i = groupFirst[(int)_activeGroup]; i <= groupLast[(int)_activeGroup]; i++)
			{
				_boneUIs[i].enabled = false;
			}
		}
		_activeGroup = group;
		if (_activeGroup != ArmorType.None)
		{
			for (int j = groupFirst[(int)_activeGroup]; j <= groupLast[(int)_activeGroup]; j++)
			{
				_boneUIs[j].enabled = true;
			}
		}
	}

	private void SetArmorsVisible(bool visible)
	{
		if (visible == _armorsVisible)
		{
			return;
		}
		_armorsVisible = visible;
		if (visible)
		{
			_armorCmpt.ForEachBone(delegate(int boneGroup, int boneIndex, bool hasNormal, bool hasDecoration)
			{
				_boneUIs[groupFirst[boneGroup] + boneIndex].enabled = hasNormal || hasDecoration;
			});
			return;
		}
		for (int i = 0; i < _boneUIs.Length; i++)
		{
			_boneUIs[i].enabled = false;
		}
	}

	public void OnSettingsButtonClick()
	{
		if (!_armorCmpt.hasRequest)
		{
			SetMoveHandle(null);
			SetScaleHandle(null);
			SetRotateHandle(null);
			focused = false;
			SetActiveGroup(ArmorType.None);
			highlightIndex = -1;
			SetArmorsVisible(!_armorsVisible);
		}
	}

	public void OnRemoveArmorButtonClick()
	{
		if (_highlightIndex < 0)
		{
			return;
		}
		Action<bool> action = delegate
		{
			focused = false;
			SetArmorsVisible(visible: true);
		};
		if (PeGameMgr.IsMulti)
		{
			if (!_armorCmpt.hasRequest)
			{
				_armorCmpt.C2S_RemoveArmorPart(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration, action);
			}
		}
		else
		{
			action(_armorCmpt.RemoveArmorPart(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration));
		}
	}

	public void OnMoveArmorButtonClick()
	{
		if (!_armorCmpt.hasRequest && _highlightIndex >= 0)
		{
			focused = false;
			SetMoveHandle(_armorBones.GetArmorPart(_highlightIndex, _isDecoration));
		}
	}

	public void OnRotateArmorButtonClick()
	{
		if (!_armorCmpt.hasRequest && _highlightIndex >= 0)
		{
			focused = false;
			SetRotateHandle(_armorBones.GetArmorPart(_highlightIndex, _isDecoration));
		}
	}

	public void OnScaleArmorButtonClick()
	{
		if (!_armorCmpt.hasRequest && _highlightIndex >= 0)
		{
			focused = false;
			SetScaleHandle(_armorBones.GetArmorPart(_highlightIndex, _isDecoration));
		}
	}

	public void OnMirrorArmorButtonClick()
	{
		if (_highlightIndex < 0)
		{
			return;
		}
		Transform part = _armorBones.GetArmorPart(_highlightIndex, _isDecoration);
		Action<bool> action = delegate(bool s)
		{
			if (s && (bool)part)
			{
				Vector3 localScale = part.localScale;
				localScale.z = 0f - localScale.z;
				part.localScale = localScale;
			}
		};
		if (PeGameMgr.IsMulti)
		{
			if (!_armorCmpt.hasRequest)
			{
				_armorCmpt.C2S_SwitchArmorPartMirror(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration, action);
			}
		}
		else
		{
			action(_armorCmpt.SwitchArmorPartMirror(arrayIndexToBoneGroup[_highlightIndex], arrayIndexToBoneIndex[_highlightIndex], _isDecoration));
		}
	}

	private void LateUpdate()
	{
		if ((bool)_viewCamera && (bool)_armorBones && (bool)_armorCmpt)
		{
			if (_armorCmpt.hasRequest)
			{
				return;
			}
			if (_uiCamera == null)
			{
				_uiCamera = GetComponentInParent<Camera>();
			}
			Vector2 vector = Input.mousePosition - _uiCamera.WorldToScreenPoint(base.transform.position);
			_toolTip.UpdateToolTip(vector);
			bool flag = false;
			if (Input.GetMouseButtonDown(1))
			{
				_lastRightButtonDownPosition = vector;
			}
			if (Input.GetMouseButtonUp(1))
			{
				flag = (_lastRightButtonDownPosition - vector).sqrMagnitude <= 2.1f;
			}
			if (_activeGroup != ArmorType.None)
			{
				bool flag2 = false;
				for (int i = groupFirst[(int)_activeGroup]; i <= groupLast[(int)_activeGroup]; i++)
				{
					_boneUIs[i].UpdatePosition(_armorBones.nodes(i).bone, _viewCamera);
					if (!flag2 && _boneUIs[i].isHover(vector))
					{
						flag2 = true;
						highlightIndex = i;
					}
				}
				if (!flag2)
				{
					highlightIndex = -1;
				}
			}
			else if (_armorsVisible)
			{
				bool flag3 = false;
				for (int j = 0; j < 16; j++)
				{
					if (_boneUIs[j].enabled)
					{
						_boneUIs[j].UpdatePosition(_armorBones.nodes(j).bone, _viewCamera);
						if (!flag3 && _boneUIs[j].isHover(vector))
						{
							flag3 = true;
							highlightIndex = j;
						}
					}
				}
				if (!flag3)
				{
					highlightIndex = -1;
					if (flag)
					{
						OnSettingsButtonClick();
					}
				}
				else
				{
					_boneUIs[_highlightIndex].pressing = Input.GetMouseButton(0);
					if (Input.GetMouseButtonDown(0))
					{
						_mouseDownIndex = _highlightIndex;
					}
					if (Input.GetMouseButtonUp(0) && _mouseDownIndex == _highlightIndex)
					{
						SetArmorsVisible(visible: false);
						isDecoration = false;
						focused = true;
					}
				}
			}
			else if (_focused)
			{
				_boneUIs[_highlightIndex].UpdatePosition(_armorBones.nodes(_highlightIndex).bone, _viewCamera);
				if (_boneUIs[_highlightIndex].isHover(vector))
				{
					_boneUIs[_highlightIndex].pressing = Input.GetMouseButton(0);
					if (Input.GetMouseButtonUp(0))
					{
						isDecoration = !_isDecoration;
					}
				}
				else if (flag)
				{
					OnSettingsButtonClick();
				}
			}
			else if (_showHandles)
			{
				Vector3 customMousePosition = new Vector3(vector.x * (float)_viewCamera.targetTexture.width / viewSize.x, vector.y * (float)_viewCamera.targetTexture.height / viewSize.y);
				_vc.moveHandle.customMousePosition = customMousePosition;
				_vc.rotateHandle.customMousePosition = customMousePosition;
				_vc.scaleHandle.customMousePosition = customMousePosition;
				if (flag)
				{
					SetMoveHandle(null);
					SetRotateHandle(null);
					SetScaleHandle(null);
					focused = true;
				}
			}
			if ((_focused || _showHandles) && (bool)_outline)
			{
				_outlineStartTime += Time.unscaledDeltaTime;
				_outlineStartTime %= 1f;
				_outline.color = Interpolation.Parabolic(_outlineStartTime) * (outlineColor2 - outlineColor1) + outlineColor1;
			}
		}
		if ((bool)_light)
		{
			if (_activeGroup != ArmorType.None || _armorsVisible || _showHandles || _focused)
			{
				_light.color = Color.Lerp(_light.color, darkLightColor, Time.unscaledDeltaTime * 5f);
			}
			else
			{
				_light.color = Color.Lerp(_light.color, normalLightColor, Time.unscaledDeltaTime * 5f);
			}
		}
	}
}

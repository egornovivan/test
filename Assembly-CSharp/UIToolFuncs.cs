using ItemAsset;
using Pathea;
using UnityEngine;

public static class UIToolFuncs
{
	public enum InScreenOffsetMode
	{
		OffsetBounds,
		OffsetLimit
	}

	public static bool CanEquip(ItemObject item, PeSex targetSex)
	{
		if (item == null)
		{
			return false;
		}
		Equip cmpt = item.GetCmpt<Equip>();
		if (cmpt == null)
		{
			return false;
		}
		if (cmpt.equipPos == 0)
		{
			return false;
		}
		if (!PeGender.IsMatch(cmpt.sex, targetSex))
		{
			return false;
		}
		return true;
	}

	public static void SetTransInScreenByMousePos(this Transform trans, InScreenOffsetMode offsetMode = InScreenOffsetMode.OffsetBounds)
	{
		Vector3 toPos = Input.mousePosition;
		toPos = new Vector3(toPos.x - (float)(Screen.width / 2), toPos.y - (float)(Screen.height / 2), trans.localPosition.z);
		Vector2 posInScreenByPos = trans.GetPosInScreenByPos(toPos, offsetMode);
		trans.localPosition = new Vector3(posInScreenByPos.x, posInScreenByPos.y, trans.localPosition.z);
	}

	public static Vector2 GetPosInScreenByPos(this Transform trans, Vector3 toPos, InScreenOffsetMode offsetMode)
	{
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(trans);
		float num = Screen.width / 2;
		float num2 = Screen.height / 2;
		float num3 = toPos.x + bounds.size.x;
		float num4 = toPos.y - bounds.size.y;
		switch (offsetMode)
		{
		case InScreenOffsetMode.OffsetBounds:
			if (num3 < 0f - num)
			{
				toPos.x += bounds.size.x;
			}
			else if (num3 > num)
			{
				toPos.x -= bounds.size.x;
			}
			if (num4 < 0f - num2)
			{
				toPos.y += bounds.size.y;
			}
			else if (num4 > num2)
			{
				toPos.y -= bounds.size.y;
			}
			break;
		case InScreenOffsetMode.OffsetLimit:
			toPos.x = Mathf.Clamp(num3, 0f - num, num);
			toPos.y = Mathf.Clamp(num4, 0f - num2, num2);
			break;
		}
		return toPos;
	}
}

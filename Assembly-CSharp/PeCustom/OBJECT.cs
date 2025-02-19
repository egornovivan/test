namespace PeCustom;

public struct OBJECT
{
	public enum OBJECTTYPE
	{
		Null,
		AnyObject,
		Player,
		WorldObject,
		ItemProto,
		MonsterProto
	}

	public OBJECTTYPE type;

	public int Group;

	public int Id;

	public static OBJECT Null
	{
		get
		{
			OBJECT result = default(OBJECT);
			result.type = OBJECTTYPE.Null;
			return result;
		}
	}

	public static OBJECT Any
	{
		get
		{
			OBJECT result = default(OBJECT);
			result.type = OBJECTTYPE.AnyObject;
			result.Group = -1;
			result.Id = -1;
			return result;
		}
	}

	public bool isSpecificEntity => isPlayerId || isNpoId;

	public bool isPlayerId => type == OBJECTTYPE.Player && Id >= 0 && Group == 0;

	public bool isForceId => type == OBJECTTYPE.Player && Id == -1 && Group >= 0;

	public bool isCurrentPlayer => type == OBJECTTYPE.Player && Id == 0 && Group == -1;

	public bool isAnyPlayer => type == OBJECTTYPE.Player && Id == -1 && Group == -1;

	public bool isAnyOtherPlayer => type == OBJECTTYPE.Player && Id == -2 && Group == -1;

	public bool isAnyOtherForce => type == OBJECTTYPE.Player && Id == -1 && Group == -2;

	public bool isAllyForce => type == OBJECTTYPE.Player && Id == -1 && Group == -3;

	public bool isEnemyForce => type == OBJECTTYPE.Player && Id == -1 && Group == -4;

	public bool isNpoId => type == OBJECTTYPE.WorldObject && Id > 0 && Group >= 0;

	public bool isAnyNpo => type == OBJECTTYPE.WorldObject && Id == -1 && Group == -1;

	public bool isAnyNpoInSpecificWorld => type == OBJECTTYPE.WorldObject && Id == -1 && Group >= 0;

	public bool isPrototype => type == OBJECTTYPE.ItemProto || type == OBJECTTYPE.MonsterProto;

	public bool isAnyPrototype => isPrototype && Id == -1 && Group == -1;

	public bool isAnyPrototypeInCategory => isPrototype && Id == -1 && Group >= 0;

	public bool isSpecificPrototype => isPrototype && Id >= 0;
}

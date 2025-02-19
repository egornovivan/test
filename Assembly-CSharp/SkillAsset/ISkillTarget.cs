using UnityEngine;

namespace SkillAsset;

public interface ISkillTarget
{
	ESkillTargetType GetTargetType();

	Vector3 GetPosition();
}

using UnityEngine;

public class PlayerScoreItem_N : MonoBehaviour
{
	public UILabel mName;

	public UILabel mKillAndDead;

	public UILabel mScore;

	public void SetInfo(string name, int killNum, int deadNum, int score)
	{
		mName.text = name;
		mKillAndDead.text = killNum + " / " + deadNum;
		mScore.text = score.ToString();
	}
}

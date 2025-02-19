using UnityEngine;

public class Sentence
{
	public class Speaker
	{
		public enum EType
		{
			Player,
			Npc,
			Monster,
			Alien,
			Max
		}

		public EType mType;

		public int mId;

		public override string ToString()
		{
			return $"Speaker[{mType},{mId}]";
		}
	}

	public class Mgr
	{
		private static Mgr instance;

		public static Mgr Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Mgr();
				}
				return instance;
			}
		}

		public void Show(Sentence sentence)
		{
			Debug.Log(sentence);
		}

		public void Close()
		{
		}
	}

	public Speaker mSpeaker;

	public string mText;

	public string mAnimation;

	public int mSoundId;

	public override string ToString()
	{
		return $"Sentence[{mSpeaker},{mText},{mAnimation},{mSoundId}]";
	}
}

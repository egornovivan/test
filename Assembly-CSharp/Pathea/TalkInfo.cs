namespace Pathea;

public class TalkInfo
{
	public float mStartTime;

	public float mLoopTime;

	public ETalkLevel mLevel;

	public TalkInfo(float time, float loopTime, ETalkLevel level)
	{
		mStartTime = time;
		mLevel = level;
		mLoopTime = loopTime;
	}
}

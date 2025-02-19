using System.Collections.Generic;
using System.IO;
using PatheaScript;

namespace PatheaScriptExt;

public class PeConversation : Action
{
	private List<Sentence> mList;

	private int mCurIndex;

	public override bool Parse()
	{
		mList = new List<Sentence>(5);
		for (int i = 0; i < 10; i++)
		{
			Sentence sentence = new Sentence();
			sentence.mAnimation = "talk " + i;
			sentence.mSoundId = 123 + i;
			sentence.mText = "sentence " + i;
			sentence.mSpeaker = new Sentence.Speaker();
			sentence.mSpeaker.mId = i;
			sentence.mSpeaker.mType = Sentence.Speaker.EType.Npc;
			mList.Add(sentence);
		}
		return true;
	}

	protected override bool OnInit()
	{
		if (!base.OnInit())
		{
			return false;
		}
		mCurIndex = 0;
		PeEventMgr.Instance.SubscribeEvent(PeEventMgr.EEventType.MouseClicked, MouseEventHandler);
		return true;
	}

	protected override TickResult OnTick()
	{
		if (base.OnTick() == TickResult.Finished)
		{
			return TickResult.Finished;
		}
		if (mList == null || mCurIndex >= mList.Count)
		{
			return TickResult.Finished;
		}
		return TickResult.Running;
	}

	protected override void OnReset()
	{
		base.OnReset();
		PeEventMgr.Instance.UnsubscribeEvent(PeEventMgr.EEventType.MouseClicked, MouseEventHandler);
		ShowCurSentence();
	}

	private void MouseEventHandler(PeEventMgr.EventArg arg)
	{
		if (arg is PeEventMgr.MouseEvent { mType: PeEventMgr.MouseEvent.Type.LeftClicked })
		{
			ShowNextSentence();
		}
	}

	private bool ShowCurSentence()
	{
		if (mCurIndex >= mList.Count)
		{
			Sentence.Mgr.Instance.Close();
			PeEventMgr.Instance.EmitEvent(PeEventMgr.EEventType.Conversation, null);
			return false;
		}
		Sentence.Mgr.Instance.Show(mList[mCurIndex]);
		return true;
	}

	private void ShowNextSentence()
	{
		mCurIndex++;
		if (ShowCurSentence())
		{
		}
	}

	public override void Store(BinaryWriter w)
	{
		base.Store(w);
		w.Write(mCurIndex);
	}

	public override void Restore(BinaryReader r)
	{
		base.Restore(r);
		mCurIndex = r.ReadInt32();
		ShowCurSentence();
	}
}

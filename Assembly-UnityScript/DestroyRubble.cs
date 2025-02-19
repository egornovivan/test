using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Boo.Lang;
using UnityEngine;
using UnityScript.Lang;

[Serializable]
public class DestroyRubble : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class _0024Start_002425 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class _0024 : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal int _0024i_002426;

			internal DestroyRubble _0024self__002427;

			public _0024(DestroyRubble self_)
			{
				_0024self__002427 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					result = (Yield(2, new WaitForSeconds(_0024self__002427.time)) ? 1 : 0);
					break;
				case 2:
					_0024i_002426 = 0;
					goto IL_0095;
				case 3:
					UnityEngine.Object.Destroy(_0024self__002427.gameObject);
					_0024i_002426++;
					goto IL_0095;
				case 1:
					{
						result = 0;
						break;
					}
					IL_0095:
					if (_0024i_002426 < Extensions.get_length((System.Array)_0024self__002427.particleEmitters))
					{
						_0024self__002427.particleEmitters[_0024i_002426].emit = false;
						result = (Yield(3, new WaitForSeconds(_0024self__002427.maxTime)) ? 1 : 0);
						break;
					}
					YieldDefault(1);
					goto case 1;
				}
				return (byte)result != 0;
			}
		}

		internal DestroyRubble _0024self__002428;

		public _0024Start_002425(DestroyRubble self_)
		{
			_0024self__002428 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new _0024(_0024self__002428);
		}
	}

	public float maxTime;

	public ParticleEmitter[] particleEmitters;

	public float time;

	public DestroyRubble()
	{
		maxTime = 3f;
	}

	public virtual IEnumerator Start()
	{
		return new _0024Start_002425(this).GetEnumerator();
	}

	public virtual void Main()
	{
	}
}

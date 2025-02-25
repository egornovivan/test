using System;
using System.Threading;

namespace Pathfinding;

public class ThreadControlQueue
{
	public class QueueTerminationException : Exception
	{
	}

	private Path head;

	private Path tail;

	private object lockObj = new object();

	private int numReceivers;

	private bool blocked;

	private int blockedReceivers;

	private bool starving;

	private bool terminate;

	private ManualResetEvent block = new ManualResetEvent(initialState: true);

	public bool IsEmpty => head == null;

	public bool IsTerminating => terminate;

	public bool AllReceiversBlocked => blocked && blockedReceivers == numReceivers;

	public ThreadControlQueue(int numReceivers)
	{
		this.numReceivers = numReceivers;
	}

	public void Block()
	{
		lock (lockObj)
		{
			blocked = true;
			block.Reset();
		}
	}

	public void Unblock()
	{
		lock (lockObj)
		{
			blocked = false;
			block.Set();
		}
	}

	public void Lock()
	{
		Monitor.Enter(lockObj);
	}

	public void Unlock()
	{
		Monitor.Exit(lockObj);
	}

	public void PushFront(Path p)
	{
		if (terminate)
		{
			return;
		}
		lock (lockObj)
		{
			if (tail == null)
			{
				head = p;
				tail = p;
				if (starving && !blocked)
				{
					starving = false;
					block.Set();
				}
				else
				{
					starving = false;
				}
			}
			else
			{
				p.next = head;
				head = p;
			}
		}
	}

	public void Push(Path p)
	{
		if (terminate)
		{
			return;
		}
		lock (lockObj)
		{
			if (tail == null)
			{
				head = p;
				tail = p;
				if (starving && !blocked)
				{
					starving = false;
					block.Set();
				}
				else
				{
					starving = false;
				}
			}
			else
			{
				tail.next = p;
				tail = p;
			}
		}
	}

	private void Starving()
	{
		starving = true;
		block.Reset();
	}

	public void TerminateReceivers()
	{
		terminate = true;
		block.Set();
	}

	public Path Pop()
	{
		Monitor.Enter(lockObj);
		try
		{
			if (terminate)
			{
				blockedReceivers++;
				throw new QueueTerminationException();
			}
			if (head == null)
			{
				Starving();
			}
			while (blocked || starving)
			{
				blockedReceivers++;
				if (terminate)
				{
					throw new QueueTerminationException();
				}
				if (blockedReceivers != numReceivers && blockedReceivers > numReceivers)
				{
					throw new InvalidOperationException("More receivers are blocked than specified in constructor (" + blockedReceivers + " > " + numReceivers + ")");
				}
				Monitor.Exit(lockObj);
				block.WaitOne();
				Monitor.Enter(lockObj);
				blockedReceivers--;
				if (head == null)
				{
					Starving();
				}
			}
			Path result = head;
			if (head.next == null)
			{
				tail = null;
			}
			head = head.next;
			return result;
		}
		finally
		{
			Monitor.Exit(lockObj);
		}
	}

	public void ReceiverTerminated()
	{
		Monitor.Enter(lockObj);
		blockedReceivers++;
		Monitor.Exit(lockObj);
	}

	public Path PopNoBlock(bool blockedBefore)
	{
		Monitor.Enter(lockObj);
		try
		{
			if (terminate)
			{
				blockedReceivers++;
				throw new QueueTerminationException();
			}
			if (head == null)
			{
				Starving();
			}
			if (blocked || starving)
			{
				if (!blockedBefore)
				{
					blockedReceivers++;
					if (terminate)
					{
						throw new QueueTerminationException();
					}
					if (blockedReceivers != numReceivers && blockedReceivers > numReceivers)
					{
						throw new InvalidOperationException("More receivers are blocked than specified in constructor (" + blockedReceivers + " > " + numReceivers + ")");
					}
				}
				return null;
			}
			if (blockedBefore)
			{
				blockedReceivers--;
			}
			Path result = head;
			if (head.next == null)
			{
				tail = null;
			}
			head = head.next;
			return result;
		}
		finally
		{
			Monitor.Exit(lockObj);
		}
	}
}

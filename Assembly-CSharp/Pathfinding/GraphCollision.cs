using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using UnityEngine;

namespace Pathfinding;

[Serializable]
public class GraphCollision
{
	public const float RaycastErrorMargin = 0.005f;

	public ColliderType type = ColliderType.Capsule;

	public float diameter = 1f;

	public float height = 2f;

	public float collisionOffset;

	public RayDirection rayDirection = RayDirection.Both;

	public LayerMask mask;

	public LayerMask heightMask = -1;

	public float fromHeight = 100f;

	public bool thickRaycast;

	public float thickRaycastDiameter = 1f;

	public bool unwalkableWhenNoGround = true;

	public bool use2D;

	public bool collisionCheck = true;

	public bool heightCheck = true;

	public Vector3 up;

	private Vector3 upheight;

	private float finalRadius;

	private float finalRaycastRadius;

	public void Initialize(Matrix4x4 matrix, float scale)
	{
		up = matrix.MultiplyVector(Vector3.up);
		upheight = up * height;
		finalRadius = diameter * scale * 0.5f;
		finalRaycastRadius = thickRaycastDiameter * scale * 0.5f;
	}

	public bool Check(Vector3 position)
	{
		if (!collisionCheck)
		{
			return true;
		}
		if (use2D)
		{
			return type switch
			{
				ColliderType.Capsule => throw new Exception("Capsule mode cannot be used with 2D since capsules don't exist in 2D. Please change the Physics Testing -> Collider Type setting."), 
				ColliderType.Sphere => Physics2D.OverlapCircle(position, finalRadius, mask) == null, 
				_ => Physics2D.OverlapPoint(position, mask) == null, 
			};
		}
		position += up * collisionOffset;
		return type switch
		{
			ColliderType.Capsule => !Physics.CheckCapsule(position, position + upheight, finalRadius, mask), 
			ColliderType.Sphere => !Physics.CheckSphere(position, finalRadius, mask), 
			_ => rayDirection switch
			{
				RayDirection.Both => !Physics.Raycast(position, up, height, mask) && !Physics.Raycast(position + upheight, -up, height, mask), 
				RayDirection.Up => !Physics.Raycast(position, up, height, mask), 
				_ => !Physics.Raycast(position + upheight, -up, height, mask), 
			}, 
		};
	}

	public Vector3 CheckHeight(Vector3 position)
	{
		RaycastHit hit;
		bool walkable;
		return CheckHeight(position, out hit, out walkable);
	}

	public Vector3 CheckHeight(Vector3 position, out RaycastHit hit, out bool walkable)
	{
		walkable = true;
		if (!heightCheck || use2D)
		{
			hit = default(RaycastHit);
			return position;
		}
		if (thickRaycast)
		{
			Vector3 vector = position + up * fromHeight;
			Vector3 vector2 = -up;
			if (Physics.SphereCast(vector, finalRaycastRadius, vector2, out hit, fromHeight + 0.005f, heightMask))
			{
				return AstarMath.NearestPointDir(vector, vector2, hit.point);
			}
			if (unwalkableWhenNoGround)
			{
				walkable = false;
			}
		}
		else
		{
			if (Physics.Raycast(position + up * fromHeight, -up, out hit, fromHeight + 0.005f, heightMask))
			{
				return hit.point;
			}
			if (unwalkableWhenNoGround)
			{
				walkable = false;
			}
		}
		return position;
	}

	public Vector3 Raycast(Vector3 origin, out RaycastHit hit, out bool walkable)
	{
		walkable = true;
		if (!heightCheck || use2D)
		{
			hit = default(RaycastHit);
			return origin - up * fromHeight;
		}
		if (thickRaycast)
		{
			Ray ray = new Ray(origin, -up);
			if (Physics.SphereCast(ray, finalRaycastRadius, out hit, fromHeight + 0.005f, heightMask))
			{
				return AstarMath.NearestPoint(ray.origin, ray.origin + ray.direction, hit.point);
			}
			if (unwalkableWhenNoGround)
			{
				walkable = false;
			}
		}
		else
		{
			if (Physics.Raycast(origin, -up, out hit, fromHeight + 0.005f, heightMask))
			{
				return hit.point;
			}
			if (unwalkableWhenNoGround)
			{
				walkable = false;
			}
		}
		return origin - up * fromHeight;
	}

	public RaycastHit[] CheckHeightAll(Vector3 position)
	{
		if (!heightCheck || use2D)
		{
			RaycastHit raycastHit = default(RaycastHit);
			raycastHit.point = position;
			raycastHit.distance = 0f;
			return new RaycastHit[1] { raycastHit };
		}
		if (thickRaycast)
		{
			Debug.LogWarning("Thick raycast cannot be used with CheckHeightAll. Disabling thick raycast...");
			thickRaycast = false;
		}
		List<RaycastHit> list = new List<RaycastHit>();
		bool walkable = true;
		Vector3 vector = position + up * fromHeight;
		Vector3 vector2 = Vector3.zero;
		int num = 0;
		while (true)
		{
			Raycast(vector, out var hit, out walkable);
			if (hit.transform == null)
			{
				break;
			}
			if (hit.point != vector2 || list.Count == 0)
			{
				vector = hit.point - up * 0.005f;
				vector2 = hit.point;
				num = 0;
				list.Add(hit);
				continue;
			}
			vector -= up * 0.001f;
			num++;
			if (num <= 10)
			{
				continue;
			}
			Debug.LogError(string.Concat("Infinite Loop when raycasting. Please report this error (arongranberg.com)\n", vector, " : ", vector2));
			break;
		}
		return list.ToArray();
	}

	public void SerializeSettings(GraphSerializationContext ctx)
	{
		ctx.writer.Write((int)type);
		ctx.writer.Write(diameter);
		ctx.writer.Write(height);
		ctx.writer.Write(collisionOffset);
		ctx.writer.Write((int)rayDirection);
		ctx.writer.Write(mask);
		ctx.writer.Write(heightMask);
		ctx.writer.Write(fromHeight);
		ctx.writer.Write(thickRaycast);
		ctx.writer.Write(thickRaycastDiameter);
		ctx.writer.Write(unwalkableWhenNoGround);
		ctx.writer.Write(use2D);
		ctx.writer.Write(collisionCheck);
		ctx.writer.Write(heightCheck);
	}

	public void DeserializeSettings(GraphSerializationContext ctx)
	{
		type = (ColliderType)ctx.reader.ReadInt32();
		diameter = ctx.reader.ReadSingle();
		height = ctx.reader.ReadSingle();
		collisionOffset = ctx.reader.ReadSingle();
		rayDirection = (RayDirection)ctx.reader.ReadInt32();
		mask = ctx.reader.ReadInt32();
		heightMask = ctx.reader.ReadInt32();
		fromHeight = ctx.reader.ReadSingle();
		thickRaycast = ctx.reader.ReadBoolean();
		thickRaycastDiameter = ctx.reader.ReadSingle();
		unwalkableWhenNoGround = ctx.reader.ReadBoolean();
		use2D = ctx.reader.ReadBoolean();
		collisionCheck = ctx.reader.ReadBoolean();
		heightCheck = ctx.reader.ReadBoolean();
	}
}

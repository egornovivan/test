using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding;

[RequireComponent(typeof(CharacterController))]
public class LocalAvoidance : MonoBehaviour
{
	public enum ResolutionType
	{
		Sampled,
		Geometric
	}

	public struct VOLine
	{
		public VO vo;

		public Vector3 start;

		public Vector3 end;

		public bool inf;

		public int id;

		public bool wrongSide;

		public VOLine(VO vo, Vector3 start, Vector3 end, bool inf, int id, bool wrongSide)
		{
			this.vo = vo;
			this.start = start;
			this.end = end;
			this.inf = inf;
			this.id = id;
			this.wrongSide = wrongSide;
		}
	}

	public struct VOIntersection
	{
		public VO vo1;

		public VO vo2;

		public float factor1;

		public float factor2;

		public bool inside;

		public VOIntersection(VO vo1, VO vo2, float factor1, float factor2, bool inside = false)
		{
			this.vo1 = vo1;
			this.vo2 = vo2;
			this.factor1 = factor1;
			this.factor2 = factor2;
			this.inside = inside;
		}
	}

	public class HalfPlane
	{
		public Vector3 point;

		public Vector3 normal;

		public bool Contains(Vector3 p)
		{
			p -= point;
			return Vector3.Dot(normal, p) >= 0f;
		}

		public Vector3 ClosestPoint(Vector3 p)
		{
			p -= point;
			Vector3 vector = Vector3.Cross(normal, Vector3.up);
			float num = Vector3.Dot(vector, p);
			return point + vector * num;
		}

		public Vector3 ClosestPoint(Vector3 p, float minX, float maxX)
		{
			p -= point;
			Vector3 vector = Vector3.Cross(normal, Vector3.up);
			if (vector.x < 0f)
			{
				vector = -vector;
			}
			float value = Vector3.Dot(vector, p);
			float min = (minX - point.x) / vector.x;
			float max = (maxX - point.x) / vector.x;
			value = Mathf.Clamp(value, min, max);
			return point + vector * value;
		}

		public Vector3 Intersection(HalfPlane hp)
		{
			Vector3 dir = Vector3.Cross(normal, Vector3.up);
			Vector3 dir2 = Vector3.Cross(hp.normal, Vector3.up);
			return Polygon.IntersectionPointOptimized(point, dir, hp.point, dir2);
		}

		public void DrawBounds(float left, float right)
		{
			Vector3 vector = Vector3.Cross(normal, Vector3.up);
			if (vector.x < 0f)
			{
				vector = -vector;
			}
			float num = (left - point.x) / vector.x;
			float num2 = (right - point.x) / vector.x;
			Debug.DrawLine(point + vector * num + Vector3.up * 0.1f, point + vector * num2 + Vector3.up * 0.1f, Color.yellow);
		}

		public void Draw()
		{
			Vector3 vector = Vector3.Cross(normal, Vector3.up);
			Debug.DrawLine(point - vector * 10f, point + vector * 10f, Color.blue);
			Debug.DrawRay(point, normal, new Color(0.8f, 0.1f, 0.2f));
		}
	}

	public enum IntersectionState
	{
		Inside,
		Outside,
		Enter,
		Exit
	}

	public struct IntersectionPair : IComparable<IntersectionPair>
	{
		public float factor;

		public IntersectionState state;

		public IntersectionPair(float factor, bool inside)
		{
			this.factor = factor;
			state = ((!inside) ? IntersectionState.Outside : IntersectionState.Inside);
		}

		public void SetState(IntersectionState s)
		{
			state = s;
		}

		public int CompareTo(IntersectionPair o)
		{
			if (o.factor < factor)
			{
				return 1;
			}
			if (o.factor > factor)
			{
				return -1;
			}
			return 0;
		}
	}

	public class VO
	{
		public Vector3 origin;

		public Vector3 direction;

		public float angle;

		public float limit;

		public Vector3 pLeft;

		public Vector3 pRight;

		public Vector3 nLeft;

		public Vector3 nRight;

		public List<IntersectionPair> ints1 = new List<IntersectionPair>();

		public List<IntersectionPair> ints2 = new List<IntersectionPair>();

		public List<IntersectionPair> ints3 = new List<IntersectionPair>();

		public void AddInt(float factor, bool inside, int id)
		{
			switch (id)
			{
			case 1:
				ints1.Add(new IntersectionPair(factor, inside));
				break;
			case 2:
				ints2.Add(new IntersectionPair(factor, inside));
				break;
			case 3:
				ints3.Add(new IntersectionPair(factor, inside));
				break;
			}
		}

		public bool FinalInts(Vector3 target, Vector3 closeEdgeConstraint, bool drawGizmos, out Vector3 closest)
		{
			ints1.Sort();
			ints2.Sort();
			ints3.Sort();
			float num = (float)Math.Atan2(direction.z, direction.x);
			Vector3 vector = Vector3.Cross(direction, Vector3.up);
			Vector3 vector2 = vector * (float)Math.Tan(angle) * limit;
			Vector3 vector3 = origin + direction * limit + vector2;
			Vector3 vector4 = origin + direction * limit - vector2;
			Vector3 vector5 = vector3 + new Vector3((float)Math.Cos(num + angle), 0f, (float)Math.Sin(num + angle)) * 100f;
			Vector3 vector6 = vector4 + new Vector3((float)Math.Cos(num - angle), 0f, (float)Math.Sin(num - angle)) * 100f;
			bool flag = false;
			closest = Vector3.zero;
			int num2 = ((!(Vector3.Dot(closeEdgeConstraint - origin, vector) > 0f)) ? 1 : 2);
			for (int i = 1; i <= 3; i++)
			{
				if (i == num2)
				{
					continue;
				}
				List<IntersectionPair> list = i switch
				{
					1 => ints1, 
					2 => ints2, 
					_ => ints3, 
				};
				Vector3 vector7 = ((i != 1 && i != 3) ? vector4 : vector3);
				Vector3 vector8 = i switch
				{
					1 => vector5, 
					2 => vector6, 
					_ => vector4, 
				};
				float num3 = AstarMath.NearestPointFactor(vector7, vector8, target);
				float num4 = float.PositiveInfinity;
				float num5 = float.NegativeInfinity;
				bool flag2 = false;
				for (int j = 0; j < list.Count - ((i == 3) ? 1 : 0); j++)
				{
					if (drawGizmos)
					{
						Debug.DrawRay(vector7 + (vector8 - vector7) * list[j].factor, Vector3.down, (list[j].state != IntersectionState.Outside) ? Color.red : Color.green);
					}
					if (list[j].state == IntersectionState.Outside && ((j == list.Count - 1 && (j == 0 || list[j - 1].state != IntersectionState.Outside)) || (j < list.Count - 1 && list[j + 1].state == IntersectionState.Outside)))
					{
						flag2 = true;
						float factor = list[j].factor;
						float num6 = ((j != list.Count - 1) ? list[j + 1].factor : ((i != 3) ? float.PositiveInfinity : 1f));
						if (drawGizmos)
						{
							Debug.DrawLine(vector7 + (vector8 - vector7) * factor + Vector3.up, vector7 + (vector8 - vector7) * Mathf.Clamp01(num6) + Vector3.up, Color.green);
						}
						if (factor <= num3 && num6 >= num3)
						{
							num4 = num3;
							num5 = num3;
							break;
						}
						if (num6 < num3 && num6 > num5)
						{
							num5 = num6;
						}
						else if (factor > num3 && factor < num4)
						{
							num4 = factor;
						}
					}
				}
				if (flag2)
				{
					float num7 = ((num4 == float.NegativeInfinity) ? num5 : ((num5 == float.PositiveInfinity) ? num4 : ((!(Mathf.Abs(num3 - num4) < Mathf.Abs(num3 - num5))) ? num5 : num4)));
					Vector3 vector9 = vector7 + (vector8 - vector7) * num7;
					if (!flag || (vector9 - target).sqrMagnitude < (closest - target).sqrMagnitude)
					{
						closest = vector9;
					}
					if (drawGizmos)
					{
						Debug.DrawLine(target, closest, Color.yellow);
					}
					flag = true;
				}
			}
			return flag;
		}

		public bool Contains(Vector3 p)
		{
			return Vector3.Dot(nLeft, p - origin) > 0f && Vector3.Dot(nRight, p - origin) > 0f && Vector3.Dot(direction, p - origin) > limit;
		}

		public float ScoreContains(Vector3 p)
		{
			return 0f;
		}

		public void Draw(Color c)
		{
			float num = (float)Math.Atan2(direction.z, direction.x);
			Vector3 vector = Vector3.Cross(direction, Vector3.up) * (float)Math.Tan(angle) * limit;
			Debug.DrawLine(origin + direction * limit + vector, origin + direction * limit - vector, c);
			Debug.DrawRay(origin + direction * limit + vector, new Vector3((float)Math.Cos(num + angle), 0f, (float)Math.Sin(num + angle)) * 10f, c);
			Debug.DrawRay(origin + direction * limit - vector, new Vector3((float)Math.Cos(num - angle), 0f, (float)Math.Sin(num - angle)) * 10f, c);
		}

		public static explicit operator HalfPlane(VO vo)
		{
			HalfPlane halfPlane = new HalfPlane();
			halfPlane.point = vo.origin + vo.direction * vo.limit;
			halfPlane.normal = -vo.direction;
			return halfPlane;
		}
	}

	public const float Rad2Deg = 57.29578f;

	private const int maxVOCounter = 50;

	public float speed = 2f;

	public float delta = 1f;

	public float responability = 0.5f;

	public ResolutionType resType = ResolutionType.Geometric;

	private Vector3 velocity;

	public float radius = 0.5f;

	public float maxSpeedScale = 1.5f;

	public Vector3[] samples;

	public float sampleScale = 1f;

	public float circleScale = 0.5f;

	public float circlePoint = 0.5f;

	public bool drawGizmos;

	protected CharacterController controller;

	protected LocalAvoidance[] agents;

	private Vector3 preVelocity;

	private List<VO> vos = new List<VO>();

	private void Start()
	{
		controller = GetComponent<CharacterController>();
		agents = UnityEngine.Object.FindObjectsOfType(typeof(LocalAvoidance)) as LocalAvoidance[];
	}

	public void Update()
	{
		SimpleMove(base.transform.forward * speed);
	}

	public Vector3 GetVelocity()
	{
		return preVelocity;
	}

	public void LateUpdate()
	{
		preVelocity = velocity;
	}

	public void SimpleMove(Vector3 desiredMovement)
	{
		Vector3 vector = UnityEngine.Random.insideUnitSphere * 0.1f;
		vector.y = 0f;
		Vector3 vector2 = ClampMovement(desiredMovement + vector);
		if (vector2 != Vector3.zero)
		{
			vector2 /= delta;
		}
		if (drawGizmos)
		{
			Debug.DrawRay(base.transform.position, desiredMovement, Color.magenta);
			Debug.DrawRay(base.transform.position, vector2, Color.yellow);
			Debug.DrawRay(base.transform.position + vector2, Vector3.up, Color.yellow);
		}
		controller.SimpleMove(vector2);
		velocity = controller.velocity;
		Debug.DrawRay(base.transform.position, velocity, Color.blue);
	}

	public Vector3 ClampMovement(Vector3 direction)
	{
		Vector3 vector = direction * delta;
		Vector3 vector2 = base.transform.position + direction;
		Vector3 vector3 = vector2;
		float num = 0f;
		int num2 = 0;
		vos.Clear();
		float magnitude = velocity.magnitude;
		LocalAvoidance[] array = agents;
		foreach (LocalAvoidance localAvoidance in array)
		{
			if (localAvoidance == this || localAvoidance == null)
			{
				continue;
			}
			Vector3 vector4 = localAvoidance.transform.position - base.transform.position;
			float magnitude2 = vector4.magnitude;
			float num3 = radius + localAvoidance.radius;
			if (!(magnitude2 > vector.magnitude * delta + num3 + magnitude + localAvoidance.GetVelocity().magnitude) && num2 <= 50)
			{
				num2++;
				VO vO = new VO();
				vO.origin = base.transform.position + Vector3.Lerp(velocity * delta, localAvoidance.GetVelocity() * delta, responability);
				vO.direction = vector4.normalized;
				if (num3 > vector4.magnitude)
				{
					vO.angle = (float)Math.PI / 2f;
				}
				else
				{
					vO.angle = (float)Math.Asin(num3 / magnitude2);
				}
				vO.limit = magnitude2 - num3;
				if (vO.limit < 0f)
				{
					vO.origin += vO.direction * vO.limit;
					vO.limit = 0f;
				}
				float num4 = Mathf.Atan2(vO.direction.z, vO.direction.x);
				vO.pRight = new Vector3(Mathf.Cos(num4 + vO.angle), 0f, Mathf.Sin(num4 + vO.angle));
				vO.pLeft = new Vector3(Mathf.Cos(num4 - vO.angle), 0f, Mathf.Sin(num4 - vO.angle));
				vO.nLeft = new Vector3(Mathf.Cos(num4 + vO.angle - (float)Math.PI / 2f), 0f, Mathf.Sin(num4 + vO.angle - (float)Math.PI / 2f));
				vO.nRight = new Vector3(Mathf.Cos(num4 - vO.angle + (float)Math.PI / 2f), 0f, Mathf.Sin(num4 - vO.angle + (float)Math.PI / 2f));
				vos.Add(vO);
			}
		}
		if (resType == ResolutionType.Geometric)
		{
			for (int j = 0; j < vos.Count; j++)
			{
				if (vos[j].Contains(vector3))
				{
					num = float.PositiveInfinity;
					if (drawGizmos)
					{
						Debug.DrawRay(vector3, Vector3.down, Color.red);
					}
					vector3 = base.transform.position;
					break;
				}
			}
			if (drawGizmos)
			{
				for (int k = 0; k < vos.Count; k++)
				{
					vos[k].Draw(Color.black);
				}
			}
			if (num == 0f)
			{
				return vector;
			}
			List<VOLine> list = new List<VOLine>();
			for (int l = 0; l < vos.Count; l++)
			{
				VO vO2 = vos[l];
				float num5 = (float)Math.Atan2(vO2.direction.z, vO2.direction.x);
				Vector3 vector5 = vO2.origin + new Vector3((float)Math.Cos(num5 + vO2.angle), 0f, (float)Math.Sin(num5 + vO2.angle)) * vO2.limit;
				Vector3 vector6 = vO2.origin + new Vector3((float)Math.Cos(num5 - vO2.angle), 0f, (float)Math.Sin(num5 - vO2.angle)) * vO2.limit;
				Vector3 end = vector5 + new Vector3((float)Math.Cos(num5 + vO2.angle), 0f, (float)Math.Sin(num5 + vO2.angle)) * 100f;
				Vector3 end2 = vector6 + new Vector3((float)Math.Cos(num5 - vO2.angle), 0f, (float)Math.Sin(num5 - vO2.angle)) * 100f;
				int num6 = (Polygon.Left(vO2.origin, vO2.origin + vO2.direction, base.transform.position + velocity) ? 1 : 2);
				list.Add(new VOLine(vO2, vector5, end, inf: true, 1, num6 == 1));
				list.Add(new VOLine(vO2, vector6, end2, inf: true, 2, num6 == 2));
				list.Add(new VOLine(vO2, vector5, vector6, inf: false, 3, wrongSide: false));
				bool flag = false;
				bool flag2 = false;
				if (!flag)
				{
					for (int m = 0; m < vos.Count; m++)
					{
						if (m != l && vos[m].Contains(vector5))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag2)
				{
					for (int n = 0; n < vos.Count; n++)
					{
						if (n != l && vos[n].Contains(vector6))
						{
							flag2 = true;
							break;
						}
					}
				}
				vO2.AddInt(0f, flag, 1);
				vO2.AddInt(0f, flag2, 2);
				vO2.AddInt(0f, flag, 3);
				vO2.AddInt(1f, flag2, 3);
			}
			for (int num7 = 0; num7 < list.Count; num7++)
			{
				for (int num8 = num7 + 1; num8 < list.Count; num8++)
				{
					VOLine vOLine = list[num7];
					VOLine vOLine2 = list[num8];
					if (vOLine.vo == vOLine2.vo || !Polygon.IntersectionFactor(vOLine.start, vOLine.end, vOLine2.start, vOLine2.end, out var factor, out var factor2) || factor < 0f || factor2 < 0f || (!vOLine.inf && factor > 1f) || (!vOLine2.inf && factor2 > 1f))
					{
						continue;
					}
					Vector3 p = vOLine.start + (vOLine.end - vOLine.start) * factor;
					bool flag3 = vOLine.wrongSide || vOLine2.wrongSide;
					if (!flag3)
					{
						for (int num9 = 0; num9 < vos.Count; num9++)
						{
							if (vos[num9] != vOLine.vo && vos[num9] != vOLine2.vo && vos[num9].Contains(p))
							{
								flag3 = true;
								break;
							}
						}
					}
					vOLine.vo.AddInt(factor, flag3, vOLine.id);
					vOLine2.vo.AddInt(factor2, flag3, vOLine2.id);
					if (drawGizmos)
					{
						Debug.DrawRay(vOLine.start + (vOLine.end - vOLine.start) * factor, Vector3.up, (!flag3) ? Color.green : Color.magenta);
					}
				}
			}
			for (int num10 = 0; num10 < vos.Count; num10++)
			{
				if (!vos[num10].FinalInts(vector2, base.transform.position + velocity, drawGizmos, out var closest))
				{
					continue;
				}
				float sqrMagnitude = (closest - vector2).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					vector3 = closest;
					num = sqrMagnitude;
					if (drawGizmos)
					{
						Debug.DrawLine(vector2 + Vector3.up, vector3 + Vector3.up, Color.red);
					}
				}
			}
			if (drawGizmos)
			{
				Debug.DrawLine(vector2 + Vector3.up, vector3 + Vector3.up, Color.red);
			}
			return Vector3.ClampMagnitude(vector3 - base.transform.position, vector.magnitude * maxSpeedScale);
		}
		if (resType == ResolutionType.Sampled)
		{
			Vector3 vector7 = vector;
			Vector3 normalized = vector7.normalized;
			Vector3 vector8 = Vector3.Cross(normalized, Vector3.up);
			int num11 = 10;
			int num12 = 0;
			while (num12 < 10)
			{
				float num13 = (float)(Math.PI * (double)circlePoint / (double)num11);
				float num14 = (float)(Math.PI - (double)circlePoint * Math.PI) * 0.5f;
				for (int num15 = 0; num15 < num11; num15++)
				{
					float num16 = num13 * (float)num15;
					Vector3 vector9 = base.transform.position + vector - (vector7 * (float)Math.Sin(num16 + num14) * num12 * circleScale + vector8 * (float)Math.Cos(num16 + num14) * num12 * circleScale);
					if (CheckSample(vector9, vos))
					{
						return vector9 - base.transform.position;
					}
				}
				num12++;
				num11 += 2;
			}
			for (int num17 = 0; num17 < samples.Length; num17++)
			{
				Vector3 vector10 = base.transform.position + samples[num17].x * vector8 + samples[num17].z * normalized + samples[num17].y * vector7;
				if (CheckSample(vector10, vos))
				{
					return vector10 - base.transform.position;
				}
			}
			return Vector3.zero;
		}
		return Vector3.zero;
	}

	public bool CheckSample(Vector3 sample, List<VO> vos)
	{
		bool flag = false;
		for (int i = 0; i < vos.Count; i++)
		{
			if (vos[i].Contains(sample))
			{
				if (drawGizmos)
				{
					Debug.DrawRay(sample, Vector3.up, Color.red);
				}
				flag = true;
				break;
			}
		}
		if (drawGizmos && !flag)
		{
			Debug.DrawRay(sample, Vector3.up, Color.yellow);
		}
		return !flag;
	}
}

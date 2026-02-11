using System.Collections.Generic;
using UnityEngine;

public class Eigen
{
	bool isRotation;
	float[] diagonal = new float[3];
	float[,] matrix = new float[3, 3];
	float[] subDiagonal = new float[3];
	public Vector3 Origin;

	void MakeEnergyMatrix(bool isLinear, List<Vector3> points)
	{
		Origin = Vector3.zero;
		foreach (var v in points) Origin += v;
		Origin /= points.Count;

		float xx = 0f, yy = 0f, zz = 0f, xy = 0f, xz = 0f, yz = 0f;
		foreach (var v in points)
		{
			Vector3 p = v - Origin;
			xx += p.x * p.x; yy += p.y * p.y; zz += p.z * p.z;
			xy += p.x * p.y; xz += p.x * p.z; yz += p.y * p.z;
		}

		if (isLinear)
		{
			float d = xx + yy + zz;
			matrix[0, 0] = d - xx; matrix[1, 1] = d - yy; matrix[2, 2] = d - zz;
			matrix[0, 1] = matrix[1, 0] = -xy;
			matrix[0, 2] = matrix[2, 0] = -xz;
			matrix[1, 2] = matrix[2, 1] = -yz;
		}
		else
		{
			matrix[0, 0] = xx; matrix[1, 1] = yy; matrix[2, 2] = zz;
			matrix[0, 1] = matrix[1, 0] = xy;
			matrix[0, 2] = matrix[2, 0] = xz;
			matrix[1, 2] = matrix[2, 1] = yz;
		}
	}

	void MakeTriDiagonal()
	{
		float a = matrix[0, 0], b = matrix[0, 1], c = matrix[0, 2];
		float d = matrix[1, 1], e = matrix[1, 2], f = matrix[2, 2];

		diagonal[0] = a;
		subDiagonal[2] = 0f;
		if (c != 0f)
		{
			float length = Mathf.Sqrt(b * b + c * c);
			b /= length; c /= length;
			float q = 2f * b * e + c * (f - d);
			diagonal[1] = d + c * q; diagonal[2] = f - c * q;
			subDiagonal[0] = length; subDiagonal[1] = e - b * q;
			matrix[0, 0] = matrix[0, 1] = matrix[1, 0] = matrix[2, 0] = 0f;
			matrix[0, 2] = 1f; matrix[1, 1] = b; matrix[1, 2] = c; matrix[2, 1] = c; matrix[2, 2] = -b;
			isRotation = false;
		}
		else
		{
			diagonal[1] = d; diagonal[2] = f;
			subDiagonal[0] = b; subDiagonal[1] = e;
			matrix[0, 0] = matrix[1, 1] = matrix[2, 2] = 1f;
			matrix[0, 1] = matrix[0, 2] = matrix[1, 0] = matrix[1, 2] = matrix[2, 0] = matrix[2, 1] = 0f;
			isRotation = true;
		}
	}

	bool QlAlgorithm()
	{
		const int maxIter = 32;
		for (int i0 = 0; i0 < 3; ++i0)
		{
			int i1;
			for (i1 = 0; i1 < maxIter; ++i1)
			{
				int i2;
				for (i2 = i0; i2 <= 1; ++i2)
					if (Mathf.Abs(subDiagonal[i2]) + Mathf.Abs(diagonal[i2]) + Mathf.Abs(diagonal[i2 + 1]) == Mathf.Abs(diagonal[i2]) + Mathf.Abs(diagonal[i2 + 1]))
						break;
				if (i2 == i0) break;

				float g = (diagonal[i0 + 1] - diagonal[i0]) / (2f * subDiagonal[i0]);
				float r = Mathf.Sqrt(g * g + 1f);
				g = diagonal[i2] - diagonal[i0] + subDiagonal[i0] / (g < 0f ? g - r : g + r);

				float sin = 1f, cos = 1f, p = 0f;
				for (int i3 = i2 - 1; i3 >= i0; --i3)
				{
					float f = sin * subDiagonal[i3], b = cos * subDiagonal[i3];
					if (Mathf.Abs(f) >= Mathf.Abs(g))
					{
						cos = g / f; r = Mathf.Sqrt(cos * cos + 1f);
						subDiagonal[i3 + 1] = f * r; sin = 1f / r; cos *= sin;
					}
					else
					{
						sin = f / g; r = Mathf.Sqrt(sin * sin + 1f);
						subDiagonal[i3 + 1] = g * r; cos = 1f / r; sin *= cos;
					}
					g = diagonal[i3 + 1] - p;
					r = (diagonal[i3] - g) * sin + 2f * b * cos;
					p = sin * r; diagonal[i3 + 1] = g + p; g = cos * r - b;
					for (int i4 = 0; i4 < 3; ++i4)
					{
						f = matrix[i4, i3 + 1];
						matrix[i4, i3 + 1] = sin * matrix[i4, i3] + cos * f;
						matrix[i4, i3] = cos * matrix[i4, i3] - sin * f;
					}
				}
				diagonal[i0] -= p; subDiagonal[i0] = g; subDiagonal[i2] = 0f;
			}
			if (i1 == maxIter) return false;
		}
		return true;
	}

	void DecreasingSort()
	{
		for (int i0 = 0; i0 <= 1; ++i0)
		{
			int i1 = i0;
			float max = diagonal[i1];
			for (int i2 = i0 + 1; i2 < 3; ++i2)
				if (diagonal[i2] > max) { i1 = i2; max = diagonal[i1]; }

			if (i1 != i0)
			{
				isRotation = !isRotation;
				diagonal[i1] = diagonal[i0]; diagonal[i0] = max;
				for (int i2 = 0; i2 < 3; ++i2)
					(matrix[i2, i0], matrix[i2, i1]) = (matrix[i2, i1], matrix[i2, i0]);
			}
		}
	}

	void Solve()
	{
		MakeTriDiagonal();
		QlAlgorithm();
		DecreasingSort();
	}

	public Vector3 Up => new(matrix[0, 2], matrix[1, 2], matrix[2, 2]);
	public Vector3 Right => new(matrix[0, 1], matrix[1, 1], matrix[2, 1]);
	public Vector3 Forward => new(matrix[0, 0], matrix[1, 0], matrix[2, 0]);
	public float Planarity => 1f - diagonal[2] / diagonal[0];

	public void SolvePlaneFor(List<Vector3> points) { MakeEnergyMatrix(false, points); Solve(); }
	public void SolveLineFor(List<Vector3> points) { MakeEnergyMatrix(true, points); Solve(); }
	public void SetPlane(ref Plane plane) => plane.SetNormalAndPosition(Up, Origin);
}

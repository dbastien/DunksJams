using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates efficient gaussian blur kernels suitable for pixel shaders
/// takes advantage of bilinear filtering to do more with less.
/// I think I got the idea from here: https://www.rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/
/// </summary>
public class GaussianBlurKernelGeneratorWindow : EditorWindow 
{
	float _sigma = 1f;
	int _radius = 2;
	string _result = string.Empty;	

	[MenuItem("‽/Gaussian Blur Kernel Generator")]
	public static void Init() => GetWindow<GaussianBlurKernelGeneratorWindow>().Show();

	void OnGUI()
	{
		GUILayout.Label("Gaussian Blur Kernel Generator");
		_sigma = EditorGUILayout.FloatField(new GUIContent("Sigma", "Controls the blur intensity."), _sigma);
		_radius = EditorGUILayout.IntField(new GUIContent("Radius", "Defines the blur kernel size."), _radius);	

		if (GUILayout.Button("Generate"))
		{
			_result = string.Empty;
			var mtx = GaussianMatrix1DHalf(_sigma, _radius);

			_result += FormatArray(mtx) + "\n";

			GaussianMatrix1DHalfToLinearOptimized(mtx, out float[] optWeights, out float[] optOffsets);
			optWeights.NormalizeHalfMatrix();

			_result += $"weights: {{{FormatArray(optWeights)}}}\n";
			_result += $"offsets: {{{FormatArray(optOffsets)}}}\n";

			mtx = mtx.ExpandHalfMatrixToFull();
			_result += FormatArray(mtx) + "\n";

			mtx.NormalizeMatrix();
			_result += FormatArray(mtx) + "\n";
		}

		EditorGUILayout.TextArea(_result);
	}

	string FormatArray(float[] arr) => string.Join(" , ", arr);

	void GaussianMatrix1DHalfToLinearOptimized(float[] wIn, out float[] wOut, out float[] offsets)
	{
		int lOut = (wIn.Length + 1) / 2;
		wOut = new float[lOut];
		offsets = new float[lOut];
		wOut[0] = wIn[0];
		offsets[0] = 0f;

		for (int i = 1, o = 1; o < lOut; ++o, i += 2)
		{		
 			wOut[o] = wIn[i] + wIn[i + 1];
			offsets[o] = (i * wIn[i] + (i + 1) * wIn[i + 1]) / wOut[o];
		}
	}
	
	float[] GaussianMatrix1DHalf(float sigma, int radius)
	{
		var res = new float[radius + 1];
		for (var x = 0; x <= radius; ++x) res[x] = Gaussian(sigma, x);
		return res;
	}

	float Gaussian(float sigma, float x) => MathF.Exp(-x * x / (2 * sigma * sigma)) / (sigma * MathConsts.TauSqrt);
}

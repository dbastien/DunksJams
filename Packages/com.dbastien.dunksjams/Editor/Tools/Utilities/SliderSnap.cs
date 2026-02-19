using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public static class SliderSnap
{
    static readonly float[] Pow10 = { 1f, 10f, 100f, 1000f, 10000f, 100000f, 1000000f };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Snap(float value, int decimals)
    {
        decimals = Mathf.Clamp(decimals, 0, Pow10.Length - 1);
        float factor = Pow10[decimals];
        return Mathf.Round(value * factor) / factor;
    }

    public static void InstallSnap(Slider slider, int decimals, System.Action<float> onSnapped = null)
    {
        slider.SetValueWithoutNotify(Snap(slider.value, decimals));
        slider.onValueChanged.AddListener(v =>
        {
            float snapped = Snap(v, decimals);
            if (snapped != v)
                slider.SetValueWithoutNotify(snapped);

            onSnapped?.Invoke(snapped);
        });
    }
}
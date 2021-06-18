// https://www.youtube.com/watch?v=BLfNP4Sc_iA&ab_channel=Brackeys

using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetMaxHealth(int health)
	{
        slider.maxValue = health;
        slider.value = health;

        fill.color = gradient.Evaluate(1f);
	}

    public void SetHealth(int health)
	{
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);
	}
}

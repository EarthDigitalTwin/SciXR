using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorHSV
{

	public float h;
	public float s;
	public float v;
	public float a;

	public ColorHSV(float h, float s, float v, float a)
	{
		this.h = h;
		this.s = s;
		this.v = v;
		this.a = a;
	}


	public static ColorHSV LerpHSV (ColorHSV a, ColorHSV b, float t)
	{
		// Hue interpolation
		float h = 0;
		float d = b.h - a.h;
		if (a.h > b.h) {
			// Swap (a.h, b.h)
			float h3 = b.h;
			b.h = a.h;
			a.h = h3;

			d = -d;
			t = 1 - t;
		}

		if (d > 0.5) {
			a.h = a.h + 1;
			h = ( a.h + t * (b.h - a.h) ) % 1;
		}
		if (d <= 0.5) {
			h = a.h + t * d;
		}

		// Interpolates the rest
		ColorHSV colorHSV = new ColorHSV (
				h,						// H
				a.s + t * (b.s-a.s),	// S
				a.v + t * (b.v-a.v),	// V
				a.v + t * (b.v-a.v)		// A
			);
		return colorHSV;
	}

}
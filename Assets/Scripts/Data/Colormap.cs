using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colormap {
	public enum Type {
		standard,
		rainbow,
		blue,
		none
	}
	public enum Method {
		linear,
		log
	}

	public Type type;
	public Method method;
	public float min, max;
	public ColorHSV min_color, max_color;
	public string name = "default";
	public List<Color> rgbColors = new List<Color> ();

	//TODO: Do the texture lookup route
	//public Texture2D texture;

	public Colormap(float min, float max, Type type, Method method = Method.linear) {
		this.min = min;
		this.max = max;
		this.type = type;
		this.method = method;

		switch (type) {
		case Type.standard:
			this.min_color = new ColorHSV (0.6f, 1f, 1f, 1f);
			this.max_color = new ColorHSV (1f, 1f, 1f, 1f);
			break;
		case Type.rainbow:
			this.min_color = new ColorHSV (0f, 1f, 1f, 1f);
			this.max_color = new ColorHSV (1f, 1f, 1f, 1f);
			break;
		case Type.blue:
			this.min_color = new ColorHSV (0.5f, 1f, 1f, 1f);
			this.max_color = new ColorHSV (0.7f, 1f, 1f, 1f);
			break;
		case Type.none:
			this.min_color = new ColorHSV (1f, 1f, 1f, 1f);
			this.max_color = new ColorHSV (1f, 1f, 1f, 1f);
			break;
		}
	}

	public Colormap(float min, float max, ColorHSV min_color, ColorHSV max_color, Method method = Method.linear) {
		this.min = min;
		this.max = max;
		this.type = Type.none;
		this.method = method;
		this.min_color = min_color;
		this.max_color = max_color;
	}

	public Colormap(float min, float max, Colorbar colorbar, Method method = Method.linear) {
		this.min = min;
		this.max = max;
		this.name = colorbar.name;
		this.method = method;

		float min_h, min_s, min_v;
		float max_h, max_s, max_v;
		Color min_rgb = colorbar.rgbColors [0];
		Color max_rgb = colorbar.rgbColors [colorbar.rgbColors.Count-1];
		Color.RGBToHSV (min_rgb, out min_h, out min_s, out min_v);
		Color.RGBToHSV (max_rgb, out max_h, out max_s, out max_v);
		this.min_color = new ColorHSV (min_h, min_s, min_v, 1f);
		this.max_color = new ColorHSV (max_h, max_s, max_v, 1f);
		this.rgbColors = colorbar.rgbColors;
	}

	public Color ValueToColor(float val) {
		Color color = Color.white;
		float normalized_val;
		if (method == Method.log) {
			normalized_val = Mathf.InverseLerp (Mathf.Log10(min), Mathf.Log10(max), Mathf.Log10(val));
		} else {
			normalized_val = Mathf.InverseLerp (min, max, val);
		}

		if (this.rgbColors.Count > 0) {
			int bin_min = Mathf.FloorToInt ((this.rgbColors.Count-1) * normalized_val);
			int bin_max = Mathf.CeilToInt ((this.rgbColors.Count-1) * normalized_val);
			Color min_rgb = this.rgbColors [bin_min];
			Color max_rgb = this.rgbColors [bin_max];

			float min_h, min_s, min_v;
			float max_h, max_s, max_v;
			Color.RGBToHSV (min_rgb, out min_h, out min_s, out min_v);
			Color.RGBToHSV (max_rgb, out max_h, out max_s, out max_v);

			float bin_normalized_val = ((this.rgbColors.Count - 1) * normalized_val) - bin_min;

//			color = Color.Lerp (min_rgb, max_rgb, bin_normalized_val);
			ColorHSV colorHSV = ColorHSV.LerpHSV(new ColorHSV(min_h, min_s, min_v, 1f), new ColorHSV(max_h, max_s, max_v, 1f), bin_normalized_val);
			color = Color.HSVToRGB (colorHSV.h, colorHSV.s, colorHSV.v);
		} else {
			// Use hue only if not RGB color
			color = Color.HSVToRGB (Mathf.Lerp (min_color.h, max_color.h, normalized_val), 1, 1);
		}

		return color;
	}

	public Color ValueToColor_old(float val) {
		Color color = Color.white;
		float normalized_val;
		if (method == Method.log) {
			normalized_val = Mathf.InverseLerp (Mathf.Log10(min), Mathf.Log10(max), Mathf.Log10(val));
		} else {
			normalized_val = Mathf.InverseLerp (min, max, val);
		}
		color = Color.HSVToRGB (Mathf.Lerp (min_color.h, max_color.h, normalized_val), 1, 1);
		return color;
	}

	public float ColorToValue(Color color) {
		float h, s, v;
		Color.RGBToHSV (color, out h, out s, out v);
		float normalized_val;
		float final_val;
		float min_val = this.min;
		float max_val = this.max;

		if (this.rgbColors.Count > 0) {
			float new_h = h / max / (this.rgbColors.Count - 1);
			Color min_rgb = Color.black;
			Color max_rgb = Color.white;
			// find where this colors belongs between two colors in the colorbar
			for (int i = 0; i < this.rgbColors.Count-1; i++) {
				if (color.r >= this.rgbColors [i].r && color.g >= this.rgbColors [i].g && color.b >= this.rgbColors [i].b && color.r < this.rgbColors [i + 1].r && color.g < this.rgbColors [i + 1].g && color.b < this.rgbColors [i + 1].b) {
					min_rgb = this.rgbColors [i];
					max_rgb = this.rgbColors [i+1];
				}
			}

			float min_h, min_s, min_v;
			float max_h, max_s, max_v;
			Color.RGBToHSV (min_rgb, out min_h, out min_s, out min_v);
			Color.RGBToHSV (max_rgb, out max_h, out max_s, out max_v);
			normalized_val = Mathf.InverseLerp (min_h, max_h, new_h);

		} else {
			normalized_val = Mathf.InverseLerp (min_color.h, max_color.h, h);
		}

		if (method == Method.log) {
			final_val = Mathf.Pow (10, Mathf.Lerp (Mathf.Log10 (min_val), Mathf.Log10 (max_val), normalized_val));
		} else {
			final_val = Mathf.Lerp (min_val, max_val, normalized_val);
		}
		return final_val;
	}

	public float ColorToValue_old(Color color) {
		float h, s, v;
		Color.RGBToHSV (color, out h, out s, out v);
		float normalized_val = Mathf.InverseLerp (min_color.h, max_color.h, h);
		float final_val;
		if (method == Method.log) {
			final_val = Mathf.Pow (10, Mathf.Lerp (Mathf.Log10 (min), Mathf.Log10 (max), normalized_val));
		} else {
			final_val = Mathf.Lerp (min, max, normalized_val);
		}
		return final_val;
	}
}

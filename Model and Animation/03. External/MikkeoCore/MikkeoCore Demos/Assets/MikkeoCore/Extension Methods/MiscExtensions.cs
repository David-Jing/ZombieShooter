using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mikkeo.Extensions {

	public static class MiscExtensions {


		/*
		 * Animation
		 */
		public static void SetSpeed (this Animation anim, float newSpeed) {
			anim [anim.clip.name].speed = newSpeed;
		}

		/*
		 * Colour
		 */
		public static Color WithAlpha (this Color color, float alpha) {
			return new Color (color.r, color.g, color.b, alpha);
		}
			
		//Usage: Color color = 0xFFDFD991.ToColor();
		/*
		public static Color FromUInt (this uint argb) {
			return Color.Moo ((byte)((argb & -16777216) >> 0x18),      
				(byte)((argb & 0xff0000) >> 0x10),   
				(byte)((argb & 0xff00) >> 8),
				(byte)(argb & 0xff));
		}

		//public static Color FromArgb (this int alpha, int red, int green, int blue) {
		public static Color Moo (this int alpha, int red, int green, int blue) {
			return new Color (((float)red) / 255.0f, ((float)green) / 255.0f, ((float)blue) / 255.0f, ((float)alpha) / 255.0f);
		}
		*/


		
		/*
		 * SpriteRenderer
		 */
		public static float GetSpriteWidth (this SpriteRenderer spriteRenderer) {
			return spriteRenderer.bounds.extents.x * 2f;
		}

		public static float GetSpriteHeight (this SpriteRenderer spriteRenderer) {
			return spriteRenderer.bounds.extents.y * 2f;
		}


		/*Randomizing a List<T>*/
		public static void Shuffle<T> (this IList<T> list) {
			int n = list.Count;
			System.Random rnd = new System.Random ();
			while (n > 1) {
				int k = (rnd.Next (0, n) % n);
				n--;
				T value = list [k];
				list [k] = list [n];
				list [n] = value;
			}
		}

		/* String to enum */
		public static TEnum ToEnum<TEnum> (this string strEnumValue, TEnum defaultValue) {
			if (!Enum.IsDefined (typeof(TEnum), strEnumValue))
				return defaultValue;

			return (TEnum)Enum.Parse (typeof(TEnum), strEnumValue);
		}

	}
		
}
//Namespace MikkeoExtensions
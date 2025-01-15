using System.Linq;
using UnityEngine;

namespace ChoosyPreset.UIElements
{
	internal class Helpers
	{
		public static void ChkMouseClick(Rect windowRect)
		{
			if ((Input.mouseScrollDelta.y != 0 || Input.GetMouseButtonUp(0)) && IsMouseOnGUI(windowRect))
			{
				Input.ResetInputAxes();
			}
		}

		public static bool IsMouseOnGUI(Rect windowRect)
		{
			var point = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			return windowRect.Contains(point);
		}

		public static Texture2D MakeTex(int width, int height, Color col)
		{
			var pix = new Color[width * height];
			for (var i = 0; i < pix.Length; ++i)
			{
				pix[i] = col;
			}
			var result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

		public static Texture2D MakeTexWithRoundedCorner(Color col)
		{
			var xy = 12;
			{
				var result = new Texture2D(xy, xy);
				for (var i = 0; i < xy; ++i)
				{
					for (var j = 0; j < xy; j++)
					{
						var topLeft = (i == 0 && (j == 0 || j == 1)) || (j == 0 && (i == 0 || i == 1));
						var bottomLeft = (i == 0 && (j == (xy - 1) || j == (xy - 2))) || (j == (xy - 1) && (i == 0 || i == 1));
						var topRight = (i == (xy - 1) && (j == 0 || j == 1)) || (j == 0 && (i == (xy - 1) || i == (xy - 2)));
						var bottomRight = (i == (xy - 1) && (j == (xy - 1) || j == (xy - 2))) || (j == (xy - 1) && (i == (xy - 1) || i == (xy - 2)));

						//Corner
						if (topLeft || topRight || bottomLeft || bottomRight)
						{
							result.SetPixels(i, j, 1, 1, new[] { new Color(0, 0, 0, 0) });
						}
						//Border
						else if (i == 0 || j == 0 || i == (xy - 1) || j == (xy - 1) ||
								(i == 1 && j == 1) || (i == (xy - 2) && j == 1) || (i == 1 && j == (xy - 2)) || (i == (xy - 2) && j == (xy - 2)))
						{
							result.SetPixels(i, j, 1, 1, new[] { Color.black });
						}
						//Normal
						else
						{
							result.SetPixels(i, j, 1, 1, new[] { col });
						}
					}
				}

				result.Apply();
				return result;
			}
		}

		public class P
		{
			public int x { get; set; }
			public int y { get; set; }

			public P(int x_, int y_)
			{
				x = x_;
				y = y_;
			}
		}

		public static Texture2D MakeWindowTex(Color col, Color col2)
		{
			var x = 17;
			var y = 27;
			var nulls = new[] { new P(0,0), new P(0,1), new P(0,2), new P(0,3), new P(0,4),
								  new P(1,0), new P(1,1), new P(1,2), new P(1,3),
								  new P(2,0), new P(2,1), new P(2,2),
								  new P(3,0), new P(3,1),
								  new P(4,0),
								  new P(x-1-0,0), new P(x-1-0,1), new P(x-1-0,2), new P(x-1-0,3), new P(x-1-0,4),
								  new P(x-1-1,0), new P(x-1-1,1), new P(x-1-1,2), new P(x-1-1,3),
								  new P(x-1-2,0), new P(x-1-2,1), new P(x-1-2,2),
								  new P(x-1-3,0), new P(x-1-3,1),
								  new P(x-1-4,0),
								  new P(0,y-1-0), new P(0,y-1-1), new P(0,y-1-2), new P(0,y-1-3), new P(0,y-1-4),
								  new P(1,y-1-0), new P(1,y-1-1), new P(1,y-1-2), new P(1,y-1-3),
								  new P(2,y-1-0), new P(2,y-1-1), new P(2,y-1-2),
								  new P(3,y-1-0), new P(3,y-1-1),
								  new P(4,y-1-0),
								  new P(x-1-0,y-1-0), new P(x-1-0,y-1-1), new P(x-1-0,y-1-2), new P(x-1-0,y-1-3), new P(x-1-0,y-1-4),
								  new P(x-1-1,y-1-0), new P(x-1-1,y-1-1), new P(x-1-1,y-1-2), new P(x-1-1,y-1-3),
								  new P(x-1-2,y-1-0), new P(x-1-2,y-1-1), new P(x-1-2,y-1-2),
								  new P(x-1-3,y-1-0), new P(x-1-3,y-1-1),
								  new P(x-1-4,y-1-0)};
			var brdrS = new[] { new P(4,1), new P(3,2), new P(2,3), new P(1, 4),
								  new P(x-1-4,1), new P(x-1-3,2), new P(x-1-2,3), new P(x-1-1, 4),
								  new P(4,y-1-1), new P(3,y-1-2), new P(2,y-1-3), new P(1, y-1-4),
								  new P(x-1-4,y-1-1), new P(x-1-3,y-1-2), new P(x-1-2,y-1-3), new P(x-1-1, y-1-4)};

			var result = new Texture2D(x, y);
			for (var i = 0; i < x; i++)
			{
				for (var j = 0; j < y; j++)
				{
					//Border
					if (i == 0 || j == 0 || i == (x - 1) || j == (y - 1) || brdrS.ToList().Exists(p => p.x == i && p.y == j))
					{
						result.SetPixels(i, j, 1, 1, new[] { Color.black });
					}
					else
					{
						if (j <= 10)
						{
							result.SetPixels(i, j, 1, 1, new[] { col });
						}
						else
						{
							result.SetPixels(i, j, 1, 1, new[] { col2 });
						}
					}

					//Corner
					if (nulls.ToList().Exists(p => p.x == i && p.y == j))
					{
						result.SetPixels(i, j, 1, 1, new[] { new Color(0, 0, 0, 0) });
					}
				}
			}

			result.Apply();
			return result;
		}
	}
}
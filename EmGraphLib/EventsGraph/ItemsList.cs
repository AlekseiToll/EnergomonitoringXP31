using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace EmGraphLib.DNS
{
	public class ItemsList: IList<Item>
	{
		//private struct SRegItem
		//{
		//    public DateTime start;
		//    public DateTime end;
		//    public float value;
		//}

		#region Fields

		private List<Item> items = new List<Item>();
		private DateTime start;
		private DateTime end;

		private ItemRegion normalReg = new ItemRegion();
		private ItemRegion outOfNormalReg = new ItemRegion();

		private CurrentHighlight highlight = new CurrentHighlight();
		private double highlightTicks = -1;

		#endregion

		#region Properties

		/// <summary>Gets or sets start datetime</summary>
		internal DateTime Start
		{
			get { return this.start; }
			set { this.start = value; }
		}

		/// <summary>Gets or sets end datetime</summary>
		internal DateTime End
		{
			get { return this.end; }
			set { this.end = value; }
		}

		/// <summary>Gets the duration of measurement</summary>
		internal TimeSpan Duration
		{
			get
			{
				TimeSpan duration = new TimeSpan();
				try
				{
					duration = this.end - this.start;
				}
				catch { }
				return duration;
			}
		}

		/// <summary>Gets or sets highlight object</summary>
		public CurrentHighlight Highlight
		{
			get { return highlight; }
			set { highlight = value; }
		}

		/// <summary>Gets or sets zero based highlight index</summary>
		public double HighlightTicks
		{
			get { return highlightTicks; }
			set { highlightTicks = value; }
		}

		/// <summary>Gets or sets normal region settings object</summary>
		public ItemRegion NormalRegion
		{
			get { return normalReg; }
			set { normalReg = value; }
		}

		/// <summary>Gets or sets out of normal region settings object</summary>
		public ItemRegion OutOfNormalRegion
		{
			get { return outOfNormalReg; }
			set { outOfNormalReg = value; }
		}

		#endregion

		#region Constructors

		public ItemsList() { }

		#endregion

		#region IList<Item> Members

		public int IndexOf(Item item)
		{
			return items.IndexOf(item);
		}

		public void Insert(int index, Item item)
		{
			items.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			items.RemoveAt(index);
		}

		public Item this[int index]
		{
			get { return items[index]; }
			set { items[index] = value; }
		}

		#endregion

		#region ICollection<Item> Members

		public void Add(Item item)
		{
			items.Add(item);
		}

		public void Clear()
		{
			items.Clear();
		}

		public bool Contains(Item item)
		{
			return items.Contains(item);
		}

		public void CopyTo(Item[] array, int arrayIndex)
		{
			items.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return items.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(Item item)
		{
			return items.Remove(item);
		}

		#endregion

		#region IEnumerable<Item> Members

		public IEnumerator<Item> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return (items as System.Collections.IEnumerable).GetEnumerator();
		}

		#endregion

		#region Advanced collection methods
		
		public void Add(DateTime start, TimeSpan duration, float value, Phase phase)
		{
			items.Add(new Item(start, duration, value, phase));
		}

		#endregion

		#region Internal methods

		/// <summary>
		/// Draw grid lines
		/// </summary>
		/// <param name="g">Graphics object</param>
		/// <param name="rect">Destination rectangle</param>
		/// <param name="minVal">Minimun value</param>
		/// <param name="maxVal">Maximum value</param>
		/// <param name="limit">Limit value</param>
        /// <param name="HighlightOnly">Is only highlight is need to be redrawn</param>
		/// <param name="phases">Phases to draw</param>
		internal void Draw(Graphics g, Rectangle baseRect, float minVal, float maxVal, float limit, bool HighlightOnly, Phase[] phases, DateTime visibleStart, DateTime visibleEnd)
		{
			// check for some troubles
			if (this.start == null || this.end == null) return;
			if (this.start == DateTime.MinValue ||
				this.start == DateTime.MaxValue ||
				this.end == DateTime.MinValue ||
				this.end == DateTime.MaxValue ||
				this.start == this.end) return;

			ItemToDraw[] itemsToDraw;
			CreateRegions(baseRect, minVal, maxVal, limit, phases, out itemsToDraw,
				visibleStart, visibleEnd);

			if (itemsToDraw == null) return;

			if (!HighlightOnly)
			{
				for (int i = 0; i < itemsToDraw.Length; i++)
				{
					DrawRegion(g, itemsToDraw[i].Rect, itemsToDraw[i].Normal, baseRect);
				}
			}

			if (highlight.Visible)
			{
				int x = baseRect.Left + (int)((highlightTicks - visibleStart.Ticks) * ((double)baseRect.Width / (visibleEnd.Ticks - visibleStart.Ticks)));

				if (highlightTicks >= visibleStart.Ticks && highlightTicks <= visibleEnd.Ticks)
					highlight.Draw(g, x, baseRect.Top - highlight.Size.Height - 3);
			}
		}

		/// <summary>
		/// Вычисляет самое большое и самое маленькое значение среди видимых элементов
		/// </summary>
		internal void GetMinMaxValues(out float minVal, out float maxVal, float limit, Phase[] phases,
					DateTime visibleStart, DateTime visibleEnd)
		{
			List<ItemToDraw>[] lists = new List<ItemToDraw>[phases.Length];

			for (int p = 0; p < lists.Length; p++)
			{
				lists[p] = new List<ItemToDraw>();
				ItemToDraw itemToDraw;

				// если есть хотя бы одно
				for (int i = 0; i < items.Count; i++)
				{
					if (phases[p] != items[i].Phase) continue;

					if (items[i].End <= visibleStart) continue;
					if (items[i].Start > visibleEnd) continue;

					itemToDraw = new ItemToDraw();
					itemToDraw.Value = items[i].Value;
					itemToDraw.Phase = phases[p];
					lists[p].Add(itemToDraw);
				} // for i
			} // for p

			minVal = 0F; 
			maxVal = 0F;
			for (int p = 0; p < lists.Length; ++p)
			{
				for (int i = 0; i < lists[p].Count; ++i)
				{
					if (lists[p][i].Value > maxVal) maxVal = lists[p][i].Value;
					if (lists[p][i].Value < minVal) minVal = lists[p][i].Value;
				}
			}

			if (minVal > -limit) minVal = -limit;
			if (maxVal < limit) maxVal = limit;
		}

		#endregion

		#region Private methods

		private void CreateRegions(Rectangle baseRect, float minVal, float maxVal,
			float limit, Phase[] phases, out ItemToDraw[] itemsToDraw,
			DateTime visibleStart, DateTime visibleEnd)
		{
			#region Creating list of SRegItem objects

			itemsToDraw = null;

			if (phases == null) return;

			List<ItemToDraw>[] lists = new List<ItemToDraw>[phases.Length];

			for (int p = 0; p < lists.Length; p++)
			{
				lists[p] = new List<ItemToDraw>();

				// "конец" предыдущего региона
				DateTime lastRegEnd = visibleStart;
				DateTime theEnd = visibleStart;

				ItemToDraw itemToDraw;

				// если есть хотя бы одно
				for (int i = 0; i < items.Count; i++)
				{
					if (phases[p] != items[i].Phase) continue;

					if (items[i].End <= visibleStart) continue;
					//if (items[i].Start >= visibleEnd) continue;

					DateTime item_start = (items[i].Start < visibleStart) ? visibleStart : items[i].Start;
					DateTime item_end = (items[i].End > visibleEnd) ? visibleEnd : items[i].End;

					// если начало нового отклонения не совпадает
					// с концом предыдущего будем вставлять между
					// тем концом и этим началом "нормальную" область
					if (item_start != lastRegEnd)
					{
						// добавляем "нормальный" регион перед текущим отклонением
						itemToDraw = new ItemToDraw();
						itemToDraw.Start = lastRegEnd;
						itemToDraw.Value = 0;
						itemToDraw.End = items[i].Start;
						itemToDraw.Phase = phases[p];
						lists[p].Add(itemToDraw);
					}
					// теперь добавляем непосредственно текущее отклонение
					itemToDraw = new ItemToDraw();
					itemToDraw.Value = items[i].Value;
					itemToDraw.Start = item_start;
					itemToDraw.End = item_end;
					itemToDraw.Phase = phases[p];
					lists[p].Add(itemToDraw);
					if (theEnd < item_end) theEnd = item_end;
					lastRegEnd = item_end;

					bool isLastOfThisPhase = true;
					for (int l = i + 1; l < items.Count; l++)
					{
						if (items[l].Phase == phases[p])
						{
							isLastOfThisPhase = false;
							break;
						}
					}

					// если это последнее отклонение - то надо проверить
					// когда оно закончилось, так как в большинстве случаев
					// придется рисовать остаток нормального региона
					if (isLastOfThisPhase && theEnd < visibleEnd)
					{
						// добавляем "нормальный" регион
						// после текущего (и последнего) отклонения
						itemToDraw = new ItemToDraw();
						itemToDraw.Value = 0;
						itemToDraw.Start = item_end;
						itemToDraw.End = this.end;
						itemToDraw.Phase = phases[p];
						lists[p].Add(itemToDraw);
					}
				} // for i

				// если нет ни одного отклонения
				if (lists[p].Count == 0)
				{
					itemToDraw = new ItemToDraw();
					itemToDraw.Start = (this.start < visibleStart) ? visibleStart : this.start;
					itemToDraw.End = (this.end > visibleEnd) ? visibleEnd : this.end;
					itemToDraw.Value = 0;
					itemToDraw.Phase = phases[p];
					lists[p].Add(itemToDraw);
				}
			} // for p

			#endregion

			#region Creating rects	
		
			// вычисляем коэффициенты для вычисления размеров
			float ratio = (maxVal == minVal) ? 0.5F : (maxVal / (maxVal - minVal));
			float dYLimit = limit / (maxVal - minVal) * baseRect.Height;

			for (int p = 0; p < lists.Length; p++)
			{
				int lastRegionXEnd = baseRect.Left;

				// в эту переменную будем считать потерянные пиксели (при округлении ширины айтемов 
				// заполненных пикселей получается меньше, чем должно быть, поэтому потерянные пискели 
				// будем рисовать как "нормальный" регион)
				double lostPixel = 0F;

				for (int i = 0; i < lists[p].Count; i++)
				{
					if (lists[p][i].Value == 0)		// если это "нормальный" регион
					{
						// setting normal flag
						lists[p][i].Normal = true;

						// setting rectangle bounds:
						// vertical
						lists[p][i].Rect.Y = (int)Math.Ceiling((baseRect.Top + ratio * baseRect.Height - dYLimit));
						lists[p][i].Rect.Height = (int)(2 * dYLimit);

						// horizontal
						lists[p][i].Rect.X = lastRegionXEnd;

						float cur = ((TimeSpan)(lists[p][i].End - lists[p][i].Start)).Ticks;
						float common = ((TimeSpan)(visibleEnd - visibleStart)).Ticks;
						float r = cur / common;
						lists[p][i].Rect.Width = (int)(r * baseRect.Width);

						lostPixel += (r * baseRect.Width) - lists[p][i].Rect.Width;
						if (lostPixel >= 1)
						{
							lists[p][i].Rect.Width += (int)lostPixel;
							lostPixel -= (int)lostPixel;
						}

						lastRegionXEnd += lists[p][i].Rect.Width;
					}
					else // если не "нормальный"
					{
						// setting normal flag
						lists[p][i].Normal = false;

						// setting rectangle bounds
						// vertical
						lists[p][i].Rect.Height = (int)((Math.Abs(lists[p][i].Value) - limit) / (maxVal - minVal) * baseRect.Height);
						if (lists[p][i].Value > 0)
						{
							lists[p][i].Rect.Y = (int)Math.Ceiling((baseRect.Top + ratio * baseRect.Height - dYLimit - lists[p][i].Rect.Height));
						}
						else
						{
							lists[p][i].Rect.Y = (int)Math.Ceiling((baseRect.Top + ratio * baseRect.Height + dYLimit));
						}

						// horizontal
						lists[p][i].Rect.X = lastRegionXEnd;

						float cur = ((TimeSpan)(lists[p][i].End - lists[p][i].Start)).Ticks;
						float common = ((TimeSpan)(visibleEnd - visibleStart)).Ticks;
						float r = cur / common;
						lists[p][i].Rect.Width = (int)(r * baseRect.Width);
						
						lostPixel += (r * baseRect.Width) - lists[p][i].Rect.Width;

						lastRegionXEnd += lists[p][i].Rect.Width;
					} // else 
				} // for i
			} // for p

			// суммарное количество элементов по всем фазам
			int sumCount = 0, copied = 0;
			for (int p = 0; p < lists.Length; ++p)
			{
				sumCount += lists[p].Count;
			}
			// копируем в выходной буфер
			itemsToDraw = new ItemToDraw[sumCount];
			for (int p = 0; p < lists.Length; ++p)
			{
				lists[p].CopyTo(0, itemsToDraw, copied, lists[p].Count);
				copied += lists[p].Count;
			}

			#endregion
		}

		/// <summary>
		/// Draw one region
		/// </summary>
		/// <param name="g">Graphics object</param>
		/// <param name="rect">Rectangle</param>
		/// <param name="normal">Is region normal or not</param>
		private void DrawRegion(Graphics g, Rectangle rect, bool normal, Rectangle baseRect)
		{
			if (rect.Right > baseRect.Right)
				rect.Width -= (rect.Right - baseRect.Right);
			g.FillRectangle(normal ? normalReg.Brush : outOfNormalReg.Brush, rect);
			g.DrawRectangle(normal ? normalReg.Pen : outOfNormalReg.Pen, rect);
		}

		#endregion

	}
}

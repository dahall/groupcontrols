
namespace System.Collections.Generic
{
	/// <summary>
	/// Summary description for SparseArray.
	/// </summary>
	public class SparseArray<T> : IList<T>
	{
		/// <summary>
		/// 
		/// </summary>
		protected Dictionary<int, T> hashtable;

		/// <summary>
		/// Initializes a new instance of the <see cref="SparseArray&lt;T&gt;"/> class.
		/// </summary>
		public SparseArray()
		{
			hashtable = new Dictionary<int,T>();
		}
		
		private class SparseArrayEnumerator<ET> : IEnumerator<T>
		{
			private Dictionary<int, T>.Enumerator dict;
			private SparseArray<T> parent;
			public SparseArrayEnumerator(SparseArray<T> array) { parent = array; Reset(); }
			public void Reset() { dict = parent.hashtable.GetEnumerator(); }
			public bool MoveNext() => dict.MoveNext();
			T IEnumerator<T>.Current => dict.Current.Value;
			public object Current => Current;
			public void Dispose() { dict.Dispose(); parent = null; }
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator() => new SparseArrayEnumerator<T>(this);

		/// <summary>
		/// Determines whether [contains] [the specified value].
		/// </summary>
		/// <param name="item">The value.</param>
		/// <returns>
		/// 	<c>true</c> if [contains] [the specified value]; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(T item) => hashtable.ContainsValue(item);

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
		public void Clear()
		{
			hashtable.Clear();
		}

		/// <summary>
		/// Indexes the of.
		/// </summary>
		/// <param name="item">The value.</param>
		/// <returns></returns>
		public int IndexOf(T item)
		{
			foreach (KeyValuePair<int, T> hash in hashtable)
			{
				if (hash.Value.Equals(item))
					return hash.Key;
			}
			return -1;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
		public void Insert(int index, T item)
		{
			if (hashtable.ContainsKey(index))
				hashtable[index] = item;
			else
				hashtable.Add(index, item);
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
		public void RemoveAt(int index)
		{
			hashtable.Remove(index);
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The index of the element.</param>
		/// <value>The element at the specified index.</value>
		public T this[int index]
		{
			get
			{
				T ret;
				if (hashtable.TryGetValue(index, out ret))
					return ret;
				return default(T);
			}
			set
			{
				Insert(index, value);
			}
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
		public void Add(T item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="array"/> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="arrayIndex"/> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="array"/> is multidimensional.-or-<paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <c>T</c> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			hashtable.Values.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <value></value>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</returns>
		public int Count => hashtable.Count;

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.</returns>
		public bool IsReadOnly => false;

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
		public bool Remove(T item)
		{
			int idx = IndexOf(item);
			if (idx == -1)
				return false;
			RemoveAt(idx);
			return true;
		}
	}
}

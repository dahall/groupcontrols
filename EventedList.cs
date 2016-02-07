using System.Collections.ObjectModel;
using System.ComponentModel;

namespace System.Collections.Generic
{
	/// <summary>
	/// A generic list that provides event for changes to the list.
	/// </summary>
	/// <typeparam name="T">Type for the list.</typeparam>
	[Serializable]
	public class EventedList<T> : IList<T>, IList where T : INotifyPropertyChanged
	{
		// Fields
		private const int _defaultCapacity = 4;

		private static T[] _emptyArray = new T[0];

		private T[] _items;
		private int _size;
		[NonSerialized]
		private object _syncRoot;
		private int _version;

		/// <summary>
		/// Initializes a new instance of the <see cref="EventedList&lt;T&gt;"/> class.
		/// </summary>
		public EventedList()
		{
			_items = EventedList<T>._emptyArray;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventedList&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="collection">The collection.</param>
		public EventedList(IEnumerable<T> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			ICollection<T> is2 = collection as ICollection<T>;
			if (is2 != null)
			{
				int count = is2.Count;
				_items = new T[count];
				is2.CopyTo(_items, 0);
				_size = count;
			}
			else
			{
				_size = 0;
				_items = new T[4];
				using (IEnumerator<T> enumerator = collection.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Add(enumerator.Current);
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventedList&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		public EventedList(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity));
			}
			_items = new T[capacity];
		}

		/// <summary>
		/// Occurs when an item has been added.
		/// </summary>
		public event EventHandler<ListChangedEventArgs<T>> ItemAdded;

		/// <summary>
		/// Occurs when an item has changed.
		/// </summary>
		public event EventHandler<ListChangedEventArgs<T>> ItemChanged;

		/// <summary>
		/// Occurs when an item has been deleted.
		/// </summary>
		public event EventHandler<ListChangedEventArgs<T>> ItemDeleted;

		/// <summary>
		/// Occurs when an item's property value has been changed.
		/// </summary>
		public event PropertyChangedEventHandler ItemPropertyChanged;

		/// <summary>
		/// Occurs when the list has been reset.
		/// </summary>
		public event EventHandler<ListChangedEventArgs<T>> Reset;

		/// <summary>
		/// Gets or sets the capacity.
		/// </summary>
		/// <value>The capacity.</value>
		public int Capacity
		{
			get
			{
				return _items.Length;
			}
			set
			{
				if (value != _items.Length)
				{
					if (value < _size)
					{
						throw new ArgumentOutOfRangeException("value");
					}
					if (value > 0)
					{
						T[] destinationArray = new T[value];
						if (_size > 0)
						{
							Array.Copy(_items, 0, destinationArray, 0, _size);
						}
						_items = destinationArray;
					}
					else
					{
						_items = EventedList<T>._emptyArray;
					}
				}
			}
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <value>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</value>
		public int Count => _size;

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
		/// </summary>
		/// <value></value>
		/// <returns>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.</returns>
		bool ICollection.IsSynchronized => false;

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
		/// </summary>
		/// <value></value>
		/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.</returns>
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					System.Threading.Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				}
				return _syncRoot;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.</returns>
		bool ICollection<T>.IsReadOnly => false;

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.</returns>
		bool IList.IsFixedSize => false;

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.</returns>
		bool IList.IsReadOnly => false;

		/// <summary>
		/// Gets or sets the <see cref="System.Object"/> at the specified index.
		/// </summary>
		/// <value></value>
		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				EventedList<T>.VerifyValueType(value);
				this[index] = (T)value;
			}
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <value>The element at the specified index.</value>
		public T this[int index]
		{
			get
			{
				CheckIndex(index);
				return _items[index];
			}
			set
			{
				CheckIndex(index);
				T oldValue = _items[index];
				_items[index] = value;
				_version++;
				OnItemChanged(index, oldValue, value);
			}
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
		public void Add(T item)
		{
			if (_size == _items.Length)
			{
				EnsureCapacity(_size + 1);
			}
			_items[_size++] = item;
			_version++;
			OnItemAdded(_size, item);
		}

		/// <summary>
		/// Adds the range of items to the list.
		/// </summary>
		/// <param name="collection">The collection of items to add.</param>
		public void AddRange(IEnumerable<T> collection)
		{
			InsertRange(_size, collection);
		}

		/// <summary>
		/// Adds the range of items to the list.
		/// </summary>
		/// <param name="items">The items to add.</param>
		public void AddRange(T[] items)
		{
			InsertRange(_size, items);
		}

		/// <summary>
		/// Determines if the collection is read-only.
		/// </summary>
		/// <returns></returns>
		public ReadOnlyCollection<T> AsReadOnly() => new ReadOnlyCollection<T>(this);

		/// <summary>
		/// Binaries the search.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public int BinarySearch(T item) => BinarySearch(0, _size, item, null);

		/// <summary>
		/// Binaries the search.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns></returns>
		public int BinarySearch(T item, IComparer<T> comparer) => BinarySearch(0, _size, item, comparer);

		/// <summary>
		/// Binaries the search.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		/// <param name="item">The item.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns></returns>
		public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
		{
			CheckRange(index, count);
			return Array.BinarySearch<T>(_items, index, count, item, comparer);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
		public void Clear()
		{
			Array.Clear(_items, 0, _size);
			_size = 0;
			_version++;
			OnReset();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		public bool Contains(T item)
		{
			if (item == null)
			{
				for (int j = 0; j < _size; j++)
				{
					if (_items[j] == null)
					{
						return true;
					}
				}
				return false;
			}
			EqualityComparer<T> comparer = EqualityComparer<T>.Default;
			for (int i = 0; i < _size; i++)
			{
				if (comparer.Equals(_items[i], item))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Converts all.
		/// </summary>
		/// <typeparam name="TOutput">The type of the output.</typeparam>
		/// <param name="converter">The converter.</param>
		/// <returns></returns>
		public EventedList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) where TOutput : INotifyPropertyChanged
		{
			if (converter == null)
			{
				throw new ArgumentNullException(nameof(converter));
			}
			EventedList<TOutput> list = new EventedList<TOutput>(_size);
			for (int i = 0; i < _size; i++)
			{
				list._items[i] = converter(_items[i]);
			}
			list._size = _size;
			return list;
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		public void CopyTo(T[] array)
		{
			CopyTo(array, 0);
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
			Array.Copy(_items, 0, array, arrayIndex, _size);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <param name="count">The count.</param>
		public void CopyTo(int index, T[] array, int arrayIndex, int count)
		{
			if ((_size - index) < count)
				throw new ArgumentOutOfRangeException(nameof(index));
			Array.Copy(_items, index, array, arrayIndex, count);
		}

		/// <summary>
		/// Determines if an item matches the specified predicate.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public bool Exists(Predicate<T> match) => (FindIndex(match) != -1);

		/// <summary>
		/// Finds the specified match.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public T Find(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}
			for (int i = 0; i < _size; i++)
			{
				if (match(_items[i]))
				{
					return _items[i];
				}
			}
			return default(T);
		}

		/// <summary>
		/// Finds all.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public EventedList<T> FindAll(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}
			EventedList<T> list = new EventedList<T>();
			for (int i = 0; i < _size; i++)
			{
				if (match(_items[i]))
				{
					list.Add(_items[i]);
				}
			}
			return list;
		}

		/// <summary>
		/// Finds the index.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public int FindIndex(Predicate<T> match) => FindIndex(0, _size, match);

		/// <summary>
		/// Finds the index.
		/// </summary>
		/// <param name="startIndex">The start index.</param>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public int FindIndex(int startIndex, Predicate<T> match) => FindIndex(startIndex, _size - startIndex, match);

		/// <summary>
		/// Finds the index.
		/// </summary>
		/// <param name="startIndex">The start index.</param>
		/// <param name="count">The count.</param>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public int FindIndex(int startIndex, int count, Predicate<T> match)
		{
			CheckRange(startIndex, count);
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}
			int num = startIndex + count;
			for (int i = startIndex; i < num; i++)
			{
				if (match(_items[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Finds the last.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public T FindLast(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}
			for (int i = _size - 1; i >= 0; i--)
			{
				if (match(_items[i]))
				{
					return _items[i];
				}
			}
			return default(T);
		}

		/// <summary>
		/// Finds the last index.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public int FindLastIndex(Predicate<T> match) => FindLastIndex(_size - 1, _size, match);

		/// <summary>
		/// Finds the last index.
		/// </summary>
		/// <param name="startIndex">The start index.</param>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public int FindLastIndex(int startIndex, Predicate<T> match) => FindLastIndex(startIndex, startIndex + 1, match);

		/// <summary>
		/// Finds the last index.
		/// </summary>
		/// <param name="startIndex">The start index.</param>
		/// <param name="count">The count.</param>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public int FindLastIndex(int startIndex, int count, Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}
			if (_size == 0)
			{
				if (startIndex != -1)
				{
					throw new ArgumentOutOfRangeException(nameof(startIndex));
				}
			}
			CheckIndex(startIndex, "startIndex");
			if ((count < 0) || (((startIndex - count) + 1) < 0))
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}
			int num = startIndex - count;
			for (int i = startIndex; i > num; i--)
			{
				if (match(_items[i]))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Performs an action on each item in the collection.
		/// </summary>
		/// <param name="action">The action.</param>
		public void ForEach(Action<T> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));
			for (int i = 0; i < _size; i++)
				action(_items[i]);
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public EventedList<T>.Enumerator GetEnumerator() => new EventedList<T>.Enumerator((EventedList<T>)this);

		/// <summary>
		/// Gets the range of items and returns then in another list.
		/// </summary>
		/// <param name="index">The starting index.</param>
		/// <param name="count">The count of items to place in the list.</param>
		/// <returns>An <see cref="EventedList&lt;T&gt;"/> with the requested items.</returns>
		public EventedList<T> GetRange(int index, int count)
		{
			CheckRange(index, count);
			EventedList<T> list = new EventedList<T>(count);
			Array.Copy(_items, index, list._items, 0, count);
			list._size = count;
			return list;
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if ((array != null) && (array.Rank != 1))
			{
				throw new ArgumentException();
			}
			try
			{
				Array.Copy(_items, 0, array, arrayIndex, _size);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException();
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator() => new EventedList<T>.Enumerator((EventedList<T>)this);

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => new EventedList<T>.Enumerator((EventedList<T>)this);

		/// <summary>
		/// Adds the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		int IList.Add(object item)
		{
			EventedList<T>.VerifyValueType(item);
			Add((T)item);
			return (_size - 1);
		}

		/// <summary>
		/// Determines whether [contains] [the specified item].
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>
		/// 	<c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
		/// </returns>
		bool IList.Contains(object item) => (EventedList<T>.IsCompatibleObject(item) && Contains((T)item));

		/// <summary>
		/// Indexes the of.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		int IList.IndexOf(object item)
		{
			if (EventedList<T>.IsCompatibleObject(item))
			{
				return IndexOf((T)item);
			}
			return -1;
		}

		/// <summary>
		/// Inserts the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="item">The item.</param>
		void IList.Insert(int index, object item)
		{
			EventedList<T>.VerifyValueType(item);
			Insert(index, (T)item);
		}

		/// <summary>
		/// Removes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		void IList.Remove(object item)
		{
			if (EventedList<T>.IsCompatibleObject(item))
			{
				Remove((T)item);
			}
		}

		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(T item) => Array.IndexOf<T>(_items, item, 0, _size);

		/// <summary>
		/// Indexes the of.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public int IndexOf(T item, int index)
		{
			CheckIndex(index);
			return Array.IndexOf<T>(_items, item, index, _size - index);
		}

		/// <summary>
		/// Indexes the of.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		/// <returns></returns>
		public int IndexOf(T item, int index, int count)
		{
			CheckRange(index, count);
			return Array.IndexOf<T>(_items, item, index, count);
		}

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
			if (index != _size)
				CheckIndex(index);
			if (_size == _items.Length)
			{
				EnsureCapacity(_size + 1);
			}
			if (index < _size)
			{
				Array.Copy(_items, index, _items, index + 1, _size - index);
			}
			_items[index] = item;
			_size++;
			_version++;
			OnItemAdded(index, item);
		}

		/// <summary>
		/// Inserts the range.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="collection">The collection.</param>
		public void InsertRange(int index, IEnumerable<T> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			if (index != _size)
				CheckIndex(index);
			ICollection<T> is2 = collection as ICollection<T>;
			if (is2 != null)
			{
				int count = is2.Count;
				if (count > 0)
				{
					EnsureCapacity(_size + count);
					if (index < _size)
					{
						Array.Copy(_items, index, _items, index + count, _size - index);
					}
					if (this == is2)
					{
						Array.Copy(_items, 0, _items, index, index);
						Array.Copy(_items, (int)(index + count), _items, (int)(index * 2), (int)(_size - index));
					}
					else
					{
						T[] array = new T[count];
						is2.CopyTo(array, 0);
						array.CopyTo(_items, index);
					}
					_size += count;
					for (int i = index; i < index + count; i++)
						OnItemAdded(i, _items[i]);
				}
			}
			else
			{
				using (IEnumerator<T> enumerator = collection.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Insert(index++, enumerator.Current);
					}
				}
			}
			_version++;
		}

		/// <summary>
		/// Lasts the index of.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public int LastIndexOf(T item) => LastIndexOf(item, _size - 1, _size);

		/// <summary>
		/// Lasts the index of.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public int LastIndexOf(T item, int index)
		{
			CheckIndex(index);
			return LastIndexOf(item, index, index + 1);
		}

		/// <summary>
		/// Lasts the index of.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		/// <returns></returns>
		public int LastIndexOf(T item, int index, int count)
		{
			if (_size == 0)
			{
				return -1;
			}
			CheckIndex(index);
			if (count < 0 || count > (index + 1))
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}
			return Array.LastIndexOf<T>(_items, item, index, count);
		}

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
			int index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes all.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public int RemoveAll(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}
			int index = 0;
			while ((index < _size) && !match(_items[index]))
			{
				index++;
			}
			if (index >= _size)
			{
				return 0;
			}
			int num2 = index + 1;
			while (num2 < _size)
			{
				while ((num2 < _size) && match(_items[num2]))
				{
					num2++;
				}
				if (num2 < _size)
				{
					T oldVal = _items[index + 1];
					_items[index++] = _items[num2++];
					OnItemDeleted(index, oldVal);
				}
			}
			Array.Clear(_items, index, _size - index);
			int num3 = _size - index;
			_size = index;
			_version++;
			return num3;
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
			CheckIndex(index);
			_size--;
			T oldVal = _items[index];
			if (index < _size)
			{
				Array.Copy(_items, index + 1, _items, index, _size - index);
			}
			_items[_size] = default(T);
			_version++;
			OnItemDeleted(index, oldVal);
		}

		/// <summary>
		/// Removes the range.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		public void RemoveRange(int index, int count)
		{
			CheckRange(index, count);
			if (count > 0)
			{
				_size -= count;
				T[] array = new T[count];
				Array.Copy(_items, index, array, 0, count);
				if (index < _size)
				{
					Array.Copy(_items, index + count, _items, index, _size - index);
				}
				Array.Clear(_items, _size, count);
				_version++;
				for (int i = index; i < index + count; i++)
					OnItemDeleted(i, array[i - index]);
			}
		}

		/// <summary>
		/// Reverses this instance.
		/// </summary>
		public void Reverse()
		{
			Reverse(0, _size);
		}

		/// <summary>
		/// Reverses the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		public void Reverse(int index, int count)
		{
			CheckRange(index, count);
			Array.Reverse(_items, index, count);
			_version++;
		}

		/// <summary>
		/// Sorts this instance.
		/// </summary>
		public void Sort()
		{
			Sort(0, _size, null);
		}

		/// <summary>
		/// Sorts the specified comparer.
		/// </summary>
		/// <param name="comparer">The comparer.</param>
		public void Sort(IComparer<T> comparer)
		{
			Sort(0, _size, comparer);
		}

		/// <summary>
		/// Sorts the specified index.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		/// <param name="comparer">The comparer.</param>
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			CheckRange(index, count);
			Array.Sort<T>(_items, index, count, comparer);
			_version++;
		}

		/// <summary>
		/// Toes the array.
		/// </summary>
		/// <returns></returns>
		public T[] ToArray()
		{
			T[] destinationArray = new T[_size];
			Array.Copy(_items, 0, destinationArray, 0, _size);
			return destinationArray;
		}

		/// <summary>
		/// Trims the excess.
		/// </summary>
		public void TrimExcess()
		{
			int num = (int)(_items.Length * 0.9);
			if (_size < num)
			{
				Capacity = _size;
			}
		}

		/// <summary>
		/// Trues for all.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		public bool TrueForAll(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}
			for (int i = 0; i < _size; i++)
			{
				if (!match(_items[i]))
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Called when [insert].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="value">The value.</param>
		protected virtual void OnItemAdded(int index, T value)
		{
			if (value != null)
			{
				value.PropertyChanged += OnItemPropertyChanged;
				ItemAdded?.Invoke(this, new EventedList<T>.ListChangedEventArgs<T>(ListChangedType.ItemAdded, value, index));
			}
		}

		/// <summary>
		/// Called when [item property changed].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ItemPropertyChanged?.Invoke(sender, e);
		}

		/// <summary>
		/// Called when [set].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="oldValue">The old value.</param>
		/// <param name="newValue">The new value.</param>
		protected virtual void OnItemChanged(int index, T oldValue, T newValue)
		{
			if (oldValue != null && !oldValue.Equals(newValue))
			{
				oldValue.PropertyChanged -= OnItemPropertyChanged;
				if (newValue != null)
					newValue.PropertyChanged += OnItemPropertyChanged;
			}
			ItemChanged?.Invoke(this, new EventedList<T>.ListChangedEventArgs<T>(ListChangedType.ItemChanged, newValue, index, oldValue));
		}

		/// <summary>
		/// Called when [remove].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="value">The value.</param>
		protected virtual void OnItemDeleted(int index, T value)
		{
			if (value != null)
			{
				value.PropertyChanged -= OnItemPropertyChanged;
				ItemDeleted?.Invoke(this, new EventedList<T>.ListChangedEventArgs<T>(ListChangedType.ItemDeleted, value, index));
			}
		}

		/// <summary>
		/// Called when [clear].
		/// </summary>
		protected virtual void OnReset()
		{
			ForEach(delegate(T item) { item.PropertyChanged -= OnItemPropertyChanged; });
			Reset?.Invoke(this, new EventedList<T>.ListChangedEventArgs<T>(ListChangedType.Reset));
		}

		/// <summary>
		/// Determines whether [is compatible object] [the specified value].
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// 	<c>true</c> if [is compatible object] [the specified value]; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsCompatibleObject(object value) => (value is T || (value == null && !typeof(T).IsValueType));

		/// <summary>
		/// Verifies the type of the value.
		/// </summary>
		/// <param name="value">The value.</param>
		private static void VerifyValueType(object value)
		{
			if (!EventedList<T>.IsCompatibleObject(value))
			{
				throw new ArgumentException("Incompatible object", nameof(value));
			}
		}

		/// <summary>
		/// Checks the index to ensure it is valid and in the list.
		/// </summary>
		/// <param name="idx">The index to validate.</param>
		/// <param name="varName">Name of the variable this is being checked.</param>
		/// <exception cref="ArgumentOutOfRangeException">Called with the index is out of range.</exception>
		private void CheckIndex(int idx, string varName = "index")
		{
			if (idx >= _size || idx < 0)
				throw new ArgumentOutOfRangeException(varName);
		}

		/// <summary>
		/// Checks the range.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="count">The count.</param>
		private void CheckRange(int index, int count)
		{
			if (index >= _size || index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));
			if (count < 0 || (_size - index) < count)
				throw new ArgumentOutOfRangeException(nameof(count));
		}

		/// <summary>
		/// Ensures the capacity.
		/// </summary>
		/// <param name="min">The min.</param>
		private void EnsureCapacity(int min)
		{
			if (_items.Length < min)
			{
				int num = (_items.Length == 0) ? 4 : (_items.Length * 2);
				if (num < min)
				{
					num = min;
				}
				Capacity = num;
			}
		}

		/// <summary>
		/// Enumerates over the <see cref="EventedList&lt;T&gt;"/>.
		/// </summary>
		[Serializable,
		System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private EventedList<T> list;
			private int index;
			private int version;
			private T current;

			/// <summary>
			/// Initializes a new instance of the <see cref="EventedList&lt;T&gt;.Enumerator"/> struct.
			/// </summary>
			/// <param name="list">The list.</param>
			internal Enumerator(EventedList<T> list)
			{
				this.list = list;
				index = 0;
				version = list._version;
				current = default(T);
			}

			/// <summary>
			/// Gets the current.
			/// </summary>
			/// <value>The current.</value>
			public T Current => current;

			/// <summary>
			/// Gets the current.
			/// </summary>
			/// <value>The current.</value>
			object IEnumerator.Current
			{
				get
				{
					if ((index == 0) || (index == (list._size + 1)))
					{
						throw new InvalidOperationException();
					}
					return Current;
				}
			}

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the collection.
			/// </summary>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			void IEnumerator.Reset()
			{
				if (version != list._version)
				{
					throw new InvalidOperationException();
				}
				index = 0;
				current = default(T);
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
			/// </returns>
			/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
			public bool MoveNext()
			{
				if (version != list._version)
				{
					throw new InvalidOperationException();
				}
				if (index < list._size)
				{
					current = list._items[index];
					index++;
					return true;
				}
				index = list._size + 1;
				current = default(T);
				return false;
			}
		}

		/// <summary>
		/// An <see cref="EventArgs"/> structure passed to events generated by an <see cref="EventedList{T}"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
#pragma warning disable 693
		public class ListChangedEventArgs<T> : EventArgs
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="EventedList&lt;T&gt;.ListChangedEventArgs&lt;T&gt;"/> class.
			/// </summary>
			/// <param name="type">The type of change.</param>
			public ListChangedEventArgs(ListChangedType type)
			{
				ItemIndex = -1;
				ListChangedType = type;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="EventedList&lt;T&gt;.ListChangedEventArgs&lt;T&gt;"/> class.
			/// </summary>
			/// <param name="type">The type of change.</param>
			/// <param name="item">The item that has changed.</param>
			/// <param name="itemIndex">Index of the changed item.</param>
			public ListChangedEventArgs(ListChangedType type, T item, int itemIndex)
			{
				Item = item;
				ItemIndex = itemIndex;
				ListChangedType = type;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="EventedList&lt;T&gt;.ListChangedEventArgs&lt;T&gt;"/> class.
			/// </summary>
			/// <param name="type">The type of change.</param>
			/// <param name="item">The item that has changed.</param>
			/// <param name="itemIndex">Index of the changed item.</param>
			/// <param name="oldItem">The old item when an item has changed.</param>
			public ListChangedEventArgs(ListChangedType type, T item, int itemIndex, T oldItem)
				: this(type, item, itemIndex)
			{
				OldItem = oldItem;
			}

			/// <summary>
			/// Gets the item that has changed.
			/// </summary>
			/// <value>The item.</value>
			public T Item { get; }

			/// <summary>
			/// Gets the index of the item.
			/// </summary>
			/// <value>The index of the item.</value>
			public int ItemIndex { get; }

			/// <summary>
			/// Gets the type of change for the list.
			/// </summary>
			/// <value>The type of change for the list.</value>
			public ListChangedType ListChangedType { get; }

			/// <summary>
			/// Gets the item's previous value.
			/// </summary>
			/// <value>The old item.</value>
			public T OldItem
			{
				get;
			}
		}
#pragma warning restore 693
	}
}
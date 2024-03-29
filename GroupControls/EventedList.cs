#pragma warning disable GlobalUsingsAnalyzer // Using should be in global file
using System.Collections.ObjectModel;
using System.Threading;
#pragma warning restore GlobalUsingsAnalyzer // Using should be in global file

namespace System.Collections.Generic;

/// <summary>A generic list that provides event for changes to the list.</summary>
/// <typeparam name="T">Type for the list.</typeparam>
[Serializable]
public class EventedList<T> : IList<T>, IList where T : INotifyPropertyChanged
{
	private const int defaultCapacity = 4;

	private static readonly T[] emptyArray = new T[0];

	private T[] internalItems;
	[NonSerialized] private object syncRoot;
	private int version;

	/// <summary>Initializes a new instance of the <see cref="EventedList{T}"/> class.</summary>
	public EventedList() => internalItems = emptyArray;

	/// <summary>Initializes a new instance of the <see cref="EventedList{T}"/> class.</summary>
	/// <param name="collection">The collection.</param>
	public EventedList(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException(nameof(collection));
		}
		var is2 = collection as ICollection<T>;
		if (is2 != null)
		{
			var count = is2.Count;
			internalItems = new T[count];
			is2.CopyTo(internalItems, 0);
			Count = count;
		}
		else
		{
			Count = 0;
			internalItems = new T[4];
			using var enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Add(enumerator.Current);
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="EventedList{T}"/> class.</summary>
	/// <param name="capacity">The capacity.</param>
	public EventedList(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity));
		}
		internalItems = new T[capacity];
	}

	/// <summary>Gets or sets the capacity.</summary>
	/// <value>The capacity.</value>
	public int Capacity
	{
		get => internalItems.Length;
		set
		{
			if (value != internalItems.Length)
			{
				if (value < Count)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}
				if (value > 0)
				{
					var destinationArray = new T[value];
					if (Count > 0)
					{
						Array.Copy(internalItems, 0, destinationArray, 0, Count);
					}
					internalItems = destinationArray;
				}
				else
				{
					internalItems = emptyArray;
				}
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
	/// </summary>
	/// <value></value>
	/// <returns>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.</returns>
	bool ICollection.IsSynchronized => false;

	/// <summary>Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.</summary>
	/// <value></value>
	/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.</returns>
	object ICollection.SyncRoot
	{
		get
		{
			if (syncRoot == null)
			{
				Interlocked.CompareExchange(ref syncRoot, new object(), null);
			}
			return syncRoot;
		}
	}

	/// <summary>Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.</summary>
	/// <value></value>
	/// <returns>true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.</returns>
	bool IList.IsFixedSize => false;

	/// <summary>Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.</summary>
	/// <value></value>
	/// <returns>true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.</returns>
	bool IList.IsReadOnly => false;

	/// <summary>Gets or sets the <see cref="System.Object"/> at the specified index.</summary>
	/// <value></value>
	object IList.this[int index]
	{
		get => this[index];
		set
		{
			VerifyValueType(value);
			this[index] = (T)value;
		}
	}

	/// <summary>Copies to.</summary>
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
			Array.Copy(internalItems, 0, array, arrayIndex, Count);
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException();
		}
	}

	/// <summary>Adds the specified item.</summary>
	/// <param name="item">The item.</param>
	/// <returns></returns>
	int IList.Add(object item)
	{
		VerifyValueType(item);
		Add((T)item);
		return Count - 1;
	}

	/// <summary>Determines whether [contains] [the specified item].</summary>
	/// <param name="item">The item.</param>
	/// <returns><c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.</returns>
	bool IList.Contains(object item) => IsCompatibleObject(item) && Contains((T)item);

	/// <summary>Indexes the of.</summary>
	/// <param name="item">The item.</param>
	/// <returns></returns>
	int IList.IndexOf(object item) => IsCompatibleObject(item) ? IndexOf((T)item) : -1;

	/// <summary>Inserts the specified index.</summary>
	/// <param name="index">The index.</param>
	/// <param name="item">The item.</param>
	void IList.Insert(int index, object item)
	{
		VerifyValueType(item);
		Insert(index, (T)item);
	}

	/// <summary>Removes the specified item.</summary>
	/// <param name="item">The item.</param>
	void IList.Remove(object item)
	{
		if (IsCompatibleObject(item))
		{
			Remove((T)item);
		}
	}

	/// <summary>Gets the number of elements contained in the <see cref="ICollection{T}"/>.</summary>
	/// <value>The number of elements contained in the <see cref="ICollection{T}"/>.</value>
	public int Count { get; private set; }

	/// <summary>Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.</summary>
	/// <value></value>
	/// <returns>true if the <see cref="ICollection{T}"/> is read-only; otherwise, false.</returns>
	bool ICollection<T>.IsReadOnly => false;

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <param name="index">The zero-based index of the element to get or set.</param>
	/// <value>The element at the specified index.</value>
	public T this[int index]
	{
		get
		{
			CheckIndex(index);
			return internalItems[index];
		}
		set
		{
			CheckIndex(index);
			var oldValue = internalItems[index];
			internalItems[index] = value;
			version++;
			OnItemChanged(index, oldValue, value);
		}
	}

	/// <summary>Adds an item to the <see cref="ICollection{T}"/>.</summary>
	/// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
	/// <exception cref="T:System.NotSupportedException">The <see cref="ICollection{T}"/> is read-only.</exception>
	public void Add(T item)
	{
		if (Count == internalItems.Length)
		{
			EnsureCapacity(Count + 1);
		}
		internalItems[Count++] = item;
		version++;
		OnItemAdded(Count, item);
	}

	/// <summary>Removes all items from the <see cref="ICollection{T}"/>.</summary>
	/// <exception cref="T:System.NotSupportedException">The <see cref="ICollection{T}"/> is read-only.</exception>
	public void Clear()
	{
		Array.Clear(internalItems, 0, Count);
		Count = 0;
		version++;
		OnReset();
	}

	/// <summary>Determines whether the <see cref="ICollection{T}"/> contains a specific value.</summary>
	/// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
	/// <returns>true if <paramref name="item"/> is found in the <see cref="ICollection{T}"/>; otherwise, false.</returns>
	public bool Contains(T item)
	{
		if (item == null)
		{
			for (var j = 0; j < Count; j++)
			{
				if (internalItems[j] == null)
				{
					return true;
				}
			}
			return false;
		}
		var comparer = EqualityComparer<T>.Default;
		for (var i = 0; i < Count; i++)
		{
			if (comparer.Equals(internalItems[i], item))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="T:System.Array"/>, starting at a particular <see
	/// cref="T:System.Array"/> index.
	/// </summary>
	/// <param name="array">
	/// The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see
	/// cref="ICollection{T}"/>. The <see cref="T:System.Array"/> must have zero-based indexing.
	/// </param>
	/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
	/// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
	/// <exception cref="T:System.ArgumentException">
	/// <paramref name="array"/> is multidimensional.-or- <paramref name="arrayIndex"/> is equal to or greater than the length of
	/// <paramref name="array"/>.-or-The number of elements in the source <see cref="ICollection{T}"/> is greater than the available
	/// space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <c>T</c> cannot be cast
	/// automatically to the type of the destination <paramref name="array"/>.
	/// </exception>
	public void CopyTo(T[] array, int arrayIndex = 0) => Array.Copy(internalItems, 0, array, arrayIndex, Count);

	/// <summary>Returns an enumerator that iterates through a collection.</summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

	/// <summary>Returns an enumerator that iterates through the collection.</summary>
	/// <returns>A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

	/// <summary>Determines the index of a specific item in the <see cref="IList{T}"/>.</summary>
	/// <param name="item">The object to locate in the <see cref="IList{T}"/>.</param>
	/// <returns>The index of <paramref name="item"/> if found in the list; otherwise, -1.</returns>
	public int IndexOf(T item) => Array.IndexOf(internalItems, item, 0, Count);

	/// <summary>Inserts an item to the <see cref="IList{T}"/> at the specified index.</summary>
	/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
	/// <param name="item">The object to insert into the <see cref="IList{T}"/>.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="IList{T}"/> is read-only.</exception>
	public void Insert(int index, T item)
	{
		if (index != Count)
			CheckIndex(index);
		if (Count == internalItems.Length)
		{
			EnsureCapacity(Count + 1);
		}
		if (index < Count)
		{
			Array.Copy(internalItems, index, internalItems, index + 1, Count - index);
		}
		internalItems[index] = item;
		Count++;
		version++;
		OnItemAdded(index, item);
	}

	/// <summary>Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.</summary>
	/// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
	/// <returns>
	/// true if <paramref name="item"/> was successfully removed from the <see cref="ICollection{T}"/>; otherwise, false. This method
	/// also returns false if <paramref name="item"/> is not found in the original <see cref="ICollection{T}"/>.
	/// </returns>
	/// <exception cref="T:System.NotSupportedException">The <see cref="ICollection{T}"/> is read-only.</exception>
	public bool Remove(T item)
	{
		var index = IndexOf(item);
		if (index >= 0)
		{
			RemoveAt(index);
			return true;
		}
		return false;
	}

	/// <summary>Removes the <see cref="IList{T}"/> item at the specified index.</summary>
	/// <param name="index">The zero-based index of the item to remove.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.</exception>
	/// <exception cref="T:System.NotSupportedException">The <see cref="IList{T}"/> is read-only.</exception>
	public void RemoveAt(int index)
	{
		CheckIndex(index);
		Count--;
		var oldVal = internalItems[index];
		if (index < Count)
		{
			Array.Copy(internalItems, index + 1, internalItems, index, Count - index);
		}
		internalItems[Count] = default(T);
		version++;
		OnItemDeleted(index, oldVal);
	}

	/// <summary>Occurs when an item has been added.</summary>
	public event EventHandler<ListChangedEventArgs<T>> ItemAdded;

	/// <summary>Occurs when an item has changed.</summary>
	public event EventHandler<ListChangedEventArgs<T>> ItemChanged;

	/// <summary>Occurs when an item has been deleted.</summary>
	public event EventHandler<ListChangedEventArgs<T>> ItemDeleted;

	/// <summary>Occurs when an item's property value has been changed.</summary>
	public event PropertyChangedEventHandler ItemPropertyChanged;

	/// <summary>Occurs when the list has been reset.</summary>
	public event EventHandler<ListChangedEventArgs<T>> Reset;

	/// <summary>Adds the range of items to the list.</summary>
	/// <param name="collection">The collection of items to add.</param>
	public void AddRange(IEnumerable<T> collection) => InsertRange(Count, collection);

	/// <summary>Adds the range of items to the list.</summary>
	/// <param name="items">The items to add.</param>
	public void AddRange(T[] items) => InsertRange(Count, items);

	/// <summary>Determines if the collection is read-only.</summary>
	/// <returns></returns>
	public ReadOnlyCollection<T> AsReadOnly() => new ReadOnlyCollection<T>(this);

	/// <summary>
	/// Searches the entire sorted <see cref="EventedList{T}"/> for an element using the default comparer and returns the zero-based
	/// index of the element.
	/// </summary>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <returns>
	/// The zero-based index of item in the sorted <see cref="EventedList{T}"/>, if item is found; otherwise, a negative number that is
	/// the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise
	/// complement of <see cref="Count"/>.
	/// </returns>
	public int BinarySearch(T item) => BinarySearch(0, Count, item, null);

	/// <summary>
	/// Searches the entire sorted <see cref="EventedList{T}"/> for an element using the specified comparer and returns the zero-based
	/// index of the element.
	/// </summary>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <param name="comparer">
	/// The <see cref="IComparer{T}"/> implementation to use when comparing elements, or null to use the default comparer <see cref="Comparer{T}.Default"/>.
	/// </param>
	/// <returns>
	/// The zero-based index of item in the sorted <see cref="EventedList{T}"/>, if item is found; otherwise, a negative number that is
	/// the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise
	/// complement of <see cref="Count"/>.
	/// </returns>
	public int BinarySearch(T item, IComparer<T> comparer) => BinarySearch(0, Count, item, comparer);

	/// <summary>
	/// Searches a range of elements in the sorted <see cref="EventedList{T}"/> for an element using the specified comparer and returns
	/// the zero-based index of the element.
	/// </summary>
	/// <param name="index">The zero-based starting index of the range to search.</param>
	/// <param name="count">The length of the range to search.</param>
	/// <param name="item">The object to locate. The value can be null for reference types.</param>
	/// <param name="comparer">
	/// The <see cref="IComparer{T}"/> implementation to use when comparing elements, or null to use the default comparer <see cref="Comparer{T}.Default"/>.
	/// </param>
	/// <returns>
	/// The zero-based index of item in the sorted <see cref="EventedList{T}"/>, if item is found; otherwise, a negative number that is
	/// the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise
	/// complement of <see cref="Count"/>.
	/// </returns>
	public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
	{
		CheckRange(index, count);
		return Array.BinarySearch(internalItems, index, count, item, comparer);
	}

	/// <summary>Converts all.</summary>
	/// <typeparam name="TOutput">The type of the output.</typeparam>
	/// <param name="converter">The converter.</param>
	/// <returns></returns>
	public EventedList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
		where TOutput : INotifyPropertyChanged
	{
		if (converter == null)
		{
			throw new ArgumentNullException(nameof(converter));
		}
		var list = new EventedList<TOutput>(Count);
		for (var i = 0; i < Count; i++)
		{
			list.internalItems[i] = converter(internalItems[i]);
		}
		list.Count = Count;
		return list;
	}

	/// <summary>Copies to.</summary>
	/// <param name="index">The index.</param>
	/// <param name="array">The array.</param>
	/// <param name="arrayIndex">Index of the array.</param>
	/// <param name="count">The count.</param>
	public void CopyTo(int index, T[] array, int arrayIndex, int count)
	{
		if (Count - index < count)
			throw new ArgumentOutOfRangeException(nameof(index));
		Array.Copy(internalItems, index, array, arrayIndex, count);
	}

	/// <summary>Determines if an item matches the specified predicate.</summary>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public bool Exists(Predicate<T> match) => FindIndex(match) != -1;

	/// <summary>Finds the specified match.</summary>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public T Find(Predicate<T> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException(nameof(match));
		}
		for (var i = 0; i < Count; i++)
		{
			if (match(internalItems[i]))
			{
				return internalItems[i];
			}
		}
		return default(T);
	}

	/// <summary>Finds all.</summary>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public EventedList<T> FindAll(Predicate<T> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException(nameof(match));
		}
		var list = new EventedList<T>();
		for (var i = 0; i < Count; i++)
		{
			if (match(internalItems[i]))
			{
				list.Add(internalItems[i]);
			}
		}
		return list;
	}

	/// <summary>Finds the index.</summary>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public int FindIndex(Predicate<T> match) => FindIndex(0, Count, match);

	/// <summary>Finds the index.</summary>
	/// <param name="startIndex">The start index.</param>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public int FindIndex(int startIndex, Predicate<T> match) => FindIndex(startIndex, Count - startIndex, match);

	/// <summary>Finds the index.</summary>
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
		var num = startIndex + count;
		for (var i = startIndex; i < num; i++)
		{
			if (match(internalItems[i]))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Finds the last.</summary>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public T FindLast(Predicate<T> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException(nameof(match));
		}
		for (var i = Count - 1; i >= 0; i--)
		{
			if (match(internalItems[i]))
			{
				return internalItems[i];
			}
		}
		return default(T);
	}

	/// <summary>Finds the last index.</summary>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public int FindLastIndex(Predicate<T> match) => FindLastIndex(Count - 1, Count, match);

	/// <summary>Finds the last index.</summary>
	/// <param name="startIndex">The start index.</param>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public int FindLastIndex(int startIndex, Predicate<T> match) => FindLastIndex(startIndex, startIndex + 1, match);

	/// <summary>Finds the last index.</summary>
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
		if (Count == 0)
		{
			if (startIndex != -1)
			{
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}
		}
		CheckIndex(startIndex, "startIndex");
		if ((count < 0) || (startIndex - count + 1 < 0))
		{
			throw new ArgumentOutOfRangeException(nameof(count));
		}
		var num = startIndex - count;
		for (var i = startIndex; i > num; i--)
		{
			if (match(internalItems[i]))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>Performs an action on each item in the collection.</summary>
	/// <param name="action">The action.</param>
	public void ForEach(Action<T> action)
	{
		if (action == null)
			throw new ArgumentNullException(nameof(action));
		for (var i = 0; i < Count; i++)
			action(internalItems[i]);
	}

	/// <summary>Gets the enumerator.</summary>
	/// <returns></returns>
	public Enumerator GetEnumerator() => new Enumerator(this);

	/// <summary>Gets the range of items and returns then in another list.</summary>
	/// <param name="index">The starting index.</param>
	/// <param name="count">The count of items to place in the list.</param>
	/// <returns>An <see cref="EventedList{T}"/> with the requested items.</returns>
	public EventedList<T> GetRange(int index, int count)
	{
		CheckRange(index, count);
		var list = new EventedList<T>(count);
		Array.Copy(internalItems, index, list.internalItems, 0, count);
		list.Count = count;
		return list;
	}

	/// <summary>Indexes the of.</summary>
	/// <param name="item">The item.</param>
	/// <param name="index">The index.</param>
	/// <returns></returns>
	public int IndexOf(T item, int index)
	{
		CheckIndex(index);
		return Array.IndexOf(internalItems, item, index, Count - index);
	}

	/// <summary>Indexes the of.</summary>
	/// <param name="item">The item.</param>
	/// <param name="index">The index.</param>
	/// <param name="count">The count.</param>
	/// <returns></returns>
	public int IndexOf(T item, int index, int count)
	{
		CheckRange(index, count);
		return Array.IndexOf(internalItems, item, index, count);
	}

	/// <summary>Inserts the range.</summary>
	/// <param name="index">The index.</param>
	/// <param name="collection">The collection.</param>
	public void InsertRange(int index, IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException(nameof(collection));
		}
		if (index != Count)
			CheckIndex(index);
		var is2 = collection as ICollection<T>;
		if (is2 != null)
		{
			var count = is2.Count;
			if (count > 0)
			{
				EnsureCapacity(Count + count);
				if (index < Count)
				{
					Array.Copy(internalItems, index, internalItems, index + count, Count - index);
				}
				if (this == is2)
				{
					Array.Copy(internalItems, 0, internalItems, index, index);
					Array.Copy(internalItems, index + count, internalItems, index * 2, Count - index);
				}
				else
				{
					var array = new T[count];
					is2.CopyTo(array, 0);
					array.CopyTo(internalItems, index);
				}
				Count += count;
				for (var i = index; i < index + count; i++)
					OnItemAdded(i, internalItems[i]);
			}
		}
		else
		{
			using var enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Insert(index++, enumerator.Current);
			}
		}
		version++;
	}

	/// <summary>Lasts the index of.</summary>
	/// <param name="item">The item.</param>
	/// <returns></returns>
	public int LastIndexOf(T item) => LastIndexOf(item, Count - 1, Count);

	/// <summary>Lasts the index of.</summary>
	/// <param name="item">The item.</param>
	/// <param name="index">The index.</param>
	/// <returns></returns>
	public int LastIndexOf(T item, int index)
	{
		CheckIndex(index);
		return LastIndexOf(item, index, index + 1);
	}

	/// <summary>Lasts the index of.</summary>
	/// <param name="item">The item.</param>
	/// <param name="index">The index.</param>
	/// <param name="count">The count.</param>
	/// <returns></returns>
	public int LastIndexOf(T item, int index, int count)
	{
		if (Count == 0)
		{
			return -1;
		}
		CheckIndex(index);
		return count < 0 || count > index + 1
			?         throw new ArgumentOutOfRangeException(nameof(count))
			: Array.LastIndexOf(internalItems, item, index, count);
	}

	/// <summary>Removes all.</summary>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public int RemoveAll(Predicate<T> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException(nameof(match));
		}
		var index = 0;
		while ((index < Count) && !match(internalItems[index]))
		{
			index++;
		}
		if (index >= Count)
		{
			return 0;
		}
		var num2 = index + 1;
		while (num2 < Count)
		{
			while ((num2 < Count) && match(internalItems[num2]))
			{
				num2++;
			}
			if (num2 < Count)
			{
				var oldVal = internalItems[index + 1];
				internalItems[index++] = internalItems[num2++];
				OnItemDeleted(index, oldVal);
			}
		}
		Array.Clear(internalItems, index, Count - index);
		var num3 = Count - index;
		Count = index;
		version++;
		return num3;
	}

	/// <summary>Removes the range.</summary>
	/// <param name="index">The index.</param>
	/// <param name="count">The count.</param>
	public void RemoveRange(int index, int count)
	{
		CheckRange(index, count);
		if (count > 0)
		{
			Count -= count;
			var array = new T[count];
			Array.Copy(internalItems, index, array, 0, count);
			if (index < Count)
			{
				Array.Copy(internalItems, index + count, internalItems, index, Count - index);
			}
			Array.Clear(internalItems, Count, count);
			version++;
			for (var i = index; i < index + count; i++)
				OnItemDeleted(i, array[i - index]);
		}
	}

	/// <summary>Reverses this instance.</summary>
	public void Reverse() => Reverse(0, Count);

	/// <summary>Reverses the specified index.</summary>
	/// <param name="index">The index.</param>
	/// <param name="count">The count.</param>
	public void Reverse(int index, int count)
	{
		CheckRange(index, count);
		Array.Reverse(internalItems, index, count);
		version++;
	}

	/// <summary>Sorts this instance.</summary>
	public void Sort() => Sort(0, Count, null);

	/// <summary>Sorts the specified comparer.</summary>
	/// <param name="comparer">The comparer.</param>
	public void Sort(IComparer<T> comparer) => Sort(0, Count, comparer);

	/// <summary>Sorts the specified index.</summary>
	/// <param name="index">The index.</param>
	/// <param name="count">The count.</param>
	/// <param name="comparer">The comparer.</param>
	public void Sort(int index, int count, IComparer<T> comparer)
	{
		CheckRange(index, count);
		Array.Sort(internalItems, index, count, comparer);
		version++;
	}

	/// <summary>Toes the array.</summary>
	/// <returns></returns>
	public T[] ToArray()
	{
		var destinationArray = new T[Count];
		Array.Copy(internalItems, 0, destinationArray, 0, Count);
		return destinationArray;
	}

	/// <summary>Trims the excess.</summary>
	public void TrimExcess()
	{
		var num = (int)(internalItems.Length * 0.9);
		if (Count < num)
		{
			Capacity = Count;
		}
	}

	/// <summary>Trues for all.</summary>
	/// <param name="match">The match.</param>
	/// <returns></returns>
	public bool TrueForAll(Predicate<T> match)
	{
		if (match == null)
		{
			throw new ArgumentNullException(nameof(match));
		}
		for (var i = 0; i < Count; i++)
		{
			if (!match(internalItems[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Called when [insert].</summary>
	/// <param name="index">The index.</param>
	/// <param name="value">The value.</param>
	protected virtual void OnItemAdded(int index, T value)
	{
		if (value != null)
		{
			value.PropertyChanged += OnItemPropertyChanged;
			ItemAdded?.Invoke(this, new ListChangedEventArgs<T>(ListChangedType.ItemAdded, value, index));
		}
	}

	/// <summary>Called when [item property changed].</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
	protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) => ItemPropertyChanged?.Invoke(sender, e);

	/// <summary>Called when [set].</summary>
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
		ItemChanged?.Invoke(this, new ListChangedEventArgs<T>(ListChangedType.ItemChanged, newValue, index, oldValue));
	}

	/// <summary>Called when [remove].</summary>
	/// <param name="index">The index.</param>
	/// <param name="value">The value.</param>
	protected virtual void OnItemDeleted(int index, T value)
	{
		if (value != null)
		{
			value.PropertyChanged -= OnItemPropertyChanged;
			ItemDeleted?.Invoke(this, new ListChangedEventArgs<T>(ListChangedType.ItemDeleted, value, index));
		}
	}

	/// <summary>Called when [clear].</summary>
	protected virtual void OnReset()
	{
		ForEach(delegate (T item) { item.PropertyChanged -= OnItemPropertyChanged; });
		Reset?.Invoke(this, new ListChangedEventArgs<T>(ListChangedType.Reset));
	}

	/// <summary>Determines whether [is compatible object] [the specified value].</summary>
	/// <param name="value">The value.</param>
	/// <returns><c>true</c> if [is compatible object] [the specified value]; otherwise, <c>false</c>.</returns>
	private static bool IsCompatibleObject(object value) => value is T || (value == null && !typeof(T).IsValueType);

	/// <summary>Verifies the type of the value.</summary>
	/// <param name="value">The value.</param>
	private static void VerifyValueType(object value)
	{
		if (!IsCompatibleObject(value))
		{
			throw new ArgumentException(@"Incompatible object", nameof(value));
		}
	}

	/// <summary>Checks the index to ensure it is valid and in the list.</summary>
	/// <param name="idx">The index to validate.</param>
	/// <param name="varName">Name of the variable this is being checked.</param>
	/// <exception cref="ArgumentOutOfRangeException">Called with the index is out of range.</exception>
	private void CheckIndex(int idx, string varName = "index")
	{
		if (idx >= Count || idx < 0)
			throw new ArgumentOutOfRangeException(varName);
	}

	/// <summary>Checks the range.</summary>
	/// <param name="index">The index.</param>
	/// <param name="count">The count.</param>
	private void CheckRange(int index, int count)
	{
		if (index >= Count || index < 0)
			throw new ArgumentOutOfRangeException(nameof(index));
		if (count < 0 || Count - index < count)
			throw new ArgumentOutOfRangeException(nameof(count));
	}

	/// <summary>Ensures the capacity.</summary>
	/// <param name="min">The min.</param>
	private void EnsureCapacity(int min)
	{
		if (internalItems.Length < min)
		{
			var num = internalItems.Length == 0 ? 4 : internalItems.Length * 2;
			if (num < min)
			{
				num = min;
			}
			Capacity = num;
		}
	}

	/// <summary>Enumerates over the <see cref="EventedList{T}"/>.</summary>
	[Serializable,
	 StructLayout(LayoutKind.Sequential)]
	public struct Enumerator : IEnumerator<T>
	{
		private readonly EventedList<T> list;
		private int index;
		private readonly int version;

		/// <summary>Initializes a new instance of the <see cref="EventedList{T}.Enumerator"/> struct.</summary>
		/// <param name="list">The list.</param>
		internal Enumerator(EventedList<T> list)
		{
			this.list = list;
			index = 0;
			version = list.version;
			Current = default(T);
		}

		/// <summary>Gets the current.</summary>
		/// <value>The current.</value>
		public T Current { get; private set; }

		/// <summary>Gets the current.</summary>
		/// <value>The current.</value>
		object IEnumerator.Current
		{
			get
			{
				return (index == 0) || (index == list.Count + 1) ? throw new InvalidOperationException() : (object)Current;
			}
		}

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose() { }

		/// <summary>Sets the enumerator to its initial position, which is before the first element in the collection.</summary>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		void IEnumerator.Reset()
		{
			if (version != list.version)
			{
				throw new InvalidOperationException();
			}
			index = 0;
			Current = default(T);
		}

		/// <summary>Advances the enumerator to the next element of the collection.</summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public bool MoveNext()
		{
			if (version != list.version)
			{
				throw new InvalidOperationException();
			}
			if (index < list.Count)
			{
				Current = list.internalItems[index];
				index++;
				return true;
			}
			index = list.Count + 1;
			Current = default(T);
			return false;
		}
	}

	/// <summary>An <see cref="EventArgs"/> structure passed to events generated by an <see cref="EventedList{T}"/>.</summary>
	/// <typeparam name="T"></typeparam>
#pragma warning disable 693

	public class ListChangedEventArgs<T> : EventArgs
	{
		/// <summary>Initializes a new instance of the <see cref="EventedList{T}.ListChangedEventArgs{T}"/> class.</summary>
		/// <param name="type">The type of change.</param>
		public ListChangedEventArgs(ListChangedType type)
		{
			ItemIndex = -1;
			ListChangedType = type;
		}

		/// <summary>Initializes a new instance of the <see cref="EventedList{T}.ListChangedEventArgs{T}"/> class.</summary>
		/// <param name="type">The type of change.</param>
		/// <param name="item">The item that has changed.</param>
		/// <param name="itemIndex">Index of the changed item.</param>
		public ListChangedEventArgs(ListChangedType type, T item, int itemIndex)
		{
			Item = item;
			ItemIndex = itemIndex;
			ListChangedType = type;
		}

		/// <summary>Initializes a new instance of the <see cref="EventedList{T}.ListChangedEventArgs{T}"/> class.</summary>
		/// <param name="type">The type of change.</param>
		/// <param name="item">The item that has changed.</param>
		/// <param name="itemIndex">Index of the changed item.</param>
		/// <param name="oldItem">The old item when an item has changed.</param>
		public ListChangedEventArgs(ListChangedType type, T item, int itemIndex, T oldItem)
			: this(type, item, itemIndex) => OldItem = oldItem;

		/// <summary>Gets the item that has changed.</summary>
		/// <value>The item.</value>
		public T Item { get; }

		/// <summary>Gets the index of the item.</summary>
		/// <value>The index of the item.</value>
		public int ItemIndex { get; }

		/// <summary>Gets the type of change for the list.</summary>
		/// <value>The type of change for the list.</value>
		public ListChangedType ListChangedType { get; }

		/// <summary>Gets the item's previous value.</summary>
		/// <value>The old item.</value>
		public T OldItem { get; }
	}

#pragma warning restore 693
}
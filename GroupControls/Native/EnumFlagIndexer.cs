using System.Collections.Generic;

namespace System
{
	/// <summary>
	/// Structure to use in place of a enumerated type with the <see cref="FlagsAttribute"/> set. Allows for indexer access to flags and simplifies boolean logic.
	/// </summary>
	/// <example>
	/// Use this structure by replacing an enumerated type field for simpler access. Instead of:
	/// <code language="C#" title="Old way">
	/// var fileInfo = new FileInfo(@"C:\MyFile.txt");
	/// FileAttributes fileAttr = fileInfo.Attributes;
	/// if ((fileAttr &amp; FileAttributes.Hidden) != FileAttributes.Hidden)
	/// {
	///    Console.WriteLine("The file is hidden. Trying to unhide now.");
	///    fileInfo.Attributes = (fileAttr &amp; ~FileAttributes.Hidden);
	/// }
	/// </code>
	/// Do this instead:
	/// <code language="C#" title="New way">
	/// var fileInfo = new FileInfo(@"C:\MyFile.txt");
	/// EnumFlagIndexer&lt;FileAttributes&gt; fileAttr = fileInfo.Attributes;
	/// if (fileAttr[FileAttributes.Hidden])
	/// {
	///    Console.WriteLine("The file is hidden. Trying to unhide now.");
	///    fileAttr[FileAttributes.Hidden] = false;
	///    fileInfo.Attributes = fileAttr;
	/// }
	/// </code>
	/// </example>
	/// <typeparam name="E">Must be an enumerated type or constructor will fail.</typeparam>
	internal struct EnumFlagIndexer<E> where E : struct, IComparable
	{
		private E flags;

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumFlagIndexer{E}" /> struct.
		/// </summary>
		/// <param name="initialValue">The initial value. Defaults to <c>default(E)</c>.</param>
		/// <param name="checkFlagAttr">if set to <c>true</c> check for the [Flags] attribute and throw an exception if it is not set for this enumerated type.</param>
		public EnumFlagIndexer(E initialValue = default(E), bool checkFlagAttr = true)
		{
			if (!typeof(E).IsEnum)
				throw new ArgumentException($"Type '{typeof(E).FullName}' is not an enum");
			if (checkFlagAttr && !Attribute.IsDefined(typeof(E), typeof(FlagsAttribute)))
				throw new ArgumentException($"Type '{typeof(E).FullName}' doesn't have the 'Flags' attribute");
			flags = initialValue;
		}

		/// <summary>
		/// Gets or sets the specified flag.
		/// </summary>
		/// <value>A boolean value representing the presence of the specified enumerated flag.</value>
		/// <param name="flag">A value in the enumerated type to check.</param>
		/// <returns><c>true</c> if the flag is set; <c>false</c> otherwise.</returns>
		public bool this[E flag]
		{
			get { return (Convert.ToInt64(flags) & Convert.ToInt64(flag)) != 0; }
			set
			{
				long flagsValue = Convert.ToInt64(flags);
				long flagValue = Convert.ToInt64(flag);
				if (value)
					flagsValue |= flagValue;
				else
					flagsValue &= (~flagValue);
				flags = (E)Enum.ToObject(typeof(E), flagsValue);
			}
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="a">An instance of <see cref="EnumFlagIndexer{E}"/>.</param>
		/// <param name="b">An instance of the <typeparamref name="E"/> enumerated type.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(EnumFlagIndexer<E> a, E b) => !(a == b);

		/// <summary>
		/// Implements the operator &amp;.
		/// </summary>
		/// <param name="a">An instance of <see cref="EnumFlagIndexer{E}"/>.</param>
		/// <param name="b">An instance of the <typeparamref name="E"/> enumerated type.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static E operator &(EnumFlagIndexer<E> a, E b) => (E)Enum.ToObject(typeof(E), Convert.ToInt64(a.flags) & Convert.ToInt64(b));

		/// <summary>
		/// Implements the operator |.
		/// </summary>
		/// <param name="a">An instance of <see cref="EnumFlagIndexer{E}"/>.</param>
		/// <param name="b">An instance of the <typeparamref name="E"/> enumerated type.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static E operator |(EnumFlagIndexer<E> a, E b) => (E)Enum.ToObject(typeof(E), Convert.ToInt64(a.flags) | Convert.ToInt64(b));

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="a">An instance of <see cref="EnumFlagIndexer{E}"/>.</param>
		/// <param name="b">An instance of the <typeparamref name="E"/> enumerated type.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(EnumFlagIndexer<E> a, E b) => Convert.ToInt64(a.flags) == Convert.ToInt64(b);

		/// <summary>
		/// Implicitly converts an instance of <see cref="EnumFlagIndexer{E}"/> to the value of enumerated type E.
		/// </summary>
		/// <param name="f">The f.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static implicit operator E(EnumFlagIndexer<E> f) => f.flags;

		/// <summary>
		/// Implicitly converts a value of E to an instance of <see cref="EnumFlagIndexer{E}"/>.
		/// </summary>
		/// <param name="e">The e.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static implicit operator EnumFlagIndexer<E>(E e) => new EnumFlagIndexer<E>(e);

		/// <summary>
		/// Clears and sets to <c>default(E)</c>.
		/// </summary>
		public void Clear()
		{
			flags = default(E);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => Equals(obj, flags);

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => flags.GetHashCode();

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString() => flags.ToString();

		/// <summary>
		/// Unions the specified flags.
		/// </summary>
		/// <param name="flags">The flags.</param>
		public void Union(E flags)
		{
			this[flags] = true;
		}

		/// <summary>
		/// Unions the specified flags.
		/// </summary>
		/// <param name="flags">The flags.</param>
		public void Union(IEnumerable<E> flags)
		{
			foreach (E e in flags) this[e] = true;
		}
	}
}

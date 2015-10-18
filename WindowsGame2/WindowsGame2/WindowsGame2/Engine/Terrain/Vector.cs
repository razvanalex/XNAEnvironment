using System.Collections.Generic;
namespace Engine.Terrain.Collections.Generic
{

    [System.Diagnostics.DebuggerDisplay("{Count} items")]
    public sealed class Vector<T>
    {

        #region Fields

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private T[] _items = null;
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int _size = 0;

        #endregion


        #region Properties

        /// <summary>
        /// <para>Minimal size of an array if no size if specified.</para>
        /// </summary>
        public const int BaseArraySize = 8;

        /// <summary>
        /// <para>Gets the number of elements actually contained in the <see cref="Vector&lt;T&gt;"/>.</para>
        /// </summary>
        public int Count
        {
            get
            {
                return _size;
            }
            private set
            {
                _size = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets the value at the specified index.</para>
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public T this[int Index]
        {
            get
            {
                return _items[Index];
            }
            set
            {
                _items[Index] = value;
            }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// <para>Initialize a new instance of the <see cref="Vector&lt;T&gt;"/> class that is empty and has the default initial capacity.</para>
        /// </summary>
        public Vector()
            : this(Vector<T>.BaseArraySize)
        {
        }

        /// <summary>
        /// <para>Initialize a new instance of the <see cref="Vector&lt;T&gt;"/> class that is empty and has the specified capacity.</para>
        /// </summary>
        /// <param name="capacity">The number of elements that the <see cref="Vector&lt;T&gt;"/> can initialy store.</param>
        public Vector(int capacity)
        {
            this.Clear(capacity);
        }

        #endregion


        #region Methods

        /// <summary>
        /// <para>Remove the element at the specified index of the <see cref="Vector&lt;T&gt;"/>.</para>
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index)
        {
            this._size--;
            if (index < this._size)
            {
                System.Array.Copy(this._items, index + 1, this._items, index, this._size - index);
            }
            this._items[this._size] = default(T);
        }

        /// <summary>
        /// <para>Remove the fist occurence of a specific object from the <see cref="Vector&lt;T&gt;"/>.</para>
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="Vector&lt;T&gt;"/>. The value can be null for reference type.</param>
        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// <para>Determines whether an element is in the <see cref="Vector&lt;T&gt;"/>.</para>
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="Vector&lt;T&gt;"/>. The value can be null for reference type.</param>
        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int j = 0; j < this._size; j++)
                {
                    if (this._items[j] == null)
                    {
                        return true;
                    }
                }
                return false;
            }
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < this._size; i++)
            {
                if (comparer.Equals(this._items[i], item))
                {
                    return true;
                }
            }
            return false;
        }
 
        /// <summary>
        /// <para>Searches for the specified index and returns an index of the first occurence.</para>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            return System.Array.IndexOf(this._items, item);
        }


        /// <summary>
        /// <para>Remove all elements from the <see cref="Vector&lt;T&gt;"/>.</para>
        /// </summary>
        public void Clear()
        {
            this.Clear(Vector<T>.BaseArraySize);
        }

        /// <summary>
        /// <para>Remove all elements from the <see cref="Vector&lt;T&gt;"/>.</para>
        /// </summary>
        /// <param name="capacity">The number of elements that the <see cref="Vector&lt;T&gt;"/> can store after reset.</param>
        public void Clear(int capacity)
        {
            this._items = new T[capacity];
            this._size = 0;
        }

        /// <summary>
        /// <para>Adds an object to the end of the <see cref="Vector&lt;T&gt;"/>.</para>
        /// </summary>
        /// <param name="item">The object to be added to the end of the <see cref="Vector&lt;T&gt;"/>. The value can be null for reference type.</param>
        public void Add(T item)
        {
            this._items[this.Count] = item;
            this.Count++;
            if (this.Count >= this._items.Length)
            {
                System.Array.Resize<T>(ref this._items, this._items.Length << 1);
            }
            return;
        }

        /// <summary>
        /// <para>Copies the elements of the <see cref="Vector&lt;T&gt;"/> to a new array.</para>
        /// </summary>
        /// <param name="reserved"></param>
        public T[] ToArray()
        {
            if (this.Count == this._items.Length)
            {
                return _items;
            }
            T[] newArray = new T[this.Count];
            System.Array.Copy(this._items, newArray, this.Count);
            return newArray;
        }

        #endregion

    }

}

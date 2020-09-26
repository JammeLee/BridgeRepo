using System.Runtime.Serialization;
using System.Threading;

namespace System.Collections.Generic
{
	[Serializable]
	internal class TreeSet<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable, ISerializable, IDeserializationCallback
	{
		internal class Node
		{
			private bool isRed;

			private T item;

			private Node left;

			private Node right;

			public T Item
			{
				get
				{
					return item;
				}
				set
				{
					item = value;
				}
			}

			public Node Left
			{
				get
				{
					return left;
				}
				set
				{
					left = value;
				}
			}

			public Node Right
			{
				get
				{
					return right;
				}
				set
				{
					right = value;
				}
			}

			public bool IsRed
			{
				get
				{
					return isRed;
				}
				set
				{
					isRed = value;
				}
			}

			public Node(T item)
			{
				this.item = item;
				isRed = true;
			}

			public Node(T item, bool isRed)
			{
				this.item = item;
				this.isRed = isRed;
			}
		}

		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private const string TreeName = "Tree";

			private const string NodeValueName = "Item";

			private const string EnumStartName = "EnumStarted";

			private const string VersionName = "Version";

			private TreeSet<T> tree;

			private int version;

			private Stack<Node> stack;

			private Node current;

			private static Node dummyNode = new Node(default(T));

			public T Current
			{
				get
				{
					if (current != null)
					{
						return current.Item;
					}
					return default(T);
				}
			}

			object IEnumerator.Current
			{
				get
				{
					if (current == null)
					{
						ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
					}
					return current.Item;
				}
			}

			internal bool NotStartedOrEnded => current == null;

			internal Enumerator(TreeSet<T> set)
			{
				tree = set;
				version = tree.version;
				stack = new Stack<Node>(2 * (int)Math.Log(set.Count + 1));
				current = null;
				Intialize();
			}

			private void Intialize()
			{
				current = null;
				for (Node node = tree.root; node != null; node = node.Left)
				{
					stack.Push(node);
				}
			}

			public bool MoveNext()
			{
				if (version != tree.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				if (stack.Count == 0)
				{
					current = null;
					return false;
				}
				current = stack.Pop();
				for (Node node = current.Right; node != null; node = node.Left)
				{
					stack.Push(node);
				}
				return true;
			}

			public void Dispose()
			{
			}

			internal void Reset()
			{
				if (version != tree.version)
				{
					ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
				}
				stack.Clear();
				Intialize();
			}

			void IEnumerator.Reset()
			{
				Reset();
			}
		}

		private const string ComparerName = "Comparer";

		private const string CountName = "Count";

		private const string ItemsName = "Items";

		private const string VersionName = "Version";

		private Node root;

		private IComparer<T> comparer;

		private int count;

		private int version;

		private object _syncRoot;

		private SerializationInfo siInfo;

		public int Count => count;

		public IComparer<T> Comparer => comparer;

		bool ICollection<T>.IsReadOnly => false;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				}
				return _syncRoot;
			}
		}

		public TreeSet(IComparer<T> comparer)
		{
			if (comparer == null)
			{
				this.comparer = Comparer<T>.Default;
			}
			else
			{
				this.comparer = comparer;
			}
		}

		protected TreeSet(SerializationInfo info, StreamingContext context)
		{
			siInfo = info;
		}

		public void Add(T item)
		{
			if (root == null)
			{
				root = new Node(item, isRed: false);
				count = 1;
				return;
			}
			Node node = root;
			Node parent = null;
			Node node2 = null;
			Node greatGrandParent = null;
			int num = 0;
			while (node != null)
			{
				num = comparer.Compare(item, node.Item);
				if (num == 0)
				{
					root.IsRed = false;
					ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
				}
				if (Is4Node(node))
				{
					Split4Node(node);
					if (IsRed(parent))
					{
						InsertionBalance(node, ref parent, node2, greatGrandParent);
					}
				}
				greatGrandParent = node2;
				node2 = parent;
				parent = node;
				node = ((num < 0) ? node.Left : node.Right);
			}
			Node node3 = new Node(item);
			if (num > 0)
			{
				parent.Right = node3;
			}
			else
			{
				parent.Left = node3;
			}
			if (parent.IsRed)
			{
				InsertionBalance(node3, ref parent, node2, greatGrandParent);
			}
			root.IsRed = false;
			count++;
			version++;
		}

		public void Clear()
		{
			root = null;
			count = 0;
			version++;
		}

		public bool Contains(T item)
		{
			return FindNode(item) != null;
		}

		internal bool InOrderTreeWalk(TreeWalkAction<T> action)
		{
			if (root == null)
			{
				return true;
			}
			Stack<Node> stack = new Stack<Node>(2 * (int)Math.Log(Count + 1));
			for (Node left = root; left != null; left = left.Left)
			{
				stack.Push(left);
			}
			while (stack.Count != 0)
			{
				Node left = stack.Pop();
				if (!action(left))
				{
					return false;
				}
				for (Node node = left.Right; node != null; node = node.Left)
				{
					stack.Push(node);
				}
			}
			return true;
		}

		public void CopyTo(T[] array, int index)
		{
			if (array == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
			}
			if (index < 0)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
			}
			if (array.Length - index < Count)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			InOrderTreeWalk(delegate(Node node)
			{
				array[index++] = node.Item;
				return true;
			});
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
			}
			if (array.Rank != 1)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
			}
			if (array.GetLowerBound(0) != 0)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
			}
			if (index < 0)
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
			}
			if (array.Length - index < Count)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
			}
			T[] array2 = array as T[];
			if (array2 != null)
			{
				CopyTo(array2, index);
				return;
			}
			object[] objects = array as object[];
			if (objects == null)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
			}
			try
			{
				InOrderTreeWalk(delegate(Node node)
				{
					objects[index++] = node.Item;
					return true;
				});
			}
			catch (ArrayTypeMismatchException)
			{
				ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		internal Node FindNode(T item)
		{
			Node node = root;
			while (node != null)
			{
				int num = comparer.Compare(item, node.Item);
				if (num == 0)
				{
					return node;
				}
				node = ((num < 0) ? node.Left : node.Right);
			}
			return null;
		}

		public bool Remove(T item)
		{
			if (root == null)
			{
				return false;
			}
			Node node = root;
			Node node2 = null;
			Node node3 = null;
			Node node4 = null;
			Node parentOfMatch = null;
			bool flag = false;
			while (node != null)
			{
				if (Is2Node(node))
				{
					if (node2 == null)
					{
						node.IsRed = true;
					}
					else
					{
						Node node5 = GetSibling(node, node2);
						if (node5.IsRed)
						{
							if (node2.Right == node5)
							{
								RotateLeft(node2);
							}
							else
							{
								RotateRight(node2);
							}
							node2.IsRed = true;
							node5.IsRed = false;
							ReplaceChildOfNodeOrRoot(node3, node2, node5);
							node3 = node5;
							if (node2 == node4)
							{
								parentOfMatch = node5;
							}
							node5 = ((node2.Left == node) ? node2.Right : node2.Left);
						}
						if (Is2Node(node5))
						{
							Merge2Nodes(node2, node, node5);
						}
						else
						{
							TreeRotation treeRotation = RotationNeeded(node2, node, node5);
							Node node6 = null;
							switch (treeRotation)
							{
							case TreeRotation.RightRotation:
								node5.Left.IsRed = false;
								node6 = RotateRight(node2);
								break;
							case TreeRotation.LeftRotation:
								node5.Right.IsRed = false;
								node6 = RotateLeft(node2);
								break;
							case TreeRotation.RightLeftRotation:
								node6 = RotateRightLeft(node2);
								break;
							case TreeRotation.LeftRightRotation:
								node6 = RotateLeftRight(node2);
								break;
							}
							node6.IsRed = node2.IsRed;
							node2.IsRed = false;
							node.IsRed = true;
							ReplaceChildOfNodeOrRoot(node3, node2, node6);
							if (node2 == node4)
							{
								parentOfMatch = node6;
							}
							node3 = node6;
						}
					}
				}
				int num = (flag ? (-1) : comparer.Compare(item, node.Item));
				if (num == 0)
				{
					flag = true;
					node4 = node;
					parentOfMatch = node2;
				}
				node3 = node2;
				node2 = node;
				node = ((num >= 0) ? node.Right : node.Left);
			}
			if (node4 != null)
			{
				ReplaceNode(node4, parentOfMatch, node2, node3);
				count--;
			}
			if (root != null)
			{
				root.IsRed = false;
			}
			version++;
			return flag;
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			GetObjectData(info, context);
		}

		protected void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.info);
			}
			info.AddValue("Count", count);
			info.AddValue("Comparer", comparer, typeof(IComparer<T>));
			info.AddValue("Version", version);
			if (root != null)
			{
				T[] array = new T[Count];
				CopyTo(array, 0);
				info.AddValue("Items", array, typeof(T[]));
			}
		}

		void IDeserializationCallback.OnDeserialization(object sender)
		{
			OnDeserialization(sender);
		}

		protected void OnDeserialization(object sender)
		{
			if (comparer != null)
			{
				return;
			}
			if (siInfo == null)
			{
				ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_InvalidOnDeser);
			}
			comparer = (IComparer<T>)siInfo.GetValue("Comparer", typeof(IComparer<T>));
			int @int = siInfo.GetInt32("Count");
			if (@int != 0)
			{
				T[] array = (T[])siInfo.GetValue("Items", typeof(T[]));
				if (array == null)
				{
					ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_MissingValues);
				}
				for (int i = 0; i < array.Length; i++)
				{
					Add(array[i]);
				}
			}
			version = siInfo.GetInt32("Version");
			if (count != @int)
			{
				ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_MismatchedCount);
			}
			siInfo = null;
		}

		private static Node GetSibling(Node node, Node parent)
		{
			if (parent.Left == node)
			{
				return parent.Right;
			}
			return parent.Left;
		}

		private void InsertionBalance(Node current, ref Node parent, Node grandParent, Node greatGrandParent)
		{
			bool flag = grandParent.Right == parent;
			bool flag2 = parent.Right == current;
			Node node;
			if (flag == flag2)
			{
				node = (flag2 ? RotateLeft(grandParent) : RotateRight(grandParent));
			}
			else
			{
				node = (flag2 ? RotateLeftRight(grandParent) : RotateRightLeft(grandParent));
				parent = greatGrandParent;
			}
			grandParent.IsRed = true;
			node.IsRed = false;
			ReplaceChildOfNodeOrRoot(greatGrandParent, grandParent, node);
		}

		private static bool Is2Node(Node node)
		{
			if (IsBlack(node) && IsNullOrBlack(node.Left))
			{
				return IsNullOrBlack(node.Right);
			}
			return false;
		}

		private static bool Is4Node(Node node)
		{
			if (IsRed(node.Left))
			{
				return IsRed(node.Right);
			}
			return false;
		}

		private static bool IsBlack(Node node)
		{
			if (node != null)
			{
				return !node.IsRed;
			}
			return false;
		}

		private static bool IsNullOrBlack(Node node)
		{
			if (node != null)
			{
				return !node.IsRed;
			}
			return true;
		}

		private static bool IsRed(Node node)
		{
			return node?.IsRed ?? false;
		}

		private static void Merge2Nodes(Node parent, Node child1, Node child2)
		{
			parent.IsRed = false;
			child1.IsRed = true;
			child2.IsRed = true;
		}

		private void ReplaceChildOfNodeOrRoot(Node parent, Node child, Node newChild)
		{
			if (parent != null)
			{
				if (parent.Left == child)
				{
					parent.Left = newChild;
				}
				else
				{
					parent.Right = newChild;
				}
			}
			else
			{
				root = newChild;
			}
		}

		private void ReplaceNode(Node match, Node parentOfMatch, Node succesor, Node parentOfSuccesor)
		{
			if (succesor == match)
			{
				succesor = match.Left;
			}
			else
			{
				if (succesor.Right != null)
				{
					succesor.Right.IsRed = false;
				}
				if (parentOfSuccesor != match)
				{
					parentOfSuccesor.Left = succesor.Right;
					succesor.Right = match.Right;
				}
				succesor.Left = match.Left;
			}
			if (succesor != null)
			{
				succesor.IsRed = match.IsRed;
			}
			ReplaceChildOfNodeOrRoot(parentOfMatch, match, succesor);
		}

		internal void UpdateVersion()
		{
			version++;
		}

		private static Node RotateLeft(Node node)
		{
			Node right = node.Right;
			node.Right = right.Left;
			right.Left = node;
			return right;
		}

		private static Node RotateLeftRight(Node node)
		{
			Node left = node.Left;
			Node right = left.Right;
			node.Left = right.Right;
			right.Right = node;
			left.Right = right.Left;
			right.Left = left;
			return right;
		}

		private static Node RotateRight(Node node)
		{
			Node left = node.Left;
			node.Left = left.Right;
			left.Right = node;
			return left;
		}

		private static Node RotateRightLeft(Node node)
		{
			Node right = node.Right;
			Node left = right.Left;
			node.Right = left.Left;
			left.Left = node;
			right.Left = left.Right;
			left.Right = right;
			return left;
		}

		private static TreeRotation RotationNeeded(Node parent, Node current, Node sibling)
		{
			if (IsRed(sibling.Left))
			{
				if (parent.Left == current)
				{
					return TreeRotation.RightLeftRotation;
				}
				return TreeRotation.RightRotation;
			}
			if (parent.Left == current)
			{
				return TreeRotation.LeftRotation;
			}
			return TreeRotation.LeftRightRotation;
		}

		private static void Split4Node(Node node)
		{
			node.IsRed = true;
			node.Left.IsRed = false;
			node.Right.IsRed = false;
		}
	}
}

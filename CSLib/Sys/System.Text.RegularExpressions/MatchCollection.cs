using System.Collections;

namespace System.Text.RegularExpressions
{
	[Serializable]
	public class MatchCollection : ICollection, IEnumerable
	{
		internal Regex _regex;

		internal ArrayList _matches;

		internal bool _done;

		internal string _input;

		internal int _beginning;

		internal int _length;

		internal int _startat;

		internal int _prevlen;

		private static int infinite = int.MaxValue;

		public int Count
		{
			get
			{
				if (_done)
				{
					return _matches.Count;
				}
				GetMatch(infinite);
				return _matches.Count;
			}
		}

		public object SyncRoot => this;

		public bool IsSynchronized => false;

		public bool IsReadOnly => true;

		public virtual Match this[int i]
		{
			get
			{
				Match match = GetMatch(i);
				if (match == null)
				{
					throw new ArgumentOutOfRangeException("i");
				}
				return match;
			}
		}

		internal MatchCollection(Regex regex, string input, int beginning, int length, int startat)
		{
			if (startat < 0 || startat > input.Length)
			{
				throw new ArgumentOutOfRangeException("startat", SR.GetString("BeginIndexNotNegative"));
			}
			_regex = regex;
			_input = input;
			_beginning = beginning;
			_length = length;
			_startat = startat;
			_prevlen = -1;
			_matches = new ArrayList();
			_done = false;
		}

		internal Match GetMatch(int i)
		{
			if (i < 0)
			{
				return null;
			}
			if (_matches.Count > i)
			{
				return (Match)_matches[i];
			}
			if (_done)
			{
				return null;
			}
			Match match;
			do
			{
				match = _regex.Run(quick: false, _prevlen, _input, _beginning, _length, _startat);
				if (!match.Success)
				{
					_done = true;
					return null;
				}
				_matches.Add(match);
				_prevlen = match._length;
				_startat = match._textpos;
			}
			while (_matches.Count <= i);
			return match;
		}

		public void CopyTo(Array array, int arrayIndex)
		{
			_ = Count;
			_matches.CopyTo(array, arrayIndex);
		}

		public IEnumerator GetEnumerator()
		{
			return new MatchEnumerator(this);
		}
	}
}

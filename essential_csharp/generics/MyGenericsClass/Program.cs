namespace MyGenericsClass;

// How to use generic type?
class Stack<T>
{
	private T[] _internal_items { get; }
	private int _last_index; 
	private const int MAX_NUM = 100;
	
	public Stack() {
		_last_index = 0;
		_internal_items = new T[MAX_NUM];
	}

	public void Push(T data) {
		if (_last_index == MAX_NUM) {
			throw new Exception("Stack overflow.");
		}			
		
		_internal_items[_last_index] = data;
		_last_index += 1;
	}

	public T Pop() {
		if (_last_index == 0)
		{
			throw new Exception("There is no item in Stack.");
		}

		T removed_element = _internal_items[_last_index-1];
		_last_index -= 1;

		return removed_element;
	}
}

// How to use multiple generic types?
class Pair<TFirst, TSecond>
{
	private TFirst _first {get; }
	private TSecond _second {get; }

	public Pair(TFirst first, TSecond second) {
		_first = first;
		_second = second;
	}

	public Pair(TFirst first)
	{
		_first = first;
		_second = default(TSecond);
	}

	public TFirst get_first() {
		return _first;
	}
	
	public TSecond get_second() {
		return _second;
	}
}

static class Math
{
	public static T Max<T>(T[] items) 
		where T : IComparable<T> 	// Constraint needed because how otherwise can we compare a non-comparable values to find a maximum? 
	{
		T maximum = items[0];
		
		foreach (T item in items) {
			if (item.CompareTo(maximum) > 0) {
				maximum = item;
			}
		}	
	
		return maximum;
	}
	
	public static bool Contains<T>(T searched_item, T[] items) 
	{
		foreach (T item in items) {
			if (searched_item.Equals(item)) {
				return true;
			}
		}	
	
		return false;
	}
}

class MyGeneric
{
	public static void Main(string[] args)
	{
		int[] arr = Enumerable.Range(10, 20).ToArray(); // Array 10..29
		Console.WriteLine(Math.Max(arr));
		Console.WriteLine(Math.Contains(11, arr));
		Console.WriteLine(Math.Contains(30, arr));
	}
}

<Query Kind="Program" />

void Main()
{
	var vertices = new List<string>
	{
		"Zero",
		"One",
		"Two",
		"Three",
		"Four"
	};
	
	var edges = new List<UnorderedPair<string>>
	{
		new("Zero", "One"),
		new("One", "Four"),
		new("Two", "Three"),
		new("Two", "Four"),
		new("Three", "Four")
	};
	
	var result = Tarjan.GetArticulationPoints(vertices, edges);
	Console.WriteLine($"[{string.Join(", ", result)}]");
}

public class VertexState<T>
{
	public T Value { get; }
	public List<VertexState<T>> Adjacency { get; }
	public bool IsVisited { get; private set; }
	public bool IsArticulationPoint { get; private set; }
	public int DiscoveredTime { get; private set; }
	public int LowTime { get; set; }
	
	public VertexState(T value)
	{
		Value = value;
		Adjacency = new List<VertexState<T>>();
		IsVisited = false;
		IsArticulationPoint = false;
		DiscoveredTime = 0;
		LowTime = 0;
	}

	public void SetVisited(int time)
	{
		DiscoveredTime = time;
		LowTime = time;
		IsVisited = true;
	}
	
	public void SetArticulationPoint()
	{
		IsArticulationPoint = true;
	}
}

static class Tarjan
{
	// Main function to find articulation points in the graph
	public static List<T> GetArticulationPoints<T>(
		IEnumerable<T> vertices,
		IEnumerable<UnorderedPair<T>> edges)
	{
		var items = InitialiseState(vertices, edges);
		var time = 0;

		// Run depth-first search from each vertex if not already visited (to handle DiscoveredTimeonnected graphs)
		foreach (var item in items)
		{
			if (!item.IsVisited)
			{
				FindPoints(item, null, ref time);
			}
		}

		// Collect all vertices that are articulation points
		return items
			.Where(s => s.IsArticulationPoint)
			.Select(s => s.Value)
			.ToList();
	}

	private static List<VertexState<T>> InitialiseState<T>(
		IEnumerable<T> vertices,
		IEnumerable<UnorderedPair<T>> edges)
	{
		var state = vertices.ToDictionary(
			v => v,
			v => new VertexState<T>(v));
		
		foreach (var (u, v) in edges)
		{
			var ustate = state[u];
			var vstate = state[v];
			
			ustate.Adjacency.Add(vstate);
			vstate.Adjacency.Add(ustate);
		}

		return state.Values.ToList();
	}

	// Helper function to perform DFS and find articulation points using Tarjan's algorithm.
	private static void FindPoints<T>(
		VertexState<T> current,
		VertexState<T> parent,
		ref int time)
	{
		// Mark vertex u as visited and assign DiscoveredTimeovery time and LowTime value
		time++;
		current.SetVisited(time);
		
		var children = 0;

		foreach (var other in current.Adjacency)
		{
			if (!other.IsVisited)
			{
				children++;
				FindPoints(other, current, ref time);

				// Check if the subtree rooted at v has a connection to one of the ancestors of u
				if (other.LowTime < current.LowTime)
				{
					current.LowTime = other.LowTime;
				}

				// If u is not a root and LowTime[v] is greater than or equal to DiscoveredTime[u], then u is an articulation point
				if (parent is not null && other.LowTime >= current.DiscoveredTime)
				{
					current.SetArticulationPoint();
				}
			}
			else if (other != parent && other.DiscoveredTime < current.LowTime)
			{
				// Update LowTime value of u for back edge
				current.LowTime = other.DiscoveredTime;
			}
		}

		// If current vertex is root of DFS tree and has more than one child, it is an articulation point
		if (parent is null && children > 1)
		{
			current.SetArticulationPoint();
		}
	}
}

public readonly struct UnorderedPair<T>(T item1, T item2)
	: IEquatable<UnorderedPair<T>>
{
	private readonly T _v0 = item1 ?? throw new ArgumentNullException(nameof(item1));
	private readonly T _v1 = item2 ?? throw new ArgumentNullException(nameof(item2));

	public IEnumerable<T> Values
	{
		get
		{
			yield return _v0;
			yield return _v1;
		}
	}

	public T this[int index] => index switch
	{
		0 => _v0,
		1 => _v1,
		_ => throw new IndexOutOfRangeException("Index must be 0 or 1.")
	};

	public void Deconstruct(out T item1, out T item2)
	{
		item1 = _v0;
		item2 = _v1;
	}

	public bool EqualsEither(T value)
	{
		return _v0!.Equals(value) || _v1!.Equals(value);
	}

	public bool Equals(UnorderedPair<T> other)
	{
		return _v0!.Equals(other[0]) && _v1!.Equals(other[1]) ||
			   _v0!.Equals(other[1]) && _v1!.Equals(other[0]);
	}

	public override bool Equals(object? obj)
	{
		return obj is UnorderedPair<T> pair && pair.Equals(this);
	}

	public override int GetHashCode()
	{
		var hashX = _v0?.GetHashCode() ?? 0;
		var hashY = _v1?.GetHashCode() ?? 0;

		unchecked
		{
			return hashX > hashY
				? hashY * 127 + hashX
				: hashX * 127 + hashY;
		}
	}

	public static bool operator ==(UnorderedPair<T> left, UnorderedPair<T> right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(UnorderedPair<T> left, UnorderedPair<T> right)
	{
		return !Equals(left, right);
	}

	public static implicit operator UnorderedPair<T>((T, T) pair)
	{
		return new(pair.Item1, pair.Item2);
	}

	public override string ToString()
	{
		return $"{{{_v0},{_v1}}}";
	}
}

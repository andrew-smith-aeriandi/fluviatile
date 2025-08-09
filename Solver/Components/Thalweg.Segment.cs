using Solver.Framework;
using System.Runtime.CompilerServices;
using System.Text;

namespace Solver.Components;

public partial class Thalweg
{
    public class Segment : ISegment
    {
        private readonly LinkedList<ILinkable> _links;
        private readonly Thalweg _thalweg;
        private int _terminationCount;
        private int _rotation;
        private string _string;

        public Segment(Thalweg thalweg, params IEnumerable<ILinkable> links)
        {
            _thalweg = thalweg;
            _links = new LinkedList<ILinkable>();
            _terminationCount = 0;
            _rotation = 0;
            _string = string.Empty;

            foreach (var link in links)
            {
                AddToLast(link);
            }

            _thalweg._segments.Add(this);
        }

        public Thalweg Thalweg => _thalweg;

        public int Count => _links.Count;

        public int TileCount => _links.Count - _terminationCount;

        public int TerminationCount => _terminationCount;

        public int Rotation => _rotation;

        public ILinkable? First => _links.First?.Value;

        public ILinkable? Last => _links.Last?.Value;

        public IEnumerable<ILinkable> Links => _links;

        public IEnumerable<Tile> Tiles => _links.OfType<Tile>();

        public IEnumerable<Termination> Terminations
        {
            get
            {
                if (_links.Count >= 1 && _links.First!.Value is Termination first)
                {
                    yield return first;
                }

                if (_links.Count >= 2 && _links.Last!.Value is Termination last)
                {
                    yield return last;
                }
            }
        }

        internal void AddToFirst(ILinkable link)
        {
            ArgumentNullException.ThrowIfNull(link);

            var self = _links.First;

            if (_links.Count >= 2 && self?.Value is Termination)
            {
                throw new InvalidOperationException("Cannot add a link to a termination.");
            }

            if (_thalweg._membership.ContainsKey(link))
            {
                throw new InvalidOperationException("Link is already in a segment.");
            }

            _links.AddFirst(link);
            _thalweg._membership[link] = this;
            _rotation += GetRotation(self);

            if (link is Termination termination)
            {
                _terminationCount += 1;
                _thalweg._terminations.Add(termination);
            }

            InvalidateStringRepresention();
        }

        internal void AddLastToFirst(Segment segment)
        {
            ArgumentNullException.ThrowIfNull(segment);

            if (ReferenceEquals(segment, this))
            {
                throw new ArgumentException("Cannot join segment to itself.", nameof(segment));
            }

            var self = _links.First;
            var other = segment._links.Last;

            if (self?.Value is Termination || other?.Value is Termination)
            {
                throw new InvalidOperationException("Cannot join segments at a termination.");
            }

            _thalweg._segments.Remove(segment);

            if (segment.Count == 0)
            {
                // Nothing more to do
                return;
            }

            // Join the end of other segment to the start of this segment
            var node = other;
            while (node is not null)
            {
                _links.AddFirst(node.Value);
                _thalweg._membership[node.Value] = this;
                node = node.Previous;
            }

            _rotation += GetRotation(self) + GetRotation(self?.Previous) + segment.Rotation;
            _terminationCount += segment.TerminationCount;

            InvalidateStringRepresention();
        }

        internal void AddFirstToFirst(Segment segment)
        {
            ArgumentNullException.ThrowIfNull(segment);

            if (ReferenceEquals(segment, this))
            {
                throw new ArgumentException("Cannot join segment to itself.", nameof(segment));
            }

            var self = _links.First;
            var other = segment._links.First;

            if (self?.Value is Termination || other?.Value is Termination)
            {
                throw new InvalidOperationException("Cannot join segments at a termination.");
            }

            _thalweg._segments.Remove(segment);

            if (segment.Count == 0)
            {
                // Nothing more to do
                return;
            }

            // Join the start of other segment to the start of this segment
            var node = other;
            while (node is not null)
            {
                _links.AddFirst(node.Value);
                _thalweg._membership[node.Value] = this;
                node = node.Next;
            }

            _rotation += GetRotation(self) + GetRotation(self?.Previous) - segment.Rotation;
            _terminationCount += segment.TerminationCount;

            InvalidateStringRepresention();
        }

        internal void AddToLast(ILinkable link)
        {
            ArgumentNullException.ThrowIfNull(link);

            var self = _links.Last;

            if (_links.Count >= 2 && self?.Value is Termination)
            {
                throw new InvalidOperationException("Cannot add a link to a termination.");
            }

            if (_thalweg._membership.ContainsKey(link))
            {
                throw new InvalidOperationException("Link is already in a segment.");
            }

            _links.AddLast(link);
            _thalweg._membership[link] = this;
            _rotation += GetRotation(self);

            if (link is Termination termination)
            {
                _terminationCount += 1;
                _thalweg._terminations.Add(termination);
            }

            InvalidateStringRepresention();
        }

        internal void AddFirstToLast(Segment segment)
        {
            ArgumentNullException.ThrowIfNull(segment);

            if (ReferenceEquals(segment, this))
            {
                throw new ArgumentException("Cannot join segment to itself.", nameof(segment));
            }

            var self = _links.Last;
            var other = segment._links.First;

            if (self?.Value is Termination || other?.Value is Termination)
            {
                throw new InvalidOperationException("Cannot join segments at a termination.");
            }

            _thalweg._segments.Remove(segment);

            if (segment.Count == 0)
            {
                // Nothing more to do
                return;
            }

            // Join the start of other segment to the end of this segment
            var node = other;
            while (node is not null)
            {
                _links.AddLast(node.Value);
                _thalweg._membership[node.Value] = this;
                node = node.Next;
            }

            _rotation += GetRotation(self) + GetRotation(self?.Next) + segment.Rotation;
            _terminationCount += segment.TerminationCount;

            InvalidateStringRepresention();
        }

        internal void AddLastToLast(Segment segment)
        {
            ArgumentNullException.ThrowIfNull(segment);

            if (ReferenceEquals(segment, this))
            {
                throw new ArgumentException("Cannot join segment to itself.", nameof(segment));
            }

            var self = _links.Last;
            var other = segment._links.Last;

            if (self?.Value is Termination || other?.Value is Termination)
            {
                throw new InvalidOperationException("Cannot join segments at a termination.");
            }

            _thalweg._segments.Remove(segment);

            if (segment.Count == 0)
            {
                // Nothing more to do
                return;
            }

            // Join the end of other segment to the end of this segment
            var node = other;
            while (node is not null)
            {
                _links.AddLast(node.Value);
                _thalweg._membership[node.Value] = this;
                node = node.Previous;
            }

            _rotation +=  GetRotation(self) + GetRotation(self?.Next) - segment.Rotation;
            _terminationCount += segment.TerminationCount;

            InvalidateStringRepresention();
        }

        private static int GetRotation(LinkedListNode<ILinkable>? node)
        {
            if (node is null || node.Previous?.Value is null || node.Value is null || node.Next?.Value is null)
            {
                return 0;
            }

            var p1 = node.Previous.Value.Coordinates;
            var p2 = node.Value.Coordinates;
            var p3 = node.Next.Value.Coordinates;

            return ((p2.X - p1.X) * (p3.Y - p2.Y) - (p2.Y - p1.Y) * (p3.X - p2.X)) / Grid.Scale;
        }

        private static string GetStringRepresentation(LinkedList<ILinkable> links)
        {
            if (links.Count == 0)
            {
                return "[]";
            }

            var builder = new StringBuilder();
            var count = 0;
            var startIsTerminal = links.First?.Value.IsTerminal;
            var endIsTerminal = links.Last?.Value.IsTerminal;

            builder.Append('[');

            if (startIsTerminal == false)
            {
                builder.Append('?');
                count += 1;
            }

            var node = links.First;
            while (node is not null)
            {
                if (node.Value.IsTerminal)
                {
                    node = node.Next;
                    continue;
                }

                if (count > 0)
                {
                    builder.Append(',').Append(' ');
                }

                builder.Append(node.Value.Coordinates.ToString());

                count += 1;
                node = node.Next;
            }

            if (endIsTerminal == false)
            {
                if (count > 0)
                {
                    builder.Append(',').Append(' ');
                }

                builder.Append('?');
            }

            builder.Append(']');
            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvalidateStringRepresention()
        {
            _string = string.Empty;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_string))
            {
                _string = GetStringRepresentation(_links);
            }

            return _string;
        }
    }
}

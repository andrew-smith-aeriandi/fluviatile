using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Fluviatile.Grid
{
    public static class StepExtensions
    {
        private static readonly ByteArrayEqualityComparer ByteArrayComparer = new();

        public static Step TakeStep(this Step step, Torsion twist)
        {
            var direction = step.Direction.Turn(twist);
            var node = step.Node.Links[direction];

            return new Step(node, direction, twist, step);
        }

        public static byte[] GetPath(this Step lastStep)
        {
            var step = lastStep;
            var path = new byte[lastStep.Count];

            while (step.Previous is not null)
            {
                var twist = step.Twist.Value;
                path[step.Count - 1] = (byte)(twist >= 0 ? twist : twist + 256);
                step = step.Previous;
            }

            return path;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte InvertDirection(byte value)
        {
            return (byte)(value > 0 ? 256 - value : 0);
        }

        public static IEnumerable<byte[]> GetEquivalentPathsWithIdentitySymmetry(this Step lastStep)
        {
            yield return lastStep.GetPath();
        }

        public static IEnumerable<byte[]> GetEquivalentPathsWithRotationalSymmetry(this Step lastStep)
        {
            var path = lastStep.GetPath();
            var rotatedPath = new byte[path.Length];

            var maxIndex = path.Length - 1;
            for (var index = 0; index <= maxIndex; index++)
            {
                rotatedPath[maxIndex - index] = InvertDirection(path[index]);
            }

            yield return path;
            yield return rotatedPath;
        }

        public static IEnumerable<byte[]> GetEquivalentPathsWithMirrorSymmetry(this Step lastStep)
        {
            var path = lastStep.GetPath();
            var mirroredPath = new byte[path.Length];

            var maxIndex = path.Length - 1;
            for (var index = 0; index <= maxIndex; index++)
            {
                mirroredPath[maxIndex - index] = path[index];
            }

            yield return path;

            if (!ByteArrayComparer.Equals(mirroredPath, path))
            {
                yield return mirroredPath;
            }
        }

        public static IEnumerable<byte[]> GetEquivalentPathsWithMirrorAndRotationalSymmetry(this Step lastStep)
        {
            var path = lastStep.GetPath();
            var invertedPath = new byte[path.Length];
            var mirroredPath = new byte[path.Length];
            var rotatedPath = new byte[path.Length];

            var maxIndex = path.Length - 1;
            for (var index = 0; index <= maxIndex; index++)
            {
                invertedPath[index] = InvertDirection(path[index]);
                mirroredPath[maxIndex - index] = path[index];
                rotatedPath[maxIndex - index] = invertedPath[index];
            }

            yield return path;
            yield return invertedPath;

            if (!ByteArrayComparer.Equals(mirroredPath, path))
            {
                yield return mirroredPath;
                yield return rotatedPath;
            }
        }

        public static bool TryFindNodeInRoute(this Step step, Node node, out Step footprint)
        {
            if (step is not null)
            {
                do
                {
                    if (step.Node == node)
                    {
                        footprint = step;
                        return true;
                    }
                } while ((step = step.Previous) != null);
            }

            footprint = null;
            return false;
        }
    }
}

namespace Fluviatile.Grid
{
    public static class StepExtensions
    {
        public static Step TakeStep(this Step step, Torsion twist)
        {
            var direction = step.Direction.Turn(twist);
            var node = step.Node.Links[direction];

            return new Step(node, direction, twist, step);
        }

        public static bool TryFindNodeInRoute(this Step step, Node node, out Step footprint)
        {
            //if (step != null && step.IsNodeIncluded(node))
            if (step != null)
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

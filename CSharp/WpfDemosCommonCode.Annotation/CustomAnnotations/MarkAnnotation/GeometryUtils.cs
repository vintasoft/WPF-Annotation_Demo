using System.Windows;
using System.Windows.Media;

namespace WpfDemosCommonCode.Annotation
{
    static class GeometryUtils
    {
        internal static PathFigure CreatePolygon(Point[] points, bool isClosed)
        {
            PathFigure figure = new PathFigure();
            figure.IsClosed = isClosed;
            figure.StartPoint = points[0];
            for (int i = 1; i < points.Length; i++)
                figure.Segments.Add(new LineSegment(points[i], true));
            return figure;
        }

        internal static PathFigure CreateCurve(Point[] points, bool isClosed)
        {
            return CreateCurve(points, isClosed, 0.18);
        }

        internal static PathFigure CreateCurve(Point[] points, bool isClosed, double tension)
        {
            int count = points.Length;

            PathFigure figure = new PathFigure();
            figure.StartPoint = points[0];

            if (count == 2)
            {
                figure.Segments.Add(CreateCurveSegment(points[0], points[0], points[1], points[1], tension));
            }
            else
            {
                if (isClosed)
                    figure.Segments.Add(CreateCurveSegment(points[count - 1], points[0], points[1], points[2], tension));
                else
                    figure.Segments.Add(CreateCurveSegment(points[0], points[0], points[1], points[2], tension));

                for (int i = 1; i < count - 2; i++)
                    figure.Segments.Add(CreateCurveSegment(points[i - 1], points[i], points[i + 1], points[i + 2], tension));

                if (isClosed)
                {
                    figure.Segments.Add(CreateCurveSegment(points[count - 3], points[count - 2], points[count - 1], points[0], tension));
                    figure.Segments.Add(CreateCurveSegment(points[count - 2], points[count - 1], points[0], points[1], tension));
                }
                else
                {
                    figure.Segments.Add(CreateCurveSegment(points[count - 3], points[count - 2], points[count - 1], points[count - 1], tension));
                }
            }
            return figure;
        }

        private static BezierSegment CreateCurveSegment(Point pt0, Point pt1, Point pt2, Point pt3, double tension3)
        {
            return new BezierSegment(
                new Point(pt1.X + tension3 * (pt2.X - pt0.X), pt1.Y + tension3 * (pt2.Y - pt0.Y)),
                new Point(pt2.X - tension3 * (pt3.X - pt1.X), pt2.Y - tension3 * (pt3.Y - pt1.Y)),
                pt2, true);
        }
    }
}

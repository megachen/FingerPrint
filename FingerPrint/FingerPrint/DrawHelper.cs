using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace FingerPrint
{
    public enum Drawstate
    {
        Idle,
        Line,
        Circle,
        Free,
        Smart,
        Rectangle,
        Polygon,
    };

    public struct ParaCircle
    {
        public double X, Y, R;
    };

    public class DrawHelper : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Color m_clrfore;
        public Color color_foreground { get { return m_clrfore; } set { m_clrfore = value; NotifyPropertyChanged("brush_foreground"); } }
        public SolidColorBrush brush_foreground { get { return new SolidColorBrush(color_foreground); } }
        private Color m_clrback;
        public Color color_background { get { return m_clrback; } set { m_clrback = value; NotifyPropertyChanged("brush_background"); } }
        public SolidColorBrush brush_background { get { return new SolidColorBrush(color_background); } }
        private Color m_clrfill;
        public Color color_fill { get { return m_clrfill; } set { m_clrfill = value; NotifyPropertyChanged("brush_fill"); } }
        public SolidColorBrush brush_fill { get { return new SolidColorBrush(color_fill); } }
        public bool closureState;

        private Canvas board, preview, debug;
        private Point pt_begin, pt_last;
        private Drawstate state;
        private UIElement tempDraw;
        private List<object> drawHistory;
        private PointCollection rawCollection;
        private PointCollection inputCollection;

        private Dictionary<string, Color> colorDict;

        DrawHelper() { }

        public DrawHelper(Canvas input, Canvas pv, Canvas deb)
        {
            tempSmartDraw = new List<UIElement>();
            drawHistory = new List<object>();
            angles = new List<double>();
            length = new List<double>();
            points = new List<Point>();
            rawpoints = new List<Point>();
            directions = new List<Point>();
            inputCollection = new PointCollection();
            board = input;
            preview = pv;
            debug = deb;
            //dispindex = 0;
            Clearboard();

            colorDict = new Dictionary<string, Color>();
            colorDict["Bk"] = Colors.Black;
            colorDict["Wh"] = Colors.White;
            colorDict["Rd"] = Colors.Red;
            colorDict["Bl"] = Color.FromArgb(255, 30, 90, 240);
            colorDict["Ye"] = Colors.Yellow;
            colorDict["Gr"] = Colors.Green;
            colorDict["Tr"] = Colors.Transparent;
            color_foreground = Colors.White;
            color_background = Colors.Black;
            color_fill = Colors.Transparent;

            closureState = false;
        }

        public void Clearboard()
        {
            preview.Children.Clear();
            board.Children.Clear();
            state = Drawstate.Idle;
            pt_begin = new Point(0, 0);
            pt_last = new Point(0, 0);
            vect_last = new Point(0, 0);
            pt_collectlast = new Point(0, 0);
            tempDraw = null;
            tempSmartDraw.Clear();
            inputCollection = new PointCollection();
            angles.Clear();
            length.Clear();
            points.Clear();
            fardist = ang_begin = ang_sum = 0.0;
        }

        public void Changemode(Drawstate newstate)
        {
            state = newstate;

            pt_begin = new Point(0, 0);
            pt_last = new Point(0, 0);
            pt_collectlast = new Point(0, 0);
            vect_last = new Point(0, 0);
            tempDraw = null;
            inputCollection = new PointCollection();
        }

        public void ChangeColor(string type, string color)
        {
            switch (type)
            {
                case "S":
                    color_foreground = colorDict[color];
                    break;
                case "B":
                    color_background = colorDict[color];
                    board.Background = new SolidColorBrush(colorDict[color]);
                    break;
                case "F":
                    color_fill = colorDict[color];
                    break;
                default: break;
            }
        }

        public void Pressdown(Point input)
        {
            switch (state)
            {
                case Drawstate.Polygon:
                    if (pt_begin.X == 0 && pt_begin.Y == 0) pt_begin = pt_last = input;
                    else
                    {
                        Line line = new Line();
                        line.X1 = pt_last.X;
                        line.Y1 = pt_last.Y;
                        line.X2 = input.X;
                        line.Y2 = input.Y;

                        line.Stroke = new SolidColorBrush(color_foreground);
                        line.StrokeStartLineCap = PenLineCap.Round;
                        line.StrokeEndLineCap = PenLineCap.Round;
                        line.StrokeThickness = 15;
                        line.Opacity = 1;

                        preview.Children.Add(line);
                        pt_last = input;
                    }
                    inputCollection.Add(input);
                    break;
                case Drawstate.Free:
                case Drawstate.Smart:
#if DEBUG
                    debug.Children.Clear();
                    Line dline = new Line(); Line dline2 = new Line(); Line dline3 = new Line(); Line dline4 = new Line(); Line dline5 = new Line();
                    dline.X1 = dline2.X1 = dline3.X1 = dline4.X1 = dline5.X1 = 0;
                    dline.Y1 = debug.ActualHeight / 2;
                    dline2.Y1 = 60; dline3.Y1 = 80; dline4.Y1 = 100; dline5.Y1 = 120;
                    dline.X2 = dline2.X2 = dline3.X2 = dline4.X2 = dline5.X2 = debug.ActualWidth;
                    dline.Y2 = debug.ActualHeight / 2;
                    dline2.Y2 = 60; dline3.Y2 = 80; dline4.Y2 = 100; dline5.Y2 = 120;
                    dline.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 200, 0));
                    dline.StrokeThickness = dline2.StrokeThickness = dline3.StrokeThickness = dline4.StrokeThickness = dline5.StrokeThickness = 1;
                    dline.Opacity = dline2.Opacity = dline3.Opacity = dline4.Opacity = dline5.Opacity = 1;
                    dline2.Stroke = dline3.Stroke = dline4.Stroke = dline5.Stroke = new SolidColorBrush(Color.FromArgb(180, 200, 200, 0));
                    debug.Children.Add(dline); debug.Children.Add(dline2); debug.Children.Add(dline3); debug.Children.Add(dline4); debug.Children.Add(dline5);
                    dispindex = 0;
#endif
                    inputCollection.Add(input);
                    points.Clear(); directions.Clear(); angles.Clear();
                    pt_last = pt_begin = input; break;
                default:
                    pt_last = pt_begin = input; break;
            }
        }

        public void Move(Point input)
        {
            Line line = new Line();
            switch (state)
            {
                case Drawstate.Line:
                    if (tempDraw != null)
                    {
                        board.Children.Remove(tempDraw);
                    }
                    line.X1 = pt_begin.X;
                    line.Y1 = pt_begin.Y;
                    line.X2 = input.X;
                    line.Y2 = input.Y;
                    
                    line.Stroke = new SolidColorBrush(color_foreground);
                    line.StrokeStartLineCap = PenLineCap.Round;
                    line.StrokeEndLineCap = PenLineCap.Round;
                    line.StrokeThickness = 15;
                    line.Opacity = 1;

                    board.Children.Add(line);
                    tempDraw = line;
                    break;
                case Drawstate.Circle:
                    if (tempDraw != null)
                    {
                        board.Children.Remove(tempDraw);
                    }
                    Ellipse ellipse = new Ellipse();
                    double radius = CalcDist(pt_begin, input) / 2.0;
                    Canvas.SetLeft(ellipse, (pt_begin.X + input.X) / 2.0 - radius);
                    Canvas.SetTop(ellipse, (pt_begin.Y + input.Y) / 2.0 - radius);

                    ellipse.Stroke = new SolidColorBrush(color_foreground);
                    if(closureState) ellipse.Fill = new SolidColorBrush(color_fill);
                    ellipse.StrokeThickness = 15;
                    ellipse.Width = ellipse.Height = radius * 2;
                    ellipse.Opacity = 1;

                    board.Children.Add(ellipse);
                    tempDraw = ellipse;
                    break;
                case Drawstate.Rectangle:
                    if (tempDraw != null)
                    {
                        board.Children.Remove(tempDraw);
                    }
                    Rectangle rect = new Rectangle();
                    rect.Width = Math.Abs(input.X - pt_begin.X);
                    rect.Height = Math.Abs(input.Y - pt_begin.Y);
                    Canvas.SetLeft(rect, input.X < pt_begin.X ? input.X : pt_begin.X);
                    Canvas.SetTop(rect, input.Y < pt_begin.Y ? input.Y : pt_begin.Y);

                    rect.Stroke = new SolidColorBrush(color_foreground);
                    if (closureState) rect.Fill = new SolidColorBrush(color_fill);
                    rect.StrokeThickness = 15;

                    board.Children.Add(rect);
                    tempDraw = rect;
                    break;
                case Drawstate.Polygon:
                    break;
                case Drawstate.Free:
                    line.X1 = pt_last.X;
                    line.Y1 = pt_last.Y;
                    line.X2 = input.X;
                    line.Y2 = input.Y;

                    line.Stroke = new SolidColorBrush(color_foreground);
                    line.StrokeStartLineCap = PenLineCap.Round;
                    line.StrokeEndLineCap = PenLineCap.Round;
                    line.StrokeThickness = 15;
                    line.Opacity = 1;

                    preview.Children.Add(line);
                    pt_last = input;
                    inputCollection.Add(input);
                    break;
                case Drawstate.Smart:
                    //draw
                    line.X1 = pt_last.X;
                    line.Y1 = pt_last.Y;
                    line.X2 = input.X;
                    line.Y2 = input.Y;

                    line.Stroke = new SolidColorBrush(color_foreground);
                    line.StrokeStartLineCap = PenLineCap.Round;
                    line.StrokeEndLineCap = PenLineCap.Round;
                    line.StrokeThickness = 15;
                    line.Opacity = 1;

                    preview.Children.Add(line);
                    tempSmartDraw.Add(line);

                    //collect
                    SmartCollect(input);

                    //finalize
                    pt_last = input;
                    inputCollection.Add(input);
                    break;
                default: break;
            }
        }

        public void Release(Point input)
        {
            Path poly = new Path();
            PathGeometry geoPoly = new PathGeometry();
            geoPoly.Figures = new PathFigureCollection();
            PolyLineSegment seg = new PolyLineSegment();
            PathFigure figPoly = new PathFigure();
            switch (state)
            {
                case Drawstate.Line:
                case Drawstate.Circle:
                case Drawstate.Rectangle:
                    drawHistory.Add(tempDraw);
                    break;
                case Drawstate.Polygon:
                    return;
                case Drawstate.Free:
                    preview.Children.Clear();

                    figPoly.StartPoint = inputCollection[0];
                    seg.Points = inputCollection;
                    figPoly.Segments.Add(seg);
                    geoPoly.Figures.Add(figPoly);

                    poly.Data = geoPoly;
                    poly.StrokeStartLineCap = poly.StrokeEndLineCap = PenLineCap.Round;
                    poly.Stroke = new SolidColorBrush(color_foreground);
                    poly.StrokeThickness = 15;
                    poly.Opacity = 1;

                    board.Children.Add(poly);
                    drawHistory.Add(poly);
                    break;
                case Drawstate.Smart:
                {
                    SmartCollect(input);
                    if (angles.Count < 2) break;
                    int nRet = SmartAnalyze();

                    switch(nRet)
                    {
                        case 1: //line
                            Line line = new Line();
                            line.X1 = pt_begin.X;
                            line.Y1 = pt_begin.Y;
                            line.X2 = input.X;
                            line.Y2 = input.Y;

                            line.Stroke = new SolidColorBrush(color_foreground);
                            line.StrokeStartLineCap = PenLineCap.Round;
                            line.StrokeEndLineCap = PenLineCap.Round;
                            line.StrokeThickness = 15;
                            line.Opacity = 1;

                            board.Children.Add(line);
                            drawHistory.Add(line);
                            break;
                        case 2: //polygon
                            PointCollection polypoint = new PointCollection();
                            polypoint.Add(points.First());
                            foreach (int i in outliers)
                            {
                                polypoint.Add(points[i + 2]);
                            }
                            polypoint.Add(points.Last());

                            figPoly.StartPoint = polypoint[0];

                            polypoint.RemoveAt(polypoint.Count - 1);
                            figPoly.IsClosed = true;
                            figPoly.IsFilled = true;
                            geoPoly.FillRule = FillRule.Nonzero;
                            if (closureState) poly.Fill = new SolidColorBrush(color_fill);

                            seg.Points = polypoint;
                            figPoly.Segments.Add(seg);
                            geoPoly.Figures.Add(figPoly);

                            poly.Data = geoPoly;
                            poly.StrokeStartLineCap = poly.StrokeEndLineCap = PenLineCap.Round;
                            poly.Stroke = new SolidColorBrush(color_foreground);
                            poly.StrokeThickness = 15;
                            poly.Opacity = 1;

                            board.Children.Add(poly);
                            drawHistory.Add(poly);
                            break;
                        case 3: //circle
                            ParaCircle param = CalcCircleFitting();

                            Ellipse circle = new Ellipse();
                            Canvas.SetLeft(circle, param.X - param.R);
                            Canvas.SetTop(circle, param.Y - param.R);

                            circle.Stroke = new SolidColorBrush(color_foreground);
                            if (closureState) circle.Fill = new SolidColorBrush(color_fill);
                            circle.StrokeThickness = 15;
                            circle.Width = circle.Height = param.R * 2;
                            circle.Opacity = 1;

                            board.Children.Add(circle);
                            drawHistory.Add(circle);
                            break;
                        case 4: //ellipse
                            List<double> result = getEllipseparGauss(rawpoints);

                            Ellipse ellipse = new Ellipse();
                            Canvas.SetLeft(ellipse, result[0] - result[3]);
                            Canvas.SetTop(ellipse, result[1] - result[2]);

                            ellipse.Stroke = new SolidColorBrush(color_foreground);
                            if (closureState) ellipse.Fill = new SolidColorBrush(color_fill);
                            ellipse.StrokeThickness = 15;
                            ellipse.Width = result[3] * 2;
                            ellipse.Height = result[2] * 2;
                            ellipse.Opacity = 1;

                            RotateTransform trans = new RotateTransform();
                            trans.CenterX = result[3];
                            trans.CenterY = result[2];
                            trans.Angle = result[4] - 90.0;
                            ellipse.RenderTransform = trans;
                            //ellipse.RenderTransformOrigin

                            board.Children.Add(ellipse);
                            drawHistory.Add(ellipse);
                            break;
                        case 5: //fold
                            PointCollection foldpoint = new PointCollection();
                            foldpoint.Add(points.First());

                            foreach (int i in outliers)
                            {
                                foldpoint.Add(points[i + 2]);
                            }
                            foldpoint.Add(points.Last());

                            figPoly.StartPoint = foldpoint[0];

                            seg.Points = foldpoint;
                            figPoly.Segments.Add(seg);
                            geoPoly.Figures.Add(figPoly);

                            poly.Data = geoPoly;
                            poly.StrokeStartLineCap = poly.StrokeEndLineCap = PenLineCap.Round;
                            poly.Stroke = new SolidColorBrush(color_foreground);
                            poly.StrokeThickness = 15;
                            poly.Opacity = 1;

                            board.Children.Add(poly);
                            drawHistory.Add(poly);
                            break;
                        case 6: //free
                            preview.Children.Clear();

                            figPoly.StartPoint = inputCollection[0];
                            seg.Points = inputCollection;
                            figPoly.Segments.Add(seg);
                            geoPoly.Figures.Add(figPoly);

                            poly.Data = geoPoly;
                            poly.StrokeStartLineCap = poly.StrokeEndLineCap = PenLineCap.Round;
                            poly.Stroke = new SolidColorBrush(color_foreground);
                            poly.StrokeThickness = 15;
                            poly.Opacity = 1;

                            board.Children.Add(poly);
                            drawHistory.Add(poly);
                            break;
                        default:
                            break;
                    }
                    preview.Children.Clear();


                    ////test line
                    //if (outliers.Count > 0)
                    //{
                    //    //clear tempdraw
                    //    preview.Children.Clear();

                    //    //polyline
                    //    PointCollection polypoint = new PointCollection();
                    //    polypoint.Add(points.First());
                    //    foreach (int i in outliers)
                    //    {
                    //        polypoint.Add(points[i + 2]);
                    //    }
                    //    polypoint.Add(points.Last());

                    //    figPoly.StartPoint = polypoint[0];

                    //    if (CalcDist(polypoint.First(), polypoint.Last()) < 50)
                    //    {
                    //        polypoint.RemoveAt(polypoint.Count - 1);
                    //        figPoly.IsClosed = true;
                    //        figPoly.IsFilled = true;
                    //        geoPoly.FillRule = FillRule.Nonzero;
                    //        if (closureState) poly.Fill = new SolidColorBrush(color_fill);
                    //    }

                    //    seg.Points = polypoint;
                    //    figPoly.Segments.Add(seg);
                    //    geoPoly.Figures.Add(figPoly);

                    //    poly.Data = geoPoly;
                    //    poly.StrokeStartLineCap = poly.StrokeEndLineCap = PenLineCap.Round;
                    //    poly.Stroke = new SolidColorBrush(color_foreground);
                    //    poly.StrokeThickness = 15;
                    //    poly.Opacity = 1;

                    //    board.Children.Add(poly);
                    //    drawHistory.Add(poly);
                    //}
                    //else if (ang_avg < 6 && test_closure / fardist > 0.9)
                    //{
                    //    //clear tempdraw
                    //    preview.Children.Clear();

                    //    //line
                    //    Line line = new Line();
                    //    line.X1 = pt_begin.X;
                    //    line.Y1 = pt_begin.Y;
                    //    line.X2 = input.X;
                    //    line.Y2 = input.Y;

                    //    line.Stroke = new SolidColorBrush(color_foreground);
                    //    line.StrokeStartLineCap = PenLineCap.Round;
                    //    line.StrokeEndLineCap = PenLineCap.Round;
                    //    line.StrokeThickness = 15;
                    //    line.Opacity = 1;

                    //    board.Children.Add(line);
                    //    drawHistory.Add(line);
                    //}
                    ////test freedraw
                    //else if (CalcVariance(angles) > 500)
                    //{
                    //preview.Children.Clear();

                    //figPoly.StartPoint = inputCollection[0];
                    //seg.Points = inputCollection;
                    //figPoly.Segments.Add(seg);
                    //geoPoly.Figures.Add(figPoly);

                    //poly.Data = geoPoly;
                    //poly.StrokeStartLineCap = poly.StrokeEndLineCap = PenLineCap.Round;
                    //poly.Stroke = new SolidColorBrush(color_foreground);
                    //poly.StrokeThickness = 15;
                    //poly.Opacity = 1;

                    //board.Children.Add(poly);
                    //drawHistory.Add(poly);
                    //break;
                    //}
                    //else if (ang_avg > 3 && test_closure < fardist / 2)
                    //{
                    //    //clear tempdraw
                    //    preview.Children.Clear();

                    //    //circle
                    //    ParaCircle param = CalcCircleFitting();

                    //    Ellipse ellipse = new Ellipse();
                    //    Canvas.SetLeft(ellipse, param.X - param.R);
                    //    Canvas.SetTop(ellipse, param.Y - param.R);

                    //    ellipse.Stroke = new SolidColorBrush(color_foreground);
                    //    if (closureState) ellipse.Fill = new SolidColorBrush(color_fill);
                    //    ellipse.StrokeThickness = 15;
                    //    ellipse.Width = ellipse.Height = param.R * 2;
                    //    ellipse.Opacity = 1;

                    //    board.Children.Add(ellipse);
                    //    drawHistory.Add(ellipse);
                    //}

                    //finalize
                    tempSmartDraw.Clear();
                    angles.Clear();
                    length.Clear();
                    points.Clear();
                    rawpoints.Clear();
                    fardist = ang_begin = ang_sum = 0.0;
                    //}
                    break;
                }
                default: drawHistory.Add(tempDraw); break;
            }
            pt_begin = new Point(0, 0);
            pt_last = new Point(0, 0);
            pt_collectlast = new Point(0, 0);
            vect_last = new Point(0, 0);
            tempDraw = null;
            inputCollection = new PointCollection();
        }

        public void DoubleTap(Point input)
        {
            if (state != Drawstate.Polygon) return;

            //inputCollection.Add(input);
            preview.Children.Clear();

            Path poly = new Path();
            PathGeometry geoPoly = new PathGeometry();
            geoPoly.Figures = new PathFigureCollection();
            PathFigure figPoly = new PathFigure();
            figPoly.StartPoint = inputCollection[0];

            if (CalcDist(inputCollection.First(), inputCollection.Last()) < 50)
            {
                inputCollection.RemoveAt(inputCollection.Count - 1);
                inputCollection.RemoveAt(inputCollection.Count - 1);
                figPoly.IsClosed = true;
                figPoly.IsFilled = true;
                geoPoly.FillRule = FillRule.Nonzero;
                if (closureState) poly.Fill = new SolidColorBrush(color_fill);
            }

            PolyLineSegment seg = new PolyLineSegment();
            seg.Points = inputCollection;
            figPoly.Segments.Add(seg);
            geoPoly.Figures.Add(figPoly);

            poly.Data = geoPoly;
            poly.StrokeStartLineCap = poly.StrokeEndLineCap = PenLineCap.Round;
            poly.Stroke = new SolidColorBrush(color_foreground);
            poly.StrokeThickness = 15;
            poly.Opacity = 1;

            board.Children.Add(poly);
            drawHistory.Add(poly);

            pt_begin = new Point(0, 0);
            pt_last = new Point(0, 0);
            pt_collectlast = new Point(0, 0);
            vect_last = new Point(0, 0);
            tempDraw = null;
            inputCollection = new PointCollection();
        }

        public void ModPosition(Point shift)
        {
            foreach (UIElement i in board.Children)
            {
                Canvas.SetLeft(i, Canvas.GetLeft(i) + shift.X * 10.0);
                Canvas.SetTop(i, Canvas.GetTop(i) - shift.Y * 10.0);
            }
        }

        public void Undo()
        {
            if (drawHistory.Count < 1) return;
            board.Children.Remove(drawHistory.Last() as UIElement);
            drawHistory.RemoveAt(drawHistory.Count - 1);
        }

        public double CalcDist(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        private double CalcAngle(Point a, Point b)
        {
            double n = a.X * b.X + a.Y * b.Y, m = Math.Sqrt(a.X * a.X + a.Y * a.Y) * Math.Sqrt(b.X * b.X + b.Y * b.Y);
            double ret = Math.Acos(n / m) * (180.0 / Math.PI), cross = a.X * b.Y - a.Y * b.X;
            return cross > 0 ? ret : -ret;
        }

        private double CalcAverage(List<double> input)
        {
            return CalcSum(input)/(double)input.Count;
        }

        private double CalcSum(List<double> input)
        {
            double sum = 0.0;
            foreach (double i in input) sum += i;
            return sum;
        }

        private double CalcVariance(List<double> input)
        {
            double avg = CalcAverage(input), variance=0.0;
            foreach (double i in input) { variance += (i - avg) * (i - avg); }
            return variance/(double)input.Count;
        }


        //for smart recognizaion
        private int dispindex;
        private List<UIElement> tempSmartDraw;
        private List<double> length;
        private List<Point> rawpoints, points, directions;
        private List<double> angles;
        private double fardist, ang_begin, ang_sum;
        private Point vect_last, pt_collectlast;
        List<int> outliers;

        private void SmartCollect(Point input)
        {
            if (rawpoints.Count == 0)
            {
                rawpoints.Add(input);
                points.Add(input);
                return;
            }
            double distance = CalcDist(points.Last(), input);
            if (distance < 8) return;

            double distance2 = CalcDist(pt_begin, input);
            if (distance2 > fardist) fardist = distance2;
            
            rawpoints.Add(input);
            if (rawpoints.Count < 3) return;

            //gauss filter
            Point pt1 = rawpoints[rawpoints.Count - 1], pt2 = rawpoints[rawpoints.Count - 2], pt3 = rawpoints[rawpoints.Count - 3];
            Point gpt = new Point();
            gpt.X = (pt1.X + 2.0 * pt2.X + pt3.X) / 4.0;
            gpt.Y = (pt1.Y + 2.0 * pt2.Y + pt3.Y) / 4.0;
            points.Add(gpt);

            Point dir = new Point(points[points.Count - 1].X - points[points.Count - 2].X, points[points.Count - 1].Y - points[points.Count - 2].Y);

            if (directions.Count < 2)
            {
                directions.Add(dir);
                return;
            }
            double ang = CalcAngle(dir, directions.Last());
            if (double.IsNaN(ang)) ang = 0;
            
            directions.Add(dir);
            angles.Add(ang);
            
#if DEBUG
            //Ellipse pt = new Ellipse();
            //pt.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            //pt.Width = pt.Height = 5;
            //debug.Children.Add(pt);
            //Canvas.SetLeft(pt, dispindex++ * 4.0);
            //Canvas.SetTop(pt, 90.0 - ang);
#endif
        }

        private int SmartAnalyze()
        {
#if DEBUG
            //Point linedir = new Point(pt_last.X - pt_begin.X, pt_last.Y - pt_begin.Y);
            //List<double> noice = new List<double>();
            //foreach(Point dir in directions)
            //{
            //    noice.Add(CalcAngle(dir, linedir));
            //}
            //linedir = new Point();
#endif

            List<double> ang_stat = new List<double>(angles);
            ang_stat.RemoveRange(0, 1); ang_stat.RemoveRange(ang_stat.Count - 1, 1);

            //closure
            double test_closure = CalcDist(pt_begin, rawpoints.Last());
            //angle
            double avg_ang = Math.Abs(CalcAverage(ang_stat));
            //angle variance
            double var_ang = CalcVariance(ang_stat);
            //corner points
            outliers = CheckOutlier(avg_ang);
            //mbr
            double[] mbr_rect = MinAreaRec(points);
            double ratio_rect = mbr_rect[0] > mbr_rect[1] ? mbr_rect[0] / mbr_rect[1] : mbr_rect[1] / mbr_rect[0]; 

            //analyze
            int ret = 0;
            if(outliers.Count==0)
            {
                if (ratio_rect > 10 && avg_ang < 6) ret = 1;
                else if (avg_ang > 4 && ratio_rect < 1.5) ret = 3;
            }
            else
            {
                if (test_closure > 100) ret = 5;
                else ret = 2;
            }
            if (ret==0 && outliers.Count <= 2 && ratio_rect>1.5 && test_closure<80) ret = 4;
            else if (ret == 0 && var_ang > 600) ret = 6;
            return ret;
        }

        double[] MinAreaRec(List<Point> pts)
        {
            double minArea = 3.402823466e+38d;
            int ptsNum = pts.Count;
            double[] rect = new double[2]; 

	        for(int i = 0, j = ptsNum - 1; i < ptsNum; j = i, i++)
            {//遍历边
		        Point u0 = Normalize(new Point(pts[i].X-pts[j].X,pts[i].Y-pts[j].Y));//构造边
		        Point u1 = new Point(0-u0.Y, u0.X);//与u0垂直
		        double min0 = 0.0d, max0 = 0.0d, min1 = 0.0d, max1 = 0.0d;

		        for(int k = 0; k < ptsNum; k++) {//遍历点
                    Point d = new Point(pts[k].X - pts[j].X, pts[k].Y - pts[j].Y);
			        //投影在u0
			        double dot = Dot(d, u0);
			        if(dot < min0) min0 = dot;
			        if(dot > max0) max0 = dot;
			        //投影在u1
                    dot = Dot(d, u1);
			        if(dot < min1) min1 = dot;
			        if(dot > max1) max1 = dot;
		        }

		        double area = (max0 - min0) * (max1 - min1);
		        if( area < minArea ) {
                    minArea = area;
                    rect[0] = (max0 - min0);
                    rect[1] = (max1 - min1);
                    //obb.c = pts[j] + ( u0 * (max0 + min0) + u1 * (max1 + min1) )*0.5f;

                    //obb.u[0] = u0;
                    //obb.u[1] = u1;

                    //obb.e[0] = (max0 - min0)*0.5f;
                    //obb.e[1] = (max1 - min1)*0.5f;
		        }
	        }
	        return rect;
        }

        Point Normalize(Point input)
        {
            double len = Math.Sqrt(input.X * input.X + input.Y * input.Y);
            input.X /= len; input.Y /= len;
            return input;
        }

        double Dot(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        private List<int> CheckOutlier(double avg)
        {
            List<int> ret = new List<int>();

            List<double> ang_stat = new List<double>(angles);
            ang_stat.RemoveRange(0, 1); ang_stat.RemoveRange(ang_stat.Count - 1, 1);
            ang_stat.Sort();
            double q1 = ang_stat[ang_stat.Count / 4], q2 = ang_stat[ang_stat.Count / 2], q3 = ang_stat[ang_stat.Count * 3 / 4];
            double split_top = (q3 - q1) * 2 + q3, split_down = q1 - (q3 - q1) * 2;

            for(int i=1;i<angles.Count-1;i++)
            {
                if (angles[i] >= split_top || angles[i] <= split_down)
                {
                    int pos = i;
                    i++; if (i > angles.Count - 2) break;
                    while (angles[i] >= split_top || angles[i] <= split_down)
                    {
                        if (Math.Abs(angles[i]) > Math.Abs(angles[i - 1])) pos = i;
                        i++; if (i > angles.Count - 2) break;
                    }
                    ret.Add(pos);
                }
            }

            return ret;
        }

        private ParaCircle CalcCircleFitting()
        {
            ParaCircle ret;
            ret.X = ret.Y = ret.R = 0.0;
            if (points.Count < 3) return ret;

            double x1 = 0.0f;  //x一次方的初值
            double y1 = 0.0f;
            double x2 = 0.0f;  //x平方的初始值
            double y2 = 0.0f;
            double x3 = 0.0f;  //x立方的初始值
            double y3 = 0.0f;
            double x1y1 = 0.0f;
            double x1y2 = 0.0f;
            double x2y1 = 0.0f;

            foreach (Point pt in points)
            {
                x1 = x1 + pt.X;
                y1 = y1 + pt.Y;
                x2 = x2 + pt.X * pt.X;
                y2 = y2 + pt.Y * pt.Y;
                x3 = x3 + pt.X * pt.X * pt.X;
                y3 = y3 + pt.Y * pt.Y * pt.Y;
                x1y1 = x1y1 + pt.X * pt.Y;
                x1y2 = x1y2 + pt.X * pt.Y * pt.Y;
                x2y1 = x2y1 + pt.X * pt.X * pt.Y;
            }

            double C, D, E, G, H, N;
            double a, b, c;
            N = points.Count;
            C = N * x2 - x1 * x1;
            D = N * x1y1 - x1 * y1;
            E = N * x3 + N * x1y2 - (x2 + y2) * x1;
            G = N * y2 - y1 * y1;
            H = N * x2y1 + N * y3 - (x2 + y2) * y1;
            a = (H * D - E * G) / (C * G - D * D);
            b = (H * C - E * D) / (D * D - G * C);
            c = -(a * x1 + b * y1 + x2 + y2) / N;

            double A, B, R;
            A = a / (-2);
            B = b / (-2);
            R = (double)Math.Sqrt(a * a + b * b - 4 * c) / 2;

            ret.X = A;
            ret.Y = B;
            ret.R = R;
            return ret;
        }
        
        List<double> RGauss(List<List<double> > A)
        {
	        int n = A.Count();
	        int m = A[0].Count();
            List<double> x = new List<double>(n);
            x.Add(0.0); x.Add(0.0); x.Add(0.0); x.Add(0.0); x.Add(0.0); x.Add(0.0); x.Add(0.0);
	        //复制系数矩阵，防止修改原矩阵
	        List<List<double> > Atemp=new List<List<double>>(A);
	        for (int k = 0; k < n; k++)
	        {
		        //选主元
		        double max = -1;
		        int l = -1;
		        for (int i = k; i < n; i++)
		        {
			        if (Math.Abs(Atemp[i][k]) > max)
			        {
                        max = Math.Abs(Atemp[i][k]);
				        l = i;
			        }
		        }
		        if (l != k)
		        {
			        //交换系数矩阵的l行和k行
			        for (int i = 0; i < m; i++)
			        {
				        double temp = Atemp[l][i];
				        Atemp[l][i] = Atemp[k][i];
				        Atemp[k][i] = temp;
			        }
		        }
		        //消元
		        for (int i = k + 1; i < n; i++)
		        {
			        double li = Atemp[i][k] / Atemp[k][k];
			        for (int j = k; j < m; j++)
			        {
				        Atemp[i][j] = Atemp[i][j] - li*Atemp[k][j];
			        }
		        }
	        }
	        //回代
	        x[n - 1] = Atemp[n - 1][m - 1] / Atemp[n - 1][m - 2];
	        for (int k = n - 2; k >= 0; k--)
	        {
		        double s = 0.0;
		        for (int j = k + 1; j < n; j++)
		        {
			        s += Atemp[k][j] * x[j];
		        }
		        x[k] = (Atemp[k][m - 1] - s) / Atemp[k][k];
	        }
	        return x;
        }

        List<double> getEllipseparGauss(List<Point> vec_point)
        {
	        List<double> vec_result = new List<double>();
	        double x3y1 = 0, x1y3 = 0, x2y2 = 0, yyy4 = 0, xxx3 = 0, xxx2 = 0, x2y1 = 0, yyy3 = 0, x1y2 = 0, yyy2 = 0, x1y1 = 0, xxx1 = 0, yyy1 = 0;
	        int N = vec_point.Count;
	        foreach (Point i in vec_point)
	        {
		        double xi = i.X;
		        double yi = i.Y;
		        x3y1 += xi*xi*xi*yi;
		        x1y3 += xi*yi*yi*yi;
		        x2y2 += xi*xi*yi*yi;;
		        yyy4 += yi*yi*yi*yi;
		        xxx3 += xi*xi*xi;
		        xxx2 += xi*xi;
		        x2y1 += xi*xi*yi;

		        x1y2 += xi*yi*yi;
		        yyy2 += yi*yi;
		        x1y1 += xi*yi;
		        xxx1 += xi;
		        yyy1 += yi;
		        yyy3 += yi*yi*yi;
	        }
	        double[] resul=new double[5];
	        resul[0] = -(x3y1);
	        resul[1] = -(x2y2);
	        resul[2] = -(xxx3);
	        resul[3] = -(x2y1);
	        resul[4] = -(xxx2);
	        double[] Bb=new double[5], Cc=new double[5], Dd=new double[5], Ee=new double[5], Aa=new double[5];
	        Bb[0] = x1y3; Cc[0] = x2y1; Dd[0] = x1y2; Ee[0] = x1y1; Aa[0] = x2y2;
	        Bb[1] = yyy4; Cc[1] = x1y2; Dd[1] = yyy3; Ee[1] = yyy2; Aa[1] = x1y3;
	        Bb[2] = x1y2; Cc[2] = xxx2; Dd[2] = x1y1; Ee[2] = xxx1; Aa[2] = x2y1;
	        Bb[3] = yyy3; Cc[3] = x1y1; Dd[3] = yyy2; Ee[3] = yyy1; Aa[3] = x1y2;
	        Bb[4] = yyy2; Cc[4] = xxx1; Dd[4] = yyy1; Ee[4] = N; Aa[4] = x1y1;

            List<List<double>> Ma=new List<List<double>>(5);
	        List<double>Md= new List<double>(5);
	        for (int i = 0; i<5; i++)
	        {
                Ma.Add(new List<double>());
		        Ma[i].Add(Aa[i]);
		        Ma[i].Add(Bb[i]);
		        Ma[i].Add(Cc[i]);
		        Ma[i].Add(Dd[i]);
		        Ma[i].Add(Ee[i]);
		        Ma[i].Add(resul[i]);
	        }

	        Md=RGauss(Ma);
	        double A = Md[0];
	        double B = Md[1];
	        double C = Md[2];
	        double D = Md[3];
	        double E = Md[4];
	        double XC = (2 * B*C - A*D) / (A*A - 4 * B);
	        double YC = (2 * D - A*C) / (A*A - 4 * B);
	        double fenzi = 2 * (A*C*D - B*C*C - D*D + 4 * E*B - A*A*E);
	        double fenmu = (A*A - 4 * B)*(B - Math.Sqrt(A*A + (1 - B)*(1 - B)) + 1);
            double fenmu2 = (A * A - 4 * B) * (B + Math.Sqrt(A * A + (1 - B) * (1 - B)) + 1);
            double XA = Math.Sqrt(Math.Abs(fenzi / fenmu));
            double XB = Math.Sqrt(Math.Abs(fenzi / fenmu2));
	        double Xtheta = 0.5*Math.Atan(A / (1 - B)) * 180 / 3.1415926;
	        if (B<1)
		        Xtheta += 90;
	        vec_result.Add(XC);
	        vec_result.Add(YC);
	        vec_result.Add(XA);
	        vec_result.Add(XB);
	        vec_result.Add(Xtheta);
	        return vec_result;
        }
    }
}

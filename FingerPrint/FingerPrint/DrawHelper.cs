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
                    debug.Children.Clear();
                    //Line dline = new Line(); Line dline2 = new Line(); Line dline3 = new Line(); Line dline4 = new Line(); Line dline5 = new Line();
                    //dline.X1 = dline2.X1 = dline3.X1 = dline4.X1 = dline5.X1 = 0;
                    //dline.Y1 = debug.ActualHeight / 2;
                    //dline2.Y1 = 60; dline3.Y1 = 80; dline4.Y1 = 100; dline5.Y1 = 120;
                    //dline.X2 = dline2.X2 = dline3.X2 = dline4.X2 = dline5.X2 = debug.ActualWidth;
                    //dline.Y2 = debug.ActualHeight / 2;
                    //dline2.Y2 = 60; dline3.Y2 = 80; dline4.Y2 = 100; dline5.Y2 = 120;
                    //dline.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 200, 0));
                    //dline.StrokeThickness = dline2.StrokeThickness = dline3.StrokeThickness = dline4.StrokeThickness = dline5.StrokeThickness = 1;
                    //dline.Opacity = dline2.Opacity = dline3.Opacity = dline4.Opacity = dline5.Opacity = 1;
                    //dline2.Stroke = dline3.Stroke = dline4.Stroke = dline5.Stroke = new SolidColorBrush(Color.FromArgb(180, 200, 200, 0));
                    //debug.Children.Add(dline); debug.Children.Add(dline2); debug.Children.Add(dline3); debug.Children.Add(dline4); debug.Children.Add(dline5);
                    //dispindex = 0;

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
                    //SmartAnalyze();
                    if (angles.Count < 2) break;

                    List<double> ang_stat = new List<double>(angles);
                    ang_stat.RemoveRange(0, 1); ang_stat.RemoveRange(ang_stat.Count - 1, 1);
                    double test_closure = CalcDist(pt_begin, input),
                        ang_avg = Math.Abs(CalcAverage(ang_stat));
                    List<int> outliers = CheckOutlier(ang_avg);

                    //test line
                    if (outliers.Count > 0)
                    {
                        //clear tempdraw
                        preview.Children.Clear();

                        //polyline
                        PointCollection polypoint = new PointCollection();
                        polypoint.Add(points.First());
                        foreach (int i in outliers)
                        {
                            polypoint.Add(points[i + 2]);
                        }
                        polypoint.Add(points.Last());
                        
                        figPoly.StartPoint = polypoint[0];

                        if (CalcDist(polypoint.First(), polypoint.Last()) < 50)
                        {
                            polypoint.RemoveAt(polypoint.Count - 1);
                            figPoly.IsClosed = true;
                            figPoly.IsFilled = true;
                            geoPoly.FillRule = FillRule.Nonzero;
                            if (closureState) poly.Fill = new SolidColorBrush(color_fill);
                        }

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
                    }
                    else if (ang_avg < 6 && test_closure / fardist > 0.9)
                    {
                        //clear tempdraw
                        preview.Children.Clear();

                        //line
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
                    }
                    //test freedraw
                    else if (CalcVariance(angles) > 500)
                    {
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
                    }
                    else if (ang_avg > 3 && test_closure < fardist / 2)
                    {
                        //clear tempdraw
                        preview.Children.Clear();

                        //circle
                        ParaCircle param = CalcCircleFitting();

                        Ellipse ellipse = new Ellipse();
                        Canvas.SetLeft(ellipse, param.X - param.R);
                        Canvas.SetTop(ellipse, param.Y - param.R);

                        ellipse.Stroke = new SolidColorBrush(color_foreground);
                        if (closureState) ellipse.Fill = new SolidColorBrush(color_fill);
                        ellipse.StrokeThickness = 15;
                        ellipse.Width = ellipse.Height = param.R * 2;
                        ellipse.Opacity = 1;

                        board.Children.Add(ellipse);
                        drawHistory.Add(ellipse);
                    }

                    //finalize
                    tempSmartDraw.Clear();
                    angles.Clear();
                    length.Clear();
                    points.Clear();
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

        public void ModSize(double ratio)
        {

        }

        public void ModPosition(Point shift)
        {
            foreach (UIElement i in board.Children)
            {
                Canvas.SetLeft(i, shift.X);
                Canvas.SetTop(i, shift.Y);
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

        private ParaCircle CalcCircleFitting()
        {
            ParaCircle ret;
            ret.X = ret.Y = ret.R = 0.0;
            if (points.Count < 3)  return ret;

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


        //for smart recognizaion
        private int dispindex;
        private List<UIElement> tempSmartDraw;
        private List<double> length;
        private List<Point> points, directions;
        private List<double> angles;
        private double fardist, ang_begin, ang_sum;
        private Point vect_last, pt_collectlast;

        private void SmartCollect(Point input)
        {
            if(points.Count==0)
            {
                points.Add(input);
                return;
            }
            double distance = CalcDist(points.Last(), input);
            if (distance < 8) return;

            double distance2 = CalcDist(pt_begin, input);
            if (distance2 > fardist) fardist = distance2;

            Point dir = new Point(input.X - points.Last().X, input.Y - points.Last().Y);
            points.Add(input);

            if (points.Count < 3)
            {
                directions.Add(dir);
                return;
            }

            double ang = CalcAngle(dir, directions.Last());
            if (double.IsNaN(ang)) ang = 0;
            directions.Add(dir);

            angles.Add(ang);

            //debug
            //Ellipse pt = new Ellipse();
            //pt.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            //pt.Width = pt.Height = 5;
            //debug.Children.Add(pt);
            //Canvas.SetLeft(pt, dispindex++ * 4.0);
            //Canvas.SetTop(pt, 90.0 - ang);
        }

        private void SmartAnalyze()
        {

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
    }
}

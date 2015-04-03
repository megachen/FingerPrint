using System;
using System.Collections.Generic;
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
    };

    public struct ParaCircle
    {
        public double X, Y, R;
    };

    public class DrawHelper
    {
        private Canvas board;
        private Point pt_begin, pt_last;
        private Drawstate state;
        private UIElement tempDraw;
        //for smart recognizaion
        private List<UIElement> tempSmartDraw;
        private List<double> angles;
        private List<double> length;
        private List<Point> points;
        private double fardist, ang_begin, ang_sum;
        private Point vect_last, pt_collectlast;

        DrawHelper() { }

        public DrawHelper(Canvas input)
        {
            tempSmartDraw = new List<UIElement>();
            angles = new List<double>();
            length = new List<double>();
            points = new List<Point>();
            board = input;
            Clearboard();
        }

        public void Clearboard()
        {
            board.Children.Clear();
            state = Drawstate.Idle;
            pt_begin = new Point(0, 0);
            pt_last = new Point(0, 0);
            vect_last = new Point(0, 0);
            pt_collectlast = new Point(0, 0);
            tempDraw = null;
            tempSmartDraw.Clear();
            angles.Clear();
            length.Clear();
            points.Clear();
            fardist = ang_begin = ang_sum = 0.0;
        }

        public void Changemode(Drawstate newstate)
        {
            state = newstate;
        }

        public void Pressdown(Point input)
        {
            pt_last = pt_begin = input;
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

                    line.Stroke = new SolidColorBrush(Color.FromArgb(255, 41, 159, 227));
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

                    ellipse.Stroke = new SolidColorBrush(Color.FromArgb(255, 41, 159, 227));
                    ellipse.StrokeThickness = 15;
                    ellipse.Width = ellipse.Height = radius * 2;
                    ellipse.Opacity = 1;

                    board.Children.Add(ellipse);
                    tempDraw = ellipse;
                    break;
                case Drawstate.Free:
                    line.X1 = pt_last.X;
                    line.Y1 = pt_last.Y;
                    line.X2 = input.X;
                    line.Y2 = input.Y;

                    line.Stroke = new SolidColorBrush(Color.FromArgb(255, 41, 159, 227));
                    line.StrokeStartLineCap = PenLineCap.Round;
                    line.StrokeEndLineCap = PenLineCap.Round;
                    line.StrokeThickness = 15;
                    line.Opacity = 1;

                    board.Children.Add(line);
                    pt_last = input;
                    break;
                case Drawstate.Smart:
                    double distance = CalcDist(pt_collectlast, input);
                    
                    //draw
                    line.X1 = pt_last.X;
                    line.Y1 = pt_last.Y;
                    line.X2 = input.X;
                    line.Y2 = input.Y;

                    line.Stroke = new SolidColorBrush(Color.FromArgb(255, 41, 159, 227));
                    line.StrokeStartLineCap = PenLineCap.Round;
                    line.StrokeEndLineCap = PenLineCap.Round;
                    line.StrokeThickness = 15;
                    line.Opacity = 1;

                    board.Children.Add(line);
                    tempSmartDraw.Add(line);

                    //collect
                    if (distance > 8)
                    {
                        Point vect_input = new Point(input.X - pt_collectlast.X, input.Y - pt_collectlast.Y);
                        if (vect_input.X == 0.0 && vect_input.Y == 0.0) { pt_last = input; break; }
                        if (!(vect_last.X == 0.0 && vect_last.Y == 0.0))
                        {
                            double ang = CalcAngle(vect_input, vect_last);
                            if (double.IsNaN(ang)) { pt_last = input; break; }
                            if (Math.Abs(ang) < 100.0)
                            {
                                ang_sum += ang;
                                angles.Add(ang);
                                length.Add(distance);
                                points.Add(input);
                            }
                        }
                        else
                        {
                            ang_begin = CalcAngle(new Point(0, 1), vect_input);
                            points.Add(input);
                        }
                        double distance2 = CalcDist(pt_begin, input);
                        if (distance2 > fardist) fardist = distance2;

                        pt_collectlast = input;
                        vect_last = vect_input;
                    }

                    //finalize
                    pt_last = input;
                    break;
                default: break;
            }
        }

        public void Release(Point input)
        {
            if(state==Drawstate.Smart)
            {
                List<double> ang_stat = new List<double>(angles);
                ang_stat.RemoveRange(0, 1); ang_stat.RemoveRange(ang_stat.Count - 1, 1);
                double test_closure = CalcDist(pt_begin, input),
                    ang_avg = Math.Abs(CalcAverage(ang_stat));

                //test line
                if (ang_avg < 6 && test_closure / fardist >0.9)
                {
                    //clear tempdraw
                    foreach (UIElement i in tempSmartDraw)
                    {
                        board.Children.Remove(i);
                    }
                    //line
                    Line line = new Line();
                    line.X1 = pt_begin.X;
                    line.Y1 = pt_begin.Y;
                    line.X2 = input.X;
                    line.Y2 = input.Y;

                    line.Stroke = new SolidColorBrush(Color.FromArgb(255, 41, 159, 227));
                    line.StrokeStartLineCap = PenLineCap.Round;
                    line.StrokeEndLineCap = PenLineCap.Round;
                    line.StrokeThickness = 15;
                    line.Opacity = 1;

                    board.Children.Add(line);
                }
                //test freedraw
                else if (CalcVariance(angles) > 500)
                {

                }
                else if(ang_avg > 3 && test_closure < fardist / 2)
                {
                    //clear tempdraw
                    foreach (UIElement i in tempSmartDraw)
                    {
                        board.Children.Remove(i);
                    }
                    //circle
                    ParaCircle param = CalcCircleFitting();

                    Ellipse ellipse = new Ellipse();
                    Canvas.SetLeft(ellipse, param.X - param.R);
                    Canvas.SetTop(ellipse, param.Y - param.R);

                    ellipse.Stroke = new SolidColorBrush(Color.FromArgb(255, 41, 159, 227));
                    ellipse.StrokeThickness = 15;
                    ellipse.Width = ellipse.Height = param.R * 2;
                    ellipse.Opacity = 1;

                    board.Children.Add(ellipse);
                } 

                //finalize
                tempSmartDraw.Clear();
                angles.Clear();
                length.Clear();
                points.Clear();
                fardist = ang_begin = ang_sum = 0.0;
                //}
            }
            pt_begin = new Point(0, 0);
            pt_last = new Point(0, 0);
            pt_collectlast = new Point(0, 0);
            vect_last = new Point(0, 0);
            tempDraw = null;
        }

        private double CalcDist(Point a, Point b)
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
    }
}

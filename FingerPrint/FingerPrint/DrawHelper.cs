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

    public class DrawHelper
    {
        private Canvas board;
        private Point pt_begin, pt_last;
        private Drawstate state;
        private UIElement tempDraw;
        //for smart recognizaion
        private UIElementCollection tempSmartDraw;
        private List<double> angles;
        private List<double> length;
        private double farpoint, ang_begin, ang_sum;
        private Point vect_last;

        DrawHelper() { }

        public DrawHelper(Canvas input)
        {
            tempSmartDraw = new UIElementCollection();
            angles = new List<double>();
            length = new List<double>();
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
            tempDraw = null;
            tempSmartDraw.Clear();
            angles.Clear();
            length.Clear();
            farpoint = ang_begin = ang_sum = 0.0;
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
                    double radius = Dist(pt_begin, input) / 2.0;
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
                    double distance = Dist(pt_last, input);
                    if (distance < 8) break;

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

                    //collect
                    tempSmartDraw.Add(line);
                    Point vect_input = new Point(input.X - pt_last.X, input.Y - pt_last.Y);
                    if (vect_input.X == 0.0 && vect_input.Y == 0.0) break;
                    if (!(vect_last.X == 0.0 && vect_last.Y == 0.0))
                    {
                        double ang = Angles(vect_input, vect_last);
                        if (double.IsNaN(ang)) break;
                        ang_sum += ang;
                        angles.Add(ang);
                        length.Add(distance);
                    }
                    else
                    {
                        ang_begin = Angles(new Point(0, 1), vect_input);
                    }

                    //finalize
                    vect_last = vect_input;
                    pt_last = input;
                    break;
                default: break;
            }
        }

        public void Release(Point input)
        {
            if(state==Drawstate.Smart)
            {

            }
            pt_begin = new Point(0, 0);
            pt_last = new Point(0, 0);
            tempDraw = null;
        }

        private double Dist(Point a, Point b)
        {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        private double Angles(Point a, Point b)
        {
            double n = a.X * b.X + a.Y * b.Y, m = Math.Sqrt(a.X * a.X + a.Y * a.Y) * Math.Sqrt(b.X * b.X + b.Y * b.Y);
            double ret = Math.Acos(n / m) * (180.0 / Math.PI), cross = a.X * b.Y - a.Y * b.X;
            return cross > 0 ? ret : -ret;
        }
    }
}

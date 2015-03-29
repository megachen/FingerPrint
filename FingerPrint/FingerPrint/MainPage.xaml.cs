using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using FingerPrint.Resources;

namespace FingerPrint
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Point old_point, old_vect;
        private int dispindex;
        private double stat, last_ang;
        private Ellipse last_el;
        private Line last_line;
        private bool draw = false;

        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            old_point = new Point(0, 0);
            // 用于本地化 ApplicationBar 的示例代码
            //BuildLocalizedApplicationBar();
        }

        private void OnPressed(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            old_point = e.GetPosition(paint);
            old_vect.X = 0; old_vect.Y = 0;
            dispindex = 0;
            stat = 0.0;
            disp.Children.Clear();
            paint.Children.Clear();
            Line line = new Line();
            line.X1 = 0;
            line.Y1 = disp.ActualHeight/2;
            line.X2 = disp.ActualWidth;
            line.Y2 = disp.ActualHeight / 2;
            line.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 200, 0));
            line.StrokeThickness = 1;
            line.Opacity = 1;
            disp.Children.Add(line);
            draw = true;
        }

        private void OnMoving(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point point = e.GetPosition(paint);
            if (Dist(point, old_point) < 8) return;
            if (!draw) { old_point = point; return; }
            {
                //draw line
                Line line = new Line();

                line.X1 = point.X;
                line.Y1 = point.Y;
                line.X2 = old_point.X;
                line.Y2 = old_point.Y;

                line.Stroke = new SolidColorBrush(Color.FromArgb(255, 41, 159, 227));
                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeEndLineCap = PenLineCap.Round;

                line.StrokeThickness = 15;
                line.Opacity = 1;

                paint.Children.Add(line);

                //analyze
                Point vect = new Point(point.X - old_point.X, point.Y - old_point.Y);
                if (vect.X == 0.0 && vect.Y == 0.0) return;

                if (!(old_vect.X == 0.0 && old_vect.Y == 0.0))
                {
                    double ang = Angles(vect, old_vect);
                    if (double.IsNaN(ang)) return;
                    stat += ang;
                    msg.Text = ang.ToString() + "           " + stat.ToString();
                    Ellipse pt = new Ellipse();
                    pt.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    pt.Width = pt.Height = 5;
                    disp.Children.Add(pt);
                    Canvas.SetLeft(pt, dispindex++ * 6.0);
                    if (ang > 85.0) ang = 85.0;
                    if (ang < -85.0) ang = -85.0;
                    Canvas.SetTop(pt, 90.0 - ang);
                    last_ang = ang;
                    last_el = pt;
                    last_line = line;
                }
                
                //finalize
                old_point = point;
                old_vect = vect;
            }
            
        }

        private void OnReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            disp.Children.Remove(last_el);
            stat -= last_ang;
            paint.Children.Remove(last_line);
            msg.Text = last_ang.ToString() + "           " + stat.ToString();
            draw = false;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            paint.MouseMove += new MouseEventHandler(OnMoving);
            paint.MouseLeftButtonUp += new MouseButtonEventHandler(OnReleased);
            paint.MouseLeftButtonDown += new MouseButtonEventHandler(OnPressed);
        }

         //用于生成本地化 ApplicationBar 的示例代码
        private void BuildLocalizedApplicationBar()
        {
            // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
            ApplicationBar = new ApplicationBar();

            // 创建新按钮并将文本值设置为 AppResources 中的本地化字符串。
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
            appBarButton.Text = AppResources.AppBarButtonText;
            ApplicationBar.Buttons.Add(appBarButton);

            // 使用 AppResources 中的本地化字符串创建新菜单项。
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            ApplicationBar.MenuItems.Add(appBarMenuItem);
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
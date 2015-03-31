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
        private DrawHelper draw;

        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            // 用于本地化 ApplicationBar 的示例代码
            BuildLocalizedApplicationBar();

            draw = new DrawHelper(paint);
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            //paint.MouseMove += new MouseEventHandler(OnMoving);
            //paint.MouseLeftButtonUp += new MouseButtonEventHandler(OnReleased);
            //paint.MouseLeftButtonDown += new MouseButtonEventHandler(OnPressed);
        }

        //用于生成本地化 ApplicationBar 的示例代码
        private void BuildLocalizedApplicationBar()
        {
            // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
            ApplicationBar = new ApplicationBar();

            // 创建新按钮并将文本值设置为 AppResources 中的本地化字符串。
            ApplicationBarIconButton appBtn_line = new ApplicationBarIconButton(new Uri("/Assets/Icons/edit.png", UriKind.Relative));
            appBtn_line.Text = AppResources.DT_line;
            appBtn_line.Click += appBtn_line_Click;
            ApplicationBarIconButton appBtn_circle = new ApplicationBarIconButton(new Uri("/Assets/Icons/edit.png", UriKind.Relative));
            appBtn_circle.Text = AppResources.DT_circle;
            appBtn_circle.Click += appBtn_circle_Click;
            ApplicationBarIconButton appBtn_free = new ApplicationBarIconButton(new Uri("/Assets/Icons/edit.png", UriKind.Relative));
            appBtn_free.Text = AppResources.DT_free;
            appBtn_free.Click += appBtn_free_Click;
            ApplicationBarIconButton appBtn_smart = new ApplicationBarIconButton(new Uri("/Assets/Icons/edit.png", UriKind.Relative));
            appBtn_smart.Text = AppResources.DT_smart;
            appBtn_smart.Click += appBtn_smart_Click;
            ApplicationBar.Buttons.Add(appBtn_line);
            ApplicationBar.Buttons.Add(appBtn_circle);
            ApplicationBar.Buttons.Add(appBtn_free);
            ApplicationBar.Buttons.Add(appBtn_smart);

            // 使用 AppResources 中的本地化字符串创建新菜单项。
            ApplicationBarMenuItem appMenu_clear = new ApplicationBarMenuItem(AppResources.FO_clear);
            appMenu_clear.Click += appMenu_clear_Click;
            ApplicationBar.MenuItems.Add(appMenu_clear);
        }

        private void OnPressed(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            draw.Pressdown(e.GetPosition(paint));
            (sender as Canvas).CaptureMouse();

            //old_vect.X = 0; old_vect.Y = 0;
            //dispindex = 0;
            //stat = 0.0;
            //disp.Children.Clear();
            //paint.Children.Clear();
            //Line line = new Line();
            //line.X1 = 0;
            //line.Y1 = disp.ActualHeight/2;
            //line.X2 = disp.ActualWidth;
            //line.Y2 = disp.ActualHeight / 2;
            //line.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 200, 0));
            //line.StrokeThickness = 1;
            //line.Opacity = 1;
            //disp.Children.Add(line);
            //draw = true;
        }

        private void OnMoving(object sender, System.Windows.Input.MouseEventArgs e)
        {
            draw.Move(e.GetPosition(paint));
            //Point point = e.GetPosition(paint);
            //if (Dist(point, old_point) < 8) return;
            //if (!draw) { old_point = point; return; }
            //{
            //    //draw line
            //    Line line = new Line();

            //    line.X1 = point.X;
            //    line.Y1 = point.Y;
            //    line.X2 = old_point.X;
            //    line.Y2 = old_point.Y;

            //    line.Stroke = new SolidColorBrush(Color.FromArgb(255, 41, 159, 227));
            //    line.StrokeStartLineCap = PenLineCap.Round;
            //    line.StrokeEndLineCap = PenLineCap.Round;

            //    line.StrokeThickness = 15;
            //    line.Opacity = 1;

            //    paint.Children.Add(line);

            //    //analyze
            //    Point vect = new Point(point.X - old_point.X, point.Y - old_point.Y);
            //    if (vect.X == 0.0 && vect.Y == 0.0) return;

            //    if (!(old_vect.X == 0.0 && old_vect.Y == 0.0))
            //    {
            //        double ang = Angles(vect, old_vect);
            //        if (double.IsNaN(ang)) return;
            //        stat += ang;
            //        msg.Text = ang.ToString() + "           " + stat.ToString();
            //        Ellipse pt = new Ellipse();
            //        pt.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            //        pt.Width = pt.Height = 5;
            //        disp.Children.Add(pt);
            //        Canvas.SetLeft(pt, dispindex++ * 6.0);
            //        if (ang > 85.0) ang = 85.0;
            //        if (ang < -85.0) ang = -85.0;
            //        Canvas.SetTop(pt, 90.0 - ang);
            //        last_ang = ang;
            //        last_el = pt;
            //        last_line = line;
            //    }
                
            //    //finalize
            //    old_point = point;
            //    old_vect = vect;
            //}
            
        }

        private void OnReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            draw.Release(e.GetPosition(paint));
            (sender as Canvas).ReleaseMouseCapture();
            //disp.Children.Remove(last_el);
            //stat -= last_ang;
            //paint.Children.Remove(last_line);
            //msg.Text = last_ang.ToString() + "           " + stat.ToString();
            //draw = false;
        }

        void appMenu_clear_Click(object sender, EventArgs e)
        {
            draw.Clearboard();
            Showtips(AppResources.FO_clear);
        }

        void appBtn_smart_Click(object sender, EventArgs e)
        {
            draw.Changemode(Drawstate.Smart);
            Showtips(AppResources.DT_smart);
        }

        void appBtn_free_Click(object sender, EventArgs e)
        {
            draw.Changemode(Drawstate.Free);
            Showtips(AppResources.DT_free);
        }

        void appBtn_circle_Click(object sender, EventArgs e)
        {
            draw.Changemode(Drawstate.Circle);
            Showtips(AppResources.DT_circle);
        }

        void appBtn_line_Click(object sender, EventArgs e)
        {
            draw.Changemode(Drawstate.Line);
            Showtips(AppResources.DT_line);
        }

        void Showtips(string content)
        {
            tips.Text = content;
            tipAnime.Begin();
        }

        //private double Dist(Point a, Point b)
        //{
        //    return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        //}

        //private double Angles(Point a, Point b)
        //{
        //    double n = a.X * b.X + a.Y * b.Y, m = Math.Sqrt(a.X * a.X + a.Y * a.Y) * Math.Sqrt(b.X * b.X + b.Y * b.Y);
        //    double ret = Math.Acos(n / m) * (180.0 / Math.PI), cross = a.X * b.Y - a.Y * b.X;
        //    return cross > 0 ? ret : -ret;
        //}
    }
}
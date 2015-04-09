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
        private bool menuState;
        private string colorSelectionState;

        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            // 用于本地化 ApplicationBar 的示例代码
            //BuildLocalizedApplicationBar();

            draw = new DrawHelper(cnv_paint);
            menuState = false;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            //cnv_paint.MouseMove += new MouseEventHandler(OnMoving);
            //cnv_paint.MouseLeftButtonUp += new MouseButtonEventHandler(OnReleased);
            //cnv_paint.MouseLeftButtonDown += new MouseButtonEventHandler(OnPressed);
        }

        //用于生成本地化 ApplicationBar 的示例代码
        //private void BuildLocalizedApplicationBar()
        //{
        //    // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
        //    ApplicationBar = new ApplicationBar();

        //    // 创建新按钮并将文本值设置为 AppResources 中的本地化字符串。
        //    ApplicationBarIconButton appBtn_line = new ApplicationBarIconButton(new Uri("/Assets/Icons/edit.png", UriKind.Relative));
        //    appBtn_line.Text = AppResources.DT_line;
        //    appBtn_line.Click += appBtn_line_Click;
        //    ApplicationBarIconButton appBtn_circle = new ApplicationBarIconButton(new Uri("/Assets/Icons/edit.png", UriKind.Relative));
        //    appBtn_circle.Text = AppResources.DT_circle;
        //    appBtn_circle.Click += appBtn_circle_Click;
        //    ApplicationBarIconButton appBtn_free = new ApplicationBarIconButton(new Uri("/Assets/Icons/edit.png", UriKind.Relative));
        //    appBtn_free.Text = AppResources.DT_free;
        //    appBtn_free.Click += appBtn_free_Click;
        //    ApplicationBarIconButton appBtn_smart = new ApplicationBarIconButton(new Uri("/Assets/Icons/edit.png", UriKind.Relative));
        //    appBtn_smart.Text = AppResources.DT_smart;
        //    appBtn_smart.Click += appBtn_smart_Click;
        //    ApplicationBar.Buttons.Add(appBtn_line);
        //    ApplicationBar.Buttons.Add(appBtn_circle);
        //    ApplicationBar.Buttons.Add(appBtn_free);
        //    ApplicationBar.Buttons.Add(appBtn_smart);

        //    // 使用 AppResources 中的本地化字符串创建新菜单项。
        //    ApplicationBarMenuItem appMenu_clear = new ApplicationBarMenuItem(AppResources.FO_clear);
        //    appMenu_clear.Click += appMenu_clear_Click;
        //    ApplicationBar.MenuItems.Add(appMenu_clear);
        //}

        private void OnPressed(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            draw.Pressdown(e.GetPosition(cnv_paint));
            (sender as Canvas).CaptureMouse();
        }

        private void OnMoving(object sender, System.Windows.Input.MouseEventArgs e)
        {
            draw.Move(e.GetPosition(cnv_paint));
        }

        private void OnReleased(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            draw.Release(e.GetPosition(cnv_paint));
            (sender as Canvas).ReleaseMouseCapture();
            //disp.Children.Remove(last_el);
            //stat -= last_ang;
            //cnv_paint.Children.Remove(last_line);
            //msg.Text = last_ang.ToString() + "           " + stat.ToString();
            //draw = false;
        }

        private void OnDoubleTap(object sender, GestureEventArgs e)
        {
            //draw.Release(e.GetPosition(cnv_paint));
            //(sender as Canvas).ReleaseMouseCapture();
        }

        void appMenu_clear_Click(object sender, RoutedEventArgs e)
        {
            MenuClose();
            draw.Clearboard();
            Showtips(AppResources.FO_clear);
        }

        void appBtn_smart_Click(object sender, RoutedEventArgs e)
        {
            MenuClose();
            draw.Changemode(Drawstate.Smart);
            Showtips(AppResources.DT_smart);
        }

        void appBtn_free_Click(object sender, RoutedEventArgs e)
        {
            MenuClose();
            draw.Changemode(Drawstate.Free);
            Showtips(AppResources.DT_free);
        }

        void appBtn_circle_Click(object sender, RoutedEventArgs e)
        {
            MenuClose();
            draw.Changemode(Drawstate.Circle);
            Showtips(AppResources.DT_circle);
        }

        void appBtn_line_Click(object sender, RoutedEventArgs e)
        {
            MenuClose();
            draw.Changemode(Drawstate.Line);
            Showtips(AppResources.DT_line);
        }

        private void OnClkRectanger(object sender, RoutedEventArgs e)
        {
            MenuClose();
            draw.Changemode(Drawstate.Rectangle);
            Showtips(AppResources.DT_rectangle);
        }

        private void OnClkPolygon(object sender, RoutedEventArgs e)
        {
            MenuClose();
            draw.Changemode(Drawstate.Polygon);
            Showtips(AppResources.DT_polygon);
        }

        void Showtips(string content)
        {
            txt_tips.Text = content;
            tipAnime.Begin();
        }

        private void OnClkMenu(object sender, RoutedEventArgs e)
        {
            if (!menuState) MenuOpen();
            else MenuClose();
        }

        private void MenuOpen()
        {
            menuState = true;
            Showtips("Menu");
            menuOpenAnime.Begin();
        }

        private void MenuClose()
        {
            menuState = false;
            menuCloseAnime.Begin();
        }

        private void OnClkMod(object sender, RoutedEventArgs e)
        {

        }

        private void OnCLkSave(object sender, RoutedEventArgs e)
        {

        }

        private void OnClkColorChange(object sender, RoutedEventArgs e)
        {
            colorSelectionState = (sender as Button).Content.ToString();
            MenuClose();
            menuColorOpenAnime.Begin();
        }

        private void OnClkColor(object sender, RoutedEventArgs e)
        {
            draw.ChangeColor(colorSelectionState, (sender as Button).Content.ToString());

            colorSelectionState = "";
            menuColorCloseAnime.Begin();
            MenuOpen();
        }
    }
}
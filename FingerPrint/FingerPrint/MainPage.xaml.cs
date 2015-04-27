using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using FingerPrint.Resources;
using ShakeGestures;

namespace FingerPrint
{
    public partial class MainPage : PhoneApplicationPage
    {
        private DrawHelper draw;
        private bool menuState;
        private bool leftHandMode;
        private string colorSelectionState;

        public MainPage()
        {
            ShakeGesturesHelper.Instance.ShakeGesture +=
                new EventHandler<ShakeGestureEventArgs>(Instance_ShakeGesture);
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 4;
            ShakeGesturesHelper.Instance.Active = true;

            InitializeComponent();

            draw = new DrawHelper(cnv_paint, cnv_preview, cnv_debug);
            menuState = false; leftHandMode = false;
            btn_front.DataContext = btn_board.DataContext = btn_fill.DataContext = draw;
        }

        void Instance_ShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            LayoutRoot.Dispatcher.BeginInvoke(() =>
            {
                VibrateController.Default.Start(TimeSpan.FromMilliseconds(300));
                Showtips(AppResources.FO_undo);
                draw.Undo();
            });
        }

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
        }

        private void OnDoubleTap(object sender, GestureEventArgs e)
        {
            draw.DoubleTap(e.GetPosition(cnv_paint));
            (sender as Canvas).ReleaseMouseCapture();
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
            Showtips(AppResources.MN_menu);
            menuOpenAnime.Begin();
        }

        private void MenuClose()
        {
            menuState = false;
            menuCloseAnime.Begin();
        }

        private void OnClkBack(object sender, RoutedEventArgs e)
        {
            colorSelectionState = "";
            menuColorCloseAnime.Begin();
            menuSubMainAnime.Begin();
            menuBackOutAnime.Begin();
        }

        private void OnClkMod(object sender, RoutedEventArgs e)
        {

        }

        private void OnCLkSave(object sender, RoutedEventArgs e)
        {
            WriteableBitmap _bitmap = new WriteableBitmap(cnv_paint, null);
            MediaLibrary ml = new MediaLibrary();

            MemoryStream ms=new MemoryStream();
            _bitmap.SaveJpeg(ms,_bitmap.PixelWidth,_bitmap.PixelHeight,0,80);
            ms.Seek(0, SeekOrigin.Begin);

            ml.SavePicture("FP" + DateTime.Now.ToShortDateString() + DateTime.Now.ToShortTimeString() + ".jpg", ms);
            Showtips(AppResources.TP_saved);
        }

        private void OnClkColorChange(object sender, RoutedEventArgs e)
        {
            colorSelectionState = (sender as Button).Content.ToString();
            menuMainSubAnime.Begin();
            menuColorOpenAnime.Begin();
            menuBackInAnime.Begin();
        }

        private void OnClkColor(object sender, RoutedEventArgs e)
        {
            draw.ChangeColor(colorSelectionState, (sender as Button).Content.ToString());

            colorSelectionState = "";
            menuColorCloseAnime.Begin();
            menuSubMainAnime.Begin();
        }

        private void OnClkClosure(object sender, RoutedEventArgs e)
        {
            if(draw.closureState)
            {
                draw.closureState = false;
                btn_closure.Background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                draw.closureState = true;
                btn_closure.Background = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
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
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using FingerPrint.Resources;
using ShakeGestures;

namespace FingerPrint
{
    public partial class MainPage : PhoneApplicationPage
    {
        private DrawHelper draw;
        private bool menuState;
        private bool holdState;
        private bool leftHandMode;
        private System.Windows.Point last_pos;
        private string colorSelectionState;
        Accelerometer sen_accel;

        public MainPage()
        {
            ShakeGesturesHelper.Instance.ShakeGesture +=
                new EventHandler<ShakeGestureEventArgs>(Instance_ShakeGesture);
            ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 6;
            ShakeGesturesHelper.Instance.Active = true;

            sen_accel = new Accelerometer();
            sen_accel.TimeBetweenUpdates = TimeSpan.FromMilliseconds(20);
            sen_accel.CurrentValueChanged += sen_accel_CurrentValueChanged;
            
            InitializeComponent();

            draw = new DrawHelper(cnv_paint, cnv_preview, cnv_debug);
            menuState = false; leftHandMode = false; colorSelectionState = "";
            btn_front_left.DataContext = btn_board_left.DataContext = btn_fill_left.DataContext = btn_front.DataContext = btn_board.DataContext = btn_fill.DataContext = draw;
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

        void sen_accel_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            Point shift = new Point(e.SensorReading.Acceleration.X, e.SensorReading.Acceleration.Y);
            Dispatcher.BeginInvoke(() => draw.ModPosition(shift));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            IDictionary<string, string> param = NavigationContext.QueryString;
            if (!param.ContainsKey("msg")) { RightHandMode(); return; }
            if (param["msg"] == "left")
            {
                LeftHandMode();
            }
            else
            {
                RightHandMode();
            }
        }

        void LeftHandMode()
        {
            grd_menu_left.Visibility = Visibility.Visible;
            grd_menu_right.Visibility = Visibility.Collapsed;
            leftHandMode = true;
            leftHandModeInAnime.Begin();
        }

        void RightHandMode()
        {
            grd_menu_left.Visibility = Visibility.Collapsed;
            grd_menu_right.Visibility = Visibility.Visible;
            leftHandMode = false;
            rightHandModeInAnime.Begin();
        }

        private void OnHold(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (menuState) return;
            holdState = true;
            if (leftHandMode)
            {
                cnv_menu_left_panel.CaptureMouse();
                last_pos = new Point(10, 320);
            }
            else
            {
                cnv_menu_right_panel.CaptureMouse();
                last_pos = new Point(320, 320);
            }
        }

        private void OnMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (holdState)
            {
                Point pos;
                if (leftHandMode)
                {
                    pos = e.GetPosition(cnv_menu_left_panel);
                    Canvas.SetLeft(btn_menu_left, pos.X - 35);
                    Canvas.SetTop(btn_menu_left, pos.Y - 35);
                }
                else
                {
                    pos = e.GetPosition(cnv_menu_right_panel);
                    Canvas.SetLeft(btn_menu, pos.X - 35);
                    Canvas.SetTop(btn_menu, pos.Y - 35);
                }
                last_pos = pos;
            }
        }

        private void OnRelease(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (holdState)
            {
                holdState = false;

                if(leftHandMode)
                {
                    if(draw.CalcDist(last_pos, new Point(10,320))>20)
                    {
                        leftHandModeOutAnime.Begin();
                        RightHandMode();
                    }
                    else
                    {
                        Canvas.SetLeft(btn_menu_left, 10);
                        Canvas.SetTop(btn_menu_left, 320);
                    }
                    cnv_menu_left_panel.ReleaseMouseCapture();
                }
                else
                {
                    if (draw.CalcDist(last_pos, new Point(320, 320)) > 20)
                    {
                        rightHandModeOutAnime.Begin();
                        LeftHandMode();
                    }
                    else
                    {
                        Canvas.SetLeft(btn_menu_left, 320);
                        Canvas.SetTop(btn_menu_left, 320);
                    }
                    cnv_menu_right_panel.ReleaseMouseCapture();
                }
            }
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
            double dist;
            if(leftHandMode)dist = draw.CalcDist(new Point(10,320), new Point(Canvas.GetLeft(btn_menu_left),Canvas.GetTop(btn_menu_left)));
            else dist = draw.CalcDist(new Point(320,320), new Point(Canvas.GetLeft(btn_menu),Canvas.GetTop(btn_menu)));
            if (dist > 5) return;
            if (!menuState) MenuOpen();
            else MenuClose();
        }

        private void MenuOpen()
        {
            menuState = true;
            Showtips(AppResources.MN_menu);
            menuOpenAnime.Begin();
            menuOpenAnime_left.Begin();
        }

        private void MenuClose()
        {
            menuState = false;
            menuCloseAnime.Begin();
            menuCloseAnime_left.Begin();
        }

        private void OnClkBack(object sender, RoutedEventArgs e)
        {
            if(colorSelectionState != "")
            {
                colorSelectionState = "";
                menuColorCloseAnime.Begin();
                menuColorCloseAnime_left.Begin();
            }
            else
            {
                menuModCloseAnime.Begin();
                menuModCloseAnime_left.Begin();
            }
            menuSubMainAnime.Begin();
            menuBackOutAnime.Begin();
            menuSubMainAnime_left.Begin();
            menuBackOutAnime_left.Begin();
        }

        private void OnClkMod(object sender, RoutedEventArgs e)
        {
            menuModOpenAnime.Begin();
            menuModOpenAnime_left.Begin();

            menuMainSubAnime.Begin();
            menuBackInAnime.Begin();
            menuMainSubAnime_left.Begin();
            menuBackInAnime_left.Begin();
        }

        double scale = 1.0;
        private void OnModHold(object sender, System.Windows.Input.MouseEventArgs e)
        {
            holdState = true;
            scale = scale_paint.ScaleX;
            if (leftHandMode)
            {
                cnv_menu_left_panel.CaptureMouse();
                last_pos = new Point(170, 170);
            }
            else
            {
                cnv_menu_right_panel.CaptureMouse();
                last_pos = new Point(170, 170);
            }

            sen_accel.Start();
        }

        private void OnModMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (holdState)
            {
                Point pos;
                if (leftHandMode)
                {
                    pos = e.GetPosition(cnv_menu_left_panel);
                    Canvas.SetLeft(btn_mod_left, pos.X - 30);
                    Canvas.SetTop(btn_mod_left, pos.Y - 30);

                    double para = 400.0 - pos.X - pos.Y;
                    if (para > 20)
                    {
                        scale_paint.ScaleX = scale_paint.ScaleY = scale * Math.Pow(2, (para - 20) / 100.0);
                        scale_preview.ScaleX = scale_preview.ScaleY = scale * Math.Pow(2, (para - 20) / 100.0);
                    }
                    else if (para < -20)
                    {
                        scale_paint.ScaleX = scale_paint.ScaleY = scale * Math.Pow(2, (para + 20) / 100.0);
                        scale_preview.ScaleX = scale_preview.ScaleY = scale * Math.Pow(2, (para + 20) / 100.0);
                    }
                }
                else
                {
                    pos = e.GetPosition(cnv_menu_right_panel);
                    Canvas.SetLeft(btn_mod, pos.X - 30);
                    Canvas.SetTop(btn_mod, pos.Y - 30);

                    double para = pos.X - pos.Y;
                    if (para > 20)
                    {
                        scale_paint.ScaleX = scale_paint.ScaleY = scale * Math.Pow(2, (para - 20) / 100.0);
                        scale_preview.ScaleX = scale_preview.ScaleY = scale * Math.Pow(2, (para - 20) / 100.0);
                    }
                    else if (para < -20)
                    {
                        scale_paint.ScaleX = scale_paint.ScaleY = scale * Math.Pow(2, (para + 20) / 100.0);
                        scale_preview.ScaleX = scale_preview.ScaleY = scale * Math.Pow(2, (para + 20) / 100.0);
                    }
                }
                last_pos = pos;
            }
        }

        private void OnModRelease(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (holdState)
            {
                holdState = false;

                sen_accel.Stop();

                if (leftHandMode)
                {
                    menuModRestoreAnime_left.Begin();
                    cnv_menu_left_panel.ReleaseMouseCapture();
                }
                else
                {
                    menuModRestoreAnime.Begin();
                    cnv_menu_right_panel.ReleaseMouseCapture();
                }
            }
        }

        private void ModPos(Point shift)
        {
            scale_paint.TranslateX += shift.X * 10.0;
            scale_paint.TranslateY -= shift.Y * 10.0;
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
            menuMainSubAnime_left.Begin();
            menuColorOpenAnime_left.Begin();
            menuBackInAnime_left.Begin();
        }

        private void OnClkColor(object sender, RoutedEventArgs e)
        {
            draw.ChangeColor(colorSelectionState, (sender as Button).Content.ToString());

            colorSelectionState = "";
            menuColorCloseAnime.Begin();
            menuSubMainAnime.Begin();
            menuBackOutAnime.Begin();
            menuColorCloseAnime_left.Begin();
            menuSubMainAnime_left.Begin();
            menuBackOutAnime_left.Begin();
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
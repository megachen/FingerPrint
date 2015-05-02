using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace FingerPrint
{
    public partial class InitPage : PhoneApplicationPage
    {
        bool hold;
        Point last_pos;

        public InitPage()
        {
            hold = false;
            last_pos = new Point();
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PhoneApplicationFrame myFrame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (myFrame != null)
            {
                try
                {
                    myFrame.RemoveBackEntry();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void OnHold(object sender, System.Windows.Input.MouseEventArgs e)
        {
            hold = true;
            cnv_drag.CaptureMouse();
        }

        private void OnMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (hold)
            {
                Point pos = e.GetPosition(cnv_drag);
                Canvas.SetLeft(button, pos.X - 100);
                Canvas.SetTop(button, pos.Y - 100);
                last_pos = pos;
            }
        }
    
        private void OnRelease(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(hold)
            {
                hold = false;
                if (last_pos.X > cnv_drag.ActualWidth * 2 / 3) //right
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml?msg=right", UriKind.Relative));
                }
                else if (last_pos.X < cnv_drag.ActualWidth / 3) //left
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml?msg=left", UriKind.Relative));
                }
                else
                {
                    Canvas.SetLeft(button, cnv_drag.ActualWidth / 2 - 100);
                    Canvas.SetTop(button, cnv_drag.ActualHeight / 2 + 100);
                }
            }
            cnv_drag.ReleaseMouseCapture();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(button, cnv_drag.ActualWidth / 2 - 100);
            Canvas.SetTop(button, cnv_drag.ActualHeight / 2 + 100);
            Storyboard_Init.Begin();
        }
    }
}
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
                Canvas.SetLeft(button, pos.X - 70);
                Canvas.SetTop(button, pos.Y - 70);
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
                    Canvas.SetLeft(button, cnv_drag.ActualWidth / 2 - 70);
                    Canvas.SetTop(button, cnv_drag.ActualHeight / 2 + 70);
                }
            }
            cnv_drag.ReleaseMouseCapture();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(button, cnv_drag.ActualWidth / 2 - 70);
            Canvas.SetTop(button, cnv_drag.ActualHeight / 2 + 70);
            Storyboard_Init.Begin();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;

namespace FirstLineTool.Helper
{
    public static class WindowAnimator
    {
        public static async Task FadeTransition(Window currentWindow, Window newWindow, double fadeOutSeconds = 0.5, double fadeInSeconds = 0.8)
        {
            if (currentWindow != null)
            {
                var fadeOut = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromSeconds(fadeOutSeconds))
                };
                currentWindow.BeginAnimation(Window.OpacityProperty, fadeOut);
            }

            // تأخير قبل فتح النافذة الجديدة
            await Task.Delay(TimeSpan.FromSeconds(fadeOutSeconds));

            if (newWindow != null)
            {
                newWindow.Opacity = 0;
                newWindow.Show();

                var fadeIn = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromSeconds(fadeInSeconds))
                };

                // اغلق النافذة الحالية بعد انتهاء fade-in
                fadeIn.Completed += (s, e) =>
                {
                    if (currentWindow != null)
                        currentWindow.Close();
                };

                newWindow.BeginAnimation(Window.OpacityProperty, fadeIn);
            }
        }

    }
}

using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Ozu_EMS
{
    public class CustomLongListSelector : LongListSelector
    {
        private ViewportControl _viewportControl;
        // event which we will fire on position changed
        // I listen to this event on page where I have this long list selecotr
        public event EventHandler PositionChanged;

        // this function is ovverided and is called first time 
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _viewportControl = (ViewportControl)GetTemplateChild("ViewportControl");
            _viewportControl.ManipulationStateChanged +=_viewportControl_ManipulationStateChanged;
            // This section is used for hiding scroll bar 
            // mayby you want to keep it I do not 
            // Uncomment following 2 line to do so...
            ScrollBar sb = ((FrameworkElement)VisualTreeHelper.GetChild(this, 0)).FindName("VerticalScrollBar") as ScrollBar;
            sb.RenderTransform = base.RenderTransform;
            // sb.Width = 0;
        }

        // this event fires when manupulation changes
        void _viewportControl_ManipulationStateChanged(object sender, ManipulationStateChangedEventArgs e)
        {
            //fire the event.
            PositionChanged(_viewportControl, new EventArgs());
        }
    }
}

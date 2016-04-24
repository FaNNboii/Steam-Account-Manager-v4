using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AccountManager.Views
{
    /// <summary>
    /// Interaktionslogik für TimeBar.xaml
    /// </summary>
    public partial class TimeBar : UserControl
    {
        public TimeBar()
        {           
            InitializeComponent();
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double width = (this.ActualWidth / MaxValue) * Value;
            double left = (this.ActualWidth - width)/2;

            Pen p = new Pen();

            drawingContext.DrawRectangle(Value > CriticalValue ? Color : CriticalColor, p, new Rect(left, 0.0, width, this.ActualHeight));

        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(TimeBar), new FrameworkPropertyMetadata(0,FrameworkPropertyMetadataOptions.AffectsRender));




        public int CriticalValue
        {
            get { return (int)GetValue(CriticalValueProperty); }
            set { SetValue(CriticalValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CriticalValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CriticalValueProperty =
            DependencyProperty.Register("CriticalValue", typeof(int), typeof(TimeBar), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender));




        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(TimeBar), new FrameworkPropertyMetadata(100, FrameworkPropertyMetadataOptions.AffectsRender));




        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(TimeBar), new FrameworkPropertyMetadata(Brushes.Green, FrameworkPropertyMetadataOptions.AffectsRender));




        public Brush CriticalColor
        {
            get { return (Brush)GetValue(CriticalColorProperty); }
            set { SetValue(CriticalColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CriticalColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CriticalColorProperty =
            DependencyProperty.Register("CriticalColor", typeof(Brush), typeof(TimeBar), new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));


    }
}

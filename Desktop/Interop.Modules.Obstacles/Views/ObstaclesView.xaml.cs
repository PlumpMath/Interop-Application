﻿using Prism.Events;
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

using GMap.NET.WindowsPresentation;
using System.Globalization;

namespace Interop.Modules.Obstacles.Views
{
    /// <summary>
    /// Interaction logic for ObstaclesView.xaml
    /// </summary>
    public partial class ObstaclesView : UserControl, Infrastructure.Interfaces.IView
    {
        
        public ObstaclesView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            this.Map.Zoom = 5;
            DataContext = new ViewModels.ObstaclesViewModel(eventAggregator, this);
        }
    }

    public class Map : GMapControl 
    { 
       public long ElapsedMilliseconds; 
 
       DateTime start; 
       DateTime end; 
       int delta; 
 

       private int counter; 
       readonly Typeface tf = new Typeface("GenericSansSerif"); 
       readonly System.Windows.FlowDirection fd = new System.Windows.FlowDirection(); 
 

       /// <summary> 
       /// any custom drawing here 
       /// </summary> 
       /// <param name="drawingContext"></param> 
       protected override void OnRender(DrawingContext drawingContext)
       { 
          start = DateTime.Now;
          base.OnRender(drawingContext);
          end = DateTime.Now; 
          delta = (int)(end - start).TotalMilliseconds; 
 
          FormattedText text = new FormattedText(string.Format(CultureInfo.InvariantCulture, "{0:0.0}", Zoom) + "z, " + MapProvider + ", refresh: " + counter++ + ", load: " + ElapsedMilliseconds + "ms, render: " + delta + "ms", CultureInfo.InvariantCulture, fd, tf, 10, Brushes.Lime); 
          drawingContext.DrawText(text, new Point(text.Height, text.Height)); 
          text = null; 
       }
    }
}
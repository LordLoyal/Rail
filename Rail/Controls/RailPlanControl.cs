﻿using Rail.Misc;
using Rail.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
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

namespace Rail.Controls
{
    public class RailPlanControl : Control
    {
        public const double dockDistance = 10;
        public const double rotateDistance = 12;
        public double margine = 0;

        static RailPlanControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RailPlanControl), new FrameworkPropertyMetadata(typeof(RailPlanControl)));
        }

        public RailPlanControl()
        {
        }

        #region ZoomFactor

        public static readonly DependencyProperty ZoomFactorProperty =
            DependencyProperty.Register("ZoomFactor", typeof(double), typeof(RailPlanControl),
                new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(OnZoomFactorChanged)));

        public double ZoomFactor
        {
            get
            {
                return (double)GetValue(ZoomFactorProperty);
            }
            set
            {
                SetValue(ZoomFactorProperty, value);
            }
        }

        private static void OnZoomFactorChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            RailPlanControl railPlan = (RailPlanControl)o;
            railPlan.CalcGroundSize();
        }

        public double RailMargin
        {
            get
            {
                return this.margine;
            }
            set
            {
                this.margine = value;
            }
        }

        #endregion

        #region SelectedTrack

        public static readonly DependencyProperty SelectedTrackProperty =
            DependencyProperty.Register("SelectedTrack", typeof(TrackBase), typeof(RailPlanControl));

        public TrackBase SelectedTrack
        {
            get
            {
                return (TrackBase)GetValue(SelectedTrackProperty);
            }
            set
            {
                SetValue(SelectedTrackProperty, value);
            }
        }

        #endregion

        #region RailPlan

        public static readonly DependencyProperty RailPlanProperty =
            DependencyProperty.Register("RailPlan", typeof(RailPlan), typeof(RailPlanControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnRailPlanPropertyChanged)));

        public RailPlan RailPlan
        {
            get
            {
                return (RailPlan)GetValue(RailPlanProperty);
            }
            set
            {
                SetValue(RailPlanProperty, value);
            }
        }

        private static void OnRailPlanPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            RailPlanControl railPlan = (RailPlanControl)o;
            railPlan.CalcGroundSize();
        }

        #endregion

        #region MousePosition

        public static readonly DependencyProperty MousePositionProperty =
            DependencyProperty.Register("MousePosition", typeof(Point), typeof(RailPlanControl),
                new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Point MousePosition
        {
            get
            {
                return (Point)GetValue(MousePositionProperty);
            }
            set
            {
                SetValue(MousePositionProperty, value);
            }
        }

        #endregion

        private void CalcGroundSize()
        {
            this.Width =  margine * 2 + this.RailPlan.Width  * this.ZoomFactor;
            this.Height = margine * 2 + this.RailPlan.Height * this.ZoomFactor;
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        //private double Sin(double value)
        //{
        //    return Math.Sin(value * Math.PI / 180.0);
        //}

        //private double Cos(double value)
        //{
        //    return Math.Cos(value * Math.PI / 180.0);
        //}

        private readonly Pen blackPen = new Pen(Brushes.Black, 1);
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // drawn background is needed for detecting mouse moves
            drawingContext.DrawRectangle(this.Background, null, new Rect(0, 0, this.Width, this.Height));

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(this.ZoomFactor, this.ZoomFactor));
            transformGroup.Children.Add(new TranslateTransform(margine, margine));
            drawingContext.PushTransform(transformGroup);

            // dray plate
            RenderPlate(drawingContext);

            // draw tracks
            this.RailPlan.Rails.ForEach(r => r.DrawTrack(drawingContext));
            this.RailPlan.Rails.ForEach(r => r.DrawDockPoints(drawingContext));
            
            drawingContext.Pop();
        }

        private readonly Brush plateBrush = new SolidColorBrush(Colors.Green);
        
        protected void RenderPlate(DrawingContext drawingContext)
        {
            drawingContext.DrawGeometry(plateBrush, blackPen, new PathGeometry(new PathFigureCollection
            {
                new PathFigure(new Point(0, 0), new PathSegmentCollection
                {
                    new LineSegment(new Point(0, this.RailPlan.Height1), true),
                    new LineSegment(new Point(this.RailPlan.Width1, this.RailPlan.Height1), true),
                    new LineSegment(new Point(this.RailPlan.Width1, this.RailPlan.Height2), true),
                    new LineSegment(new Point(this.RailPlan.Width1 + this.RailPlan.Width2 , this.RailPlan.Height2), true),
                    new LineSegment(new Point(this.RailPlan.Width1 + this.RailPlan.Width2 , this.RailPlan.Height3), true),
                    new LineSegment(new Point(this.RailPlan.Width1 + this.RailPlan.Width2 + this.RailPlan.Width3, this.RailPlan.Height3), true),
                    new LineSegment(new Point(this.RailPlan.Width1 + this.RailPlan.Width2 + this.RailPlan.Width3, 0), true),
                }, true)
            }));
        }


        public void InsertTrack(Point pos)
        {
            this.RailPlan.Rails.Add(new RailItem(this.SelectedTrack, pos));
            this.InvalidateVisual();
        }

        private RailItem FindRailItem(Point point)
        {
            RailItem track = null;
            track = this.RailPlan.Rails.Where(t => t.IsInside(point)).FirstOrDefault();
            return track;
        }

        //private RailTrack FindDocking(RailTrack track)
        //{
        //    if (this.Tracks != null)
        //    {
        //        List<DockPoint> dockPoints = track.DockPoints;
        //        var otherTracks = this.Tracks.Where(t => t != track).ToList();

        //        foreach (DockPoint dockPoint in dockPoints)
        //        {
        //            foreach (RailTrack t in otherTracks)
        //            {
        //                foreach (DockPoint dp in t.DockPoints)
        //                {
        //                    if (Math.Abs(dp.X - dockPoint.X) < dockDistance && Math.Abs(dp.Y - dockPoint.Y) < dockDistance)
        //                    {
        //                        track.Position += new Vector(dp.X - dockPoint.X, dp.Y - dockPoint.Y);
        //                        return t;
        //                    }
        //                }
        //            }

        //        }
        //    }
        //    return null;
        //}

        private RailItem FindDocking(RailItem railItem, IEnumerable<RailItem> docked = null)
        {
            if (this.RailPlan.Rails != null)
            {
                var dockPoints = railItem.DockPoints;
                var otherTracks =
                    docked != null ?
                    this.RailPlan.Rails.Where(t => t != railItem).Where(t => !docked.Contains(t)).ToList() :
                    this.RailPlan.Rails.Where(t => t != railItem);

                foreach (var dockPoint in dockPoints)
                {
                    foreach (RailItem t in otherTracks)
                    {
                        foreach (var dp in t.DockPoints)
                        {
                            //if (Math.Abs(dp.X - dockPoint.X) < dockDistance && Math.Abs(dp.Y - dockPoint.Y) < dockDistance)
                            if (dp.Distance(dockPoint) < dockDistance)
                            {
                                double rotate = 180.0 - dockPoint.Angle + dp.Angle;

                                var sub = FindSubgraph(railItem).Where(f => f != railItem).ToList();
                                foreach (RailItem rt in sub)
                                {
                                    //rt.Angle += rotate;
                                    //rt.Position = track.Position.Rotate(rotate, dp);
                                }
                                railItem.Rotate(rotate);
                                railItem.Move(new Vector(dp.X - dockPoint.X, dp.Y - dockPoint.Y));
                                return t;
                            }
                        }
                    }

                }
            }
            return null;
        }

        //private double Distance(Point a, Point b)
        //{
        //    return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        //}

        private void FindSubgraph(List<RailItem> list, RailItem startTrack)
        {
            if (this.RailPlan?.Rails != null)
            {
                var dockPoints = startTrack.DockPoints;
                var otherTracks = this.RailPlan.Rails.Where(t => t != startTrack).ToList();

                foreach (var dockPoint in dockPoints)
                {
                    foreach (RailItem t in otherTracks)
                    {
                        if (!list.Contains(t))
                        {
                            foreach (var dp in t.DockPoints)
                            {
                                if (dp.Distance(dockPoint) < dockDistance)
                                {
                                    list.Add(t);
                                    FindSubgraph(list, t);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<RailItem> FindSubgraph(RailItem track)
        {
            List<RailItem> tracks = new List<RailItem>();
            FindSubgraph(tracks, track);
            //tracks.Add(track);
            return tracks;
        }

        private void MoveTrack(RailItem track, Vector move, IEnumerable<RailItem> subgraph = null)
        {
            track.Move(move);
            if (subgraph != null)
            {
                foreach (RailItem tr in subgraph.Where(t => t != track))
                {
                    tr.Move(move);
                }
            }
            //FindDocking(track, subgraph);
        }

        private void RotateTrack(RailItem railItem, double angle, IEnumerable<RailItem> subgraph = null)
        {
            if (angle == 0.0)
            {
                return;
            }
            railItem.Rotate(angle);
            if (subgraph != null)
            {
                subgraph.Where(t => t != railItem).ToList().ForEach(tr => tr.Rotate(angle, railItem));
            }
            // FindDocking(track, subgraph);
        }

        private RailAction actionType;
        private RailItem actionTrack;
        private List<RailItem> dockedTracks;
        private Point lastMousePosition;

        private double startRotationValue;
        private double lastRotationAngle;
        
        private Point GetMousePosition(MouseEventArgs e)
        {
            return e.GetPosition(this).Move(-margine, -margine).Scale(1.0 / this.ZoomFactor);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            Point pos = GetMousePosition(e);
            InsertTrack(pos);

            base.OnMouseDoubleClick(e);
        }

        private double Angle(Point center, Point p)
        {
            Vector v1 = p - center;
            Vector v2 = new Vector(100,0);
            return Vector.AngleBetween(v1, v2);

        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Point pos = GetMousePosition(e);

            
            if ((this.actionTrack = FindRailItem(pos)) != null)
            {
                RailDockPoint dp = this.actionTrack.DockPoints.FirstOrDefault(d => d.Distance(pos) < rotateDistance);
                if (dp != null)
                {
                    this.actionType = RailAction.Rotate;
                //    this.dockedTracks = FindSubgraph(this.actionTrack);
                    
                    //this.startRotationValue = e.GetPosition(this).Y;
                    this.lastRotationAngle = Angle(this.actionTrack.Position, pos);
                }
                else
                {
                    //        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    //        {
                    this.actionType = RailAction.MoveSingle;
            //            this.dockedTracks = null;
            //        }
            //        else
            //        {
            //            this.actionType = RailAction.MoveGraph;
            //            this.dockedTracks = FindSubgraph(this.actionTrack);
            //        }
                }
                this.lastMousePosition = pos;
                this.CaptureMouse();
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point pos = GetMousePosition(e);
            this.MousePosition = pos;

            if (this.actionTrack != null)
            {
                //    Trace.TraceInformation("OnMouseMove ({0}, {1})", e.GetPosition(this).X, e.GetPosition(this).Y);

                //    Point zoomedMousePosition = e.GetPosition(this).Scale(1.0 / this.ZoomFactor);
                //double rotate = Math.Truncate((e.GetPosition(this).Y - this.startRotationValue) / 5.0) * 7.5; 

                switch (this.actionType)
                {
                case RailAction.MoveSingle:
                    MoveTrack(this.actionTrack, pos - this.lastMousePosition);
                    break;
                //    case RailAction.MoveGraph:
                //        MoveTrack(this.actionTrack, zoomedMousePosition - this.zoomedLastMousePosition, this.dockedTracks);
                //        FindDocking(this.actionTrack, this.dockedTracks);
                //        break;
                case RailAction.Rotate:
                    double rotateAngle = Angle(this.actionTrack.Position, pos);

                    RotateTrack(this.actionTrack, this.lastRotationAngle - rotateAngle, this.dockedTracks);
                    FindDocking(this.actionTrack, this.dockedTracks);
                    this.lastRotationAngle = rotateAngle;
                    break;
                }
                //    this.zoomedLastMousePosition = zoomedMousePosition;
               
                this.lastMousePosition = pos;
                this.InvalidateVisual();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            Point pos = GetMousePosition(e);

            if (this.actionTrack != null)
            {
                Point mousePosition = e.GetPosition(this);
                // delete track/tracks if outside
                if (mousePosition.X < 0 || mousePosition.X >= this.ActualWidth || mousePosition.Y < 0 || mousePosition.Y >= this.ActualHeight)
                {
                    //Trace.TraceInformation("OnMouseLeftButtonUp ({0}, {1}) ({2}, {3}) ", e.GetPosition(this).X, e.GetPosition(this).Y, this.ActualWidth, this.ActualHeight);
                    //this.RailPlan.Rails.Remove(this.actionTrack);
                    //if (this.dockedTracks != null)
                    //{
                    //    foreach (RailItem track in this.dockedTracks)
                    //    {
                    //        this.RailPlan.Rails.Remove(track);
                    //    }
                    //}
                }
                else
                {
                    //Point zoomedMousePosition = e.GetPosition(this).Scale(1.0 / this.ZoomFactor);
                    //Vector move = zoomedMousePosition - this.zoomedLastMousePosition;

                    ////Vector m = mousePosition - this.actionMouseLastPosition;
                    //RailTrack dockingTrack;

                    //switch (this.actionType)
                    //{
                    //case RailAction.MoveSingle:
                    //    this.actionTrack.Position += move;
                    //    dockingTrack = FindDocking(this.actionTrack);
                    //    if (dockingTrack != null)
                    //    {
                    //        this.actionTrack.DockTo(dockingTrack);
                    //    }
                    //    break;
                    //case RailAction.MoveGraph:
                    //    this.actionTrack.Position += move;
                    //    dockingTrack = FindDocking(this.actionTrack);
                    //    if (dockingTrack != null)
                    //    {
                    //        this.actionTrack.DockTo(dockingTrack);
                    //    }
                    //    foreach (RailTrack track in this.dockedTracks)
                    //    {
                    //        track.Position += move;
                    //    }
                    //    break;
                    //case RailAction.Rotate:
                    //    //this.actionTrack.Angle = this.actionTrackStartAngle + ((int)(move.Y / 5)) * 7.5;
                    //    break;
                    //}
                }
                this.dockedTracks = null;
                this.actionTrack = null;
                this.InvalidateVisual();
                this.ReleaseMouseCapture();

                //this.actionMouseLastPosition = e.GetPosition(this);
            }
            base.OnMouseLeftButtonUp(e);
        }


       

        protected enum RailAction
        {
            MoveSingle,
            MoveGraph,
            Rotate
        }
       
    }
}

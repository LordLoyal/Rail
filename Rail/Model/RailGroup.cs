﻿using Rail.Controls;
using Rail.Trigonometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Rail.Model
{
    public class RailGroup : RailBase
    {
        protected readonly Pen linePen = new Pen(TrackBrushes.TrackFrame, 2);
        protected readonly Pen dotPen = new Pen(Brushes.White, 2) { DashStyle = DashStyles.Dot };

        public RailGroup()
        { }

        public RailGroup(IEnumerable<RailBase> railItems)
        {
            RailItem firstRailItme = (RailItem)railItems.FirstOrDefault();
            this.Position = firstRailItme.Position; // set before new RailGroupItem
            this.Layer = firstRailItme.Layer;

            this.Rails = railItems.Cast<RailItem>().Select(i => new RailGroupItem(i, this)).ToList();
            
            this.Rails.ForEach(r => r.IsSelected = false);
            this.DockPoints = new List<RailDockPoint>();
            this.IsSelected = true;

            var intDockPoints =
            this.DockPoints = this.Rails.SelectMany(r => r.DockPoints).Where(d => !d.IsDocked || !this.Rails.Contains(d.DockedWith.RailItem)).ToList();

            CreateCombinedGeometries();
        }

        public IEnumerable<RailItem> Resolve()
        {
            return this.Rails.Select(r => new RailItem(r)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        [XmlArray("Rails")]
        [XmlArrayItem("Rail")]
        public List<RailGroupItem> Rails { get; set; }

        [XmlIgnore]
        public override List<TrackMaterial> Materials
        {
            get { return this.Rails.SelectMany(r => r.Materials).ToList(); }
        }

        private Geometry combinedGeometryTracks;
        private Geometry combinedGeometryRail;

        private void CreateCombinedGeometries()
        {
            this.combinedGeometryTracks = this.Rails.Select(r =>
            {
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(new RotateTransform(r.Angle));
                transformGroup.Children.Add(new TranslateTransform(r.Position.X, r.Position.Y));
                
                //Transform transformGroup = r.RailTransform;

                Geometry geometry = r.Track.GeometryTracks.Clone();
                geometry.Transform = transformGroup;
                return geometry;
            }).Aggregate((a, b) => new CombinedGeometry(GeometryCombineMode.Union, a, b));

            this.combinedGeometryRail = this.Rails.Select(r =>
            {
                
                Geometry geometry = r.Track.GeometryRail.Clone();
                geometry.Transform = r.RailTransform;
                return geometry;
            }).Aggregate((a, b) => new CombinedGeometry(GeometryCombineMode.Union, a, b));
        }

        public override void DrawRailItem(DrawingContext drawingContext, RailViewMode viewMode, RailLayer layer)
        {
            drawingContext.PushTransform(this.RailTransform);

            this.Rails.ForEach(r => r.DrawRailItem(drawingContext, viewMode, layer));

            if (this.IsSelected && viewMode < RailViewMode.Terrain)
            {
                /*
                //GeometryGroup geometryGroup = new GeometryGroup();
                //geometryGroup.FillRule = FillRule.EvenOdd;

                var x = this.Rails.Select(r =>
                {
                    Geometry geometrie = viewMode switch { RailViewMode.Tracks => r.Track.GeometryTracks, RailViewMode.Rail => r.Track.GeometryRail, _ => null };

                    TransformGroup transformGroup = new TransformGroup();
                    transformGroup.Children.Add(new RotateTransform(r.Angle));
                    transformGroup.Children.Add(new TranslateTransform(r.Position.X, r.Position.Y));

                    Geometry newGeometry = geometrie.Clone();
                    newGeometry.Transform = transformGroup;
                    //geometryGroup.Children.Add(newGeometry);
                    return newGeometry;
                }).ToArray();

                CombinedGeometry combinedGeometry = new CombinedGeometry(GeometryCombineMode.Union, x[0], x[1]);

                //GeometryGroup combinedGeometry = new GeometryGroup();
                //combinedGeometry.FillRule = FillRule.Nonzero;
                //combinedGeometry.Children.Add(x[0]);
                //combinedGeometry.Children.Add(x[1]);

                drawingContext.DrawDrawing(new GeometryDrawing(null, this.linePen, combinedGeometry));
                drawingContext.DrawDrawing(new GeometryDrawing(null, this.dotPen, combinedGeometry));
                */
                Geometry geometrie = viewMode switch { RailViewMode.Tracks => combinedGeometryTracks, RailViewMode.Rail => combinedGeometryRail, _ => null };
                drawingContext.DrawDrawing(new GeometryDrawing(null, this.linePen, geometrie));
                drawingContext.DrawDrawing(new GeometryDrawing(null, this.dotPen, geometrie));
            }

            drawingContext.Pop();
        }

        public override void Move(Vector vec)
        {
            // move group
            this.Position += vec;

            // move all items
            //this.Rails.ForEach(r => r.Position += vec);
        }

        public override void Rotate(Rotation rotation, Point center)
        {
            // rotate group
            this.Position = this.Position.Rotate(rotation, center);

            // rotate all items
            //this.Rails.ForEach(r =>
            //{
            //    r.Angle += rotation;
            //    r.Position = r.Position.Rotate(rotation, center);
            //});
        }

        //public override bool IsInside(Point point, RailViewMode viewMode)
        //{
        //    Geometry geometry = GetGeometry(viewMode, this.RailTransform);
        //    bool f = geometry.FillContains(point);
        //    return f;
        //}

        //public override bool IsInside(Rect rec, RailViewMode viewMode)
        //{
        //    Geometry geometry = GetGeometry(viewMode, this.RailTransform);
        //    bool f = rec.Contains(geometry.Bounds);
        //    return f;
        //}

        protected override Geometry GetGeometry(RailViewMode viewMode, Transform transform)
        {
            Geometry geometry = viewMode switch { RailViewMode.Tracks => this.combinedGeometryTracks.Clone(), RailViewMode.Rail => this.combinedGeometryRail.Clone(), _ => null };
            geometry.Transform = this.RailTransform;
            return geometry;
        }

    }
}

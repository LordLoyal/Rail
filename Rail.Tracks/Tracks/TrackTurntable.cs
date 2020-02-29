﻿using Rail.Tracks.Properties;
using Rail.Tracks.Trigonometry;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Rail.Tracks
{
    public class TrackTurntable : TrackBaseSingle
    {
        #region store

        [XmlAttribute("OuterRadius")]
        public string OuterRadiusName { get; set; }

        [XmlAttribute("InnerRadius")]
        public string InnerRadiusName { get; set; }

        [XmlAttribute("Angle")]
        public string AngleName { get; set; }

        [XmlAttribute("RailNum")]
        public int RailNum { get; set; }

        #endregion

        #region intern

        [XmlIgnore, JsonIgnore]
        public double OuterRadius { get; set; }

        [XmlIgnore, JsonIgnore]
        public double InnerRadius { get; set; }

        [XmlIgnore, JsonIgnore]
        public double Angle { get; set; }

        #endregion

        #region override

        /// <summary>
        /// Ramp length
        /// </summary>
        /// <remarks>Turntable can not be used in ramps</remarks>
        [XmlIgnore, JsonIgnore]
        public override double RampLength { get { return 0; } }

        [XmlIgnore, JsonIgnore]
        public override string Name
        {
            get
            {
                return $"{Resources.TrackTurntable}";
            }
        }

        [XmlIgnore, JsonIgnore]
        public override string Description
        {
            get
            {
                return $"{this.Article} {Resources.TrackTurntable}";
            }
        }

        public override void Update(TrackType trackType)
        {
            this.OuterRadius = GetValue(trackType.Radii, this.OuterRadiusName);
            this.InnerRadius = GetValue(trackType.Radii, this.InnerRadiusName);
            this.Angle = GetValue(trackType.Angles, this.AngleName);
            base.Update(trackType);
        }


        protected override Geometry CreateGeometry()
        {
            return new EllipseGeometry(new Point(0, 0), this.OuterRadius, this.OuterRadius);
        }

        protected override Drawing CreateRailDrawing()
        {
            DrawingGroup drawingRail = new DrawingGroup();
            // background
            drawingRail.Children.Add(new GeometryDrawing(new SolidColorBrush(Colors.DarkGray), linePen, new EllipseGeometry(new Point(0, 0), this.OuterRadius, this.OuterRadius)));
            drawingRail.Children.Add(new GeometryDrawing(new SolidColorBrush(Colors.Gray), linePen, new EllipseGeometry(new Point(0, 0), this.InnerRadius, this.InnerRadius)));
            if (this.HasBallast)
            {
                for (int i = 0; i < this.RailNum; i++)
                {
                    drawingRail.Children.Add(new GeometryDrawing(this.ballastBrush, null,
                        new PathGeometry(new PathFigureCollection
                        {
                        new PathFigure(new Point(-this.OuterRadius, -this.RailWidth).Rotate(this.Angle * i), new PathSegmentCollection
                        {
                            new LineSegment(new Point(-this.InnerRadius, -this.RailWidth).Rotate(this.Angle * i), true),
                            new LineSegment(new Point(-this.InnerRadius,  this.RailWidth).Rotate(this.Angle * i), true),
                            new LineSegment(new Point(-this.OuterRadius,  this.RailWidth).Rotate(this.Angle * i), true),
                            new LineSegment(new Point(-this.OuterRadius, -this.RailWidth).Rotate(this.Angle * i), true)
                        }, true)
                        })));
                }
                drawingRail.Children.Add(new GeometryDrawing(this.ballastBrush, null, new RectangleGeometry(new Rect(-this.InnerRadius, -this.RailWidth, this.InnerRadius * 2, this.RailWidth * 2))));
            }
            
            drawingRail.Children.Add(StraitRail(this.InnerRadius * 2));

            for (int i = 0; i < RailNum; i++)
            {

                drawingRail.Children.Add(new GeometryDrawing(null, this.railPen, new LineGeometry(new Point(-this.OuterRadius, -this.RailWidth / 2).Rotate(this.Angle * i), new Point(-this.InnerRadius, -this.RailWidth / 2).Rotate(this.Angle * i)))); ;
                drawingRail.Children.Add(new GeometryDrawing(null, this.railPen, new LineGeometry(new Point(-this.OuterRadius, +this.RailWidth / 2).Rotate(this.Angle * i), new Point(-this.InnerRadius, +this.RailWidth / 2).Rotate(this.Angle * i))));

                double length1 = this.OuterRadius - this.InnerRadius;
                int num1 = (int)Math.Round(length1 / (this.RailWidth / 2));
                double sleepersDistance1 = length1 / num1;

                for (int j = 0; j < num1; j++)
                {
                    drawingRail.Children.Add(new GeometryDrawing(null, this.sleeperPen, new LineGeometry(
                        new Point(-this.OuterRadius + sleepersDistance1 / 2 + sleepersDistance1 * j, -this.sleeperWidth / 2).Rotate(this.Angle * i),
                        new Point(-this.OuterRadius + sleepersDistance1 / 2 + sleepersDistance1 * j, +this.sleeperWidth / 2).Rotate(this.Angle * i))));
                }
            }
            return drawingRail;
        }

        protected override List<TrackDockPoint> CreateDockPoints()
        {
            var dockPoints = new List<TrackDockPoint>();
            for (int i = 0; i < this.RailNum; i++)
            {
                Point point = new Point(0, this.OuterRadius).Rotate(this.Angle * i);
                dockPoints.Add(new TrackDockPoint(i, point, this.Angle * i + 45, this.dockType));
            }
            return dockPoints;
        }

        #endregion
    }
}

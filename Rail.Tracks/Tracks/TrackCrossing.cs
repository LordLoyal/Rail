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
    public class TrackCrossing : TrackBaseSingle
    {
        #region store 

        [XmlElement("Number")]
        public int Number { get; set; }

        [XmlElement("Length")]
        public Guid LengthId { get; set; }

        [XmlElement("LengthB")]
        public Guid LengthBId { get; set; }

        [XmlElement("CrossingAngle")]
        public Guid CrossingAngleId { get; set; }

        #endregion

        #region internal

        [XmlIgnore, JsonIgnore]
        public double Length { get; set; }

        [XmlIgnore, JsonIgnore]
        public double LengthB { get; set; }

        [XmlIgnore, JsonIgnore]
        public double CrossingAngle { get; set; }

        #endregion

        #region override

        [XmlIgnore, JsonIgnore]
        public override double RampLength { get { return Math.Max(this.Length, this.LengthB); } }
        
        public override void Update(TrackType trackType)
        {
            this.Length = GetValue(trackType.Lengths, this.LengthId);
            this.LengthB = GetValue(trackType.Lengths, this.LengthBId);
            this.CrossingAngle = GetValue(trackType.Angles, this.CrossingAngleId);

            this.Name = $"{Resources.TrackCrossing}";
            this.Description = $"{this.Article} {Resources.TrackCrossing}";
            
            base.Update(trackType);
        }

        protected override Geometry CreateGeometry()
        {
            return this.Number switch
            {
                2 =>
                    new CombinedGeometry(
                        StraitGeometry(this.Length, StraitOrientation.Center, -this.CrossingAngle / 2),
                        StraitGeometry(this.LengthB, StraitOrientation.Center, +this.CrossingAngle / 2)),

                3 =>
                    new CombinedGeometry(
                        StraitGeometry(this.Length, StraitOrientation.Center, 0),
                        new CombinedGeometry(
                            StraitGeometry(this.Length, StraitOrientation.Center, 60),
                            StraitGeometry(this.Length, StraitOrientation.Center, 120))),

                4 =>
                    new CombinedGeometry(
                        new CombinedGeometry(
                            StraitGeometry(this.Length, StraitOrientation.Center, 0),
                            StraitGeometry(this.Length, StraitOrientation.Center, 45)),
                        new CombinedGeometry(
                            StraitGeometry(this.Length, StraitOrientation.Center, 90),
                            StraitGeometry(this.Length, StraitOrientation.Center, 135))),

                _ => null
            };
        }

        protected override Drawing CreateRailDrawing()
        {
            DrawingGroup drawingRail = new DrawingGroup();
            
            switch (this.Number)
            {
            case 2:
                if (this.HasBallast)
                {
                    drawingRail.Children.Add(StraitBallast(this.Length, StraitOrientation.Center, -this.CrossingAngle / 2));
                    drawingRail.Children.Add(StraitBallast(this.LengthB, StraitOrientation.Center, +this.CrossingAngle / 2));
                }
                drawingRail.Children.Add(StraitSleepers(this.Length, StraitOrientation.Center, -this.CrossingAngle / 2));
                drawingRail.Children.Add(StraitSleepers(this.LengthB, StraitOrientation.Center, +this.CrossingAngle / 2));
                drawingRail.Children.Add(StraitRail(this.Length, StraitOrientation.Center, -this.CrossingAngle / 2));
                drawingRail.Children.Add(StraitRail(this.LengthB, StraitOrientation.Center, +this.CrossingAngle / 2));
                break;
            case 3:
                if (this.HasBallast)
                {
                    drawingRail.Children.Add(StraitBallast(this.Length, StraitOrientation.Center, 0));
                    drawingRail.Children.Add(StraitBallast(this.Length, StraitOrientation.Center, 60));
                    drawingRail.Children.Add(StraitBallast(this.Length, StraitOrientation.Center, 120));
                }
                drawingRail.Children.Add(StraitSleepers(this.Length, StraitOrientation.Center, 0));
                drawingRail.Children.Add(StraitSleepers(this.Length, StraitOrientation.Center, 60));
                drawingRail.Children.Add(StraitSleepers(this.Length, StraitOrientation.Center, 120));
                drawingRail.Children.Add(StraitRail(this.Length, StraitOrientation.Center, 0));
                drawingRail.Children.Add(StraitRail(this.Length, StraitOrientation.Center, 60));
                drawingRail.Children.Add(StraitRail(this.Length, StraitOrientation.Center, 120));
                break;
            case 4:
                if (this.HasBallast)
                {
                    drawingRail.Children.Add(StraitBallast(this.Length, StraitOrientation.Center, 0));
                    drawingRail.Children.Add(StraitBallast(this.Length, StraitOrientation.Center, 45));
                    drawingRail.Children.Add(StraitBallast(this.Length, StraitOrientation.Center, 90));
                    drawingRail.Children.Add(StraitBallast(this.Length, StraitOrientation.Center, 135));
                }
                drawingRail.Children.Add(StraitSleepers(this.Length, StraitOrientation.Center, 0));
                drawingRail.Children.Add(StraitSleepers(this.Length, StraitOrientation.Center, 45));
                drawingRail.Children.Add(StraitSleepers(this.Length, StraitOrientation.Center, 90));
                drawingRail.Children.Add(StraitSleepers(this.Length, StraitOrientation.Center, 135));
                drawingRail.Children.Add(StraitRail(this.Length, StraitOrientation.Center, 0));
                drawingRail.Children.Add(StraitRail(this.Length, StraitOrientation.Center, 45));
                drawingRail.Children.Add(StraitRail(this.Length, StraitOrientation.Center, 90));
                drawingRail.Children.Add(StraitRail(this.Length, StraitOrientation.Center, 135));
                break;
            }
            return drawingRail;
        }

        protected override List<TrackDockPoint> CreateDockPoints()
        {
            return this.Number switch
            {
                2 =>
                    new List<TrackDockPoint>
                    {
                        new TrackDockPoint(0, new Point(-this.Length / 2.0, 0.0).Rotate( this.CrossingAngle /2),  this.CrossingAngle /2 + 135, this.dockType),
                        new TrackDockPoint(1, new Point(-this.LengthB / 2.0, 0.0).Rotate(-this.CrossingAngle /2), -this.CrossingAngle /2 + 135, this.dockType),
                        new TrackDockPoint(2, new Point( this.Length / 2.0, 0.0).Rotate( this.CrossingAngle /2),  this.CrossingAngle /2 + 45-90, this.dockType),
                        new TrackDockPoint(3, new Point( this.LengthB / 2.0, 0.0).Rotate(-this.CrossingAngle /2), -this.CrossingAngle /2 + 45-90, this.dockType),
                    },
                3 =>
                    new List<TrackDockPoint>
                    {
                        new TrackDockPoint(0, new Point(this.Length / 2.0, 0.0).Rotate(0), 0 + 135, this.dockType),
                        new TrackDockPoint(1, new Point(this.Length / 2.0, 0.0).Rotate(60), 60 + 135, this.dockType),
                        new TrackDockPoint(2, new Point(this.Length / 2.0, 0.0).Rotate(120), 120 + 135, this.dockType),
                        new TrackDockPoint(3, new Point(this.Length / 2.0, 0.0).Rotate(180), 180 + 135, this.dockType),
                        new TrackDockPoint(2, new Point(this.Length / 2.0, 0.0).Rotate(240), 240 + 135, this.dockType),
                        new TrackDockPoint(3, new Point(this.Length / 2.0, 0.0).Rotate(300), 300 + 135, this.dockType),
                    },
                4 =>
                    new List<TrackDockPoint>
                    {
                        new TrackDockPoint(0, new Point(this.Length / 2.0, 0.0).Rotate(0), 0 + 135, this.dockType),
                        new TrackDockPoint(1, new Point(this.Length / 2.0, 0.0).Rotate(45), 45 + 135, this.dockType),
                        new TrackDockPoint(2, new Point(this.Length / 2.0, 0.0).Rotate(90), 90 + 135, this.dockType),
                        new TrackDockPoint(3, new Point(this.Length / 2.0, 0.0).Rotate(135), 135 + 135, this.dockType),
                        new TrackDockPoint(0, new Point(this.Length / 2.0, 0.0).Rotate(180), 180 + 135, this.dockType),
                        new TrackDockPoint(1, new Point(this.Length / 2.0, 0.0).Rotate(225), 225 + 135, this.dockType),
                        new TrackDockPoint(2, new Point(this.Length / 2.0, 0.0).Rotate(270), 270 + 135, this.dockType),
                        new TrackDockPoint(3, new Point(this.Length / 2.0, 0.0).Rotate(315), 315 + 135, this.dockType),
                    },
                _ => null
            };
        }

        #endregion
    }
}

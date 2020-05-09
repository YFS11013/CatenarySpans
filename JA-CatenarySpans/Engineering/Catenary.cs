﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using JA.Gdi;
using System.Drawing;
using System.Xml.Serialization;

namespace JA.Engineering
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Catenary : Span, ICloneable, IFormattable
    {
        public static readonly string DefaultForceFormat="0.###";
        public static readonly double DefaultHorizontalTension=1000;
        public static readonly double DefaultWeight=1;
        
        public event EventArgs<Catenary>.Handler CatenaryChanged;

        double weight, horizontalTension;

        #region Factory
        public Catenary()
            : base()
        {
            this.weight=DefaultWeight;
            this.horizontalTension=DefaultHorizontalTension;
            this.Center=CatenaryCalculator.CenterPosition(Step, weight, horizontalTension);
            this.SpanChanged+=new EventArgs<Span>.Handler(Catenary_SpanChanged);
            this.CatenaryChanged+=new EventArgs<Catenary>.Handler(Catenary_CatenaryChanged);
        }

        public Catenary(Vector2 origin, double dx, double dy, double weight)
            : this(origin, new Vector2(dx, dy), weight)
        { }
        public Catenary(Vector2 origin, Vector2 span, double weight)
            : base(origin, span)
        {
            this.weight=weight;
            this.horizontalTension=DefaultHorizontalTension;
            this.Center=CatenaryCalculator.CenterPosition(Step, this.weight, horizontalTension);
            this.SpanChanged+=new EventArgs<Span>.Handler(Catenary_SpanChanged);
            this.CatenaryChanged+=new EventArgs<Catenary>.Handler(Catenary_CatenaryChanged);
        }
        public Catenary(ISpan span) : this(span, DefaultWeight) { }
        public Catenary(ISpan span, double weight) : this(span, weight, DefaultHorizontalTension) { }
        public Catenary(ISpan span, double weight, double H)
            : base(span)
        {
            this.weight=weight;
            this.horizontalTension=H;
            this.Center=CatenaryCalculator.CenterPosition(Step, this.weight, H);
            this.SpanChanged+=new EventArgs<Span>.Handler(Catenary_SpanChanged);
            this.CatenaryChanged+=new EventArgs<Catenary>.Handler(Catenary_CatenaryChanged);
        }
        public Catenary(Catenary other)
            : base(other)
        {
            this.Center=other.Center;
            this.weight=other.weight;
            this.horizontalTension=other.horizontalTension;
            this.SpanChanged+=new EventArgs<Span>.Handler(Catenary_SpanChanged);
            this.CatenaryChanged+=new EventArgs<Catenary>.Handler(Catenary_CatenaryChanged);
        }
        #endregion

        #region Properties
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.Yes)]
        public override bool IsOK
        {
            get { return base.IsOK&&horizontalTension.IsPositive(); }
        }
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No)]
        public Vector2 LowestPosition { get { return StartPosition+Center; } }
        [RefreshProperties(RefreshProperties.All), XmlAttribute()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double Weight
        {
            get { return weight; }
            set
            {
                if (value.IsNotFinite()||value.IsNegativeOrZero())
                {
                    throw new ArgumentException("Weight must be finite and positive");
                }
                if (!weight.Equals(value))
                {
                    this.weight=value;
                    if (RaisesChangedEvents)
                    {
                        OnCatenaryChanged(new EventArgs<Catenary>(this));
                        OnPropertyChanged(() => Weight);
                    }
                }
            }
        }

        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No)]
        protected Vector2 Center { get; private set; }

        [ReadOnly(true), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double CenterX { get { return StartPosition.X+Center.X; } }
        [ReadOnly(true), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double CenterY
        {
            get { return StartPosition.Y+Center.Y; }
        }
        [RefreshProperties(RefreshProperties.All), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double Clearance
        {
            get { return IsCenterInSpan?StartPosition.Y+Center.Y:Math.Min(StartPosition.Y, EndPosition.Y); }
            set
            {
                if (value.IsNotFinite()||IsUpliftCondition)
                {
                    throw new ArgumentException("Clerance must be finite and lowest point must be in span.");
                }
                HorizontalTension=CatenaryCalculator.SetClearance(Step, weight, StartPosition.Y-value, 1e-3);
            }
        }

        [RefreshProperties(RefreshProperties.All), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double MaximumSag
        {
            get
            {
                return CatenaryCalculator.MaximumSag(Step, Center, weight, horizontalTension);
            }
            set
            {
                if (value.IsNotFinite()||value.IsNegativeOrZero())
                {
                    throw new ArgumentException("Sag must be finite and positive.");
                }
                HorizontalTension=CatenaryCalculator.SetMaximumSag(Step, weight, value, 1e-3);
            }
        }
        [ReadOnly(true), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double MidSag
        {
            get
            {
                return CatenaryCalculator.MidSag(Step, Center, weight, horizontalTension);
            }
        }
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No)]
        public Vector2 SagPosition
        {
            get
            {
                double x=CatenaryCalculator.MaximumSagX(Step, Center, weight, horizontalTension);
                return StartPosition+CatenaryCalculator.PositionAtX(Step, Center, weight, horizontalTension, x);
            }
        }
        [RefreshProperties(RefreshProperties.All), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double CatenaryConstant
        {
            get { return horizontalTension/weight; }
            set
            {
                if (value.IsNotFinite()||value.IsNegativeOrZero())
                {
                    throw new ArgumentException("Catenary constant must be finite and positive.");
                }
                HorizontalTension=weight*value;
            }
        }
        [RefreshProperties(RefreshProperties.All), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double Eta
        {
            get { return weight*SpanX/(2*horizontalTension); }
            set
            {
                if (value.IsNotFinite()||value.IsNegativeOrZero())
                {
                    throw new ArgumentException("Eta must be finite and positive.");
                }
                HorizontalTension=weight*Step.X/(2*value);
            }
        }
        [RefreshProperties(RefreshProperties.All), XmlAttribute()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double HorizontalTension
        {
            get { return horizontalTension; }
            set
            {
                if (value.IsNotFinite()||value.IsNegativeOrZero())
                {
                    throw new ArgumentException("Horizontal Tension must be finite and positive.");
                }
                if (!horizontalTension.Equals(value))
                {
                    this.horizontalTension=value;
                    if (RaisesChangedEvents)
                    {
                        OnCatenaryChanged(new EventArgs<Catenary>(this));
                        OnPropertyChanged(() => HorizontalTension);
                    }
                }
            }
        }
        [RefreshProperties(RefreshProperties.All), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double TotalLength
        {
            get
            {
                return CatenaryCalculator.TotalLength(Step, Center, weight, horizontalTension);
            }
            set
            {
                if (value.IsNotFinite()||value<=Step.Manitude)
                {
                    throw new ArgumentException("Length must be finite and larger than the span diagonal.");
                }
                HorizontalTension=CatenaryCalculator.SetTotalLength(Step, weight, value, 1e-3);
            }
        }

        [RefreshProperties(RefreshProperties.All), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double GeometricStrainPct
        {
            get { return 100*(TotalLength/Step.Manitude-1); }
            set
            {
                if (value.IsNotFinite()||value.IsNegativeOrZero())
                {
                    throw new ArgumentException("Geometric strain must be finite and positive.");
                }
                TotalLength=SpanX*(1+value/100);
            }
        }
        [RefreshProperties(RefreshProperties.All), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double AverageTension
        {
            get
            {
                return CatenaryCalculator.AverageTension(Step, Center, weight, horizontalTension);
            }
            set
            {
                if (value.IsNotFinite()||value.IsNegativeOrZero())
                {
                    throw new ArgumentException("Average Tension must be finite and positive.");
                }
                HorizontalTension=CatenaryCalculator.SetAverageTension(Step, weight, value, 1e-3);
            }
        }
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No), Browsable(false)]
        public Vector2 StartTension
        {
            get
            {
                return new Vector2(-horizontalTension, -CatenaryCalculator.VertricalTensionAtX(Step, Center, weight, horizontalTension, 0));
            }
        }
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No), Browsable(false)]
        public Vector2 EndTension
        {
            get
            {
                return new Vector2(horizontalTension, CatenaryCalculator.VertricalTensionAtX(Step, Center, weight, horizontalTension, SpanX));
            }
        }
        /// <summary>
        /// Return the tension from the highest tower
        /// </summary>
        [ReadOnly(true), XmlIgnore()]
        public double MaxTension
        {
            get
            {
                return Center.X<=SpanX/2?
                    CatenaryCalculator.TotalTensionAtX(Step, Center, weight, horizontalTension, 0):
                    CatenaryCalculator.TotalTensionAtX(Step, Center, weight, horizontalTension, SpanX);
            }
        }
        [ReadOnly(true), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double StartVerticalTension
        {
            get { return -CatenaryCalculator.VertricalTensionAtX(Step, Center, weight, horizontalTension, 0); }
        }
        [ReadOnly(true), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double EndVerticalTension
        {
            get { return CatenaryCalculator.VertricalTensionAtX(Step, Center, weight, horizontalTension, SpanX); }
        }
        [ReadOnly(true), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double StartTotalTension
        {
            get { return CatenaryCalculator.TotalTensionAtX(Step, Center, weight, horizontalTension, 0); }
        }
        [ReadOnly(true), XmlIgnore()]
        [TypeConverter(typeof(NiceTypeConverter))]
        public double EndTotalTension
        {
            get { return CatenaryCalculator.TotalTensionAtX(Step, Center, weight, horizontalTension, SpanX); }
        }

        [ReadOnly(true), XmlIgnore()]
        public bool IsCenterInSpan
        {
            get
            {
                double L=CatenaryCalculator.LengthSegmentAtX(Step, Center, weight, horizontalTension, Center.X);
                return L>=0&&L<=CatenaryCalculator.TotalLength(Step, Center, weight, horizontalTension);
            }
        }
        /// <summary>
        /// Checks for uplift condition (vertical tension on end is upwards)
        /// </summary>
        [ReadOnly(true), XmlIgnore()]
        public bool IsUpliftCondition { get { return !IsCenterInSpan; } }
        [ReadOnly(true), XmlIgnore()]
        public bool IsStartTowerUplift { get { return StartVerticalTension<0; } }
        [ReadOnly(true), XmlIgnore()]
        public bool IsEndTowerUplift { get { return EndVerticalTension<0; } }
        #endregion

        #region Event Handlers
        protected void Catenary_SpanChanged(object sender, EventArgs<Span> e)
        {
            OnCatenaryChanged(new EventArgs<Catenary>(e.Item as Catenary));
        }
        protected void Catenary_CatenaryChanged(object sender, EventArgs<Catenary> e)
        {
            CalculateCenter();
        }

        /// <summary>
        /// Helper function that calculates the catenary lowest point and triggers property changed notifiers
        /// </summary>
        public void CalculateCenter()
        {
            this.Center=CatenaryCalculator.CenterPosition(Step, weight, horizontalTension);
            OnPropertyChanged(() => CenterX);
            OnPropertyChanged(() => CenterY);
        }

        #endregion

        #region Event Triggers
        protected void OnCatenaryChanged(EventArgs<Catenary> e)
        {
            this.CatenaryChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public override void ScaleForUnits(ProjectUnits.ChangeEventArgs g, bool raiseEvents = false)
        {
            base.ScaleForUnits(g, raiseEvents);

            var raise = RaisesChangedEvents;
            this.RaisesChangedEvents = raiseEvents;
            this.HorizontalTension *= g.ForceFactor;
            this.Weight *= g.ForceFactor/g.LengthFactor;
            this.Center *= g.LengthFactor;
            this.RaisesChangedEvents = raise;
        }

        /// <summary>
        /// Calculates the corner coordinates of a bounding box for the catenary curve.
        /// </summary>
        /// <remarks>It splits the curve into 16 segments and finds the bounds based on they node coordinates</remarks>
        /// <param name="minPosition">The lowest x,y values of the bounding box</param>
        /// <param name="maxPosition">The highest x,y values of the bounding box</param>
        public override void GetBounds(ref Vector2 minPosition, ref Vector2 maxPosition)
        {
            base.GetBounds(ref minPosition, ref maxPosition);
            Func<double, Vector2> f=ParametricCurve;
            int N=16;
            for (int i=0; i<=N; i++)
            {
                double t=(double)(i)/N;
                Vector2 pos=f(t);
                double minx = minPosition.X, miny = minPosition.Y;
                double maxx = maxPosition.X, maxy = maxPosition.Y;

                minx=Math.Min(minx, pos.X);
                miny=Math.Min(miny, pos.Y);
                maxx=Math.Max(maxx, pos.X);
                maxy=Math.Max(maxy, pos.Y);

                minPosition = new Vector2(minx, miny);
                maxPosition = new Vector2(maxx, maxy);

            }
        }

        public void SetClearancePoint(Vector2 point)
        {
            double x=point.X-StartPosition.X;
            double D=StartPosition.Y+SpanY/SpanX*x-point.Y;
            HorizontalTension=CatenaryCalculator.SetSagAtX(Step, weight, D, x, 1e-3);
        }

        #endregion

        #region Functions
        [XmlIgnore(), Browsable(false), Bindable(BindableSupport.No)]
        public Func<double, Vector2> ParametricCurve
        {
            get
            {
                return (t) => StartPosition+CatenaryCalculator.PositionAtT(Step, Center, weight, horizontalTension, t);
            }
        }
        [XmlIgnore(), Browsable(false), Bindable(BindableSupport.No)]
        public Func<double, Vector2> ParametricTension
        {
            get
            {
                return (t) => new Vector2(horizontalTension, CatenaryCalculator.VertricalTensionAtX(Step, Center, weight, horizontalTension, CatenaryCalculator.ParameterToX(Step, Center, weight, horizontalTension, t)));
            }
        }
        [XmlIgnore(), Browsable(false), Bindable(BindableSupport.No)]
        public Func<double, double> CatenaryFunction
        {
            get
            {
                return (x) => StartPosition.Y+CatenaryCalculator.PositionAtX(Step, Center, weight, horizontalTension, x-StartPosition.X).Y;
            }
        }

        #endregion

        #region ICloneable Members

        public new Catenary Clone() { return new Catenary(this); }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region Formatting
        public override string ToString()
        {
            return ToString(DefaultForceFormat);
        }
        public new string ToString(string format)
        {
            return ToString(format, null);
        }
        public new string ToString(string format, IFormatProvider provider)
        {
            return string.Format(provider, "{0}, H={1:"+format+"}, w={2:"+format+"}",
                base.ToString(), horizontalTension, weight);
        } 
        #endregion
    }

}

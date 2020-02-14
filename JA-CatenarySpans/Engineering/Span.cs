﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Diagnostics;

namespace JA.Engineering
{

    public interface ISpan
    {
        Vector2 StartPosition { get; set; }
        Vector2 EndPosition { get; }
        Vector2 Step { get; set; }
        bool IsOK { get; }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Span : ISpan, INotifyPropertyChanged, ICloneable, IFormattable
    {
        public static string DefaultLengthFormat="0.###";
        public static double DefaultSpanLength=500;
        public static double DefaultTowerHeight=100;
        public static double DefaultSpanRise=50;

        Vector2 start;
        Vector2 step;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventArgs<Span>.Handler SpanChanged;

        [XmlIgnore(), Bindable(BindableSupport.No)]
        public bool RaisesChangedEvents { get; set; }

        #region Factory
        public Span()
            : this(DefaultTowerHeight*Vector2.UnitY, DefaultSpanLength, DefaultSpanRise)
        { }
        public Span(Vector2 origin, double dx, double dy)
            : this(origin, new Vector2(dx, dy))
        { }
        public Span(Vector2 origin, Vector2 span)
        {
            this.start=origin;
            this.step=span;
            this.RaisesChangedEvents=true;
        }
        public Span(ISpan other)
        {
            this.start=other.StartPosition;
            this.step=other.Step;
            this.RaisesChangedEvents=true;
        }
        #endregion

        #region Properties

        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.Yes)]
        public virtual bool IsOK
        {
            get { return step.x.IsFinite()&&step.x.IsPositive(); }
        }
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No), Browsable(false)]
        public Vector2 StartPosition { get { return start; } set { start=value; } }
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No), Browsable(false)]
        public Vector2 EndPosition { get { return start+step; } }
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No), Browsable(false)]
        public Vector2 StartBase { get { return new Vector2(start.x, 0); } }
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No), Browsable(false)]
        public Vector2 EndBase { get { return new Vector2(start.x+step.x, 0); } }
        [ReadOnly(true), XmlIgnore(), Bindable(BindableSupport.No), Browsable(false)]
        public Vector2 Step
        {
            get { return step; }
            set
            {
                if (!step.Equals(value))
                {
                    this.step=value;
                    if (RaisesChangedEvents)
                    {
                        OnSpanChanged(new EventArgs<Span>(this));
                        OnPropertyChanged(() => Step);
                    }
                }
            }
        }
        [RefreshProperties(RefreshProperties.None), XmlAttribute()]
        public double StartX { get { return start.x; } set { start.x=value; } }
        [RefreshProperties(RefreshProperties.None), XmlAttribute()]
        public double StartY { get { return start.y; } set { start.y=value; } }
        [RefreshProperties(RefreshProperties.All), XmlAttribute()]
        public double SpanX
        {
            get { return step.x; }
            set
            {
                if (value.IsNotFinite()||value.IsNegativeOrZero())
                {
                    throw new ArgumentException("Span must be finite and positive.");
                }
                if (!step.y.Equals(value))
                {
                    step.x=value;
                    if (RaisesChangedEvents)
                    {
                        OnSpanChanged(new EventArgs<Span>(this));
                        OnPropertyChanged(() => SpanX);
                    }
                }
            }
        }
        [RefreshProperties(RefreshProperties.All), XmlAttribute()]
        public double SpanY
        {
            get { return step.y; }
            set
            {
                if (value.IsNotFinite())
                {
                    throw new ArgumentException("Span height must be finite.");
                }
                if (!step.y.Equals(value))
                {
                    step.y=value;
                    if (RaisesChangedEvents)
                    {
                        OnSpanChanged(new EventArgs<Span>(this));
                        OnPropertyChanged(() => SpanY);
                    }
                }
            }
        }
        [ReadOnly(true), RefreshProperties(RefreshProperties.None), XmlIgnore()]
        public double SpanLength
        {
            get
            {
                return step.Manitude;
            }
        }
        [XmlIgnore(), Browsable(false), Bindable(BindableSupport.No)]
        public Func<double, double> DiagonalFunction
        {
            get
            {
                return (x) => start.y+x*step.y/step.x;
            }
        }
        [XmlIgnore(), Browsable(false), Bindable(BindableSupport.No)]
        public Func<double, Vector2> ParametricDiagonal
        {
            get
            {
                return (t) => new Vector2(start.x+step.x*t, start.y+step.y*t);
            }
        }
        #endregion

        #region Event Triggers
        protected void OnSpanChanged(EventArgs<Span> e)
        {
            this.SpanChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the corner coordinates of a bounding box for the supported span.
        /// </summary>
        /// <param name="minPosition">The lowest x,y values of the bounding box</param>
        /// <param name="maxPosition">The highest x,y values of the bounding box</param>
        public virtual void GetBounds(ref Vector2 minPosition, ref Vector2 maxPosition)
        {
            if (minPosition.IsZero&&maxPosition.IsZero)
            {
                minPosition=StartBase;
                maxPosition=EndBase;
            }
            minPosition.y=Math.Min(minPosition.y, start.y);
            minPosition.y=Math.Min(minPosition.y, start.y+step.y);
            maxPosition.y=Math.Max(maxPosition.y, start.y);
            maxPosition.y=Math.Max(maxPosition.y, start.y+step.y);
            minPosition.x=Math.Min(minPosition.x, start.x);
            minPosition.x=Math.Min(minPosition.x, start.x+step.x);
            maxPosition.x=Math.Max(maxPosition.x, start.x);
            maxPosition.x=Math.Max(maxPosition.x, start.x+step.x);
        }
        public bool ContainsX(double x)
        {
            return start.x<=x&&step.x>=(x-start.x);
        }
        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }
        public Span Clone() { return new Span(this); }
        #endregion

        #region Formatting
        public override string ToString()
        {
            return ToString(DefaultLengthFormat);
        }
        public string ToString(string format)
        {
            return ToString(format, null);
        }
        public string ToString(string format, IFormatProvider provider)
        {
            return string.Format("Start={0}, Step={1}",
                start.ToString(format, provider),
                step.ToString(format, provider));
        }
        #endregion

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged<T>(System.Linq.Expressions.Expression<Func<T>> property)
        {
            var mex=property.Body as System.Linq.Expressions.MemberExpression;
            OnPropertyChanged(mex.Member.Name);
        }
        protected virtual void OnPropertyChanged(string property)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(property));
        }
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        #endregion

    }

    #region Span List
    public class SpanListBase<T> : ItemList<T> where T : Span, new()
    {

        public SpanListBase()
        {
            this.ItemChanged+=new EventHandler<ItemChangeEventArgs>(SpanList_ItemChanged);
            this.ProjectUnitsChanged+=new EventHandler<ProjectUnits.ChangeEventArgs>(SpanListBase_ProjectUnitsChanged);
        }

        public SpanListBase(ProjectUnits units, params T[] items)
            : base(units, items)
        {
            this.ItemChanged+=new EventHandler<ItemChangeEventArgs>(SpanList_ItemChanged);
            this.ProjectUnitsChanged+=new EventHandler<ProjectUnits.ChangeEventArgs>(SpanListBase_ProjectUnitsChanged);
        }

        void SpanList_ItemChanged(object sender, ItemChangeEventArgs e)
        {
            UpdateSpanEnds();
        }

        void SpanListBase_ProjectUnitsChanged(object sender, ProjectUnits.ChangeEventArgs e)
        {
            for (int i=0; i<Items.Count; i++)
            {
                this[i].RaisesChangedEvents=false;
                this[i].StartPosition*=e.LengthFactor;
                this[i].Step*=e.LengthFactor;
                //this[i].HorizontalTension*=e.ForceFactor;
                //this[i].Weight*=e.ForceFactor/e.LengthFactor;
                this[i].RaisesChangedEvents=true;
            }
            OnItemChanged(new ItemChangeEventArgs());
        }

        public void UpdateSpanEnds()
        {
            for (int i=1; i<Items.Count; i++)
            {
                this[i].StartPosition=this[i-1].EndPosition;
            }
        }
        public int FindSpanIndexFromX(double x)
        {
            T[] list=ItemArray;
            for (int i=0; i<list.Length; i++)
            {
                if (list[i].ContainsX(x))
                {
                    return i;
                }
            }
            return -1;
        }

        public override T NewItem()
        {
            var last=Last;
            if (last==null)
            {
                return new T();
            }
            else
            {
                return new T()
                {
                    StartPosition=last.EndPosition,
                    SpanX=last.SpanX,
                };
            }
        }
    }
    #endregion

}
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.ComponentModel;

using static System.Math;

namespace JA
{
    /// <summary>
    /// Extension methods for double type
    /// </summary>
    public static class DoubleEx
    {
        public const double tol=1e-12;

        #region Formatting
        static char[] prefixes= { 'f', 'a', 'p', 'n', 'μ', 'm', ' ', 'k', 'M', 'G', 'T', 'P', 'E' };
        public static string Nice(this float x, int significant_digits)
        {
            return Nice((double)x, significant_digits);
        }
        /// <summary>
        /// Format a number with standard prefixes and set significant digits.
        /// For example <![CDATA[3.14159265358979E-06 = 3.142µ]]>
        /// </summary>
        /// <param name="x">The value to format</param>
        /// <param name="significant_digits">The number of significant digits to show</param>
        /// <returns>The formatted string</returns>
        public static string Nice(this double x, int significant_digits)
        {
            //Check for special numbers and non-numbers
            if (double.IsInfinity(x)||double.IsNaN(x)||x==0)
            {
                return x.ToString();
            }
            // extract sign so we deal with positive numbers only
            string pfx=x<0?"-":string.Empty;
            x=Math.Abs(x);
            // get scientific exponent, 10^3, 10^6, ...
            int sci=(int)Math.Floor(Log(x, 10)/3)*3;
            // scale number to exponent found
            x=x*Pow(10, -sci);
            // find number of digits to the left of the decimal
            int dg=(int)Math.Floor(Log(x, 10))+1;
            // adjust decimals to display
            int decimals=(significant_digits-dg).ClampMinMax(0, 15);
            // format for the decimals
            string fmt;
            if (decimals>0)
            {
                fmt="0."+new string('0', decimals);
            }
            else
            {
                fmt="0";
            }
            x=Math.Round(x, decimals);
            if (sci==0)
            {
                //no exponent
                return string.Format("{0}{1:"+fmt+"}", pfx, x).Trim();
            }
            // find index for prefix. every 3 of sci is a new index
            int index=sci/3+6;
            if (index>=0&&index<prefixes.Length)
            {
                // with prefix
                return string.Format("{0}{1:"+fmt+"}{2}", pfx, x, prefixes[index]).Trim();
            }
            // with 10^exp format
            return string.Format("{0}{1:"+fmt+"}·10^{2}", pfx, x, sci).Trim();
        }

        public static double ParseNice(this string value)
        {
            return ParseNice(value, CultureInfo.CurrentCulture.NumberFormat);
        }

        /// <summary>
        /// Safely parses a formatted value into a double
        /// </summary>
        /// <param name="value">The string to parse</param>
        /// <returns>A number of type double</returns>
        public static double ParseNice(this string value, IFormatProvider provider)
        {
            value=value.Trim();
            if (double.TryParse(value, NumberStyles.Float, provider, out double x))
            {
                return x;
            }
            int i=value.LastIndexOfAny(prefixes);
            if (i>=0)
            {
                string si=value.Substring(i);
                value=value.Substring(0, i);
                int exp=3*(Array.IndexOf(prefixes, si[0])-6);
                if (double.TryParse(value, out x))
                {
                    return x*Pow(10, exp);
                }
            }
            return x;
        }
        /// <summary>
        /// Safely parses a string into a double
        /// </summary>
        /// <param name="value">The string to parse</param>
        /// <returns>A number of type double</returns>
        public static double ParseDouble(this string value)
        {
            double.TryParse(value, out double x);
            return x;
        }
        public static double ParseDouble(this string value, IFormatProvider provider)
        {
            double.TryParse(value, NumberStyles.Float, provider, out double x);
            return x;
        }
        #endregion

        #region Math
        /// <summary>
        /// Absolute value
        /// </summary>
        /// <returns>|value|</returns>
        public static double Abs(this double value)
        {
            return Math.Abs(value);
        }
        /// <summary>
        /// Sign of value
        /// </summary>
        /// <returns>-1 if value&lt;0, +1 if value&gt;0, 0 otherwise</returns>
        public static int Sign(this double value)
        {
            return Math.Sign(value);
        }
        /// <summary>
        /// Two parameter sign function
        /// </summary>
        /// <returns>-|value| if sign&lt;0, +|value| if sign&gt;0, 0 otherwise</returns>
        public static double SignOf(this double value, double sign)
        {
            return Math.Abs(value)*Math.Sign(sign);
        }
        public static double Floor(this double value)
        {
            return Math.Floor(value);
        }
        public static double Ceiling(this double value)
        {
            return Math.Ceiling(value);
        }
        public static double Round(this double value, MidpointRounding mode)
        {
            return Math.Round(value, mode);
        }
        public static double Round(this double value, int digits)
        {
            return Math.Round(value, digits);
        }
        public static double RoundTo(this double value, int significant_digits)
        {
            if (value.IsZero()) return value;

            int sign=Math.Sign(value);
            value=Math.Abs(value);
            int exp=(int)Math.Floor(Log(value, 10))+1; //significant digits to decimal
            double den= Pow(10, exp-significant_digits);
            value=Math.Round(value/den)*den;
            return sign*value;
        }
        public static double FloorTo(this double value, int significant_digits)
        {
            if (value.IsZero()) return value;

            int sign=Math.Sign(value);
            value=Math.Abs(value);
            int exp=(int)Log(value, 10)+1; //significant digits to decimal
            double den= Pow(10, exp-significant_digits);
            value=Math.Floor(value/den)*den;
            return sign*value;
        }
        public static double CeilingTo(this double value, int significant_digits)
        {
            if (value.IsZero()) return value;

            int sign=Math.Sign(value);
            value=Math.Abs(value);
            int exp=(int)Log(value, 10)+1; //significant digits to decimal
            double den= Pow(10, exp-significant_digits);
            value=Math.Ceiling(value/den)*den;
            return sign*value;
        }
        public static double Sqr(this double value)
        {
            return value*value;
        }
        public static double Sqrt(this double value)
        {
            return Math.Sqrt(value);
        }
        public static double Cub(this double value)
        {
            return value*value*value;
        }
        public static double Cubrt(this double value)
        {
            return Pow(value, 1/3.0);
        }
        public static double Step(this double value)
        {
            return value>0?1:(value<0?-1:0);
        }
        /// <summary>
        /// Return 1 only when values in between min and max, 0 otherwise.
        /// </summary>
        /// <param name="value">The value to evaluate</param>
        /// <param name="min_value">The min value</param>
        /// <param name="max_value">The max value</param>
        /// <returns></returns>
        public static double Chi(this double value, double min_value, double max_value)
        {
            double dx=Math.Abs(max_value-min_value);
            min_value=Min(min_value, max_value);
            max_value=min_value+dx;
            return value<min_value?0:(value>max_value?0:1);
        }
        /// <summary>
        /// Return a saw-tooth value between a minimum and a maximum value.
        /// <remarks>Sorts the min/max values from lowest to highest</remarks>
        /// </summary>
        /// <param name="value">The value to wrap</param>
        /// <param name="min_value">The lower limit</param>
        /// <param name="max_value">The upper limit</param>
        /// <returns>A scalar value</returns>
        public static double WrapAround(this double value, double min_value, double max_value)
        {
            double dx=Math.Abs(max_value-min_value);
            min_value=Min(min_value, max_value);
            return value-dx*Math.Floor((value-min_value)/dx);
        }
        /// <summary>
        /// Return a saw-tooth value between zero an a maximum value.
        /// </summary>
        /// <param name="x">The value to use</param>
        /// <param name="x_high">The maximum value allowed</param>
        /// <returns>A scalar value</returns>
        public static double WrapAround(this double value, double max_value)
        {
            return WrapAround(value, 0, max_value);
        }
        /// <summary>
        /// Return x when between min and max value, otherwise clamp at limits
        /// </summary>
        /// <example>    
        ///     ClapMinMax(-0.33, 0.0, 1.0) = 0.00
        ///     ClapMinMax( 0.33, 0.0, 1.0) = 0.33
        ///     ClapMinMax( 1.33, 0.0, 1.0) = 1.00
        /// </example>
        /// <param name="x">The value to clamp</param>
        /// <param name="min_value">The minimum value to use</param>
        /// <param name="max_value">The maximum value to use</param>
        /// <returns>A scalar value</returns>
        public static double ClampMinMax(this double value, double min_value, double max_value)
        {
            return value>max_value?max_value:value<min_value?min_value:value;
        }
        public static int ClampMinMax(this int value, int min_value, int max_value)
        {
            return value>max_value?max_value:value<min_value?min_value:value;
        }
        public static float ClampMinMax(this float value, float min_value, float max_value)
        {
            return value>max_value?max_value:value<min_value?min_value:value;
        }
        public static byte ClampMinMax(this byte value, byte min_value, byte max_value)
        {
            return value>max_value?max_value:value<min_value?min_value:value;
        }
        /// <summary>
        /// Return x when more than min, otherwise return min
        /// </summary>
        /// <param name="x">The value to clamp</param>
        /// <param name="min_value">The minimum value to use</param>
        /// <returns>A scalar value</returns>
        public static double ClampMin(this double value, double min_value)
        {
            return value<min_value?min_value:value;
        }
        public static float ClampMin(this float value, float min_value)
        {
            return value<min_value?min_value:value;
        }
        public static int ClampMin(this int value, int min_value)
        {
            return value<min_value?min_value:value;
        }
        public static byte ClampMin(this byte value, byte min_value)
        {
            return value<min_value?min_value:value;
        }
        /// <summary>
        /// Return x when less than max, otherwise return max
        /// </summary>
        /// <param name="x">The value to clamp</param>
        /// <param name="max_value">The maximum value to use</param>
        /// <returns>A scalar value</returns>
        public static double ClampMax(this double value, double max_value)
        {
            return value>max_value?max_value:value;
        }
        public static float ClampMax(this float value, float max_value)
        {
            return value>max_value?max_value:value;
        }
        public static int ClampMax(this int value, int max_value)
        {
            return value>max_value?max_value:value;
        }
        public static byte ClampMax(this byte value, byte max_value)
        {
            return value>max_value?max_value:value;
        }
        /// <summary>
        /// Return 1 only when the value is 0, 0 otherwise.
        /// </summary>
        /// <param name="value">The value to evaluate</param>
        /// <returns>A scalar value</returns>
        public static double Kronecker(this double value)
        {
            return value<0?0:(value>0?0:1);
        }
        // Radians
        public static double Cos(this double value)
        {
            return Math.Cos(value);
        }
        public static double Sin(this double value)
        {
            return Math.Sin(value);
        }
        public static double Tan(this double value)
        {
            return Math.Tan(value);
        }
        public static double Acos(this double value)
        {
            return Math.Acos(value);
        }
        public static double Asin(this double value)
        {
            return Math.Asin(value);
        }
        public static double Atan(this double value)
        {
            return Math.Atan(value);
        }
        // Degrees
        public static double CosDegrees(this double value)
        {
            return Math.Cos(Math.PI*value/180);
        }
        public static double SinDegrees(this double value)
        {
            return Math.Sin(Math.PI*value/180);
        }
        public static double TanDegrees(this double value)
        {
            return Math.Tan(Math.PI*value/180);
        }
        public static double AcosDegrees(this double value)
        {
            return Math.Acos(value)*180/Math.PI;
        }
        public static double AsinDegrees(this double value)
        {
            return Math.Asin(value)*180/Math.PI;
        }
        public static double AtanDegrees(this double value)
        {
            return Math.Atan(value)*180/Math.PI;
        }
        // Circle
        public static double CosCircle(this double value)
        {
            return Math.Cos(value*2*Math.PI);
        }
        public static double SinCircle(this double value)
        {
            return Math.Sin(value*2*Math.PI);
        }
        public static double TanCircle(this double value)
        {
            return Math.Tan(value*2*Math.PI);
        }
        public static double AcosCircle(this double value)
        {
            return Math.Acos(value)/(2*Math.PI);
        }
        public static double AsinCircle(this double value)
        {
            return Math.Asin(value)/(2*Math.PI);
        }
        public static double AtanCircle(this double value)
        {
            return Math.Atan(value)/(2*Math.PI);
        }
        public static double Asinh(this double value)
        {
            return Log(value+Math.Sqrt(value*value+1));
        }
        /// <summary>
        /// Wraps angle between 0 and 360
        /// </summary>
        /// <param name="angle">The angle</param>
        /// <returns>A bounded angle value</returns>
        public static double WrapTo360(this double angle)
        {
            return angle-360*Math.Floor(angle/360);
        }
        /// <summary>
        /// Wraps angle between 0 and 2π
        /// </summary>
        /// <param name="angle">The angle</param>
        /// <returns>A bounded angle value</returns>
        public static double WrapTo2PI(this double angle)
        {
            return angle-(2*Math.PI)*Math.Floor(angle/(2*Math.PI));
        }
        /// <summary>
        /// Wraps angle between -180 and 180
        /// </summary>
        /// <param name="angle">The angle</param>
        /// <returns>A bounded angle value</returns>
        public static double WrapBetween180(this double angle)
        {
            // see: http://stackoverflow.com/questions/7271527/inconsistency-with-math-round
            return angle+360*Math.Floor((180-angle)/360);
        }
        /// <summary>
        /// Wraps angle between -π and π
        /// </summary>
        /// <param name="angle">The angle</param>
        /// <returns>A bounded angle value</returns>
        public static double WrapBetweenPI(this double angle)
        {
            return angle+(2*Math.PI)*Math.Floor((Math.PI-angle)/(2*Math.PI));
        }

        /// <summary>
        /// Degree to Radian conversion
        /// </summary>
        /// <param name="value">The value</param>
        public static double D2R(this double value) { return Math.PI*value/180; }
        /// <summary>
        /// Radian to Degree conversion
        /// </summary>
        /// <param name="value">The value</param>
        public static double R2D(this double value) { return 180*value/Math.PI; }
        /// <summary>
        /// Multiply value with pi
        /// </summary>
        /// <param name="value">The value</param>
        public static double PI(this double value) { return Math.PI*value; }
        /// <summary>
        /// Divide value with pi
        /// </summary>
        /// <param name="value">The value</param>
        public static double divPI(this double value) { return value/Math.PI; }
        /// <summary>
        /// Check value if it is near zero (by default tolerance amount)
        /// </summary>
        /// <param name="value">The value</param>
        public static bool IsZero(this double value) { return value==0; }
        /// <summary>
        /// Check value if it is near zero (by tolerance amount)
        /// </summary>
        /// <param name="value">The value to check for zero</param>
        /// <param name="tolerance">The tolerance of equality</param>
        public static bool IsZero(this double value, double tolerance) { return Math.Abs(value)<=tolerance; }
        /// <summary>
        /// Check value if it is away zero (by default tolerance amount)
        /// <param name="value">The value</param>
        /// </summary>
        public static bool IsNotZero(this double value) { return !IsZero(value); }
        /// <summary>
        /// Check value if it is away zero (by tolerance amount)
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="tolerance">The tolerance near zero</param>
        public static bool IsNotZero(this double value, double tolerance) { return !IsZero(value, tolerance); }

        /// <summary>
        /// Checks if value is not NAN and not INF
        /// </summary>
        /// <param name="value">The value</param>
        public static bool IsFinite(this double value)
        {
            return !IsNotFinite(value);
        }
        /// <summary>
        /// Checks if value is NAN or INF
        /// </summary>
        /// <param name="value">The value</param>
        public static bool IsNotFinite(this double value)
        {
            if (double.IsNaN(value)||double.IsInfinity(value))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if value is greater than zero within tolerance
        /// </summary>
        /// <example><c>IsPositive(0.005,0.01)=false</c> and <c>IsPositive(0.05, 0.01)=true</c> </example>
        /// <param name="value">The value</param>
        /// <param name="tolerance">The tolerance amount, assumed to be positive</param>
        public static bool IsPositive(this double value, double tolerance)
        {
            return value>tolerance;
        }
        /// <summary>
        /// Checks if value is less than zero within tolerance
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="tolerance">The tolerance amount, assumed to be positive</param>
        public static bool IsNegative(this double value, double tolerance)
        {
            return value<-tolerance;
        }
        /// <summary>
        /// Checks if value is greater or equal to zero within tolerance
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="tolerance">The tolerance amount, assumed to be positive</param>
        public static bool IsPositiveOrZero(this double value, double tolerance)
        {
            return value>=-tolerance;
        }
        /// <summary>
        /// Checks if value is less than or equal to zero within tolerance
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="tolerance">The tolerance amount, assumed to be positive</param>
        public static bool IsNegativeOrZero(this double value, double tolerance)
        {
            return value<=tolerance;
        }
        /// <summary>
        /// Checks if value is greater to zero 
        /// </summary>
        /// <param name="value">The value</param>
        public static bool IsPositive(this double value)
        {
            return value>0;
        }
        /// <summary>
        /// Checks if value is less than zero 
        /// </summary>
        /// <param name="value">The value</param>
        public static bool IsNegative(this double value)
        {
            return value<0;
        }
        /// <summary>
        /// Checks if value is greater or equal to zero 
        /// </summary>
        /// <param name="value">The value</param>
        public static bool IsPositiveOrZero(this double value)
        {
            return value>=0;
        }
        /// <summary>
        /// Checks if value is less than or equal to zero
        /// </summary>
        /// <param name="value">The value</param>
        public static bool IsNegativeOrZero(this double value)
        {
            return value<=0;
        }
        /// <summary>
        /// Check value if it fits in interval. Sorts the boundary min..max before checking.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="min_value">The minimum</param>
        /// <param name="max_value">The maximum</param>
        /// <returns>True if min&lt;=x&lt;=max</returns>
        public static bool IsBetween(this double value, double min_value, double max_value)
        {
            return Chi(value, min_value, max_value)==1;
        }


        #endregion

        #region Series, CSV
        /// <summary>
        /// Fill in array with a linear number series.
        /// </summary>
        /// <param name="list">The array to fill</param>
        /// <param name="first_value">The first element value</param>
        /// <param name="last_value">The last element value</param>
        public static void Series(this double[] list, double first_value, double last_value)
        {
            int N=list.Length;

            if (N==1)
            {
                list[0]=first_value;
            }

            for (int i=0; i<N; i++)
            {
                list[i]=first_value+(double)i/(double)(N-1)*(last_value-first_value);
            }
        }
        /// <summary>
        /// Fill in array with an initializer function
        /// </summary>
        /// <param name="list">The array to fill</param>
        /// <param name="initializer">The initializer function to use</param>
        public static void Series(this double[] list, Func<int, double> initializer)
        {
            int N=list.Length;
            for (int i=0; i<N; i++)
            {
                list[i]=initializer(i);
            }
        }
        /// <summary>
        /// Convert array to comma separated string
        /// </summary>
        /// <param name="list">The array to convert</param>
        /// <returns>A string of values</returns>
        public static string ToCSV(this double[] list)
        {
            string[] parts=new string[list.Length];
            for (int i=0; i<parts.Length; i++)
            {
                parts[i]=list[i].ToString("R");
            }
            return string.Join(",", parts);
        }
        /// <summary>
        /// Convert comma separated string into array of values
        /// <remarks>Uses the <c>ParseDouble()</c> extension method. If item is not a number, zero value is used</remarks>        
        /// </summary>
        /// <param name="line">The string line to parse</param>
        /// <returns>An array of values</returns>
        public static double[] FromCSV(this string line)
        {
            string[] parts=line.Split(',');
            double[] list=new double[parts.Length];
            for (int i=0; i<list.Length; i++)
            {
                list[i]=parts[i].ParseDouble();
            }
            return list;
        }

        public static T ParseEnum<T>(this string value) where T:struct, IComparable, IFormattable 
        {
            return (T)Enum.Parse(typeof(T), value);
        }
        #endregion

        #region Algorithms

        public static int CalcHashCode<T>(this T[] array)
        {
            int hash=17*23;
            unchecked
            {
                for (int i=0; i<array.Length; i++)
                {
                    hash=23*hash+array[i].GetHashCode();
                }
            }
            return hash;
        }

        /// <arg name="x">The point to evaluate at</arg>
        /// <arg name="coef">The polynomial coefficients</arg>
        /// <returns>The polynomial value C0+C1*x+C2*x^2+...</returns>
        public static double Polynomial(this double x, params double[] coef)
        {
            double res=0;
            for (int i=coef.Length-1; i>=0; i--)
            {
                res=x*res+coef[i];
            }
            return res;
        }

        /// <summary>
        /// Calculate root of function with the bisection method. Solves <c>f(x)=y</c> for <c>x</c> near <c>x_init</c>.
        /// 
        /// <remarks>Will double or half <c>x_init</c> to establish solve bracket before calling the bracketed bisection method</remarks>
        /// </summary>
        /// <param name="f">The function</param>
        /// <param name="y_target">The right hand size</param>
        /// <param name="x_init">The initial value to search for solution bracket</param>
        /// <param name="x_tol">The solution tolerance for x</param>
        /// <param name="x">The resulting value</param>
        /// <returns>True if solution is found, false otherwise</returns>
        public static bool Bisection(this Func<double, double> f, double y_target, double x_init, double x_tol, out double x)
        {
            const int limit=40;
            if (x_tol<=0) { x_tol=1e-8; }
            double x_low=-10*x_tol;
            double x_high=x_init+10*x_tol;
            if (Math.Abs(x_init)>x_tol)
            {
                x_low=x_init-10*x_tol;
                x_high=x_init+10*x_tol;
            }

            double y_low=f(x_low);
            double y_high=f(x_high);

            bool ascending=(y_high-y_low)>=0;
            int count=0;
            while ((y_high-y_target)*(y_low-y_target)>0 && count<limit)
            {
                double x_mid=(x_low+x_high)/2;
                int sign=ascending?1:-1;
                if (sign*y_target<sign*y_low)
                {
                    x_low-=2*(x_mid-x_low);
                    y_low=f(x_low);
                }
                if (sign*y_target>sign*y_high)
                {
                    x_high+=2*(x_high-x_mid);
                    y_high=f(x_high);
                }
                ascending=(y_high-y_low)>=0;
                count++;
            }

            if (count<limit)
            {
                return Bisection(f, y_target, x_low, x_high, x_tol, out x);
            }
            else
            {
                x=x_init;
                return false;
            }
        }

        /// <summary>
        /// Calculate root of function with the bisection method. Solves <c>f(x)=y</c> for <c>x</c> in <c>[x_low..x_high]</c>.
        /// </summary>
        /// <param name="f">The function</param>
        /// <param name="y_target">The right hand size</param>
        /// <param name="x_low">The low value for the solution interval</param>
        /// <param name="x_high">The high value for the solution interval</param>
        /// <param name="x_tol">The solution tolerance for x</param>
        /// <param name="x">The resulting value</param>
        /// <returns>True if solution is found, false otherwise</returns>
        public static bool Bisection(this Func<double, double> f, double y_target, double x_low, double x_high, double x_tol, out double x)
        {
            const int limit=40;
            x=(x_low+x_high)/2;
            double y_low=f(x_low);
            double y_high=f(x_high);
            double dx=Math.Abs(x_high-x_low);
            int count=0;
            while ((y_low-y_target)*(y_high-y_target)<=0&&count<limit&&dx>x_tol)
            {

                double y=f(x);
                if ((y-y_target)*(y_high-y_target)<=0)
                {
                    x_low=x;
                }
                else
                {
                    x_high=x;
                }
                x=(x_low+x_high)/2;
                dx=Math.Abs(x_high-x_low);
                count++;
            }

            return (y_low-y_target)*(y_high-y_target)<=0;
        }

        #endregion
    }

    #region Formatters
    public class NiceTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType==typeof(string)||base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string) return DoubleEx.ParseNice(value as string, culture.NumberFormat);
            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType==typeof(string))
            {
                if (value is IConvertible && !(value is double))
                {
                    value=(value as IConvertible).ToDouble(culture.NumberFormat);
                } else if (value is double x)
                {
                    DoubleEx.Nice(x, culture.NumberFormat.NumberDecimalDigits);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    } 
    #endregion
}

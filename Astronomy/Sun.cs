/********************************************
 * 日の出と日没の時刻を返すクラス
 * 
 * Auther: Katsuhiro Morishita
 * Create: 2013/1/26
 * History: 2013/1/27   太陽の視直径を通日の関数にしたいなぁ。
 * *****************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astronomy
{
    public class Sun
    {
        /// <summary>
        /// 均時差[h]を返す
        /// <para>近似式の時刻系がUTCなのかどうかが気になる。</para>
        /// </summary>
        /// <param name="epoch">計算したい瞬間の時刻</param>
        /// <returns>均時差[h]</returns>
        private static double GetEquationOfTime(DateTime epoch)
        {
            var epoch2 = epoch.ToUniversalTime();
            var firstDayOfYear = new DateTime(epoch2.Year, 1, 1);    // UTCになる
            var omega = 2.0 * Math.PI * ((double)(epoch2.Ticks - firstDayOfYear.Ticks) / Math.Pow(10.0, 7) / 24.0 / 3600.0) / 366;
            var theta = -0.0002789049 + 0.1227715 * Math.Cos(omega + 1.498311)
                - 0.1654575 * Math.Cos(2.0 * omega - 1.261546) - 0.0053538 * Math.Cos(3.0 * omega - 1.1571);                    // http://www11.plala.or.jp/seagate/calc/calc2.html
            return theta;
        }
        /// <summary>
        /// 太陽赤緯[deg]を返す
        /// <para>近似式の時刻系がUTCなのかどうかが気になる。</para>
        /// <para>経過日数にしても、いかにも不正確だ。。。基準エポックを知りたい。。</para>
        /// </summary>
        /// <param name="epoch">計算したい瞬間の時刻</param>
        /// <returns>太陽赤緯[deg]</returns>
        private static double GetSeclinationOfTheSun(DateTime epoch)
        {
            var epoch2 = epoch.ToUniversalTime();
            var firstDayOfYear = new DateTime(epoch2.Year, 1, 1);    // UTCになる
            double J = ((double)(epoch2.Ticks - firstDayOfYear.Ticks) / Math.Pow(10.0, 7) / 24.0 / 3600.0);   // 1/1の0時からの経過日数
            var j = 365;
            if (DateTime.IsLeapYear(epoch.Year)) j++;
            double omega = 2.0 * Math.PI / j;
            var delta = 0.33281
                - 22.984 * Math.Cos(omega * J) - 0.34990 * Math.Cos(2.0 * omega * J) - 0.13980 * Math.Cos(3.0 * omega * J)
                + 3.7872 * Math.Sin(omega * J) + 0.0325 * Math.Sin(2.0 * omega * J) + 0.07187 * Math.Sin(3.0 * omega * J);      // http://www11.plala.or.jp/seagate/calc/calc2.html
            var deltaForDebug = delta;
            // 方法その2
            var delta2 = 0.006918
                - 0.399912 * Math.Cos(omega * J) - 0.006758 * Math.Cos(2.0 * omega * J) - 0.002697 * Math.Cos(3.0 * omega * J)
                + 0.070257 * Math.Sin(omega * J) + 0.000907 * Math.Sin(2.0 * omega * J) + 0.001480 * Math.Sin(3.0 * omega * J); // http://www.es.ris.ac.jp/~nakagawa/met_cal/solar.html
            //delta = delta * Math.PI / 180.0;
            delta = delta2;
            return delta;
        }
        /// <summary>
        /// 日の出・日の入り時刻を返す
        /// <para>精度は、5分以内です。</para>
        /// </summary>
        /// <param name="lat_deg">緯度[deg]</param>
        /// <param name="lon_deg">経度[deg]</param>
        /// <param name="date">日付</param>
        /// <returns>日の出時刻[datetime]と日の入り時刻[datetime]のTuple</returns>
        public static Tuple<DateTime, DateTime> GetSunrizeAndSunsetTime1(double lat_deg, double lon_deg, DateTime date)
        {
            var lat = lat_deg * Math.PI / 180.0;
            var lon = lon_deg * Math.PI / 180.0;

            // 太陽赤緯
            double J = date.DayOfYear - 0.5;
            var j = 365;
            if(DateTime.IsLeapYear(date.Year)) j++;
            double omega = 2.0 * Math.PI / j;
            var delta = 0.33281
                - 22.984 * Math.Cos(omega * J) - 0.34990 * Math.Cos(2.0 * omega * J) - 0.13980 * Math.Cos(3.0 * omega * J)
                + 3.7872 * Math.Sin(omega * J) + 0.0325 * Math.Sin(2.0 * omega * J) + 0.07187 * Math.Sin(3.0 * omega * J);      // http://www11.plala.or.jp/seagate/calc/calc2.html
            var deltaForDebug = delta;
            var delta2 = 0.006918
                - 0.399912 * Math.Cos(omega * J) - 0.006758 * Math.Cos(2.0 * omega * J) - 0.002697 * Math.Cos(3.0 * omega * J)
                + 0.070257 * Math.Sin(omega * J) + 0.000907 * Math.Sin(2.0 * omega * J) + 0.001480 * Math.Sin(3.0 * omega * J); // http://www.es.ris.ac.jp/~nakagawa/met_cal/solar.html
            delta = delta * Math.PI / 180.0;
            delta = delta2;
            // 中央標準時からのずれ[h]
            var p = (lon_deg - 135) / 15.0;
            // 均時差(公転速度の変動と地軸の傾きの影響の補正項)[h], 平均太陽時と、実際の太陽時とのずれの近似値計算
            omega = 2.0 * Math.PI * (date.DayOfYear - 1) / 366;
            var theta = -0.0002789049 + 0.1227715 * Math.Cos(omega + 1.498311)
                - 0.1654575 * Math.Cos(2.0 * omega - 1.261546) - 0.0053538 * Math.Cos(3.0 * omega - 1.1571);                    // http://www11.plala.or.jp/seagate/calc/calc2.html
            // 日出・日没時刻[h]
            var hoge = -Math.Tan(delta) * Math.Tan(lat);// -1 < tan(lat) < 1
            var ac = Math.Acos(hoge);
            var ang = (31.8 / 2.0 / 60.0 + 34.3333333 / 60.0) * 24.0 / 360.0;      // 太陽の直径による視角度32'と大気差（屈折）の影響[h]
            var rise = 12.0 - (ac / Math.PI * 12.0 + p + theta) - ang;
            var down = 12.0 - (-ac / Math.PI * 12.0 + p + theta) + ang;

            var origin = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, date.Kind);
            var riseTime = origin.AddHours(rise);
            var downTime = origin.AddHours(down);
            return new Tuple<DateTime, DateTime>(riseTime, downTime);
        }
        /// <summary>
        /// 日の出・日の入り時刻を返す
        /// <para>精度は、5分以内です。</para>
        /// </summary>
        /// <param name="lat_deg">緯度[deg]</param>
        /// <param name="lon_deg">経度[deg]</param>
        /// <param name="date">日付</param>
        /// <returns>日の出時刻[datetime]と日の入り時刻[datetime]のTuple</returns>
        public static Tuple<DateTime, DateTime> GetSunrizeAndSunsetTime2(double lat_deg, double lon_deg, DateTime date)
        {
            var sunset = Sun.GetSunrizeAndSunsetTime1(lat_deg, lon_deg, date);
            var rise = sunset.Item1;
            var down = sunset.Item2;
            // 日の出と日没時のより正確な均時差を求める
            var thetaAtRise = Sun.GetEquationOfTime(rise);
            var thetaAtDown = Sun.GetEquationOfTime(down);
            // 日の出と日没時のより正確な太陽赤緯を求める
            var deltaAtRise = Sun.GetSeclinationOfTheSun(rise);
            var deltaAtDown = Sun.GetSeclinationOfTheSun(down);

            var lat = lat_deg * Math.PI / 180.0;
            // 中央標準時からのずれ[h]
            var p = (lon_deg - 135) / 15.0;
            // 太陽の直径32'による視角度と大気差（屈折）の影響[h]
            var ang = (31.8 / 2.0 / 60.0 + 34.3333333 / 60.0) * 24.0 / 360.0;

            double hogehoge = 0.0;
            double ac = 0.0;
            // 日の出の時刻[h]
            hogehoge = -Math.Tan(deltaAtRise) * Math.Tan(lat);// -1 < tan(lat) < 1
            ac = Math.Acos(hogehoge);
            var riseTime = 12.0 - (ac / Math.PI * 12.0 + p + thetaAtRise) - ang;
            // 日没時刻[h]
            hogehoge = -Math.Tan(deltaAtDown) * Math.Tan(lat);// -1 < tan(lat) < 1
            ac = Math.Acos(hogehoge);
            var downTime = 12.0 - (-ac / Math.PI * 12.0 + p + thetaAtDown) + ang;

            var hoge = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, date.Kind);
            rise = hoge.AddHours(riseTime);
            down = hoge.AddHours(downTime);
            return new Tuple<DateTime, DateTime>(rise, down);
        }

    }
}

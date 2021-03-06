﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ThinkGeo.MapSuite.Core;

namespace ThinkGeo.MapSuite.USDemographicMap
{
    public class ThematicDemographicStyleBuilder : DemographicStyleBuilder
    {
        private int classBreakCount;
        private GeoColor endColor;
        private ColorWheelDirection colorWheelDirection;

        public ThematicDemographicStyleBuilder()
            : this(new string[] { })
        { }

        public ThematicDemographicStyleBuilder(IEnumerable<string> selectedColumns)
            : base(selectedColumns)
        {
            Opacity = 200;
            ClassBreakCount = 10;
            Color = GeoColor.SimpleColors.LightBlue;
            EndColor = GeoColor.SimpleColors.LightRed;
            ColorWheelDirection = ColorWheelDirection.CounterClockwise;
        }

        public int ClassBreakCount
        {
            get { return classBreakCount; }
            set { classBreakCount = value; }
        }

        public ColorWheelDirection ColorWheelDirection
        {
            get { return colorWheelDirection; }
            set { colorWheelDirection = value; }
        }

        public GeoColor StartColor
        {
            get { return Color; }
            set { Color = value; }
        }

        public GeoColor EndColor
        {
            get { return endColor; }
            set { endColor = value; }
        }

        protected override Collection<Style> GetStylesCore(FeatureSource featureSource)
        {
            Collection<GeoColor> familyColors = GeoColor.GetColorsInQualityFamily(Color, EndColor, classBreakCount, ColorWheelDirection);

            featureSource.Open();
            int featureCount = featureSource.GetCount();
            double[] values = new double[featureCount];
            for (int i = 0; i < featureCount; i++)
            {
                Feature feature = featureSource.GetFeatureById((i + 1).ToString(CultureInfo.InvariantCulture), SelectedColumns);
                double columnValue;
                double.TryParse(feature.ColumnValues[SelectedColumns[0]], out columnValue);
                values[i] = columnValue;
            }
            featureSource.Close();

            ClassBreakStyle classBreakStyle = new ClassBreakStyle(SelectedColumns[0]) { BreakValueInclusion = BreakValueInclusion.IncludeValue };
            double[] classBreakValues = GetClusterClassBreaks(values, ClassBreakCount - 1);
            for (int i = 0; i < classBreakValues.Length; i++)
            {
                ClassBreak classBreak = new ClassBreak(classBreakValues[i], AreaStyles.CreateSimpleAreaStyle(new GeoColor(this.Opacity, familyColors[i]), GeoColor.FromHtml("#f05133"), 1));
                classBreakStyle.ClassBreaks.Add(classBreak);
            }

            TextStyle textStyle = TextStyles.Country1("NAME");
            return new Collection<Style>() { classBreakStyle, textStyle };
        }

        private double[] GetClusterClassBreaks(double[] values, int count)
        {
            var result = new List<double>();

            var orderedValues = values.OrderBy(v => v).ToArray();
            var min = orderedValues[0];
            var max = orderedValues[orderedValues.Length - 1];

            var classesCount = (int)(orderedValues.Length / count);
            var breakValue = min;

            for (var i = 1; i < count; i++)
            {
                breakValue = orderedValues[i * classesCount];
                if (!result.Contains(breakValue))
                {
                    result.Add(breakValue);
                }
            }
            result.Insert(0, 0);

            return result.ToArray();
        }
    }
}
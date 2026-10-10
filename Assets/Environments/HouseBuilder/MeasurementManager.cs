using System.Globalization;
using UnityEngine;

public static class MeasurementManager
{
    public enum Unit
    {
        Feet,
        Inches,
        Meters,
        Millimeters,
        FeetAndInches,
        Yards,
        Centimeters,
    }

    public enum Precision
    {
        zero,
        one,
        two,
    }

    static MeasurementManager()
    {
    }

    public static Unit MeasurementTypeEnum = Unit.Meters;
    public static Precision PrecisionEnum = Precision.two;

    public static string ShortMeasurementType()
    {
        string s = "";
        switch (MeasurementTypeEnum)
        {
            case Unit.Feet:
                s = "Ft";
                break;

            case Unit.Inches:
                s = "in";
                break;

            case Unit.Meters:
                s = "M";
                break;

            case Unit.Millimeters:
                s = "mm";
                break;

            case Unit.FeetAndInches:
                s = "Ft in";
                break;

            case Unit.Yards:
                s = "yd";
                break;

            case Unit.Centimeters:
                s = "cm";
                break;
        }
        return s;
    }

    private static string TruncateTheMeasure(float measure, string str)
    {
        string s = null;
        switch (PrecisionEnum)
        {
            case Precision.zero:
                s = measure.ToString("F0");
                break;

            case Precision.one:
                s = measure.ToString("F1");
                break;

            case Precision.two:
                s = measure.ToString("F2");
                break;
        }
        return s + str;
    }

    public static string ConvertTheMeasure(float measure, bool convert = true)
    {
        float f = 0f;
        if (convert == false)
        {
            return TruncateTheMeasure(measure, "");
        }

        switch (MeasurementTypeEnum)
        {
            case Unit.Feet:
                f = measure * CoreUtils.MetersToFeet;
                return TruncateTheMeasure(f, " ft");

            case Unit.Inches:
                f = measure * CoreUtils.MetersToInches;
                return TruncateTheMeasure(f, " in");

            case Unit.Meters:
                return TruncateTheMeasure(measure, " m");

            case Unit.FeetAndInches:
                return FeetAndInchesConversion(measure);

            case Unit.Yards:
                f = measure * 1.09361f;
                return TruncateTheMeasure(f, " yd");

            case Unit.Centimeters:
                f = measure * 100f;
                return TruncateTheMeasure(f, " cm");

            case Unit.Millimeters:
                f = measure * 1000f;
                return TruncateTheMeasure(f, " mm");
        }

        return null;
    }

    public static string ConvertMeasureClean(float measure)
    {
        float f = 0f;
        switch (MeasurementTypeEnum)
        {
            case Unit.Feet:
                f = measure * CoreUtils.MetersToFeet;
                return TruncateTheMeasure(f, "");

            case Unit.Inches:
                f = measure * CoreUtils.MetersToInches;
                return TruncateTheMeasure(f, "");

            case Unit.Meters:
                return TruncateTheMeasure(measure, "");

            case Unit.FeetAndInches:
                return FeetAndInchesConversion(measure, true);

            case Unit.Yards:
                f = measure * 1.09361f;
                return TruncateTheMeasure(f, "");

            case Unit.Centimeters:
                f = measure * 100f;
                return TruncateTheMeasure(f, "");

            case Unit.Millimeters:
                f = measure * 1000f;
                return TruncateTheMeasure(f, "");
        }

        return null;
    }


    public static float ConvertToMeters(float value) => ConvertToMeters(value, MeasurementTypeEnum);

    public static float ConvertToMeters(float value, Unit unit)
    {
        float result = 0f;
        switch (unit)
        {
            case Unit.Feet:
                result = value * CoreUtils.FeetToMeters;
                break;
            case Unit.Inches:
                result = value * CoreUtils.InchesToMeters;
                break;
            case Unit.Meters:
                result = value;
                break;
            case Unit.FeetAndInches:
                result = FeetAndInchesToMeters(value);
                break;
            case Unit.Yards:
                result = value * 0.9144f;
                break;
            case Unit.Centimeters:
                result = value * 0.01f;
                break;
            case Unit.Millimeters:
                result = value * 0.001f;
                break;
        }

        return result;
    }



    public static float ConvertFromMeters(float meters) => ConvertFromMeters(meters, MeasurementTypeEnum);
    
    public static float ConvertFromMeters(float meters, Unit unit)
    {
        float result = 0f;
        switch (unit)
        {
            case Unit.Feet:
                result = meters / CoreUtils.FeetToMeters;
                break;
            case Unit.Inches:
                result = meters / CoreUtils.InchesToMeters;
                break;
            case Unit.Meters:
                result = meters;
                break;
            case Unit.FeetAndInches:
                result = MetersToFeetAndInches(meters);
                break;
            case Unit.Yards:
                result = meters / 0.9144f;
                break;
            case Unit.Centimeters:
                result = meters / 0.01f;
                break;
            case Unit.Millimeters:
                result = meters / 0.001f;
                break;
        }

        return result;
    }




    private static float FeetAndInchesToMeters(float measure)
    {
        //take the feet, convert to meters
        //take the inches, convert to meters
        //add them together
        float feet = float.Parse(measure.ToString("F0"), CultureInfo.InvariantCulture);
        float inches = measure - feet; 
        inches *= 10; // multiply by 10 to move decimal left

        feet = feet * CoreUtils.FeetToMeters;
        inches = inches * CoreUtils.InchesToMeters;
        return feet + inches;
    }

    private static float MetersToFeetAndInches(float meters)
    {
        //take the feet, convert to meters
        //take the inches, convert to meters
        //add them together

        float feet = meters * CoreUtils.MetersToFeet;

        float floored = Mathf.Floor(feet);
        float fractionalPart = feet - floored;
        return floored + (fractionalPart * 12);
    }

    private static string FeetAndInchesConversion(float metreMeasure, bool clean = false)
    {
        string result = null;

        float feet = metreMeasure * CoreUtils.MetersToFeet;
        result = feet.ToString("F0");

        float decimalPart = feet % 1;

        int inches = (int)(12 * decimalPart);

        result = feet.ToString("F0");

        string decimalPartString = inches.ToString();

        if (clean)
        {
            result += ". "+decimalPartString;
        }
        else
        {
            result = result + "ft" + decimalPartString + " in";
        }

        return result;
    }
    }

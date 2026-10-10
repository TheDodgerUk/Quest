using UnityEngine;

public class SkyTestGUI : MonoBehaviour
{
    public Sky sky;

    private Rect rect = new Rect(-3, -3, 300, 450+6);
    private const float label_width = 150;

    protected void OnEnable()
    {
        if (!sky)
        {
            Debug.LogError("Sky instance reference not set. Disabling script.");
            this.enabled = false;
        }
    }

    protected void OnGUI()
    {
        GUILayout.BeginArea(rect, "", "Box");
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("CYCLE");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Time of Day", GUILayout.Width(label_width));
        sky.Cycle.TimeOfDay = GUILayout.HorizontalSlider(sky.Cycle.TimeOfDay, 0, 24);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Julian Date", GUILayout.Width(label_width));
        sky.Cycle.JulianDate = GUILayout.HorizontalSlider(sky.Cycle.JulianDate, 0, 360);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("DAY");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Sun Coloring & Falloff", GUILayout.Width(label_width));
        sky.Sun.Coloring = GUILayout.HorizontalSlider(sky.Sun.Coloring, 0.0f, 0.5f);
        sky.Sun.Falloff = GUILayout.HorizontalSlider(sky.Sun.Falloff, 0.1f, 1.5f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Rayleigh & Mie Multiplier", GUILayout.Width(label_width));
        sky.Day.RayleighMultiplier = GUILayout.HorizontalSlider(sky.Day.RayleighMultiplier, 0, 2);
        sky.Day.MieMultiplier = GUILayout.HorizontalSlider(sky.Day.MieMultiplier, 0, 1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Brightness / Haziness", GUILayout.Width(label_width));
        sky.Day.Brightness = GUILayout.HorizontalSlider(sky.Day.Brightness, 0, 20.0f);
        sky.Day.Haziness = GUILayout.HorizontalSlider(sky.Day.Haziness, 0.1f, 1.0f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Base Color", GUILayout.Width(label_width));
        float day_r = GUILayout.HorizontalSlider(sky.Day.Color.r, 0, 1);
        float day_g = GUILayout.HorizontalSlider(sky.Day.Color.g, 0, 1);
        float day_b = GUILayout.HorizontalSlider(sky.Day.Color.b, 0, 1);
        float day_a = GUILayout.HorizontalSlider(sky.Day.Color.a, 0, 1);
        sky.Day.Color = new Color(day_r, day_g, day_b, day_a);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("NIGHT");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Moon Phase / Haziness", GUILayout.Width(label_width));
        sky.Moon.Phase = GUILayout.HorizontalSlider(sky.Moon.Phase, -1, 1);
        sky.Night.Haziness = GUILayout.HorizontalSlider(sky.Night.Haziness, 0.1f, 1.0f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Base Color", GUILayout.Width(label_width));
        float night_r = GUILayout.HorizontalSlider(sky.Night.Color.r, 0, 1);
        float night_g = GUILayout.HorizontalSlider(sky.Night.Color.g, 0, 1);
        float night_b = GUILayout.HorizontalSlider(sky.Night.Color.b, 0, 1);
        float night_a = GUILayout.HorizontalSlider(sky.Night.Color.a, 0, 1);
        sky.Night.Color = new Color(night_r, night_g, night_b, night_a);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Haze Color", GUILayout.Width(label_width));
        float nighthaze_r = GUILayout.HorizontalSlider(sky.Night.HazeColor.r, 0, 1);
        float nighthaze_g = GUILayout.HorizontalSlider(sky.Night.HazeColor.g, 0, 1);
        float nighthaze_b = GUILayout.HorizontalSlider(sky.Night.HazeColor.b, 0, 1);
        float nighthaze_a = GUILayout.HorizontalSlider(sky.Night.HazeColor.a, 0, 1);
        sky.Night.HazeColor = new Color(nighthaze_r, nighthaze_g, nighthaze_b, nighthaze_a);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Cloud Color", GUILayout.Width(label_width));
        float nightcloud_r = GUILayout.HorizontalSlider(sky.Night.CloudColor.r, 0, 1);
        float nightcloud_g = GUILayout.HorizontalSlider(sky.Night.CloudColor.g, 0, 1);
        float nightcloud_b = GUILayout.HorizontalSlider(sky.Night.CloudColor.b, 0, 1);
        float nightcloud_a = GUILayout.HorizontalSlider(sky.Night.CloudColor.a, 0, 1);
        sky.Night.CloudColor = new Color(nightcloud_r, nightcloud_g, nightcloud_b, nightcloud_a);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("CLOUDS");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Tone / Shading", GUILayout.Width(label_width));
        sky.Clouds.Tone = GUILayout.HorizontalSlider(sky.Clouds.Tone, 0, 2);
        sky.Clouds.Shading = GUILayout.HorizontalSlider(sky.Clouds.Shading, 0, 1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Density / Sharpness", GUILayout.Width(label_width));
        sky.Clouds.Density = GUILayout.HorizontalSlider(sky.Clouds.Density, 0, 2);
        sky.Clouds.Sharpness = GUILayout.HorizontalSlider(sky.Clouds.Sharpness, 0, 10);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Speed 1 / Speed 2", GUILayout.Width(label_width));
        sky.Clouds.Speed1 = GUILayout.HorizontalSlider(sky.Clouds.Speed1, 0, 5);
        sky.Clouds.Speed2 = GUILayout.HorizontalSlider(sky.Clouds.Speed2, 0, 5);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale 1 / Scale 2", GUILayout.Width(label_width));
        sky.Clouds.Scale1 = GUILayout.HorizontalSlider(sky.Clouds.Scale1, 1, 10);
        sky.Clouds.Scale2 = GUILayout.HorizontalSlider(sky.Clouds.Scale2, 1, 10);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}

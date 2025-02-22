namespace PlanetarySystem.Planet
{
    public class MinMax
    {
        public float Min { get; private set; } = float.MaxValue;
        public float Max { get; private set; } = float.MinValue;

        public void AddValue(float v)
        {
            if (v > Max) Max = v;

            if (v < Min) Min = v;
        }
    }
}
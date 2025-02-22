namespace PlanetarySystem.Planet
{
    public static class NoiseFilterFactory
    {
        public static INoiseFilter CreateNoiseFilter(NoiseSettings settings)
        {
            return settings.filterType switch
            {
                NoiseSettings.FilterType.Simple => new SimpleNoiseFilter(settings.simpleNoiseSettings),
                NoiseSettings.FilterType.Ridgid => new RidgidNoiseFilter(settings.ridgidNoiseSettings),
                _ => null
            };
        }
    }
}
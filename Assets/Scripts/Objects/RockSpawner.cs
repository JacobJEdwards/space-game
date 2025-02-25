#nullable enable

using Managers;

namespace Objects
{
    public class RockSpawner : ObjectSpawner<Rock>
    {
        protected override int GetMaxSize() => 1000;
        protected override int GetDefaultCapacity() => 100;

    }
}
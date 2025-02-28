using Managers;

namespace Objects
{
    public class TreeSpawner : ObjectSpawner<TreeObject>
    {
        protected override int GetMaxSize() => 200;
        protected override int GetDefaultCapacity() => 100;
    }
}
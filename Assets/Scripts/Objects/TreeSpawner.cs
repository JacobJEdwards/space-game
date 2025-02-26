using Managers;

namespace Objects
{
    public class TreeSpawner : ObjectSpawner<Tree>
    {
        protected override int GetMaxSize() => 200;
        protected override int GetDefaultCapacity() => 100;
    }
}
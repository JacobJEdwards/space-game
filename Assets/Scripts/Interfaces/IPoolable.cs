using UnityEngine.Pool;

namespace Interfaces
{
    public interface IPoolable<T> where T : class
    {
        void SetPool(IObjectPool<T> pool);
    }
}
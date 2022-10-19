using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace HungryWormGame
{
    public class GameEnvironment : Canvas
    {
        #region Fields

        private readonly List<GameObject> destroyableGameObjects = new(); 

        #endregion

        #region Ctor

        public GameEnvironment()
        {
            RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        }

        #endregion

        #region Methods

        public IEnumerable<T> GetGameObjects<T>()
        {
            return Children.OfType<T>();
        }

        public void AddDestroyableGameObject(GameObject destroyable)
        {
            destroyableGameObjects.Add(destroyable);
        }

        public void RemoveDestroyableGameObjects()
        {
            foreach (var destroyable in destroyableGameObjects)
            {
                RemoveGameObject(destroyable);
            }

            ClearDestroyableGameObjects();
        }

        public void RemoveGameObject(GameObject destroyable)
        {
            Children.Remove(destroyable);
        }

        public void ClearDestroyableGameObjects()
        {
            destroyableGameObjects.Clear();
        }

        #endregion
    }
}

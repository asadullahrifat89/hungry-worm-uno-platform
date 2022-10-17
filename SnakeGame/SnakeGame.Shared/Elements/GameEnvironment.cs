using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
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

        public void AddDestroyableGameObject(GameObject destroyable)
        {
            destroyableGameObjects.Add(destroyable);
        }

        public void RemoveDestroyableGameObjects()
        {
            //if (Parallel.ForEach(destroyableGameObjects, destroyable =>
            //{
            //    RemoveGameObject(destroyable);

            //}).IsCompleted)
            //{
            //    ClearDestroyableGameObjects();
            //}

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    public class UIEdgeCollection
    {
        public Image[] images;

        public UIEdgeCollection(Image[] images)
        {
            this.images = images;
        }

        // Set the color of the images in the collection
        public void SetColor(Color color)
        {
            foreach (Image img in images)
            {
                img.color = color;
            }
        }
    }
}

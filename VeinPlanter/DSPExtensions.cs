using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VeinPlanter
{
    public static class DSPExtensions
    {
        class PlanetDataExt
        {
            public int extraField;
        }
        static Dictionary<PlanetData, PlanetDataExt> ExtFields = new Dictionary<PlanetData, PlanetDataExt>();

        public static int AddVeinGroupData(this PlanetData planet, PlanetData.VeinGroup vein)
        {
            PlanetDataExt ext = ExtFields[planet];
            ext.extraField = 1;
            int newIndex = planet.veinGroups.Length;
            planet.SetVeinGroupCapacity(planet.veinGroups.Length + 1);
            planet.veinGroups[newIndex] = vein;
            return newIndex;
        }

        public static void SetVeinGroupCapacity(this PlanetData planet, int newCapacity)
        {
            PlanetData.VeinGroup[] array = planet.veinGroups;
            planet.veinGroups = new PlanetData.VeinGroup[newCapacity];
            if (array != null)
            {
                Array.Copy(array, planet.veinGroups, (newCapacity <= array.Length) ? newCapacity : array.Length);
            }
            //veinCapacity = newCapacity;
        }


        public static bool ScreenPointIntoRectSimple(Vector3 screenPoint, /*RectTransform rect, */out Vector2 rectPoint)
        {
            rectPoint = new Vector2(-1f, -1f);
            if (BGMController.instance.uiRoot == null)
            {
                return false;
            }
            Ray ray = BGMController.instance.uiRoot.overlayCanvas.worldCamera.ScreenPointToRay(screenPoint);
            //ray.origin = rect.InverseTransformPoint(ray.origin);
            //ray.direction = rect.InverseTransformVector(ray.direction);
            if (new Plane(Vector3.forward, 0f).Raycast(ray, out var enter))
            {
                rectPoint.x = ray.origin.x + ray.direction.x * enter;
                rectPoint.y = ray.origin.y + ray.direction.y * enter;
                return true;
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public static class ExtensionClass {
    public  static Vector2 Vector3ToVector2(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.y);
    }
    public static Vector2 RVOVector2ToVector2(this RVO.Vector2 vector2) {
        return new Vector2(vector2.x_, vector2.y_);
    }
}
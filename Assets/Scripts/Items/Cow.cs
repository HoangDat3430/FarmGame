using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm
{
    public class Cow : Item
    {
        public Cow () : base(10)
        {

        }
        public override bool CanUse(Vector3Int target)
        {
            return true;
        }

        public override bool Use(Vector3Int target)
        {
            return true;
        }
    }
}
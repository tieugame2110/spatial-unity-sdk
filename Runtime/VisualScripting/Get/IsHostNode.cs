using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace SpatialSys.UnitySDK.VisualScripting
{
    [UnitTitle("Is Host")]
    public class IsHostNode : Unit
    {
        [DoNotSerialize]
        public ValueOutput isHost { get; private set; }

        protected override void Definition()
        {
            isHost = ValueOutput<bool>(nameof(isHost), (f) => ClientBridge.IsLocalHost.Invoke());
        }
    }
}
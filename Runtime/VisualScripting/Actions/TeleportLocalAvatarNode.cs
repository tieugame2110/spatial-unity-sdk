using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace SpatialSys.UnitySDK.VisualScripting
{
    [UnitCategory("Spatial\\Actions")]
    [UnitTitle("Spatial: Teleport Local Avatar")]

    [UnitSurtitle("Spatial")]
    [UnitShortTitle("Teleport Local Avatar")]

    [TypeIcon(typeof(SpatialComponentBase))]
    public class TeleportAvatarSelfNode : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger { get; private set; }
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger { get; private set; }

        [DoNotSerialize]
        public ValueInput newPosition { get; private set; }

        protected override void Definition()
        {
            newPosition = ValueInput<Vector3>(nameof(newPosition), Vector3.zero);

            inputTrigger = ControlInput(nameof(inputTrigger), (f) => {
                ClientBridge.SetLocalAvatarPosition.Invoke(f.GetValue<Vector3>(newPosition));
                return outputTrigger;
            });

            outputTrigger = ControlOutput(nameof(outputTrigger));

            Succession(inputTrigger, outputTrigger);
        }
    }

    [UnitCategory("Spatial\\Actions")]
    [UnitTitle("Spatial: Teleport Local Avatar")]

    [UnitSurtitle("Spatial")]
    [UnitShortTitle("Teleport Local Avatar")]

    [TypeIcon(typeof(SpatialComponentBase))]
    public class TeleportAvatarSelfWithRotationNode : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput inputTrigger { get; private set; }
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput outputTrigger { get; private set; }

        [DoNotSerialize]
        public ValueInput newPosition { get; private set; }

        [DoNotSerialize]
        public ValueInput newRotation { get; private set; }

        protected override void Definition()
        {
            newPosition = ValueInput<Vector3>(nameof(newPosition), Vector3.zero);
            newRotation = ValueInput<Quaternion>(nameof(newRotation), Quaternion.identity);

            inputTrigger = ControlInput(nameof(inputTrigger), (f) => {
                ClientBridge.SetLocalAvatarPositionRotation.Invoke(f.GetValue<Vector3>(newPosition), f.GetValue<Quaternion>(newRotation));
                return outputTrigger;
            });

            outputTrigger = ControlOutput(nameof(outputTrigger));

            Succession(inputTrigger, outputTrigger);
        }
    }
}

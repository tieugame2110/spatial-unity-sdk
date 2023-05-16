using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace SpatialSys.UnitySDK.VisualScripting
{
    [UnitTitle("Spatial: On Backpack Menu Open Changed")]
    [UnitSurtitle("Spatial")]
    [UnitShortTitle("On Backpack Menu Open Changed")]
    [UnitCategory("Events\\Spatial")]
    [TypeIcon(typeof(SpatialComponentBase))]
    public class OnBackpackMenuOpenChangedNode : EventUnit<bool>
    {
        public static string eventName = "SpatialOnBackpackMenuOpenChanged";
        protected override bool register => true;

        [DoNotSerialize]
        public ValueOutput isOpen { get; private set; }

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(eventName);
        }

        protected override void Definition()
        {
            base.Definition();
            isOpen = ValueOutput<bool>(nameof(isOpen));
        }

        protected override bool ShouldTrigger(Flow flow, bool isOpen)
        {
            return true;
        }

        protected override void AssignArguments(Flow flow, bool isOpen)
        {
            flow.SetValue(this.isOpen, isOpen);
        }
    }

    [UnitTitle("Spatial: On Backpack Item Owned Changed")]
    [UnitSurtitle("Spatial")]
    [UnitShortTitle("On Backpack Item Owned Changed")]
    [UnitCategory("Events\\Spatial")]
    [TypeIcon(typeof(SpatialComponentBase))]
    public class OnBackpackItemOwnedChangedNode : EventUnit<(string, bool)>
    {
        public static string eventName = "SpatialOnBackpackItemOwnedChanged";
        protected override bool register => true;

        [DoNotSerialize]
        public ValueInput itemID { get; private set; }

        [DoNotSerialize]
        public ValueOutput isOwned { get; private set; }

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(eventName);
        }

        protected override void Definition()
        {
            base.Definition();
            itemID = ValueInput<string>(nameof(itemID), "");
            isOwned = ValueOutput<bool>(nameof(isOwned));
        }

        protected override bool ShouldTrigger(Flow flow, (string, bool) args)
        {
            return flow.GetValue<string>(itemID) == args.Item1;
        }

        protected override void AssignArguments(Flow flow, (string, bool) args)
        {
            flow.SetValue(isOwned, args.Item2);
        }
    }

    [UnitTitle("Spatial: On Backpack Item Used")]
    [UnitSurtitle("Spatial")]
    [UnitShortTitle("On Backpack Item Used")]
    [UnitCategory("Events\\Spatial")]
    [TypeIcon(typeof(SpatialComponentBase))]
    public class OnBackpackItemUsedNode : EventUnit<string>
    {
        public static string eventName = "SpatialOnBackpackItemUsed";
        protected override bool register => true;

        [DoNotSerialize]
        public ValueInput itemID { get; private set; }

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(eventName);
        }

        protected override void Definition()
        {
            base.Definition();
            itemID = ValueInput<string>(nameof(itemID), "");
        }

        protected override bool ShouldTrigger(Flow flow, string arg)
        {
            return flow.GetValue<string>(itemID) == arg;
        }
    }

    [UnitTitle("Spatial: On Backpack Any Item Used")]
    [UnitSurtitle("Spatial")]
    [UnitShortTitle("On Backpack Any Item Used")]
    [UnitCategory("Events\\Spatial")]
    [TypeIcon(typeof(SpatialComponentBase))]
    public class OnBackpackAnyItemUsedNode : EventUnit<string>
    {
        public static string eventName = "SpatialOnBackpackAnyItemUsed";
        protected override bool register => true;

        [DoNotSerialize]
        public ValueOutput itemID { get; private set; }

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(eventName);
        }

        protected override void Definition()
        {
            base.Definition();
            itemID = ValueOutput<string>(nameof(itemID));
        }

        protected override bool ShouldTrigger(Flow flow, string itemID)
        {
            return true;
        }

        protected override void AssignArguments(Flow flow, string itemID)
        {
            flow.SetValue(this.itemID, itemID);
        }
    }
}

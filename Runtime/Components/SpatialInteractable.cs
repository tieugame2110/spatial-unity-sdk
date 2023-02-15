using UnityEngine;

namespace SpatialSys.UnitySDK
{
    public class SpatialInteractable : SpatialComponentBase
    {
        public enum IconType
        {
            None,
            Charge,
            Couch,
            CrisisAlert,
            DoorFront,
            FitnessCenter,
            Flood,
            Hail,
            LunchDining,
            MusicNote,
            RamenDining,
            Soap,
            Timer,
            Weapon,
        }

        public override string prettyName => "Interactable";
        public override string tooltip => "An object that users can interact with to trigger an event";
        public override string documentationURL => "https://www.notion.so/spatialxr/Interactable-6fd1393ccb75422ba459e9f08ad48bb5";
        public override bool isExperimental => true;

        public string interactText = "Interact";
        public IconType iconType;
        [HideInInspector]
        public Sprite icon;
        public float radius = 5f;

        public SpatialEvent onInteractEvent;
        public SpatialEvent onEnterEvent;
        public SpatialEvent onExitEvent;
    }
}

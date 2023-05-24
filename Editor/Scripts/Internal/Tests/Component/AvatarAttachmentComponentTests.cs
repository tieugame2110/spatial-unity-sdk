using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpatialSys.UnitySDK.Editor
{
    public static class AvatarAttachmentComponentTests
    {
        // Before running validation for publishing or testing we enforce valid setup on all attachments
        // This removes references to assets not used by the attachment (like clips that are referenced but not used)
        public static void EnforceValidSetup(SpatialAvatarAttachment attachment)
        {
            if (attachment == null)
                return;

            UnityEditor.EditorUtility.SetDirty(attachment);

            // AdditionalSlots should not include the primary slot
            attachment.additionalSlots = attachment.additionalSlots & ~attachment.primarySlot.ToSlotMask();

            // If the primary slot is set to Aura, the category must also be set to Aura
            if (attachment.primarySlot == SpatialAvatarAttachment.Slot.Aura)
                attachment.category = SpatialAvatarAttachment.Category.Aura;

            // Skinning ------------------------------------------------------------------------------------------------
            attachment.isSkinnedToHumanoidSkeleton = false; // Feature not yet supported

            // Attach to bone ------------------------------------------------------------------------------------------
            if (!attachment.attachToBoneFeatureAvailable)
                attachment.attachToBone = false;

            // IK Settings ---------------------------------------------------------------------------------------------
            if (!attachment.ikFeatureAvailable)
                attachment.ikTargetsEnabled = false;

            // Clear IK targets; Only targets for slots that are occupied should be set
            SpatialAvatarAttachment.SlotMask occupiedSlots = attachment.occupiedSlots;
            if (!attachment.ikTargetsEnabled || !occupiedSlots.HasFlag(SpatialAvatarAttachment.SlotMask.LeftHand))
                attachment.ikLeftHandTarget = null;
            if (!attachment.ikTargetsEnabled || !occupiedSlots.HasFlag(SpatialAvatarAttachment.SlotMask.RightHand))
                attachment.ikRightHandTarget = null;
            if (!attachment.ikTargetsEnabled || !occupiedSlots.HasFlag(SpatialAvatarAttachment.SlotMask.Feet))
            {
                attachment.ikLeftFootTarget = null;
                attachment.ikRightFootTarget = null;
            }

            // Custom Actions ------------------------------------------------------------------------------------------
            if (!attachment.customActionsFeatureAvailable)
                attachment.customActionsEnabled = false;
            if (!attachment.customActionsEnabled)
                attachment.customActionsCount = 0;

            // Avatar animation overrides ------------------------------------------------------------------------------
            // If override is on, but no animation is actually set, turn off the override
            if (attachment.overrideAvatarAnimations
                && attachment.avatarAnimSettings.AllSettings().All(s => s.Item2.overrideClip == null && s.Item2.overrideClipMale == null))
            {
                attachment.overrideAvatarAnimations = false;
            }

            // If override is off, remove all references to clips set to override
            // We do this so that the assets don't get pulled into the asset bundle
            if (!attachment.overrideAvatarAnimations)
            {
                foreach (var setting in attachment.avatarAnimSettings.AllSettings())
                {
                    setting.Item2.overrideClip = null;
                    setting.Item2.overrideClipMale = null;
                }
            }

            // Avatar curtom actions
            Array.Resize(ref attachment.avatarAnimSettings.customActions, attachment.customActionsCount);

            // Attachment animation settings ---------------------------------------------------------------------------
            if (!attachment.animatorFeatureAvailable)
                attachment.attachmentAnimatorType = SpatialAvatarAttachment.AttachmentAnimatorType.None;

            // Make sure custom action counts are correct
            if (attachment.attachmentAnimatorType == SpatialAvatarAttachment.AttachmentAnimatorType.None
                || attachment.attachmentAnimatorType == SpatialAvatarAttachment.AttachmentAnimatorType.Custom)
            {
                attachment.attachmentAnimClips.customActions = new AnimationClip[0];
            }
            else
            {
                Array.Resize(ref attachment.attachmentAnimClips.customActions, attachment.customActionsCount);
            }

            AssetDatabase.SaveAssets();
        }

        //--------------------------------------------------------------------------------------------------------------
        // Validation methods used by both the validation system and the component editor
        //--------------------------------------------------------------------------------------------------------------

        public delegate bool ComponentValidatationDelegate(SpatialAvatarAttachment attachment, out string message);

        public static bool ValidatePrimarySlotField(SpatialAvatarAttachment attachment, out string message)
        {
            if (attachment.primarySlot == SpatialAvatarAttachment.Slot.None)
            {
                message = "The primary slot is a required field to be set";
                return false;
            }

            message = null;
            return true;
        }

        public static bool ValidateAdditionalSlotsField(SpatialAvatarAttachment attachment, out string message)
        {
            if (attachment.additionalSlots.HasFlag(attachment.primarySlot.ToSlotMask()))
            {
                message = "Additional slot types should not include the primary slot type. Remove the primary slot type from the additional slot types list on the prefab to fix this issue.";
                return false;
            }

            if (attachment.primarySlot != SpatialAvatarAttachment.Slot.Aura && attachment.additionalSlots.HasFlag(SpatialAvatarAttachment.Slot.Aura.ToSlotMask()))
            {
                message = "The aura slot can only be used for aura attachments";
                return false;
            }

            if (attachment.primarySlot == SpatialAvatarAttachment.Slot.Aura && attachment.additionalSlots != SpatialAvatarAttachment.SlotMask.None)
            {
                message = "Aura attachments cannot have additional slots";
                return false;
            }

            message = null;
            return true;
        }

        public static bool ValidateCategoryField(SpatialAvatarAttachment attachment, out string message)
        {
            if (attachment.category == SpatialAvatarAttachment.Category.Unspecified)
            {
                message = "The category is required. Set the category on the prefab to fix this issue.";
                return false;
            }

            if (attachment.primarySlot == SpatialAvatarAttachment.Slot.Aura && attachment.category != SpatialAvatarAttachment.Category.Aura)
            {
                message = "For aura attachments, the category must be set to Aura";
                return false;
            }

            if (attachment.primarySlot != SpatialAvatarAttachment.Slot.Aura && attachment.category == SpatialAvatarAttachment.Category.Aura)
            {
                message = "The aura category can only be used for aura attachments";
                return false;
            }

            message = null;
            return true;
        }

        public static Dictionary<SpatialAvatarAttachment.Slot, HashSet<HumanBodyBones>> VALID_ATTACH_BONE_TARGETS_BY_SLOT = new Dictionary<SpatialAvatarAttachment.Slot, HashSet<HumanBodyBones>> {
            {
                SpatialAvatarAttachment.Slot.None,
                new HashSet<HumanBodyBones> {}
            },
            {
                SpatialAvatarAttachment.Slot.Aura,
                new HashSet<HumanBodyBones> {
                    HumanBodyBones.Hips
                }
            },
            {
                SpatialAvatarAttachment.Slot.LeftHand,
                new HashSet<HumanBodyBones> {
                    HumanBodyBones.LeftShoulder,
                    HumanBodyBones.LeftUpperArm,
                    HumanBodyBones.LeftLowerArm,
                    HumanBodyBones.LeftHand
                }
            },
            {
                SpatialAvatarAttachment.Slot.RightHand,
                new HashSet<HumanBodyBones> {
                    HumanBodyBones.RightShoulder,
                    HumanBodyBones.RightUpperArm,
                    HumanBodyBones.RightLowerArm,
                    HumanBodyBones.RightHand
                }
            },
            {
                SpatialAvatarAttachment.Slot.Feet,
                new HashSet<HumanBodyBones> {
                    HumanBodyBones.Hips,
                    HumanBodyBones.LeftUpperLeg,
                    HumanBodyBones.RightUpperLeg,
                    HumanBodyBones.LeftLowerLeg,
                    HumanBodyBones.RightLowerLeg,
                    HumanBodyBones.LeftFoot,
                    HumanBodyBones.RightFoot
                }
            }
        };

        public static bool ValidateAttachBoneFeatureAvailability(SpatialAvatarAttachment attachment, out string note)
        {
            if (attachment.isSkinnedToHumanoidSkeleton)
            {
                note = "Bone attachment is not available when attachment is skinned to humanoid skeleton";
                return false;
            }

            if (!attachment.attachToBoneFeatureAvailable)
            {
                note = "Bone attachment is not available for this configuration";
                return false;
            }

            note = null;
            return true;
        }

        public static bool ValidateAttachBoneTarget(SpatialAvatarAttachment attachment, out string message)
        {
            if (attachment.primarySlot != SpatialAvatarAttachment.Slot.None)
            {
                if (attachment.attachToBone)
                {
                    HashSet<HumanBodyBones> validAttachBoneTargets = VALID_ATTACH_BONE_TARGETS_BY_SLOT[attachment.primarySlot];
                    if (!validAttachBoneTargets.Contains(attachment.attachBoneTarget))
                    {
                        message = $"The attach bone target is not valid for the primary slot {attachment.primarySlot}";
                        return false;
                    }
                }
            }

            message = null;
            return true;
        }

        public static bool ValidateIKFeatureAvailability(SpatialAvatarAttachment attachment, out string note)
        {
            if (attachment.primarySlot == SpatialAvatarAttachment.Slot.Aura)
            {
                note = "IK setting is not available for aura attachments";
                return false;
            }

            if (attachment.isSkinnedToHumanoidSkeleton)
            {
                note = "IK setting is not available when attachment is skinned to humanoid skeleton";
                return false;
            }

            if (!attachment.ikFeatureAvailable)
            {
                note = "IK setting is not available for this configuration";
                return false;
            }

            note = null;
            return true;
        }

        public static bool ValidateIKTargetsSetting(SpatialAvatarAttachment attachment, out string message)
        {
            if (attachment.ikTargetsEnabled)
            {
                SpatialAvatarAttachment.SlotMask occupiedSlots = attachment.occupiedSlots;
                int numIKTargetsSet = 0;
                if (attachment.ikLeftHandTarget != null)
                    numIKTargetsSet++;
                if (attachment.ikRightHandTarget != null)
                    numIKTargetsSet++;
                if (attachment.ikLeftFootTarget != null)
                    numIKTargetsSet++;
                if (attachment.ikRightFootTarget != null)
                    numIKTargetsSet++;

                if (numIKTargetsSet == 0)
                {
                    message = "At least on IK target must be set when IK targets are enabled";
                    return false;
                }
            }

            message = null;
            return true;
        }

        public static bool ValidateCustomActionsFeatureAvailability(SpatialAvatarAttachment attachment, out string note)
        {
            if (attachment.primarySlot == SpatialAvatarAttachment.Slot.Aura)
            {
                note = "Custom actions are not available for aura attachments";
                return false;
            }

            if (!attachment.customActionsFeatureAvailable)
            {
                note = "Custom actions is not available for this configuration";
                return false;
            }

            note = null;
            return true;
        }

        public static bool ValidateCustomActionsCount(SpatialAvatarAttachment attachment, out string message)
        {
            if (attachment.customActionsFeatureAvailable && attachment.customActionsEnabled)
            {
                if (attachment.customActionsCount == 0)
                {
                    message = "At least one custom action must be set when custom actions are enabled";
                    return false;
                }
            }

            message = null;
            return true;
        }

        public static bool ValidateAvatarAnimSettings(SpatialAvatarAttachment attachment, out string message)
        {
            if (attachment.overrideAvatarAnimations)
            {
                // At least one clip should be set
                if (attachment.avatarAnimSettings.AllSettings().All(s => s.Item2.overrideClip == null))
                {
                    message = "There are no avatar animation override clips set yet";
                    return false;
                }
            }

            message = null;
            return true;
        }

        public static bool ValidateAttachmentAvatarAnimCustomActionConfig(SpatialAvatarAttachment attachment, AttachmentAvatarAnimConfig config, int index, bool isCustomAction, out string message)
        {
            if (isCustomAction)
            {
                // If attachment doesn't come with standard animator, then a custom action clip must be set on the avatar settings
                if (attachment.attachmentAnimatorType != SpatialAvatarAttachment.AttachmentAnimatorType.Standard && config.overrideClip == null)
                {
                    message = $"If custom actions is enabled, a clip must set for custom action {index + 1}";
                    return false;
                }

                // If attachment comes with standard animator, then a custom action clip must be set on either the avatar or the attachment
                if (attachment.attachmentAnimatorType == SpatialAvatarAttachment.AttachmentAnimatorType.Standard
                    && config.overrideClip == null
                    && attachment.attachmentAnimClips.customActions[index] == null)
                {
                    message = $"If custom actions is enabled, a clip must set for custom action {index + 1} either for the avatar, or the attachment";
                    return false;
                }
            }

            message = null;
            return true;
        }

        public static bool ValidateAttachmentAnimatorFeatureAvailability(SpatialAvatarAttachment attachment, out string note)
        {
            if (attachment.isSkinnedToHumanoidSkeleton)
            {
                note = "Animator is not available when attachment is skinned to humanoid skeleton. This is because the attachment will use the Avatar's animator component to animate.";
                return false;
            }

            if (!attachment.animatorFeatureAvailable)
            {
                note = "Animator settings not available for this configuration";
                return false;
            }

            note = null;
            return true;
        }

        public static bool ValidateAttachmentAnimatorShouldExist(SpatialAvatarAttachment attachment, out string message)
        {
            var animator = attachment.GetComponent<Animator>();

            if (!attachment.animatorFeatureAvailable && animator != null)
            {
                message = "Animator component should not exist when the animator feature is turned off";
                return false;
            }

            if (attachment.attachmentAnimatorType == SpatialAvatarAttachment.AttachmentAnimatorType.None && animator != null)
            {
                message = "Animator component should not exist when attachment animator type is set to None";
                return false;
            }

            if (attachment.attachmentAnimatorType == SpatialAvatarAttachment.AttachmentAnimatorType.Standard && animator != null)
            {
                message = "For standard animator, the animator component will be created at runtime with the correct setup. Remove the animator component";
                return false;
            }

            if (attachment.attachmentAnimatorType == SpatialAvatarAttachment.AttachmentAnimatorType.Custom && animator == null)
            {
                message = "There should be an animator component on this object if animator type is set to Custom";
                return false;
            }

            message = null;
            return true;
        }

        //--------------------------------------------------------------------------------------------------------------
        // Validator methods for the package validation system
        //--------------------------------------------------------------------------------------------------------------

        [ComponentTest(typeof(SpatialAvatarAttachment))]
        public static void ValidateEntireComponent(SpatialAvatarAttachment attachment)
        {
            ValidateForValidationSystem(attachment, ValidatePrimarySlotField);
            ValidateForValidationSystem(attachment, ValidateAdditionalSlotsField);
            ValidateForValidationSystem(attachment, ValidateCategoryField);
            ValidateForValidationSystem(attachment, ValidateAttachBoneTarget);
            ValidateForValidationSystem(attachment, ValidateIKTargetsSetting);
            ValidateForValidationSystem(attachment, ValidateCustomActionsCount);
            ValidateForValidationSystem(attachment, ValidateAvatarAnimSettings);

            // ValidateAttachmentAvatarAnimCustomActionConfig
            if (attachment.customActionsFeatureAvailable && attachment.customActionsEnabled)
            {
                for (int i = 0; i < attachment.avatarAnimSettings.customActions.Length; i++)
                {
                    bool isValid = ValidateAttachmentAvatarAnimCustomActionConfig(attachment, attachment.avatarAnimSettings.customActions[i], i, true, out string message);
                    if (!isValid)
                        SpatialValidator.AddResponse(new SpatialTestResponse(attachment, TestResponseType.Fail, message));
                }
            }

            ValidateForValidationSystem(attachment, ValidateAttachmentAnimatorShouldExist);
        }

        private static void ValidateForValidationSystem(SpatialAvatarAttachment attachment, ComponentValidatationDelegate validatationDelegate)
        {
            if (!validatationDelegate(attachment, out string message))
            {
                SpatialValidator.AddResponse(new SpatialTestResponse(
                    attachment,
                    TestResponseType.Fail,
                    message
                ));
            }
        }
    }
}

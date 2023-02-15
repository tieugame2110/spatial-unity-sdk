using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;

namespace SpatialSys.UnitySDK.Editor
{
    public class SpatialPointOfInterestTests
    {
        [ComponentTest(typeof(SpatialPointOfInterest))]
        public static void CheckForTitle(SpatialPointOfInterest target)
        {
            if (string.IsNullOrEmpty(target.title))
            {
                SpatialValidator.AddResponse(new SpatialTestResponse(
                    target,
                    TestResponseType.Fail,
                    "Point of interest does not have a title."
                ));
            }
        }
    }
}

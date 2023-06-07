// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace ICVR
{

    public enum TouchEventType
    {
        Touch,
        Pointer,
    }

    /// <summary>
    /// Base class for all NearInteractionTouchables.
    /// </summary>
    /// <remarks>
    /// Add this component to objects to raise touch events when in [PokePointer](xref:Microsoft.MixedReality.Toolkit.Input.PokePointer) proximity.
    /// The object layer must be included of the [PokeLayerMasks](xref:Microsoft.MixedReality.Toolkit.Input.PokePointer.PokeLayerMasks).
    /// </remarks>
    public abstract class BaseTouchable : MonoBehaviour
    {
        [SerializeField]
        protected TouchEventType eventsToReceive = TouchEventType.Touch;

        /// <summary>
        /// The type of event to receive.
        /// </summary>
        public TouchEventType EventsToReceive { get => eventsToReceive; set => eventsToReceive = value; }

        [Tooltip("Distance in front of the surface at which you will receive a touch completed event")]
        [SerializeField]
        protected float debounceThreshold = 0.01f;
        /// <summary>
        /// Distance in front of the surface at which you will receive a touch completed event.
        /// </summary>
        /// <remarks>
        /// When the touchable is active and the pointer distance becomes greater than +DebounceThreshold (i.e. in front of the surface),
        /// then the Touch Completed event is raised and the touchable object is released by the pointer.
        /// </remarks>
        public float DebounceThreshold { get => debounceThreshold; set => debounceThreshold = value; }

        protected virtual void OnValidate()
        {
            debounceThreshold = Math.Max(debounceThreshold, 0);
        }

        public abstract float DistanceToTouchable(Vector3 samplePoint, out Vector3 normal);
    }

    public abstract class TouchableSurface : BaseTouchable
    {
        /// <summary>
        /// The local center point of interaction.  This may be based on a collider position or Unity UI RectTransform.
        /// </summary>
        public abstract Vector3 LocalCenter { get; }

        /// <summary>
        /// This is the direction that a user will press on this element.
        /// </summary>
        public abstract Vector3 LocalPressDirection { get; }

        /// <summary>
        /// Bounds specify where touchable interactions can occur.  They are local bounds on the plane specified by the LocalCenter and LocalPressDirection (as a normal).
        /// </summary>
        public abstract Vector2 Bounds { get; }
    }

}

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.VisualTree;

namespace DialogHost {
    static class PopupPositionerExtensions
    {
        public static void ConfigurePosition(ref this PopupPositionerParameters positionerParameters,
            TopLevel topLevel,
            IVisual target, PlacementMode placement, Point offset,
            PopupAnchor anchor, PopupGravity gravity,
            PopupPositionerConstraintAdjustment constraintAdjustment, Rect? rect)
        {
            // We need a better way for tracking the last pointer position
            var pointer = topLevel.PointToClient(topLevel.PlatformImpl.MouseDevice.Position);
            
            positionerParameters.Offset = offset;
            positionerParameters.ConstraintAdjustment = constraintAdjustment;
            if (placement == PlacementMode.Pointer)
            {
                positionerParameters.AnchorRectangle = new Rect(pointer, new Size(1, 1));
                positionerParameters.Anchor = PopupAnchor.TopLeft;
                positionerParameters.Gravity = PopupGravity.BottomRight;
            }
            else
            {
                if (target == null)
                    throw new InvalidOperationException("Placement mode is not Pointer and PlacementTarget is null");
                var matrix = target.TransformToVisual(topLevel);
                if (matrix == null)
                {
                    if (target.GetVisualRoot() == null)
                        throw new InvalidOperationException("Target control is not attached to the visual tree");
                    throw new InvalidOperationException("Target control is not in the same tree as the popup parent");
                }

                var bounds = new Rect(default, target.Bounds.Size);
                var anchorRect = rect ?? bounds;
                positionerParameters.AnchorRectangle = anchorRect.Intersect(bounds).TransformToAABB(matrix.Value);

                if (placement == PlacementMode.Right)
                {
                    positionerParameters.Anchor = PopupAnchor.TopRight;
                    positionerParameters.Gravity = PopupGravity.BottomRight;
                }
                else if (placement == PlacementMode.Bottom)
                {
                    positionerParameters.Anchor = PopupAnchor.BottomLeft;
                    positionerParameters.Gravity = PopupGravity.BottomRight;
                }
                else if (placement == PlacementMode.Left)
                {
                    positionerParameters.Anchor = PopupAnchor.TopLeft;
                    positionerParameters.Gravity = PopupGravity.BottomLeft;
                }
                else if (placement == PlacementMode.Top)
                {
                    positionerParameters.Anchor = PopupAnchor.TopLeft;
                    positionerParameters.Gravity = PopupGravity.TopRight;
                }
                else if (placement == PlacementMode.AnchorAndGravity)
                {
                    positionerParameters.Anchor = anchor;
                    positionerParameters.Gravity = gravity;
                }
                else
                    throw new InvalidOperationException("Invalid value for Popup.PlacementMode");
            }
        }
    }
}
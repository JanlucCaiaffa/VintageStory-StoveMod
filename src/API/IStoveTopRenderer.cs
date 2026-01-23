using System;
using Vintagestory.API.MathTools;

namespace StoveMod.API
{
    /// <summary>
    /// Custom renderer for cooking containers on the stove top.
    /// Implement this interface to provide custom rendering with lid animation and meal overlay.
    /// 
    /// Lifecycle:
    /// - Created when container is placed on stove (client-side only)
    /// - OnUpdate() called each frame with current temperature
    /// - OnCookingComplete() called when cooking finishes (item moves to output)
    /// - Dispose() called when container removed, BE unloaded, or chunk unloaded
    /// </summary>
    public interface IStoveTopRenderer : IDisposable
    {
        /// <summary>
        /// Called each frame with the current stove temperature.
        /// Use this to update lid wobble animation intensity and cooking sounds.
        /// </summary>
        /// <param name="temperature">Current stove temperature in Celsius</param>
        void OnUpdate(float temperature);

        /// <summary>
        /// Called when cooking completes and the container moves to output slot.
        /// Use this to transition from cooking animation to cooked/meal display.
        /// </summary>
        void OnCookingComplete();

        /// <summary>
        /// Called each frame to render the container meshes.
        /// Render pot, lid (with animation), and meal/contents overlay as needed.
        /// </summary>
        /// <param name="stovePos">Block position of the stove</param>
        /// <param name="partialTicks">Interpolation factor for smooth animation (0-1)</param>
        void Render(BlockPos stovePos, float partialTicks);
    }
}

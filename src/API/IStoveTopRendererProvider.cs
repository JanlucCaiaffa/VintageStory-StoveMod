using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace StoveMod.API
{
    /// <summary>
    /// Implement this interface on your pot/container Block or Item class to provide
    /// a custom renderer when placed on the stove.
    /// 
    /// INTEGRATION EXAMPLE (on your pot block):
    /// <code>
    /// public class BlockMetalPot : Block, IStoveTopRendererProvider
    /// {
    ///     public IStoveTopRenderer CreateStoveTopRenderer(
    ///         ICoreClientAPI capi,
    ///         ItemStack containerStack,
    ///         BlockEntity stoveBE,
    ///         bool forOutputSlot)
    ///     {
    ///         return new MetalPotStoveRenderer(capi, containerStack, stoveBE.Pos, forOutputSlot);
    ///     }
    /// }
    /// </code>
    /// </summary>
    public interface IStoveTopRendererProvider
    {
        /// <summary>
        /// Creates a custom renderer for this container when placed on the stove.
        /// Called client-side only when the container is placed in input or output slot.
        /// </summary>
        /// <param name="capi">Client API for mesh loading, rendering, etc.</param>
        /// <param name="containerStack">The container ItemStack being rendered</param>
        /// <param name="stoveBE">The stove BlockEntity (use for position, orientation, etc.)</param>
        /// <param name="forOutputSlot">True if container is in output slot (cooked), false if in input (cooking)</param>
        /// <returns>Custom renderer instance, or null to use fallback rendering</returns>
        IStoveTopRenderer CreateStoveTopRenderer(
            ICoreClientAPI capi,
            ItemStack containerStack,
            BlockEntity stoveBE,
            bool forOutputSlot
        );
    }
}

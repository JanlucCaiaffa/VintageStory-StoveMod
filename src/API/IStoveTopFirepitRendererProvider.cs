using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace StoveMod.API
{
    /// <summary>
    /// Compatibility interface for mods that already have an IInFirepitRenderer.
    /// Implement this on your pot/container Block or Item to reuse your existing firepit renderer.
    /// 
    /// The stove will wrap your IInFirepitRenderer in an adapter that handles rendering.
    /// Your renderer should also implement IProvidesStoveMeshes for optimal integration.
    /// 
    /// INTEGRATION EXAMPLE:
    /// <code>
    /// public class BlockMetalPot : Block, IStoveTopFirepitRendererProvider
    /// {
    ///     public IInFirepitRenderer CreateFirepitStyleRenderer(
    ///         ICoreClientAPI capi,
    ///         ItemStack containerStack,
    ///         BlockEntity stoveBE,
    ///         bool forOutputSlot)
    ///     {
    ///         // Return your existing firepit renderer
    ///         return new MetalPotInFirepitRenderer(capi, containerStack, forOutputSlot);
    ///     }
    /// }
    /// 
    /// // On your renderer class, implement IProvidesStoveMeshes:
    /// public class MetalPotInFirepitRenderer : IInFirepitRenderer, IProvidesStoveMeshes
    /// {
    ///     public MeshRef PotMeshRef => potMeshRef;
    ///     public MeshRef ContentMeshRef => contentMeshRef;
    ///     public MeshRef LidMeshRef => lidMeshRef;
    ///     public float LidOffsetY => lidOffsetY;
    ///     public float LidWobbleAngle => currentWobbleAngle;
    ///     // ... rest of implementation
    /// }
    /// </code>
    /// </summary>
    public interface IStoveTopFirepitRendererProvider
    {
        /// <summary>
        /// Creates a firepit-style renderer for this container when placed on the stove.
        /// Called client-side only.
        /// </summary>
        /// <param name="capi">Client API</param>
        /// <param name="containerStack">The container ItemStack</param>
        /// <param name="stoveBE">The stove BlockEntity</param>
        /// <param name="forOutputSlot">True if in output slot (cooked)</param>
        /// <returns>IInFirepitRenderer instance, or null to use fallback</returns>
        IInFirepitRenderer CreateFirepitStyleRenderer(
            ICoreClientAPI capi,
            ItemStack containerStack,
            BlockEntity stoveBE,
            bool forOutputSlot
        );
    }
}

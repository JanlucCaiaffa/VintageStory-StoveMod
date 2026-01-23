using Vintagestory.API.Client;
using Vintagestory.API.Common;
using StoveMod.API;

namespace StoveMod
{
    /// <summary>
    /// Main mod system for StoveMod.
    /// 
    /// OTHER MODS CAN INTEGRATE CUSTOM RENDERERS IN THREE WAYS:
    /// 
    /// OPTION 1: Implement IStoveTopRendererProvider on your pot Block class
    /// <code>
    /// public class BlockMetalPot : Block, IStoveTopRendererProvider
    /// {
    ///     public IStoveTopRenderer CreateStoveTopRenderer(
    ///         ICoreClientAPI capi, ItemStack stack, BlockEntity stoveBE, bool forOutput)
    ///     {
    ///         return new MetalPotStoveRenderer(capi, stack, stoveBE.Pos, forOutput);
    ///     }
    /// }
    /// </code>
    /// 
    /// OPTION 2: Register via registry in your ModSystem.StartClientSide()
    /// <code>
    /// var stoveSystem = api.ModLoader.GetModSystem&lt;StoveMod.StoveModSystem&gt;();
    /// stoveSystem.RendererRegistry.RegisterProvider(
    ///     stack => stack.Collectible?.Code?.Path?.StartsWith("metalpot-") == true,
    ///     (capi, stack, stoveBE, forOutput) => new MetalPotStoveRenderer(capi, stack, stoveBE.Pos, forOutput)
    /// );
    /// </code>
    /// 
    /// OPTION 3 (Compatibility): Reuse existing IInFirepitRenderer
    /// <code>
    /// // On your pot block:
    /// public class BlockMetalPot : Block, IStoveTopFirepitRendererProvider
    /// {
    ///     public IInFirepitRenderer CreateFirepitStyleRenderer(
    ///         ICoreClientAPI capi, ItemStack stack, BlockEntity stoveBE, bool forOutput)
    ///     {
    ///         return new MetalPotInFirepitRenderer(capi, stack, forOutput);
    ///     }
    /// }
    /// 
    /// // Make your renderer implement IProvidesStoveMeshes for best results:
    /// public class MetalPotInFirepitRenderer : IInFirepitRenderer, IProvidesStoveMeshes
    /// {
    ///     public MeshRef PotMeshRef => potMeshRef;
    ///     public MeshRef ContentMeshRef => contentMeshRef;
    ///     public MeshRef LidMeshRef => lidMeshRef;
    ///     public float LidOffsetY => 5.5f / 16f;
    ///     public float LidWobbleAngle => wobbleAngle;
    /// }
    /// </code>
    /// </summary>
    public class StoveModSystem : ModSystem
    {
        /// <summary>
        /// Registry for custom stove top renderer providers.
        /// Use this in your ModSystem.StartClientSide() to register renderers.
        /// </summary>
        public StoveRendererRegistry RendererRegistry { get; private set; }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("BlockStove", typeof(BlockStove));
            api.RegisterBlockEntityClass("Stove", typeof(BlockEntityStove));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            RendererRegistry = new StoveRendererRegistry();
            RendererRegistry.Initialize(api);
        }

        public override void Dispose()
        {
            RendererRegistry?.Clear();
            RendererRegistry = null;
            base.Dispose();
        }
    }
}

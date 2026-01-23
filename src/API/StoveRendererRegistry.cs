using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace StoveMod.API
{
    /// <summary>
    /// Registry for stove top renderer providers.
    /// Use this to register custom renderers without modifying pot block classes.
    /// 
    /// INTEGRATION EXAMPLE (in your ModSystem.StartClientSide):
    /// <code>
    /// var registry = api.ModLoader.GetModSystem&lt;StoveModSystem&gt;().RendererRegistry;
    /// 
    /// // Option 1: Register IStoveTopRenderer factory
    /// registry.RegisterProvider(
    ///     stack => stack.Collectible?.Code?.Path?.StartsWith("metalpot-") == true,
    ///     (capi, stack, stoveBE, forOutput) => new MetalPotStoveRenderer(capi, stack, stoveBE.Pos, forOutput)
    /// );
    /// 
    /// // Option 2: Register IInFirepitRenderer factory (wrapped automatically)
    /// registry.RegisterFirepitProvider(
    ///     stack => stack.Collectible?.Code?.Path?.StartsWith("metalpot-") == true,
    ///     (capi, stack, stoveBE, forOutput) => new MetalPotInFirepitRenderer(capi, stack, forOutput)
    /// );
    /// </code>
    /// </summary>
    public class StoveRendererRegistry
    {
        readonly List<(System.Func<ItemStack, bool> matches, System.Func<ICoreClientAPI, ItemStack, BlockEntity, bool, IStoveTopRenderer> factory)> providers = new();
        readonly List<(System.Func<ItemStack, bool> matches, System.Func<ICoreClientAPI, ItemStack, BlockEntity, bool, IInFirepitRenderer> factory)> firepitProviders = new();

        ICoreClientAPI capi;

        internal void Initialize(ICoreClientAPI capi)
        {
            this.capi = capi;
        }

        /// <summary>
        /// Register a custom stove top renderer provider.
        /// First matching registration wins.
        /// </summary>
        /// <param name="matches">Predicate to check if this provider handles the given ItemStack</param>
        /// <param name="factory">Factory function to create the renderer</param>
        public void RegisterProvider(
            System.Func<ItemStack, bool> matches,
            System.Func<ICoreClientAPI, ItemStack, BlockEntity, bool, IStoveTopRenderer> factory)
        {
            if (matches == null) throw new ArgumentNullException(nameof(matches));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            providers.Add((matches, factory));
        }

        /// <summary>
        /// Register an IInFirepitRenderer provider (for compatibility with existing firepit renderers).
        /// The stove will wrap the returned renderer in a FirepitRendererAdapter.
        /// First matching registration wins.
        /// </summary>
        /// <param name="matches">Predicate to check if this provider handles the given ItemStack</param>
        /// <param name="factory">Factory function to create the IInFirepitRenderer</param>
        public void RegisterFirepitProvider(
            System.Func<ItemStack, bool> matches,
            System.Func<ICoreClientAPI, ItemStack, BlockEntity, bool, IInFirepitRenderer> factory)
        {
            if (matches == null) throw new ArgumentNullException(nameof(matches));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            firepitProviders.Add((matches, factory));
        }

        /// <summary>
        /// Try to create a renderer for the given container stack.
        /// Checks in order: IStoveTopRendererProvider on collectible, registry providers,
        /// IStoveTopFirepitRendererProvider on collectible, registry firepit providers.
        /// </summary>
        internal IStoveTopRenderer TryCreateRenderer(ItemStack containerStack, BlockEntity stoveBE, bool forOutputSlot)
        {
            if (containerStack == null || capi == null) return null;

            var collectible = containerStack.Collectible;
            if (collectible == null) return null;

            if (collectible is IStoveTopRendererProvider nativeProvider)
            {
                var renderer = nativeProvider.CreateStoveTopRenderer(capi, containerStack, stoveBE, forOutputSlot);
                if (renderer != null) return renderer;
            }

            foreach (var (matches, factory) in providers)
            {
                try
                {
                    if (matches(containerStack))
                    {
                        var renderer = factory(capi, containerStack, stoveBE, forOutputSlot);
                        if (renderer != null) return renderer;
                    }
                }
                catch { }
            }

            if (collectible is IStoveTopFirepitRendererProvider firepitProvider)
            {
                var fpRenderer = firepitProvider.CreateFirepitStyleRenderer(capi, containerStack, stoveBE, forOutputSlot);
                if (fpRenderer != null)
                {
                    return new FirepitRendererAdapter(capi, fpRenderer, stoveBE.Pos);
                }
            }

            foreach (var (matches, factory) in firepitProviders)
            {
                try
                {
                    if (matches(containerStack))
                    {
                        var fpRenderer = factory(capi, containerStack, stoveBE, forOutputSlot);
                        if (fpRenderer != null)
                        {
                            return new FirepitRendererAdapter(capi, fpRenderer, stoveBE.Pos);
                        }
                    }
                }
                catch { }
            }

            return null;
        }

        internal void Clear()
        {
            providers.Clear();
            firepitProviders.Clear();
        }
    }
}

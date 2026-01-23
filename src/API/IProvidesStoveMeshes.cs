using Vintagestory.API.Client;

namespace StoveMod.API
{
    /// <summary>
    /// Optional interface for IInFirepitRenderer implementations to expose their mesh refs.
    /// Implement this to allow the stove's FirepitRendererAdapter to render your meshes correctly.
    /// 
    /// If your renderer does not implement this, the adapter will attempt to use reflection
    /// to find common field names (potMeshRef, contentMeshRef, lidMeshRef), but implementing
    /// this interface is preferred for reliability.
    /// 
    /// EXAMPLE:
    /// <code>
    /// public class MetalPotInFirepitRenderer : IInFirepitRenderer, IProvidesStoveMeshes
    /// {
    ///     MeshRef potMeshRef, contentMeshRef, lidMeshRef;
    ///     float lidY, wobbleAngle;
    ///     
    ///     // IProvidesStoveMeshes implementation
    ///     public MeshRef PotMeshRef => potMeshRef;
    ///     public MeshRef ContentMeshRef => contentMeshRef;
    ///     public MeshRef LidMeshRef => lidMeshRef;
    ///     public float LidOffsetY => lidY;
    ///     public float LidWobbleAngle => wobbleAngle;
    /// }
    /// </code>
    /// </summary>
    public interface IProvidesStoveMeshes
    {
        /// <summary>Main pot/container mesh. Required.</summary>
        MeshRef PotMeshRef { get; }

        /// <summary>Meal/contents overlay mesh. Optional, can return null.</summary>
        MeshRef ContentMeshRef { get; }

        /// <summary>Lid mesh for animation. Optional, can return null.</summary>
        MeshRef LidMeshRef { get; }

        /// <summary>Vertical offset for lid above pot (in blocks). Default ~0.34 (5.5/16).</summary>
        float LidOffsetY { get; }

        /// <summary>Current lid wobble angle in radians for animation.</summary>
        float LidWobbleAngle { get; }
    }
}

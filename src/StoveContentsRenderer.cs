using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace StoveMod
{
    public class StoveContentsRenderer : IRenderer
    {
        ICoreClientAPI capi;
        BlockPos pos;
        
        MultiTextureMeshRef potRef;
        MultiTextureMeshRef lidRef;
        MultiTextureMeshRef potWithFoodRef;
        
        public ItemStack ContentStack;
        
        public bool IsInOutputSlot;
        
        float temp;
        
        public string Orientation = "south";
        
        ILoadedSound cookingSound;
        
        Matrixf ModelMat = new Matrixf();

        public double RenderOrder => 0.5;
        public int RenderRange => 48;

        public StoveContentsRenderer(ICoreClientAPI capi, BlockPos pos)
        {
            this.capi = capi;
            this.pos = pos;
        }
        
        private void GetOrientedOffset(out float xOffset, out float zOffset)
        {
            float offset = 0.5f / 16f;
            switch (Orientation)
            {
                case "north":
                    xOffset = 0;
                    zOffset = -offset;
                    break;
                case "south":
                    xOffset = 0;
                    zOffset = offset;
                    break;
                case "east":
                    xOffset = offset;
                    zOffset = 0;
                    break;
                case "west":
                    xOffset = -offset;
                    zOffset = 0;
                    break;
                default:
                    xOffset = 0;
                    zOffset = offset;
                    break;
            }
        }

        public void SetContents(ItemStack contentStack, bool isInOutputSlot)
        {
            potRef?.Dispose();
            potRef = null;
            lidRef?.Dispose();
            lidRef = null;
            potWithFoodRef = null;
            
            ContentStack = contentStack;
            IsInOutputSlot = isInOutputSlot;
            
            if (contentStack == null) return;
            
            BlockCookedContainer potBlock = null;
            
            if (contentStack.Collectible is BlockCookedContainer bcc)
            {
                potBlock = bcc;
            }
            else if (contentStack.Collectible is BlockCookingContainer cookingContainer)
            {
                potBlock = capi.World.GetBlock(cookingContainer.CodeWithVariant("type", "cooked")) as BlockCookedContainer;
            }
            
            if (potBlock == null) return;
            
            if (isInOutputSlot)
            {
                MealMeshCache meshcache = capi.ModLoader.GetModSystem<MealMeshCache>();
                potWithFoodRef = meshcache.GetOrCreateMealInContainerMeshRef(
                    potBlock, 
                    potBlock.GetCookingRecipe(capi.World, contentStack), 
                    potBlock.GetNonEmptyContents(capi.World, contentStack), 
                    new Vec3f(0, 2.5f/16f, 0)
                );
            }
            else
            {
                Shape potShape = Vintagestory.API.Common.Shape.TryGet(capi, "shapes/block/clay/pot-opened-empty.json");
                if (potShape != null)
                {
                    capi.Tesselator.TesselateShape(potBlock, potShape, out MeshData potMesh);
                    potRef = capi.Render.UploadMultiTextureMesh(potMesh);
                }
                
                Shape lidShape = Vintagestory.API.Common.Shape.TryGet(capi, "shapes/block/clay/pot-part-lid.json");
                if (lidShape != null)
                {
                    capi.Tesselator.TesselateShape(potBlock, lidShape, out MeshData lidMesh);
                    lidRef = capi.Render.UploadMultiTextureMesh(lidMesh);
                }
            }
        }

        public void ClearContents()
        {
            potRef?.Dispose();
            potRef = null;
            lidRef?.Dispose();
            lidRef = null;
            potWithFoodRef = null;
            ContentStack = null;
            IsInOutputSlot = false;
            SetCookingSoundVolume(0);
        }

        public void OnUpdate(float temperature)
        {
            temp = temperature;
            
            float soundIntensity = GameMath.Clamp((temp - 50) / 50, 0, 1);
            SetCookingSoundVolume(IsInOutputSlot ? 0 : soundIntensity);
        }

        public void OnCookingComplete()
        {
            IsInOutputSlot = true;
            SetCookingSoundVolume(0);
        }
        
        bool disposed = false;

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            if (ContentStack == null || disposed) return;
            
            IRenderAPI rpi = capi.Render;
            Vec3d camPos = capi.World.Player.Entity.CameraPos;
            
            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);
            
            IStandardShaderProgram prog = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            prog.Use();
            prog.DontWarpVertices = 0;
            prog.AddRenderFlags = 0;
            prog.RgbaAmbientIn = rpi.AmbientColor;
            prog.RgbaFogIn = rpi.FogColor;
            prog.FogMinIn = rpi.FogMin;
            prog.FogDensityIn = rpi.FogDensity;
            prog.RgbaTint = ColorUtil.WhiteArgbVec;
            prog.ExtraGlow = 0;
            
            float heightOffset = 14f / 16f;
            GetOrientedOffset(out float xOffset, out float zOffset);
            
            if (IsInOutputSlot && potWithFoodRef != null && !potWithFoodRef.Disposed)
            {
                prog.ModelMatrix = ModelMat
                    .Identity()
                    .Translate(pos.X - camPos.X + xOffset, pos.Y - camPos.Y + heightOffset, pos.Z - camPos.Z + zOffset)
                    .Values;
                prog.ViewMatrix = rpi.CameraMatrixOriginf;
                prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                
                rpi.RenderMultiTextureMesh(potWithFoodRef, "tex");
            }
            else if (!IsInOutputSlot)
            {
                if (potRef != null && !potRef.Disposed)
                {
                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X + xOffset, pos.Y - camPos.Y + heightOffset, pos.Z - camPos.Z + zOffset)
                        .Values;
                    prog.ViewMatrix = rpi.CameraMatrixOriginf;
                    prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                    
                    rpi.RenderMultiTextureMesh(potRef, "tex");
                }
                
                if (lidRef != null && !lidRef.Disposed)
                {
                    float cookIntensity = GameMath.Clamp((temp - 50) / 50, 0, 1);
                    
                    float origx = GameMath.Sin(capi.World.ElapsedMilliseconds / 300f) * 5 / 16f;
                    float origz = GameMath.Cos(capi.World.ElapsedMilliseconds / 300f) * 5 / 16f;
                    
                    prog.ModelMatrix = ModelMat
                        .Identity()
                        .Translate(pos.X - camPos.X + xOffset, pos.Y - camPos.Y + heightOffset, pos.Z - camPos.Z + zOffset)
                        .Translate(0, 5.5f / 16f, 0)
                        .Translate(-origx, 0, -origz)
                        .RotateX(cookIntensity * GameMath.Sin(capi.World.ElapsedMilliseconds / 50f) / 60)
                        .RotateZ(cookIntensity * GameMath.Sin(capi.World.ElapsedMilliseconds / 50f) / 60)
                        .Translate(origx, 0, origz)
                        .Values;
                    prog.ViewMatrix = rpi.CameraMatrixOriginf;
                    prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                    
                    rpi.RenderMultiTextureMesh(lidRef, "tex");
                }
            }
            
            prog.Stop();
        }

        void SetCookingSoundVolume(float volume)
        {
            if (volume > 0)
            {
                if (cookingSound == null)
                {
                    cookingSound = capi.World.LoadSound(new SoundParams()
                    {
                        Location = new AssetLocation("sounds/effect/cooking.ogg"),
                        ShouldLoop = true,
                        Position = pos.ToVec3f().Add(0.5f, 0.5f, 0.5f),
                        DisposeOnFinish = false,
                        Volume = volume,
                        Range = 8
                    });
                    cookingSound?.Start();
                }
                else
                {
                    cookingSound.SetVolume(volume);
                }
            }
            else
            {
                cookingSound?.Stop();
                cookingSound?.Dispose();
                cookingSound = null;
            }
        }

        public void Dispose()
        {
            disposed = true;
            ContentStack = null;
            potRef?.Dispose();
            potRef = null;
            lidRef?.Dispose();
            lidRef = null;
            potWithFoodRef = null;
            cookingSound?.Stop();
            cookingSound?.Dispose();
            cookingSound = null;
        }
    }
}

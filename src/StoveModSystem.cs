using Vintagestory.API.Common;

namespace StoveMod
{
    public class StoveModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("BlockStove", typeof(BlockStove));
            api.RegisterBlockEntityClass("Stove", typeof(BlockEntityStove));
        }
    }
}

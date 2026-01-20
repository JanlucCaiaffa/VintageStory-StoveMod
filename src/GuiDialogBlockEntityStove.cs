using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Datastructures;
using Cairo;

namespace StoveMod
{
    public class GuiDialogBlockEntityStove : GuiDialogBlockEntity
    {
        BlockEntityStove stove;
        bool haveCookingContainer;
        string currentOutputText;
        ElementBounds cookingSlotsSlotBounds;
        long lastRedrawMs;
        EnumPosFlag screenPos;
        
        public new ITreeAttribute Attributes { get; private set; }
        
        protected override double FloatyDialogPosition => 0.6;
        protected override double FloatyDialogAlign => 0.8;
        public override double DrawOrder => 0.2;

        public GuiDialogBlockEntityStove(string dlgTitle, InventoryBase inventory, BlockPos bePos, 
            SyncedTreeAttribute tree, ICoreClientAPI capi, BlockEntityStove stove)
            : base(dlgTitle, inventory, bePos, capi)
        {
            if (IsDuplicate) return;
            
            this.stove = stove;
            this.Attributes = tree;
            
            tree.OnModified.Add(new TreeModifiedListener() { listener = OnAttributesModified });
        }

        private void OnInventorySlotModified(int slotid)
        {
            capi.Event.EnqueueMainThreadTask(SetupDialog, "setupstovegui");
        }
        
        private void OnAttributesModified()
        {
            if (!IsOpened()) return;
            
            float ftemp = Attributes.GetFloat("furnaceTemperature");
            float otemp = Attributes.GetFloat("oreTemperature");
            bool isBurning = Attributes.GetFloat("fuelBurnTime") > 0;
            
            string fuelTemp = "";
            if (isBurning || ftemp > 60)
            {
                fuelTemp = ftemp.ToString("#");
                fuelTemp += fuelTemp.Length > 0 ? "°C" : "";
                if (ftemp > 0 && ftemp <= 20) fuelTemp = Lang.Get("Cold");
            }
            
            string oreTemp = otemp.ToString("#");
            oreTemp += oreTemp.Length > 0 ? "°C" : "";
            if (otemp > 0 && otemp <= 20) oreTemp = Lang.Get("Cold");
            
            SingleComposer?.GetDynamicText("fueltemp")?.SetNewText(fuelTemp);
            SingleComposer?.GetDynamicText("oretemp")?.SetNewText(oreTemp);
            
            if (capi.ElapsedMilliseconds - lastRedrawMs > 500)
            {
                SingleComposer?.GetCustomDraw("symbolDrawer")?.Redraw();
                lastRedrawMs = capi.ElapsedMilliseconds;
            }
        }

        void SetupDialog()
        {
            ItemSlot hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory?.InventoryID != Inventory?.InventoryID)
            {
                hoveredSlot = null;
            }

            string newOutputText = Attributes.GetString("outputText", "");
            bool newHaveCookingContainer = Attributes.GetInt("haveCookingContainer") > 0;
            
            if (haveCookingContainer == newHaveCookingContainer && SingleComposer != null)
            {
                var existingOutputElem = SingleComposer.GetDynamicText("outputText");
                if (existingOutputElem != null)
                {
                    existingOutputElem.Font.WithFontSize(14);
                    existingOutputElem.SetNewText(newOutputText, true);
                }
                SingleComposer.GetCustomDraw("symbolDrawer")?.Redraw();
                haveCookingContainer = newHaveCookingContainer;
                currentOutputText = newOutputText;
                return;
            }
            
            haveCookingContainer = newHaveCookingContainer;
            currentOutputText = newOutputText;
            int qCookingSlots = Attributes.GetInt("quantityCookingSlots");

            ElementBounds stoveBounds = ElementBounds.Fixed(0, 0, 210, 250);
            
            cookingSlotsSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 30 + 45, 4, qCookingSlots / 4);
            cookingSlotsSlotBounds.fixedHeight += 10;
            
            double top = cookingSlotsSlotBounds.fixedHeight + cookingSlotsSlotBounds.fixedY;
            
            ElementBounds inputSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, top, 1, 1);
            ElementBounds fuelSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 110 + top, 1, 1);
            ElementBounds outputSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153, top, 1, 1);

            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(stoveBounds);

            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
                .WithFixedAlignmentOffset(IsRight(screenPos) ? -GuiStyle.DialogToScreenPadding : GuiStyle.DialogToScreenPadding, 0)
                .WithAlignment(IsRight(screenPos) ? EnumDialogArea.RightMiddle : EnumDialogArea.LeftMiddle);

            if (!capi.Settings.Bool["immersiveMouseMode"])
            {
                dialogBounds.fixedOffsetY += (stoveBounds.fixedHeight + 65 + (haveCookingContainer ? 25 : 0)) * YOffsetMul(screenPos);
                dialogBounds.fixedOffsetX += (stoveBounds.fixedWidth + 10) * XOffsetMul(screenPos);
            }

            int[] cookingSlotIds = new int[qCookingSlots];
            for (int i = 0; i < qCookingSlots; i++) cookingSlotIds[i] = 3 + i;

            SingleComposer = capi.Gui
                .CreateCompo("blockentitystove" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddDynamicCustomDraw(stoveBounds, OnBgDraw, "symbolDrawer")
                    .AddDynamicText("", CairoFont.WhiteSmallText().WithFontSize(16), ElementBounds.Fixed(0, 30, 210, 45), "outputText");
            
            if (haveCookingContainer)
            {
                SingleComposer.AddItemSlotGrid(Inventory, SendInvPacket, 4, cookingSlotIds, cookingSlotsSlotBounds, "ingredientSlots");
            }
            
            SingleComposer
                .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, fuelSlotBounds, "fuelslot")
                .AddDynamicText("", CairoFont.WhiteDetailText(), fuelSlotBounds.RightCopy(17, 16).WithFixedSize(60, 30), "fueltemp")
                .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, inputSlotBounds, "inputslot")
                .AddDynamicText("", CairoFont.WhiteDetailText(), inputSlotBounds.RightCopy(23, 16).WithFixedSize(60, 30), "oretemp")
                .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 2 }, outputSlotBounds, "outputslot")
                .EndChildElements()
                .Compose();

            lastRedrawMs = capi.ElapsedMilliseconds;

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }
            
            var outputTextElem = SingleComposer.GetDynamicText("outputText");
            if (outputTextElem != null)
            {
                outputTextElem.SetNewText(currentOutputText, true);
                outputTextElem.Bounds.fixedOffsetY = 0;
                
                if (outputTextElem.QuantityTextLines > 2)
                {
                    outputTextElem.Bounds.fixedOffsetY = -outputTextElem.Font.GetFontExtents().Height / RuntimeEnv.GUIScale * 0.65;
                    outputTextElem.Font.WithFontSize(12);
                    outputTextElem.RecomposeText();
                }
                outputTextElem.Bounds.CalcWorldBounds();
            }
        }

        private void OnBgDraw(Context ctx, ImageSurface surface, ElementBounds currentBounds)
        {
            double top = cookingSlotsSlotBounds.fixedHeight + cookingSlotsSlotBounds.fixedY;

            ctx.Save();
            Matrix m = ctx.Matrix;
            m.Translate(GuiElement.scaled(5), GuiElement.scaled(53 + top));
            m.Scale(GuiElement.scaled(0.25), GuiElement.scaled(0.25));
            ctx.Matrix = m;
            capi.Gui.Icons.DrawFlame(ctx);

            float burnTime = Attributes.GetFloat("fuelBurnTime", 0);
            float maxBurnTime = Attributes.GetFloat("maxFuelBurnTime", 1);
            if (maxBurnTime <= 0) maxBurnTime = 1;
            
            double dy = 210 - 210 * (burnTime / maxBurnTime);
            ctx.Rectangle(0, dy, 200, 210 - dy);
            ctx.Clip();
            LinearGradient gradient = new LinearGradient(0, GuiElement.scaled(250), 0, 0);
            gradient.AddColorStop(0, new Color(1, 1, 0, 1));
            gradient.AddColorStop(1, new Color(1, 0, 0, 1));
            ctx.SetSource(gradient);
            capi.Gui.Icons.DrawFlame(ctx, 0, false, false);
            gradient.Dispose();
            ctx.Restore();

            ctx.Save();
            m = ctx.Matrix;
            m.Translate(GuiElement.scaled(63), GuiElement.scaled(top + 2));
            m.Scale(GuiElement.scaled(0.6), GuiElement.scaled(0.6));
            ctx.Matrix = m;
            capi.Gui.Icons.DrawArrowRight(ctx, 2);

            float cookingTime = Attributes.GetFloat("oreCookingTime", 0);
            float maxCookingTime = Attributes.GetFloat("maxOreCookingTime", 1);
            bool hasValidRecipe = Attributes.GetInt("hasValidRecipe", 0) > 0;
            if (maxCookingTime <= 0) maxCookingTime = 1;
            
            double cookingRel = (hasValidRecipe && cookingTime > 0) ? (cookingTime / maxCookingTime) : 0;

            ctx.Rectangle(5, 0, 125 * cookingRel, 100);
            ctx.Clip();
            gradient = new LinearGradient(0, 0, 200, 0);
            gradient.AddColorStop(0, new Color(0, 0.4, 0, 1));
            gradient.AddColorStop(1, new Color(0.2, 0.6, 0.2, 1));
            ctx.SetSource(gradient);
            capi.Gui.Icons.DrawArrowRight(ctx, 0, false, false);
            gradient.Dispose();
            ctx.Restore();
        }

        private void SendInvPacket(object packet)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, packet);
        }

        private void OnTitleBarClose()
        {
            TryClose();
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            Inventory.SlotModified += OnInventorySlotModified;
            screenPos = GetFreePos("smallblockgui");
            OccupyPos("smallblockgui", screenPos);
            SetupDialog();
        }

        public override void OnGuiClosed()
        {
            Inventory.SlotModified -= OnInventorySlotModified;
            
            SingleComposer?.GetSlotGrid("fuelslot")?.OnGuiClosed(capi);
            SingleComposer?.GetSlotGrid("inputslot")?.OnGuiClosed(capi);
            SingleComposer?.GetSlotGrid("outputslot")?.OnGuiClosed(capi);
            SingleComposer?.GetSlotGrid("ingredientSlots")?.OnGuiClosed(capi);
            
            base.OnGuiClosed();
            FreePos("smallblockgui", screenPos);
        }

        public override string ToggleKeyCombinationCode => null;
    }
}

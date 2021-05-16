using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private const string INVENTORY_ADDITIONAL_TAG = "(Инвентарь)";

        private const string INVENTORY_MAIN_TAG = "(Главный инвентарь)";

        private const string DISPLAY_MAIN_TAG = "(Главный дисплей)";

        /** Все блоки */
        private List<IMyTerminalBlock> blocks;

        /** Главный инвентарь */
        private IMyTerminalBlock inventoryMain;

        /** Главный дисплей */
        private IMyTextPanel displayMain;

        /** Дополнительные инвентари */
        private List<IMyTerminalBlock> inventoryAdditions;

        public Program()
        {
            /** Выполнение программы каждые 100 миллисекунд */
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            /** Получаем все блоки */
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(blocks);

            /** Получаем главный инвентарь */
            inventoryMain = getTerminalBlockByTag(blocks, INVENTORY_MAIN_TAG);

            /** Получаем главный дисплей */
            displayMain = (IMyTextPanel)getTerminalBlockByTag(blocks, DISPLAY_MAIN_TAG);

            if (inventoryMain != null)
            {
                /** Дополнительные инвентари */
                inventoryAdditions = new List<IMyTerminalBlock>();
                foreach (IMyTerminalBlock block in blocks)
                {
                    if (block.CustomName.Contains(INVENTORY_ADDITIONAL_TAG))
                    {
                        inventoryAdditions.Add(block);
                    }
                }
            }

            displayMain.WriteText("");
            displayMain.WriteText($"Дополнительных инвентарей: {inventoryAdditions.Count}шт.\n", true);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            /** Перекладываю шмотки */
            foreach (IMyTerminalBlock block in inventoryAdditions)
            {
                IMyInventory inventoryAdditional = block.GetInventory();
                while (inventoryAdditional.ItemCount > 0)
                {
                    inventoryAdditional.TransferItemTo(inventoryMain.GetInventory(), 0);
                }
            }
        }

        public IMyTerminalBlock getTerminalBlockByTag(List<IMyTerminalBlock> blocks, string tag)
        {
            IMyTerminalBlock block = null;
            foreach (IMyTerminalBlock terminalBlock in blocks)
            {
                if (terminalBlock.CustomName.Contains(tag))
                {
                    block = terminalBlock;
                    break;
                }
            }

            return block;
        }
    }
}
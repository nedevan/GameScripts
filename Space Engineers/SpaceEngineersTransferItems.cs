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
        private IMyTerminalBlock mainInventory;

        /** Главный дисплей */
        private IMyTextPanel mainDisplay;

        /** Дополнительные инвентари */
        private List<IMyTerminalBlock> additionalInventory;

        public Program()
        {
            /** Выполнение программы каждые 100 миллисекунд */
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            /** Получаем все блоки */
            blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(blocks);

            /** Получаем главный инвентарь */
            mainInventory = getTerminalBlockByTag(blocks, INVENTORY_MAIN_TAG);

            /** Получаем главный дисплей */
            mainDisplay = (IMyTextPanel)getTerminalBlockByTag(blocks, DISPLAY_MAIN_TAG);

            if (mainInventory != null)
            {
                /** Дополнительные инвентари */
                additionalInventory = new List<IMyTerminalBlock>();
                foreach (IMyTerminalBlock block in blocks)
                {
                    if (block.CustomName.Contains(INVENTORY_ADDITIONAL_TAG))
                    {
                        additionalInventory.Add(block);
                    }
                }
            }

            if (mainDisplay != null)
            {
                mainDisplay.WriteText("");
                mainDisplay.WriteText($"Дополнительных инвентарей: {additionalInventory.Count}шт.\n", true);
            }
        }

        public void Main(string argument, UpdateType updateSource)
        {
            /** Перекладываю шмотки */
            foreach (IMyTerminalBlock block in additionalInventory)
            {
                IMyInventory inventoryAdditional = block.GetInventory();
                while (inventoryAdditional.ItemCount > 0)
                {
                    inventoryAdditional.TransferItemTo(mainInventory.GetInventory(), 0);
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
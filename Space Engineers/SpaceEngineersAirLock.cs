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
    // API https://github.com/malware-dev/MDK-SE/wiki/Api-Index
    partial class Program : MyGridProgram
    {
        /** Информационная панель */
        private IMyTextPanel panel;

        /** Двери */
        private IMyDoor mainDoor, firstDoor, secondDoor, hangarDoor;

        /** Вентиляция */
        private IMyAirVent mainAirVent, spaceAirVent, hangarAirVent;

        public Program()
        {
            /** Информационная панель */
            panel = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

            /** Двери */
            mainDoor = GridTerminalSystem.GetBlockWithName("MainDoor") as IMyDoor;
            firstDoor = GridTerminalSystem.GetBlockWithName("FirstDoor") as IMyDoor;
            secondDoor = GridTerminalSystem.GetBlockWithName("SecondDoor") as IMyDoor;
            hangarDoor = GridTerminalSystem.GetBlockWithName("HangarDoor") as IMyDoor;

            /** Вентиляция */
            mainAirVent = GridTerminalSystem.GetBlockWithName("VMain") as IMyAirVent;
            spaceAirVent = GridTerminalSystem.GetBlockWithName("VSpace") as IMyAirVent;
            hangarAirVent = GridTerminalSystem.GetBlockWithName("VHangar") as IMyAirVent;
        }

        void Main()
        {
            String text = "";

            /** Двери */
            text += "Двери:\n";
            if(mainDoor.Open) {
                text += "Жилой отсек: открыт\n";
            } else {
                text += "Жилой отсек: закрыт\n";
            }

            if(firstDoor.Open) {
                text += "Служебный отсек: открыт\n\n";
            } else {
                text += "Служебный отсек: закрыт\n\n";
            }

            /** Кислород: */
            text += "Кислород в отсеках:\n";
            text += "Жилой отсек: " + GetPercent(mainAirVent.GetOxygenLevel(), 1).ToString() + "%\n";
            text += "Служебный отсек: " + GetPercent(spaceAirVent.GetOxygenLevel(), 1).ToString() + "%\n";
            text += "Ангарный отсек: " + GetPercent(hangarAirVent.GetOxygenLevel(), 1).ToString() + "%\n\n";

            /** Баки */
            text += "Баки:\n";
            text += "Кислородные баки: " + GetOxygenStatus(GetOxygenReservoirs("Кислородный бак (БК)")) + "\n";
            text += "Водородные баки: " + GetOxygenStatus(GetOxygenReservoirs("Водородный бак (БК)")) + "\n\n";

            /** Энергия: */
            text += "Энергия: " + GetBatteriesStatus(GetBatteries("Батарея (БК)")) + "\n\n";

            /** hangarDoor.Open &&  mainDoor.Open */
            if(mainDoor.Open) {
                spaceAirVent.Depressurize = false;
            } else {
                spaceAirVent.Depressurize = true;
            }

            panel.WriteText(text);
        }

        /** Получаем процент от числа */
        private float GetPercent(float min, float max)
        {
            return min / (max / 100f);
        }

        /** Получаем общий статус о резервуарах */
        private String GetOxygenStatus(List<IMyGasTank> array)
        {
            String result = "";
            float middleSum = 0f;

            if(array.Count > 0) {
                /** Получаем среднее значение суммы всех резервуаров */
                for (int i = 0; i < array.Count; i++) {
                    middleSum += (GetPercent(Convert.ToSingle(array[i].FilledRatio), 1f) / Convert.ToSingle(array.Count));
                }
                result += middleSum.ToString() + "%";
            } else {
                result = "Нет информации";
            }

            return result;
        }

        /** Получить общий статус батарей */
        private String GetBatteriesStatus(List<IMyBatteryBlock> array)
        {
            String result = "";
            float middleSum = 0f;

            if(array.Count > 0) {
                /** Получаем среднее значение суммы всех резервуаров */
                for (int i = 0; i < array.Count; i++) {
                    middleSum += (GetPercent(Convert.ToSingle(array[i].CurrentStoredPower), 3f) / Convert.ToSingle(array.Count));
                }
                result += middleSum.ToString() + "%";
            } else {
                result = "Нет информации";
            }

            return result;
        }

        /** Получить любые резервуары по имени */
        private List<IMyGasTank> GetOxygenReservoirs(String name)
        {
            List<IMyGasTank> reservoirs = new List<IMyGasTank>();
            List<IMyGasTank> result = new List<IMyGasTank>();

            GridTerminalSystem.GetBlocksOfType(reservoirs);

            for(int i = 0; i < reservoirs.Count; i++) {
                if(reservoirs[i].DisplayNameText.Equals(name)) {
                    result.Add(reservoirs[i]);
                }
            }

            return result;
        }

        /** Получить батареи */
        private List<IMyBatteryBlock> GetBatteries(String name)
        {
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            List<IMyBatteryBlock> result = new List<IMyBatteryBlock>();

            GridTerminalSystem.GetBlocksOfType(batteries);

            for(int i = 0; i < batteries.Count; i++) {
                if(batteries[i].DisplayNameText.Equals(name)) {
                    result.Add(batteries[i]);
                }
            }

            return result;
        }
    }
}
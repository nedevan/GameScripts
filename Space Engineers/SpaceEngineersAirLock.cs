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
        // Просто титульный текст в LCD панели  
        private const string LCD_TITLE_MESSAGE = "Жилой отсек";

        // -----------------------------------------------------------
        // 1. Дверям нужно дать название:
        //      Двери в жилую зону:
        //          Название блока (Дверь) (Жилая зона)
        //      Двери в космос:
        //          Название блока (Дверь) (Шлюз)
        //          
        // 2. Вентиляции нужно дать название:
        //      Вентиляция в жилую зону: 
        //          Название блока (Вентиляция) (Жилая зона)
        //      Вентиляция в космос:
        //          Название блока (Вентиляция) (Шлюз)
        //
        // 3. Батареям нужно дать название:
        //      Название блока (Батарея)
        //
        // 4. Хранилищам водорода или кислорода нужно дать название:
        //      1. Название блока (Резервуар) (Кислород)
        //      2. Название блока (Резервуар) (Водород)
        // -----------------------------------------------------------

        // Доступные зоны для вентиляторов
        private const string ZONE_SPACE = "(Шлюз)";
        private const string ZONE_LIFE = "(Жилая зона)";

        // Типы газов
        private const string OXYGEN = "(Кислород)";
        private const string HYBROGEN = "(Водород)";

        private const string LCD_TAG = "(Дисплей)";
        private const string DOOR_TAG = "(Дверь)";
        private const string RESERVOIR_TAG = "(Резервуар)";
        private const string BATTAREY_TAG = "(Батарея)";
        private const string AIR_VENT_TAG = "(Вентиляция)";

        // Все блоки структуры на которой стоит программируемый блок
        private List<IMyTerminalBlock> AllBlocksList = new List<IMyTerminalBlock>();

        // Информационная панель
        private List<IMyTextPanel> PanelsList = new List<IMyTextPanel>();

        // Двери
        private List<IMyDoor> DoorsList = new List<IMyDoor>();
        private List<IMyDoor> LifeZoneDoorsList = new List<IMyDoor>();
        private List<IMyDoor> SpaceDoorsList = new List<IMyDoor>();

        // Вентиляция 
        private List<IMyAirVent> AirVentsList = new List<IMyAirVent>();
        private List<IMyAirVent> LifeZoneVentsList = new List<IMyAirVent>();
        private List<IMyAirVent> SpaceVentsList = new List<IMyAirVent>();

        // Любые резервуары по тэгу 
        private List<IMyGasTank> AllTankReservoirsList = new List<IMyGasTank>();
        private List<IMyGasTank> OxygenTankReservoirsList = new List<IMyGasTank>();
        private List<IMyGasTank> HybrogenTankReservoirsList = new List<IMyGasTank>();

        // Батареи
        private List<IMyBatteryBlock> BatteriesList = new List<IMyBatteryBlock>();

        public Program()
        {
            /** Выполнение программы каждые 100 миллисекунд */
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            AllBlocksList = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(AllBlocksList);

            // Инициализируем все блоки структуры и разбиваем их по группам
            foreach (IMyTerminalBlock block in AllBlocksList)
            {   
                // Добавляем LCD панели
                if (block.CustomName.Contains(LCD_TAG)) 
                {
                    PanelsList.Add(block as IMyTextPanel);
                }

                // Добавляем двери
                if (block.CustomName.Contains(DOOR_TAG)) 
                {
                    DoorsList.Add(block as IMyDoor);
                }

                // Добавляем вентиляторы
                if (block.CustomName.Contains(AIR_VENT_TAG)) 
                {
                    AirVentsList.Add(block as IMyAirVent);
                }

                // Добавляем резервуары
                if (block.CustomName.Contains(RESERVOIR_TAG)) 
                {
                    AllTankReservoirsList.Add(block as IMyGasTank);
                }

                // Добавляем Батареи
                if (block.CustomName.Contains(BATTAREY_TAG)) 
                {
                    BatteriesList.Add(block as IMyBatteryBlock);
                }
            }

            // Разбиваем вентиляторы
            foreach (IMyAirVent airVent in AirVentsList)
            {   
                //  Инициализируем вентиляторы в жилой зоне
                if (airVent.CustomName.Contains(ZONE_LIFE))
                {
                    LifeZoneVentsList.Add(airVent as IMyAirVent);
                }
                
                // Инициализируем вентиляторы в шлюзе
                if (airVent.CustomName.Contains(ZONE_SPACE)) 
                {
                    SpaceVentsList.Add(airVent as IMyAirVent);
                }
            }

            // Разбиваем двери
            foreach (IMyDoor door in DoorsList)
            {   
                //  Инициализируем вентиляторы в жилой зоне
                if (door.CustomName.Contains(ZONE_LIFE))
                {
                    LifeZoneDoorsList.Add(door as IMyDoor);
                }
                
                // Инициализируем вентиляторы в шлюзе
                if (door.CustomName.Contains(ZONE_SPACE)) 
                {
                    SpaceDoorsList.Add(door as IMyDoor);
                }
            }

            // Разбиваем баки
            foreach (IMyGasTank tankReservoir in AllTankReservoirsList)
            {
                if (tankReservoir.CustomName.Contains(OXYGEN))
                {
                    OxygenTankReservoirsList.Add(tankReservoir);
                }

                if (tankReservoir.CustomName.Contains(HYBROGEN))
                {
                    HybrogenTankReservoirsList.Add(tankReservoir);
                }
            }
        }

        public void Main()
        {
            string text = LCD_TITLE_MESSAGE + "\n\n";

            // Двери
            text += GetLifeZoneDoorsStatus() + "\n\n";
            // Проверить вентиляторы
            checkAirVentMode();

            // Кислород
            text += "Кислород в отсеках:\n";
            text += "Жилой отсек: " + GetOxygenLevelStatusInRoomByTag(ZONE_LIFE) + "%\n";
            text += "Служебный отсек: " + GetOxygenLevelStatusInRoomByTag(ZONE_SPACE) + "%\n\n";

            // Баки
            text += "Баки:\n";
            text += "Кислородные баки: " + GetTankReservoirsStatusByTag(OXYGEN) + "\n";
            text += "Водородные баки: " + GetTankReservoirsStatusByTag(HYBROGEN) + "\n\n";

            // Энергия
            text += "Энергия: " + GetBatteriesStatus() + "\n\n";

            // Выводим на панели
            foreach (IMyTextPanel panel in PanelsList)
            {
                panel.WriteText(text);
            }
        }

        // Получаем процент от числа 
        private float GetPercent(float min, float max)
        {
            return min / (max / 100f);
        }

        // Получить общий статус дверей жилого отсека
        private string GetLifeZoneDoorsStatus()
        {
            string result = "Двери:\n";
            int OpenDoorCount = 0;

            if(LifeZoneDoorsList.Count > 0) 
            {
                foreach (IMyDoor mainDoor in LifeZoneDoorsList) 
                {
                    if (mainDoor.Status == DoorStatus.Opening || mainDoor.Status == DoorStatus.Open) 
                    {
                        OpenDoorCount++;
                    }
                }

                if (OpenDoorCount > 0) 
                {
                    result += "Жилой отсек: открыт\n";
                } 
                else 
                {
                    result += "Жилой отсек: закрыт\n";
                }
            } 
            else 
            {
                result = "Нет информации";
            }

            return result;
        }    

        // Проверить режим работы вентиляторов по двери
        private void checkAirVentMode()
        {
            foreach (IMyDoor mainDoor in LifeZoneDoorsList) 
            {
                SetAirVentModeByTag(!(mainDoor.Status == DoorStatus.Opening || mainDoor.Status == DoorStatus.Open));
            }
        }

        // Получаем общий статус резервуаров
        private string GetTankReservoirsStatusByTag(string tagName)
        {
            List<IMyGasTank> tankList = tagName == OXYGEN ? OxygenTankReservoirsList : HybrogenTankReservoirsList;

            string result = "";
            float total = 0f;

            if(tankList.Count > 0) 
            {
                foreach (IMyGasTank oxygen in tankList)
                {
                    total += (GetPercent(Convert.ToSingle(oxygen.FilledRatio), 1f) / Convert.ToSingle(tankList.Count));
                }

                result += total.ToString() + "%";
            } 
            else 
            {
                result = "Нет информации";
            }

            return result;
        }

        //  Получить общий статус батарей
        private string GetBatteriesStatus()
        {
            string result = "";
            float percent = 0f;

            if(BatteriesList.Count > 0) 
            {
                // Получаем среднее значение суммы всех резервуаров
                foreach (IMyBatteryBlock battery in BatteriesList)
                {
                    percent += (GetPercent(Convert.ToSingle(battery.CurrentStoredPower), 3f) / Convert.ToSingle(BatteriesList.Count));
                }

                result += percent.ToString() + "%";
            } 
            else 
            {
                result = "Нет информации";
            }

            return result;
        }

        // Изменить режим работы вентиляторов по тэгу
        private void SetAirVentModeByTag(bool isVoid = false, string tagName = ZONE_SPACE)
        {
            foreach (IMyAirVent airVent in AirVentsList)
            {
                if (airVent.CustomName.Contains(tagName))
                {
                    airVent.Depressurize = isVoid;
                }
            }
        }

        // Проверить кол-во кислорода в комнатах
        private string GetOxygenLevelStatusInRoomByTag(string tagName)
        {
            float total = 0;

            List<IMyAirVent> ventsList = tagName == ZONE_LIFE ? LifeZoneVentsList : SpaceVentsList;

            foreach (IMyAirVent airVent in ventsList)
            {
                total += airVent.GetOxygenLevel();
            }

            return GetPercent(total, ventsList.Count).ToString();
        }
}
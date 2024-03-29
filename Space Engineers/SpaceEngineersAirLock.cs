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

        // НАСТРОЙКА -------------------------------------------------
        //
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
        // 
        // 5. LCD панелям нужно дать название
        //      1. Название блока (Дисплей)
        //
        // 6. Маленьким дисплеям нужно дать название
        //      Если хотим всю информацию:
        //          Название блока (Маленький дисплей) (Полная информация)
        //      Если хотим информацию по дверям в жилую зону
        //          Название блока (Маленький дисплей) (Статус дверей)
        //
        // -----------------------------------------------------------

        // Доступные зоны для вентиляторов
        private const string ZONE_SPACE = "(Шлюз)";
        private const string ZONE_LIFE = "(Жилая зона)";

        // Варианты вывода информации
        private const string SURFACE_FULL_CONTENT = "(Полная информация)";
        private const string SURFACE_DOOR_CONTENT = "(Статус дверей)";

        // Типы газов
        private const string OXYGEN = "(Кислород)";
        private const string HYBROGEN = "(Водород)";

        private const string LCD_TAG = "(Дисплей)";
        private const string SURFACE_TAG = "(Маленький дисплей)";
        private const string DOOR_TAG = "(Дверь)";
        private const string RESERVOIR_TAG = "(Резервуар)";
        private const string BATTAREY_TAG = "(Батарея)";
        private const string AIR_VENT_TAG = "(Вентиляция)";

        // Моды дверей
        private const string DOOR_CLOSE_MODE = "CLOSE";
        private const string DOOR_OPEN_MODE = "OPEN";
        private const string DOOR_BLOCK_MODE = "BLOCK";
        private const string DOOR_UNBLOCK_MODE = "UNBLOCK";

        // Все блоки структуры на которой стоит программируемый блок
        private List<IMyTerminalBlock> AllBlocksList = new List<IMyTerminalBlock>();

        // Информационная панель
        // https://github.com/malware-dev/MDK-SE/wiki/Text-Panels-and-Drawing-Sprites
        private List<IMyTextPanel> PanelsList = new List<IMyTextPanel>();
        private List<IMyTextSurface> SurfaceFullContentList = new List<IMyTextSurface>();
        private List<IMyTextSurface> SurfaceDoorContentList = new List<IMyTextSurface>();
        private List<RectangleF> SurfaceFullViewportList = new List<RectangleF>();
        private List<RectangleF> SurfaceDoorViewportList = new List<RectangleF>();

        // Двери
        private List<IMyDoor> DoorsList = new List<IMyDoor>();
        private List<IMyDoor> LifeZoneDoorsList = new List<IMyDoor>();
        private List<IMyDoor> SpaceZoneDoorsList = new List<IMyDoor>();

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

        // Инициализация программы 1 раз за компиляцию 
        public Program()
        {   
            // Выполнение Main() каждые 100 миллисекунд
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

                // Добавляем TextSurface панели
                if (block.CustomName.Contains(SURFACE_TAG))
                {
                    IMyTextSurface surface = (block as IMyTextSurfaceProvider).GetSurface(0);

                    // Полная информация
                    if (block.CustomName.Contains(SURFACE_FULL_CONTENT)) 
                    {
                        SurfaceFullContentList.Add(surface as IMyTextSurface);

                        // Вычисляем смещение области просмотра для дисплея, центрируя размер поверхности по размеру текстуры
                        RectangleF viewport = new RectangleF((surface.TextureSize - surface.SurfaceSize) / 2f, surface.SurfaceSize);
                        SurfaceFullViewportList.Add(viewport);
                    }

                    // Информация по дверям
                    if (block.CustomName.Contains(SURFACE_DOOR_CONTENT)) 
                    {
                        SurfaceDoorContentList.Add(surface as IMyTextSurface);

                        // Вычисляем смещение области просмотра для дисплея, центрируя размер поверхности по размеру текстуры
                        RectangleF viewport = new RectangleF((surface.TextureSize - surface.SurfaceSize) / 2f, surface.SurfaceSize);
                        SurfaceDoorViewportList.Add(viewport);
                    }
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
                    SpaceZoneDoorsList.Add(door as IMyDoor);
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

            // Разблокировка дверей, если они заблокированы
            SetLifeZoneDoorMode(DOOR_UNBLOCK_MODE);
            SetSpaceZoneDoorMode(DOOR_UNBLOCK_MODE);

            SetLifeZoneDoorMode(DOOR_CLOSE_MODE);
            SetSpaceZoneDoorMode(DOOR_CLOSE_MODE);
        }

        // Вызывается каждый раз при обновлении программы 
        // Runtime.UpdateFrequency = UpdateFrequency.Update100;
        public void Main()
        {
            UpdateRenderText();

            // Статус двери в космос
            bool isSpaceDoorsOpen = CheckOpenSpaceZoneDoors();
            bool isLifeDoorsOpen = CheckOpenLifeZoneDoors();

            if (isSpaceDoorsOpen) 
            {
                SetLifeZoneDoorMode(DOOR_CLOSE_MODE);
                SetLifeZoneDoorMode(DOOR_BLOCK_MODE);
                SetAirVentModeByTag(true);
            }
            else
            {
                SetLifeZoneDoorMode(DOOR_UNBLOCK_MODE);
                SetAirVentModeByTag(true);
            }

            if (isLifeDoorsOpen)
            {
                SetSpaceZoneDoorMode(DOOR_CLOSE_MODE);
                SetSpaceZoneDoorMode(DOOR_BLOCK_MODE);
                SetAirVentModeByTag(false);
            }
            else
            {
                SetSpaceZoneDoorMode(DOOR_UNBLOCK_MODE);
                SetAirVentModeByTag(true);
            }
        }

        // Обновляем отрисовку текста
        private void UpdateRenderText()
        {
            // Чтобы легко узнать индекс элемента SurfaceViewportList, так как SurfaceFullContentList и SurfaceFullViewportList взаимосвязаны
            int index = 0;
            foreach (IMyTextSurface surface in SurfaceFullContentList)
            {
                // Создать новый кадр
                MySpriteDrawFrame frame = surface.DrawFrame();

                // Добавить титульное название
                frame.Add(GetSpriteTextRow(LCD_TITLE_MESSAGE + "\n\n", new Vector2(0, 0), SurfaceFullViewportList[index], 2.0f));

                // Добавить все остальные записи (Засовываем туда строку с переносами \n) так проще
                frame.Add(GetSpriteTextRow(GetFullStatusText(), new Vector2(0, 90), SurfaceFullViewportList[index]));

                frame.Dispose();

                WriteToLCD(GetFullStatusText());
                index++;
            }

            index = 0;
            foreach (IMyTextSurface surface in SurfaceDoorContentList)
            {
                // Создать новый кадр
                MySpriteDrawFrame frame = surface.DrawFrame();

                // Добавить все остальные записи (Засовываем туда строку с переносами \n) так проще
                frame.Add(GetSpriteTextRow(GetLifeZoneDoorsStatus(), new Vector2(0, 60), SurfaceDoorViewportList[index]));

                frame.Dispose();
                index++;
            }
        }

        // Создать одну строку
        public MySprite GetSpriteTextRow(string message, Vector2 position, RectangleF viewport, float scale = 1.0f)
        {   
            // Отступ слева и сверху
            Vector2 padding = new Vector2(15, 15);

            return new MySprite
            {
                Type = SpriteType.TEXT,
                Data = message,
                Size = viewport.Size,
                Color = Color.White,
                RotationOrScale = scale,
                Alignment = TextAlignment.LEFT,
                
                // Инфа от разрабов API
                // https://github.com/malware-dev/MDK-SE/wiki/Text-Panels-and-Drawing-Sprites
                //
                // Установите начальную позицию и не забудьте добавить смещение области просмотра. 
                // Речь про viewport.Position
                Position = position + viewport.Position + padding,
            };
        }
        
        // Получить полный статус (текст)
        private string GetFullStatusText()
        {
            string text = "";

            // Двери
            text += GetLifeZoneDoorsStatus() + "\n";

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

            return text;
        }

        // Записать в LCD панели
        private void WriteToLCD(string message)
        {
            // Выводим на панели
            foreach (IMyTextPanel panel in PanelsList)
            {
                panel.WriteText(message);
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

            if(LifeZoneDoorsList.Count > 0) 
            {
                result = CheckOpenLifeZoneDoors() ? "Жилой отсек: открыт\n" : "Жилой отсек: закрыт\n";
            } 
            else 
            {
                result = "Нет информации";
            }

            return result;
        }    

        // Проверить двери выхода в космос
        private bool CheckOpenSpaceZoneDoors()
        {
            foreach (IMyDoor door in SpaceZoneDoorsList) 
            {
                if (door.Status == DoorStatus.Opening || door.Status == DoorStatus.Open) 
                {
                    return true;
                }
            }

            return false;
        }

        // Проверить двери выхода в жилую зону
        private bool CheckOpenLifeZoneDoors()
        {
            foreach (IMyDoor door in LifeZoneDoorsList) 
            {
                if (door.Status == DoorStatus.Opening || door.Status == DoorStatus.Open) 
                {
                    return true;
                }
            }

            return false;
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

        // Установить мод двери по тэгу
        private void SetDoorModeByTag(string tagName = ZONE_SPACE, string mode = DOOR_CLOSE_MODE)
        {
            List<IMyDoor> doorsList = tagName == ZONE_SPACE ? SpaceZoneDoorsList : LifeZoneDoorsList;

            foreach (IMyDoor door in doorsList)
            {
                switch (mode)
                {
                    case DOOR_OPEN_MODE:
                        door.OpenDoor();
                        break;
                    case DOOR_CLOSE_MODE:
                        door.CloseDoor();
                        break;
                    case DOOR_BLOCK_MODE:
                        door.Enabled = false;
                        break;
                    case DOOR_UNBLOCK_MODE:
                        door.Enabled = true;
                        break;
                }
            }
        }

        // Установить двери LifeZone закрытие
        private void SetLifeZoneDoorMode(string mode = DOOR_CLOSE_MODE)
        {
            SetDoorModeByTag(ZONE_LIFE, mode);
        }

        // Установить двери SpaceZone закрытие
        private void SetSpaceZoneDoorMode(string mode = DOOR_CLOSE_MODE)
        {
            SetDoorModeByTag(ZONE_SPACE, mode);
        }
    }  
}
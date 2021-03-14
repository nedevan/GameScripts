void Main() 
{
    // https://github.com/malware-dev/MDK-SE/wiki/Api-Index
    // Инфармационная панель
    IMyTextPanel panel = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    // Двери
    IMyDoor MainDoor = GridTerminalSystem.GetBlockWithName("MainDoor") as IMyDoor,
            FirstDoor = GridTerminalSystem.GetBlockWithName("FirstDoor") as IMyDoor,
            SecondDoor = GridTerminalSystem.GetBlockWithName("SecondDoor") as IMyDoor,
            HangarDoor = GridTerminalSystem.GetBlockWithName("HangarDoor") as IMyDoor;

    // Вентиляция
    IMyAirVent VMain = GridTerminalSystem.GetBlockWithName("VMain") as IMyAirVent,
               VSpace = GridTerminalSystem.GetBlockWithName("VSpace") as IMyAirVent,
               VHangar = GridTerminalSystem.GetBlockWithName("VHangar") as IMyAirVent;

    String text = "";

    // Двери
    text += "Двери:\n";
    if(MainDoor.Open) text += "Жилой отсек: открыт\n"; 
    else text += "Жилой отсек: закрыт\n"; 
    
    if(FirstDoor.Open) text += "Служебный отсек: открыт\n\n";
    else text += "Служебный отсек: закрыт\n\n";

    // Кислород:
    text += "Кислород в отсеках:\n";
    text += "Жилой отсек: " + GetPercent(VMain.GetOxygenLevel(), 1).ToString() + "%\n";
    text += "Служебный отсек: " + GetPercent(VSpace.GetOxygenLevel(), 1).ToString() + "%\n";
    text += "Ангарный отсек: " + GetPercent(VHangar.GetOxygenLevel(), 1).ToString() + "%\n\n";
    
    // Баки
    text += "Баки:\n";
    text += "Кислородные баки: " + GetOxygenStatus(GetOxygenReservoirs("Кислородный бак (БК)")) + "\n";
    text += "Водородные баки: " + GetOxygenStatus(GetOxygenReservoirs("Водородный бак (БК)")) + "\n\n";

    // Энергия:
    text += "Энергия: " + GetBattariesStatus(GetBatteries("Батарея (БК)")) + "\n\n";

    // HangarDoor.Open &&  MainDoor.Open
    if(MainDoor.Open) VSpace.Depressurize = false;

    else VSpace.Depressurize = true;
    
    panel.WritePublicText(text, false);
    panel.ShowPublicTextOnScreen();
    panel.UpdateVisual();
}

// Получаем процент от числа
float GetPercent(float min, float max) {

    return min / (max / 100f);
}

// Получаем общий статус о резервуарах
String GetOxygenStatus(List<IMyGasTank> array) {

    String result = "";

    float middleSum = 0f;

    if(array.Count > 0) {

        for (int i = 0; i < array.Count; i++) {

            // Получаем среднее значение суммы всех резервуаров
            middleSum += (GetPercent(Convert.ToSingle(array[i].FilledRatio), 1f) / Convert.ToSingle(array.Count));
        }

        result += middleSum.ToString() + "%";
    }
    
    else result = "Нет информации";

    return result;
}

// Получить общий статус батарей
String GetBattariesStatus(List<IMyBatteryBlock> array) {

    String result = "";

    float middleSum = 0f;

    if(array.Count > 0) {

        for (int i = 0; i < array.Count; i++) {

            // middleSum += array[i].CurrentStoredPower;
            middleSum += (GetPercent(Convert.ToSingle(array[i].CurrentStoredPower), 3f) / Convert.ToSingle(array.Count));
        }

        result += middleSum.ToString() + "%";
    }
    
    else result = "Нет информации";

    return result;
}

// Получить любые резервуары по имени
List<IMyGasTank> GetOxygenReservoirs(String name) {

    List<IMyGasTank> reservoirs = new List<IMyGasTank>();
    List<IMyGasTank> result = new List<IMyGasTank>();

    GridTerminalSystem.GetBlocksOfType(reservoirs);

    for(int i = 0; i < reservoirs.Count; i++) {

        if(reservoirs[i].DisplayNameText.Equals(name)) result.Add(reservoirs[i]);
    }

    return result;
}

// Получить батареии
List<IMyBatteryBlock> GetBatteries(String name) {

    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    List<IMyBatteryBlock> result = new List<IMyBatteryBlock>();

    GridTerminalSystem.GetBlocksOfType(batteries);

    for(int i = 0; i < batteries.Count; i++) {

        if(batteries[i].DisplayNameText.Equals(name)) result.Add(batteries[i]);
    }

    return result;
}

// Получить ангарные двери
List<IMyDoor> GetHangarDoor(String name) {

    // получаем все гурппы
    List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
    List<IMyDoor> result = new List<IMyDoor>();

    GridTerminalSystem.GetBlockGroups(groups);

    for(int i = 0; i < groups.Count; i++) {

        if(groups[i].Name.Equals(name)) result.Add((IMyDoor)result[i]);
    }

    return result;
}
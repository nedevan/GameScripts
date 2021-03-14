--
-- Запустить игру с параметром -DEVMODE.
-- В дериктории \FarCry\FCData\ есть .pak архивы, вскрываются Rar, там все скрипты игры
--
-- Если мы хотим удалить начальные застаквки, нужно перейти в дерикторию:
-- \FarCry\Languages\Movies\English и удалить (Crytek.bik, Ubi.bik, sandbox.bik)
--
-- Команда F1 активирует 3 лицо главного героя (В оригинале есть команды с F1 по F6)
--

cl_display_hud = 1; 	-- Отображение HUD игрока 0 -1
cl_drunken_cam = 0; 	-- Пьяная камера 0 - 1
ThirdPersonView = 0; 	-- Вид от 3 лица 0 - 1
ai_noupdate = 0; 		-- Реакция врагов на тебя 0 - 1

-- Action
Input:BindAction("SAVEPOS", "f9", "default");
Input:BindAction("LOADPOS", "f10", "default");
-- Action

-- Command
Input:BindCommandToKey("#ToggleGod()", "backspace", 1);
Input:BindCommandToKey("#GetAmmo()", "o", 1);
Input:BindCommandToKey("#GetWeapons()", "p", 1);
Input:BindCommandToKey("#DefaultSpeed()", "/", 1);
Input:BindCommandToKey("#DecreseSpeed()", ",", 1);
Input:BindCommandToKey("#IncreseSpeed()", ".", 1);
Input:BindCommandToKey("#GetCoordinate()", "=", 1);
-- Command

--- temp variables for functions below ---
prev_speed_walk=p_speed_walk;
prev_speed_run=p_speed_run;

prev_speed_walk2=p_speed_walk;
prev_speed_run2=p_speed_run;

default_speed_walk=p_speed_walk;
default_speed_run=p_speed_run;

screenshotmode=0;

-- Уменьшает скорость на 10
function DecreseSpeed()
	if tonumber(p_speed_walk) > 10 then
		p_speed_walk=p_speed_walk-10;
		p_speed_run=p_speed_run-10;
		SetMessage("Speed player decrease 10.");
	else
		SetMessage("Speed min!");
	end 
end

-- Увеличивает скорость 10
function IncreseSpeed()
	if tonumber(p_speed_walk) < 500 then
		p_speed_walk=p_speed_walk+10;
		p_speed_run=p_speed_run+10;
		SetMessage("Speed player increase 10.");
	else
		SetMessage("Speed max!");
	end 
end

-- Возвращает скорость
function DefaultSpeed()
	p_speed_walk=default_speed_walk;
	p_speed_run=default_speed_run;
	SetMessage("Speed default!");
end

-- Телепортирует на указанную точку
function Teleport(coordinate)
	_localplayer:SetPos(coordinate);
	SetMessage("Teleport!");
end

-- Получить координаты игрока
function GetCoordinate()
	local coordinate = _localplayer:GetPos();
	local message = "Coordinate: " .. "X= " .. tostring(coordinate.x) .. "; Y= " .. tostring(coordinate.y) .. "; z= " .. tostring(coordinate.z);
	SetMessage(message);
end

-- Добавляет
function GetAmmo()
	if _localplayer then
		_localplayer.cnt.ammo=999;
		_localplayer.cnt.health=999;
		_localplayer.cnt.armor=999;
		_localplayer.cnt.energy=999;
		SetMessage("Ammo and endurance has been added!");
	else 
		SetMessage("No ammo today");
	end
end

-- Добавляет оружие
function GetWeapon(Name)
	Game:AddWeapon(Name);
	for i, CurWeapon in WeaponClassesEx do
		if (i == Name) then
			_localplayer.cnt:MakeWeaponAvailable(CurWeapon.id);
		end
	end	
end

-- Добавляет все оружие
function GetWeapons()

	-- GetWeapon("MP5");
	-- GetWeapon("Shotgun");
	-- GetWeapon("AG36");
	-- GetWeapon("Falcon");
	
	GetWeapon("P90");
	GetWeapon("OICW");
	GetWeapon("SniperRifle");
	GetWeapon("RL");

	_localplayer.cnt:GiveBinoculars(1);
	_localplayer.cnt:GiveFlashLight(1);	

	SetMessage("Weapons has been added! :)");
end

-- Включает режим бога
function ToggleGod()

	if (not god) then
		god=1;
	else
		god=1-god;
	end

	if (god==1) then
		SetMessage("God-Mode ON");
	else
		SetMessage("God-Mode OFF");
	end
end

-- Создает сообщение
function SetMessage(message)
	message = "[Aleksey]: " .. message;
	Hud:AddMessage(message);
	System:LogToConsole(message);
end
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;//нужно подключить чтобы С# понимал названия клавиш
using System.Reflection;

/*
1 часть
 Вам предстоит наполнить готовую заготовку игровыми элементами. Каждый элемент должен уметь:
•	Возвращать имя файла, в котором лежит соответствующая ему картинка (например, "Terrain.png")
•	Сообщать приоритет отрисовки. Чем выше приоритет, тем раньше рисуется соответствующий элемент, это важно для анимации.
•	Действовать — возвращать направление перемещения и, если объект во что-то превращается на следующем ходу, то результат превращения.
•	Разрешать столкновения двух элементов в одной клетке.
Terrain
Сделайте класс Terrain, реализовав ICreature. Сделайте так, чтобы он ничего не делал.
Player
Сделайте класс Player, реализовав ICreature.
Сделайте так, чтобы диггер шагал в разные стороны в зависимости от нажатой клавиши (Game.KeyPressed). Убедитесь, что диггер не покидает пределы игрового поля.
Сделайте так, чтобы земля исчезала в тех местах, где прошел диггер.
Запустите проект — игра должна заработать!
В методе Game.CreateMap вы можете менять карту, на которой будет запускаться игра. Используйте эту возможность для отладки.

2 часть
Пора добавить мешки с золотом и само золото!
Sack
Сделайте класс Sack, реализовав ICreature. Это будет мешок с золотом.
•	Мешок может лежать на любой другой сущности (диггер, земля, мешок, золото, край карты).
•	Если под мешком находится пустое место, он начинает падать.
•	Если мешок падает на диггера, диггер умирает, а мешок продолжает падать, пока не приземлится на землю, другой мешок, золото или край карты.
•	Диггер не может подобрать мешок, толкнуть его или пройти по нему.
Если мешок падает, а диггер находится непосредственно под ним и идет вверх, они могут "разминуться", и диггер окажется над мешком. Это поведение непросто исправить в существующей упрощенной архитектуре, поэтому считайте его нормальным.
Gold
Сделайте класс Gold, реализовав ICreature.
•	Мешок превращается в золото, если он падал дольше одной клетки игрового поля и приземлился на землю, на другой мешок или на золото.
•	Мешок не превращается в золото, а остаётся мешком, если он падал ровно одну клетку.
•	Золото никогда не падает.
•	Когда диггер собирает золото, ему начисляется 10 очков (через Game.Scores)
 */

namespace Digger
{
    public class Terrain : ICreature
    //Напишите здесь классы Player, Terrain и другие.
    {
        public string GetImageFileName()
        {
            return "Terrain.png";
        }

        public int GetDrawingPriority() { return 0; }
        public CreatureCommand Act(int x, int y) { return new CreatureCommand { DeltaX = 0, DeltaY = 0 }; ; }
        //определяем кого следует оставить но если кто то другой кроме земли то удалять землю
        public bool DeadInConflict(ICreature conflictedObject) { return true; }
    }

    public class Player : ICreature
    {
        public string GetImageFileName()
        {
            return "Digger.png";
        }

        public int GetDrawingPriority()
        {
            return 1;
        }

        public CreatureCommand Act(int x, int y)
        {
            //Keys - это встроенное перечисление с названями клавиш в пространстве System.Windows.Forms
            if (Game.KeyPressed == Keys.Left && EncounterBorder(x, y, -1, 0)&& EncounterSack(x, y, -1, 0)) return new CreatureCommand { DeltaX = -1, DeltaY = 0 };
            if (Game.KeyPressed == Keys.Up && EncounterBorder(x, y, 0, -1) && EncounterSack(x, y, 0, -1)) return new CreatureCommand { DeltaX = 0, DeltaY = -1 };
            if (Game.KeyPressed == Keys.Right && EncounterBorder(x, y, 1, 0) && EncounterSack(x, y, 1, 0)) return new CreatureCommand { DeltaX = 1, DeltaY = 0 };
            if (Game.KeyPressed == Keys.Down && EncounterBorder(x, y, 0, 1) && EncounterSack(x, y, 0, 1)) return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
            return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            //определяем кого следует оставить
            //if (conflictedObject.GetType().Name.ToString() == "Player") return true;
            //else return false;
            if (conflictedObject.GetType().Name.ToString() == "Player")
            {
                
                return true;
            }
            else return false;
        }

        public static bool EncounterSack(int x, int y, int deltaX, int deltaY)
        {
            //игрок не может подобрать мешок, толкнуть его или пройти по нему
            //добавлена проверка на null при запросе
           return !(Game.Map[x + deltaX, y + deltaY]?.GetType().Name.ToString() == "Sack");
        }

        public static bool EncounterBorder(int x, int y, int deltaX, int deltaY)
        {
            //ничего не делаем с игроком если он дошел до края карты
            //if (x + DeltaX < 0 || x + DeltaX >= Game.MapWidth || y + DeltaY < 0 ||
            //y + DeltaY >= Game.MapHeight) return false;
            //else return true;
            return !(x + deltaX < 0 || x + deltaX >= Game.MapWidth || y + deltaY < 0 ||
                        y + deltaY >= Game.MapHeight);
        }
    }

    public class Sack : ICreature
    {
        public string GetImageFileName()
        {
            return "Sack.png";
        }

        public int GetDrawingPriority() { return 0; }
        public CreatureCommand Act(int x, int y) 
        {
            int cellDrop = 0;
            //мешок с золотом падает если снизу не граница
            while (true)
            {
                if (Player.EncounterBorder(x, y, 0, 1))
                {
                    int y0 = y;
                    //мешок с золотом падает если снизу ничего нет
                    if (Game.Map[x, y + 1] == null)
                    {
                        cellDrop++;
                        y++;
                    }
                    //мешок с золотом падает на игрока
                    else if (Game.Map[x, y + 1].GetType().Name.ToString() == "Player")
                    {
                        cellDrop++;
                        y++;
                        //игрок исчезает
                        Game.Map[x, y] = null;
                        //Game.Map[x, y] = new CreatureAnimation { Creature = (object)"Player" };
                        return new CreatureCommand { DeltaX = 0, DeltaY = cellDrop }; 
                    }
                    else return ResultFlySeak(cellDrop);
                }
                else return ResultFlySeak(cellDrop);
            }
        }
        //определяем кого следует оставить но если кто то другой кроме земли то удалять землю
        public bool DeadInConflict(ICreature conflictedObject) { return true; }

        public CreatureCommand ResultFlySeak(int cellDrop)
        {
            Type type = Assembly
                   .GetExecutingAssembly()
                   .GetTypes()
                   .FirstOrDefault(z => z.Name == "Gold");
            //object ddd1 = (object)"Gold";
            //var ddd2 = (ICreature)ddd1;
            if (cellDrop > 1) return new CreatureCommand { DeltaX = 0, DeltaY = cellDrop, TransformTo = (ICreature)Activator.CreateInstance(type) };
            //new Dictionary<string, Func<ICreature>> { "Player", }; };
            return new CreatureCommand { DeltaX = 0, DeltaY = cellDrop };
        }
    }

    public class Gold : ICreature
    {
        public string GetImageFileName()
        {
            return "Gold.png";
        }

        public int GetDrawingPriority() { return 0; }
        public CreatureCommand Act(int x, int y) { return new CreatureCommand { DeltaX = 0, DeltaY = 0 }; ; }
        //определяем кого следует оставить но если кто то другой кроме земли то удалять землю
        public bool DeadInConflict(ICreature conflictedObject) { Game.Scores  += 10; return true; }
    }
}

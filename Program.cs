using CityEvents.Data;
using CityEvents.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Linq;

namespace CityEvents
{
    public class Program
    {
        public static void Main()
        {
            using EventContext db = new();

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("1. Выборка всех данных из таблицы Places");
                Console.WriteLine("2. Выборка данных из таблицы Places по условию");
                Console.WriteLine("3. Группировка заказов по EventID и подсчет суммы билетов");
                Console.WriteLine("4. Выборка данных из Events и Places");
                Console.WriteLine("5. Выборка данных из двух таблиц по условию");
                Console.WriteLine("6. Вставка данных в таблицу Places");
                Console.WriteLine("7. Вставка данных в таблицу TicketOrders");
                Console.WriteLine("8. Удаление данных из таблицы Places");
                Console.WriteLine("9. Удаление данных из таблицы Events");
                Console.WriteLine("10. Обновление данных в таблице Events");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите действие: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        SelectAllPlaces(db);
                        break;
                    case "2":
                        SelectPlacesWithFilter(db);
                        break;
                    case "3":
                        GroupByEvents(db);
                        break;
                    case "4":
                        SelectEventsAndPlaces(db);
                        break;
                    case "5":
                        SelectTablesWithFilter(db);
                        break;
                    case "6":
                        InsertPlace(db);
                        break;
                    case "7":
                        InsertTicketOrder(db);
                        break;
                    case "8":
                        DeletePlace(db);
                        break;
                    case "9":
                        DeleteEvent(db);
                        break;
                    case "10":
                        UpdateEvents(db);
                        break;
                    case "0":
                        exit = true;
                        Console.WriteLine("Всё!");
                        break;
                    default:
                        Console.WriteLine("Неверный выбор");
                        break;
                }

                if (!exit)
                {
                    Console.WriteLine("Нажмите любую клавишу");
                    Console.ReadKey();
                }
            }
        }

        static void Print(string sqltext, IEnumerable items)
        {
            Console.WriteLine(sqltext);
            foreach (var item in items)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine();
            Console.ReadKey();
        }
        static void SelectAllPlaces(EventContext db)
        {
            Print("1. Все места:\r\n", db.Places
                .Select(p => new { Место = p.PlaceName, Геолокация = p.Geolocation })
                .ToList());
        }

        static void SelectPlacesWithFilter(EventContext db)
        {
            Print("2. Места, содержащие буквы 'ab':\r\n", db.Places
                .Where(p => p.PlaceName.Contains("ab"))
                .Select(p => new { Место = p.PlaceName, Геолокация = p.Geolocation })
                .ToList());
        }

        static void GroupByEvents(EventContext db)
        {
            var groupedOrders = db.TicketOrders
                .GroupBy(o => o.EventID)
                .Select(gr => new { EventID = gr.Key, TotalTickets = gr.Sum(o => o.TicketCount) })
                .ToList();
            Print("3. Группировка заказов по EventID с суммой билетов:", groupedOrders);
        }

        static void SelectEventsAndPlaces(EventContext db)
        {
            var eventPlaces = db.Events
                .Join(db.Places, e => e.PlaceID, p => p.PlaceID, (e, p) => new { e.EventName, p.PlaceName })
                .ToList();
            Print("4. Выборка EventName и PlaceName из таблиц Events и Places:", eventPlaces);
        }

        static void SelectTablesWithFilter(EventContext db)
        {
            var filteredData = db.Events
                .Include(e => e.Place)
                .Where(e => e.Place.PlaceName.Contains("ab") && e.TicketPrice > 1080)
                .Select(e => new { e.EventName, e.Place.PlaceName, e.TicketPrice })
                .ToList();
            Print("5. Данные из таблиц Events и Places с фильтром:", filteredData);
        }

        static void InsertPlace(EventContext db)
        {
            Console.Write("Введите название места: ");
            string placeName = Console.ReadLine();

            Console.Write("Введите геолокацию: ");
            string geolocation = Console.ReadLine();

            var newPlace = new Place
            {
                PlaceName = placeName,
                Geolocation = geolocation
            };

            db.Places.Add(newPlace);
            db.SaveChanges();

            Console.WriteLine("6. Успешно");
        }

        static void InsertTicketOrder(EventContext db)
        {
            Console.Write("Введите название события: ");
            string eventName = Console.ReadLine();

            var eventEntity = db.Events.FirstOrDefault(e => e.EventName == eventName);
            if (eventEntity == null)
            {
                Console.WriteLine("Событие не найдено");
                return;
            }

            Console.Write("Введите имя заказчика: ");
            string customerName = Console.ReadLine();

            var customerEntity = db.Customers.FirstOrDefault(c => c.FullName == customerName);
            if (customerEntity == null)
            {
                Console.WriteLine("Клиент не найден");
                return;
            }

            Console.Write("Введите количество билетов: ");
            int ticketCount = int.Parse(Console.ReadLine());

            var newOrder = new TicketOrder
            {
                EventID = eventEntity.EventID,
                CustomerID = customerEntity.CustomerID,
                OrderDate = DateTime.Now,
                TicketCount = ticketCount
            };

            db.TicketOrders.Add(newOrder);
            db.SaveChanges();

            Console.WriteLine("7. Успешно");
        }

        static void DeletePlace(EventContext db)
        {
            Console.Write("Введите название места для удаления: ");
            string placeName = Console.ReadLine();

            var place = db.Places.FirstOrDefault(p => p.PlaceName == placeName);
            if (place != null)
            {
                db.Places.Remove(place);
                db.SaveChanges();
                Console.WriteLine("8. Успешно");
            }
            else
            {
                Console.WriteLine("Место не найдено");
            }
        }

        static void DeleteEvent(EventContext db)
        {
            Console.Write("Введите название события для удаления: ");
            string eventName = Console.ReadLine();

            var eventToDelete = db.Events.FirstOrDefault(e => e.EventName == eventName);
            if (eventToDelete != null)
            {
                db.Events.Remove(eventToDelete);
                db.SaveChanges();
                Console.WriteLine("9. Успешно");
            }
            else
            {
                Console.WriteLine("Событие не найдено");
            }
        }

        static void UpdateEvents(EventContext db)
        {
            Console.Write("Введите название события для обновления: ");
            string eventName = Console.ReadLine();

            Console.Write("Введите новую цену билета: ");
            float ticketPrice = float.Parse(Console.ReadLine());

            var eventToUpdate = db.Events.FirstOrDefault(e => e.EventName == eventName);
            if (eventToUpdate != null)
            {
                eventToUpdate.TicketPrice = ticketPrice;
                db.SaveChanges();
                Console.WriteLine("10. Успешно");
            }
            else
            {
                Console.WriteLine("Событие не найдено");
            }
        }
    }
}

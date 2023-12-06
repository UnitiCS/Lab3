using Lab3.Models;
using Lab3.Services;
using Lab3.Data;
using Lab3.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FuelStationT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            // внедрение зависимости для доступа к БД с использованием EF
            string connection = builder.Configuration.GetConnectionString("SqlServerConnection");
            services.AddDbContext<BakeryDBContext>(options => options.UseSqlServer(connection));

            // добавление кэширования
            services.AddMemoryCache();

            // добавление поддержки сессии
            services.AddDistributedMemoryCache();
            services.AddSession();

            // внедрение зависимости CachedMaterialsService
            services.AddScoped<ICachedOrdersService, CachedOrdersService>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            var app = builder.Build();

            // добавляем поддержку сессий
            app.UseSession();

            //Запоминание в Сookies значений, введенных в форме
            app.Map("/searchform1", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    var order = new Order();
                    ICachedOrdersService cachedOrders = context.RequestServices.GetService<ICachedOrdersService>();
                    IEnumerable<Order> orders = cachedOrders.GetOrdersFromCache("orders20");
                    IEnumerable<string> productTypes = cachedOrders.GetTypes(orders);

                    if (context.Request.Method == "POST")
                    {
                        order.Price = decimal.Parse(context.Request.Form["priceLimit"]);
                        order.ProductType = context.Request.Form["type"];

                        context.Response.Cookies.Append("product", JsonConvert.SerializeObject(order));

                        if (order.ProductType != "all")
                        {
                            orders = orders.Where(o => o.Price <= order.Price && o.ProductType == order.ProductType);
                        }
                        else
                        {
                            orders = orders.Where(p => p.Price <= order.Price);
                        }
                    }
                    else if (context.Request.Cookies.ContainsKey("product"))
                    {
                        order = JsonConvert.DeserializeObject<Order>(context.Request.Cookies["product"]);
                    }

                    string htmlString = "<html><head><title>Заказы</title></head>" +
                        "<meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                        "<body>" +
                        "<form method='post' action='/searchform1'>" +
                            "<label>Максимальная цена:</label>" +
                            $"<input type='text' name='priceLimit' value='{order.Price}' placeholder='Максимальная цена'><br><br>" +
                            "<label>Тип продукта:</label>" +
                            "<select name='type'>" +
                            "<option value='all'>Все</option>";

                    foreach (var type in productTypes)
                    {
                        htmlString += $"<option value='{type}' {(type == order.ProductType ? "selected" : "")}>{type}</option>";
                    }

                    htmlString += "</select><br><br>" +
                        "<input type='submit' value='Поиск'>" +
                        "</form>";

                    htmlString += "<h1>Список заказов</h1>" +
                        "<table border='1'>";
                    htmlString += "<TR>";
                        htmlString += "<TH>Код</TH>";
                        htmlString += "<TH>Имя заказчика</TH>";
                        htmlString += "<TH>Название продукта</TH>";
                        htmlString += "<TH>Тип продукта</TH>";
                        htmlString += "<TH>Количевто продукта</TH>";
                        htmlString += "<TH>Цена продукта</TH>";
                        htmlString += "<TH>Дата заказа</TH>";
                        htmlString += "<TH>Дата доставки</TH>";
                    htmlString += "</TR>";

                    foreach (var o in orders)
                    {
                        htmlString += "<tr>" +
                            $"<td>{o.OrderId}</td>" +
                            $"<td>{o.CustomerName}</td>" +
                            $"<td>{o.ProductName}</td>" +
                            $"<td>{o.ProductType}</td>" +
                            $"<td>{o.Quantity}</td>" +
                            $"<td>{o.Price}</td>" +
                            $"<td>{o.OrderDate}</td>" +
                            $"<td>{o.DeliveryDate}</td>" +
                        "</tr>";
                    }

                    htmlString += "</table><br><a href='/'>Главная</a></br></body></html>";
                    await context.Response.WriteAsync(htmlString);
                });
            });

            //Запоминание в Session значений, введенных в форме
            app.Map("/searchform2", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    Order order = context.Session.Get<Order>("product") ?? new Order();
                    ICachedOrdersService cachedProducts = context.RequestServices.GetService<ICachedOrdersService>();
                    IEnumerable<Order> products = cachedProducts.GetOrdersFromCache("orders20");

                    if (context.Request.Method == "POST")
                    {
                        order.ProductName = context.Request.Form["ProductName"];
                        context.Session.Set("product", order);
                        products = products.Where(p => p.ProductName == order.ProductName);
                    }

                    string htmlString = "<html><head><title>Заказы</title></head>" +
                        "<meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                        "<body>" +
                        "<form method='post' action='/searchform2'>" +
                            "<label>Название продукта:</label>" +
                            $"<input type='text' name='ProductName' value='{order.ProductName}'><br><br>" +
                            "<input type='submit' value='Поиск'>" +
                        "</form>" +
                        "<h1>Список заказов</h1>" +
                        "<table border='1'>";
                        htmlString += "<TR>";
                            htmlString += "<TH>Код</TH>";
                            htmlString += "<TH>Имя заказчика</TH>";
                            htmlString += "<TH>Название продукта</TH>";
                            htmlString += "<TH>Тип продукта</TH>";
                            htmlString += "<TH>Количевто продукта</TH>";
                            htmlString += "<TH>Цена продукта</TH>";
                            htmlString += "<TH>Дата заказа</TH>";
                            htmlString += "<TH>Дата доставки</TH>";
                        htmlString += "</TR>";

                    foreach (var o in products)
                    {
                        htmlString += "<tr>" +
                            $"<td>{o.OrderId}</td>" +
                            $"<td>{o.CustomerName}</td>" +
                            $"<td>{o.ProductName}</td>" +
                            $"<td>{o.ProductType}</td>" +
                            $"<td>{o.Quantity}</td>" +
                            $"<td>{o.Price}</td>" +
                            $"<td>{o.OrderDate}</td>" +
                            $"<td>{o.DeliveryDate}</td>" +
                        "</tr>";
                    }
                    htmlString += "</table><br><a href='/'>Главная</a></br></body></html>";
                    await context.Response.WriteAsync(htmlString);
                });
            });

            // Вывод информации о клиенте
            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // Формирование строки для вывода 
                    string htmlString = "<HTML><HEAD><TITLE>Информация</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Информация:</H1>"
                    + "<BR> Сервер: " + context.Request.Host
                    + "<BR> Путь: " + context.Request.PathBase
                    + "<BR> Протокол: " + context.Request.Protocol
                    + "<BR><A href='/'>Главная</A></BODY></HTML>";
                    // Вывод данных
                    await context.Response.WriteAsync(htmlString);
                });
            });

            // Вывод кэшированной информации из таблицы базы данных
            app.Map("/orders", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //обращение к сервису
                    ICachedOrdersService cachedOrdersService = context.RequestServices.GetService<ICachedOrdersService>();
                    IEnumerable<Order> orders = cachedOrdersService.GetOrdersFromCache("orders20");
                    string htmlString = "<HTML><HEAD><TITLE>Заказы</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Список заказов</H1>" +
                    "<TABLE BORDER=1>";
                    htmlString += "<TR>";
                    htmlString += "<TH>Код</TH>";
                    htmlString += "<TH>Имя заказчика</TH>";
                    htmlString += "<TH>Название продукта</TH>";
                    htmlString += "<TH>Тип продукта</TH>";
                    htmlString += "<TH>Количевто продукта</TH>";
                    htmlString += "<TH>Цена продукта</TH>";
                    htmlString += "<TH>Дата заказа</TH>";
                    htmlString += "<TH>Дата доставки</TH>";
                    htmlString += "</TR>";
                    foreach (var o in orders)
                    {
                        htmlString += "<tr>" +
                            $"<td>{o.OrderId}</td>" +
                            $"<td>{o.CustomerName}</td>" +
                            $"<td>{o.ProductName}</td>" +
                            $"<td>{o.ProductType}</td>" +
                            $"<td>{o.Quantity}</td>" +
                            $"<td>{o.Price}</td>" +
                            $"<td>{o.OrderDate}</td>" +
                            $"<td>{o.DeliveryDate}</td>" +
                        "</tr>";
                    }
                    htmlString += "</TABLE>";
                    htmlString += "<BR><A href='/'>Главная</A></BR>";
                    htmlString += "</BODY></HTML>";

                    // Вывод данных
                    await context.Response.WriteAsync(htmlString);
                });
            });

            // Стартовая страница и кэширование данных таблицы на web-сервере
            app.Run((context) =>
            {
                //обращение к сервису
                ICachedOrdersService cachedMaterials = context.RequestServices.GetService<ICachedOrdersService>();
                cachedMaterials.AddOrders("orders20");

                string htmlString = "<HTML><HEAD><TITLE>Материалы</TITLE></HEAD>" +
                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                "<BODY><H1>Главная</H1>"
                + "<BR><A href='/'>Главная</A></BR>"
                + "<BR><A href='/info'>Информация о клиенте</A></BR>"
                + "<BR><A href='/orders'>Заказы</A></BR>"
                + "<BR><A href='/searchform2'>Поиск заказов по названию</A></BR>"
                + "<BR><A href='/searchform1'>Поиск заказов по характеристикам</A></BR>"
                + "</BODY></HTML>";

                return context.Response.WriteAsync(htmlString);

            });

            app.Run();
        }
    }
}
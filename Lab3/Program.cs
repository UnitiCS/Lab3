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
            // ��������� ����������� ��� ������� � �� � �������������� EF
            string connection = builder.Configuration.GetConnectionString("SqlServerConnection");
            services.AddDbContext<BakeryDBContext>(options => options.UseSqlServer(connection));

            // ���������� �����������
            services.AddMemoryCache();

            // ���������� ��������� ������
            services.AddDistributedMemoryCache();
            services.AddSession();

            // ��������� ����������� CachedMaterialsService
            services.AddScoped<ICachedOrdersService, CachedOrdersService>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            var app = builder.Build();

            // ��������� ��������� ������
            app.UseSession();

            //����������� � �ookies ��������, ��������� � �����
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

                    string htmlString = "<html><head><title>������</title></head>" +
                        "<meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                        "<body>" +
                        "<form method='post' action='/searchform1'>" +
                            "<label>������������ ����:</label>" +
                            $"<input type='text' name='priceLimit' value='{order.Price}' placeholder='������������ ����'><br><br>" +
                            "<label>��� ��������:</label>" +
                            "<select name='type'>" +
                            "<option value='all'>���</option>";

                    foreach (var type in productTypes)
                    {
                        htmlString += $"<option value='{type}' {(type == order.ProductType ? "selected" : "")}>{type}</option>";
                    }

                    htmlString += "</select><br><br>" +
                        "<input type='submit' value='�����'>" +
                        "</form>";

                    htmlString += "<h1>������ �������</h1>" +
                        "<table border='1'>";
                    htmlString += "<TR>";
                        htmlString += "<TH>���</TH>";
                        htmlString += "<TH>��� ���������</TH>";
                        htmlString += "<TH>�������� ��������</TH>";
                        htmlString += "<TH>��� ��������</TH>";
                        htmlString += "<TH>��������� ��������</TH>";
                        htmlString += "<TH>���� ��������</TH>";
                        htmlString += "<TH>���� ������</TH>";
                        htmlString += "<TH>���� ��������</TH>";
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

                    htmlString += "</table><br><a href='/'>�������</a></br></body></html>";
                    await context.Response.WriteAsync(htmlString);
                });
            });

            //����������� � Session ��������, ��������� � �����
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

                    string htmlString = "<html><head><title>������</title></head>" +
                        "<meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                        "<body>" +
                        "<form method='post' action='/searchform2'>" +
                            "<label>�������� ��������:</label>" +
                            $"<input type='text' name='ProductName' value='{order.ProductName}'><br><br>" +
                            "<input type='submit' value='�����'>" +
                        "</form>" +
                        "<h1>������ �������</h1>" +
                        "<table border='1'>";
                        htmlString += "<TR>";
                            htmlString += "<TH>���</TH>";
                            htmlString += "<TH>��� ���������</TH>";
                            htmlString += "<TH>�������� ��������</TH>";
                            htmlString += "<TH>��� ��������</TH>";
                            htmlString += "<TH>��������� ��������</TH>";
                            htmlString += "<TH>���� ��������</TH>";
                            htmlString += "<TH>���� ������</TH>";
                            htmlString += "<TH>���� ��������</TH>";
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
                    htmlString += "</table><br><a href='/'>�������</a></br></body></html>";
                    await context.Response.WriteAsync(htmlString);
                });
            });

            // ����� ���������� � �������
            app.Map("/info", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    // ������������ ������ ��� ������ 
                    string htmlString = "<HTML><HEAD><TITLE>����������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>����������:</H1>"
                    + "<BR> ������: " + context.Request.Host
                    + "<BR> ����: " + context.Request.PathBase
                    + "<BR> ��������: " + context.Request.Protocol
                    + "<BR><A href='/'>�������</A></BODY></HTML>";
                    // ����� ������
                    await context.Response.WriteAsync(htmlString);
                });
            });

            // ����� ������������ ���������� �� ������� ���� ������
            app.Map("/orders", (appBuilder) =>
            {
                appBuilder.Run(async (context) =>
                {
                    //��������� � �������
                    ICachedOrdersService cachedOrdersService = context.RequestServices.GetService<ICachedOrdersService>();
                    IEnumerable<Order> orders = cachedOrdersService.GetOrdersFromCache("orders20");
                    string htmlString = "<HTML><HEAD><TITLE>������</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>������ �������</H1>" +
                    "<TABLE BORDER=1>";
                    htmlString += "<TR>";
                    htmlString += "<TH>���</TH>";
                    htmlString += "<TH>��� ���������</TH>";
                    htmlString += "<TH>�������� ��������</TH>";
                    htmlString += "<TH>��� ��������</TH>";
                    htmlString += "<TH>��������� ��������</TH>";
                    htmlString += "<TH>���� ��������</TH>";
                    htmlString += "<TH>���� ������</TH>";
                    htmlString += "<TH>���� ��������</TH>";
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
                    htmlString += "<BR><A href='/'>�������</A></BR>";
                    htmlString += "</BODY></HTML>";

                    // ����� ������
                    await context.Response.WriteAsync(htmlString);
                });
            });

            // ��������� �������� � ����������� ������ ������� �� web-�������
            app.Run((context) =>
            {
                //��������� � �������
                ICachedOrdersService cachedMaterials = context.RequestServices.GetService<ICachedOrdersService>();
                cachedMaterials.AddOrders("orders20");

                string htmlString = "<HTML><HEAD><TITLE>���������</TITLE></HEAD>" +
                "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                "<BODY><H1>�������</H1>"
                + "<BR><A href='/'>�������</A></BR>"
                + "<BR><A href='/info'>���������� � �������</A></BR>"
                + "<BR><A href='/orders'>������</A></BR>"
                + "<BR><A href='/searchform2'>����� ������� �� ��������</A></BR>"
                + "<BR><A href='/searchform1'>����� ������� �� ���������������</A></BR>"
                + "</BODY></HTML>";

                return context.Response.WriteAsync(htmlString);

            });

            app.Run();
        }
    }
}
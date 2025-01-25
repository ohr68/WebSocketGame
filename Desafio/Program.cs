using System.Reflection;
using Desafio.Services;
using Fleck;
using WebSocketBoilerplate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var clientEventHandlers = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var server = new WebSocketServer("ws://0.0.0.0:8181");

server.Start(connection =>
{
    connection.OnOpen = () => { StateService.AddConnection(connection); };

    connection.OnMessage = async message =>
    {
        try
        {
            await app.InvokeClientEventHandler(clientEventHandlers, connection, message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException);
            Console.WriteLine(e.StackTrace);
        }
    };
});

app.Run();

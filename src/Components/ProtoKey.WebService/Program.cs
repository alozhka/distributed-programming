using ProtoKey;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProtoKey();

var app = builder.Build();

app.MapControllers();

app.Run();
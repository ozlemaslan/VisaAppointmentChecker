using VisaChecker;
using VisaChecker.Registeration;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<VisaAppointmentCheckerWorker>();

builder.Services.AddServiceRegisteration(builder.Configuration);


var host = builder.Build();
host.Run();


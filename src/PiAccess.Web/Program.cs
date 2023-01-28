
using Iot.Device.CharacterLcd;
using Iot.Device.Mcp25xxx.Register.ErrorDetection;
using Iot.Device.Pcx857x;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Device.Gpio;
using System.Device.I2c;
using System.Text.Json.Serialization;

namespace PiAccess.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
            using var driver = new Pcf8574(i2c);
            using var ledController = new GpioController();
            var redLed = new Led(21, ledController);
            var greenLed = new Led(16, ledController);
            using var lcd = new Lcd2004(registerSelectPin: 0,
                enablePin: 2,
                dataPins: new int[] { 4, 5, 6, 7 },
                backlightPin: 3,
                backlightBrightness: 0.1f,
                readWritePin: 1,
                new GpioController(PinNumberingScheme.Logical, driver));
            var lcdWriter = new Lcd2004Writer(lcd);
            lcdWriter.Write("All Systems \n Normal");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton(lcdWriter);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Urls.Add("http://0.0.0.0:5000");

            app.MapGet("/", () => "Hi");

            app.MapPost("/message", (MessageRequest req) =>
            {
                Console.WriteLine($"Need to write {req.Text} to LCD");
                var lcdWriter = app.Services.GetService<Lcd2004Writer>();
                Console.WriteLine($"LCD WRite Is Null {lcdWriter == null}");
                lcdWriter?.ClearScreen();
                lcdWriter?.Write(req.Text);
                Console.WriteLine("Done");
            });

            app.MapPost("/led", (LedRequest req) =>
            {
                if (req.RedLedOn)
                    redLed.On();
                else
                    redLed.Off();

                if (req.GreenLedOn)
                    greenLed.On();
                else
                    greenLed.Off();
            });

            app.MapPost("/status", (SystemStatus status) =>
            {
                var lcdWriter = app.Services.GetService<Lcd2004Writer>();
                lcdWriter?.ClearScreen();
                if (status.Message == string.Empty)
                {
                    if (status.IsSystemNormal)
                    {
                        lcdWriter?.Write("All systems \n normal");
                    }
                    else
                    {
                        lcdWriter?.Write("Unknown \n issue.");
                    }
                }
                else
                {
                    lcdWriter?.Write(status.Message);
                }
                
                if (status.IsSystemNormal)
                {
                    redLed.Off();
                    greenLed.On();
                }
                else
                {
                    redLed.On();
                    greenLed.Off();
                }
            });

            app.Run();
        }
    }

    public class SystemStatus
    {
        [JsonPropertyName("isSystemNormal")]
        public bool IsSystemNormal { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class MessageRequest
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class LedRequest
    {
        [JsonPropertyName("redLedOn")]
        public bool RedLedOn { get; set; }
        [JsonPropertyName("greenLedOn")]
        public bool GreenLedOn { get; set; }
    }
}
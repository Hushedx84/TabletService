using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace TabletServiceApp
{
    public class TabletService
    {
        private readonly HttpListener _listener;
        private readonly string _tabletsDataPath;

        public TabletService(string prefix, string tabletsDataPath)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _tabletsDataPath = tabletsDataPath;
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("TabletService слухає...");

            while (true)
            {
                HttpListenerResponse response = null;
                try
                {
                    var context = _listener.GetContext();
                    var request = context.Request;
                    response = context.Response;
                    string responseString = string.Empty;

                    if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/tablets")
                    {
                        var tablets = LoadTabletsFromCsv(_tabletsDataPath);
                        responseString = FormatTabletsAsText(tablets);
                        var buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    Console.WriteLine("Запрос получен: " + request.HttpMethod + " " + request.Url.AbsolutePath);
                }
                finally
                {
                    response?.OutputStream.Close();
                }
            }
        }

        private static List<Tablet> LoadTabletsFromCsv(string filePath)
        {
            Console.WriteLine($"Загрузка данных из: {filePath}");
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл не найден.");
                return new List<Tablet>();
            }

            var lines = File.ReadAllLines(filePath).Skip(1);
            var tablets = lines.Select(line =>
            {
                var parts = line.Split(',');
                return new Tablet
                {
                    Id = parts[0].Trim(),
                    Model = parts[1].Trim(),
                    Manufacturer = parts[2].Trim(),
                    OperatingSystem = parts[3].Trim(),
                    ScreenSize = parts[4].Trim(),
                    ScreenResolution = parts[5].Trim(),
                    RAM = parts[6].Trim(),
                    Storage = parts[7].Trim(),
                    Color = parts[8].Trim(),
                    Price = parts[9].Trim()
                };
            }).ToList();

            Console.WriteLine($"Загружено {tablets.Count} планшетов.");
            return tablets;
        }

        private static string FormatTabletsAsText(List<Tablet> tablets)
        {
            var stringBuilder = new StringBuilder();
            foreach (var tablet in tablets)
            {
                stringBuilder.AppendLine($"ID: {tablet.Id}, Модель: {tablet.Model}, Виробник: {tablet.Manufacturer}, Ціна: {tablet.Price}$");
            }
            return stringBuilder.ToString();
        }
    }

    public class Program
    {
        public static void Main()
        {
            string tabletsDataPath = "tablets.csv";
            TabletService tabletService = new("http://localhost:9091/", tabletsDataPath);
            tabletService.Start();
        }
    }

    public class Tablet
    {
        public string? Id { get; set; }
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public string? OperatingSystem { get; set; }
        public string? ScreenSize { get; set; }
        public string? ScreenResolution { get; set; }
        public string? RAM { get; set; }
        public string? Storage { get; set; }
        public string? Color { get; set; }
        public string? Price { get; set; }
    }
}
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows.Forms;

namespace Sem3Lab9Source2
{
    public partial class Form1 : Form
    {
        
        private string File_Path = "C:/Users/uriyd/Downloads/city.txt";

        public Form1()
        {
            InitializeComponent();
            LoadCityData();
        }

        // Метод для загрузки данных городов
        private void LoadCityData()
        {
            string[] lines = File.ReadAllLines(File_Path);
            foreach (string line in lines)
            {
                listBox1.Items.Add(line); // строку в listbox
            }
        }

        // изменения выбора в listBox
        private async void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Получаем выбранную строку
                string line = (string)listBox1.SelectedItem;
                // Разделяем строку 
                string[] parts = line.Split('\t');
                string[] temp = parts[1].Split(',');

                // координаты в тип double с учетом особенностей формата
                if (temp.Length < 2)
                {
                    MessageBox.Show("Некорректный формат данных для координат.");
                    return;
                }

                // Конвертируем строковые координаты в double
                double latitude = ParseCoordinate(temp[0]);
                double longitude = ParseCoordinate(temp[1]);

                // Получаем текущую погоду по координатам
                Weather currentWeather = await GetWeatherData(latitude, longitude);
                DisplayInfo(currentWeather);
            }
            catch (Exception ex)
            {
                // Выводим сообщение об ошибке
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Метод отображения информации о погоде в текстовом поле
        private void DisplayInfo(Weather weather)
        {
            textBox1.Text = $"Температура: {weather.Temp:F2}°C, Описание: {weather.Description}, Страна: {weather.Country}, Город: {weather.Name}";
        }

        // Метод для получения данных о погоде по широте и долготе
        private static async Task<Weather> GetWeatherData(double latitude, double longitude)
        {
            string apiKey = "884603ba7ae4aa806e5952eafaea10f0";
            string path = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={apiKey}";
            Weather weather;

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    // Обрабатываем ответ сервера и парсим данные о погоде
                    var dataString = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(dataString);
                    weather = new Weather()
                    {
                        Country = (string)data["sys"]["country"],
                        Name = (string)data["name"],
                        Temp = Convert.ToDouble(data["main"]["temp"]) - 273.15, // Конвертация температуры из Кельвинов в Цельсии

                        Description = (string)data["weather"][0]["description"]
                    };
                }
                else
                {
                    throw new Exception("Не удалось получить данные о погоде.");
                }
            }
            return weather;
        }

        // Метод для безопасного парсинга координат
        private static double ParseCoordinate(string coordinate)
        {
            // Убираем пробелы и заменяем запятые на точки (если необходимо)
            coordinate = coordinate.Trim().Replace(',', '.');
            if (double.TryParse(coordinate, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result; // Возвращаем коориднату, если парсинг успешен
            }
            throw new Exception($"Некорректный формат координаты: {coordinate}");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // Структура для хранения информации о погоде
        public struct Weather
        {
            public string Country { get; set; }
            public string Name { get; set; }
            public double Temp { get; set; }
            public string Description { get; set; }
        }
    }
}

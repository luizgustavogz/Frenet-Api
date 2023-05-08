using System.Data.SqlClient;
using System.Text.Json;

public class FrenetEntregaService
{
    private const string ApiKey = "ApiKey";

    private readonly HttpClient _httpClient;
    private readonly string _connectionString;

    public FrenetEntregaService(string connectionString)
    {
        _httpClient = new HttpClient();
        _connectionString = connectionString;
    }

    public async Task<decimal> CalcularPreco(decimal height, decimal length, int quantity, decimal weight, decimal width)
    {
        decimal price = length * width * height / 6000;
        return price;
    }

    public async Task<FrenetEntregaResponse> CalcularEntrega(decimal height, decimal length, int quantity, decimal weight, decimal width, string zipCodeFrom, string zipCodeTo)
    {
        var price = await CalcularPreco(height, length, quantity, weight, width);

        var request = new
        {
            SellerCEP = zipCodeFrom,
            RecipientCEP = zipCodeTo,
            ShipmentInvoiceValue = price,
            ShipmentWeight = weight,
            EntregaItemArray = new[]
            {
                new
                {
                    Weight = weight,
                    Length = length,
                    Height = height,
                    Width = width,
                    Quantity = quantity
                }
            }
        };

        var requestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"https://frenetapi.docs.apiary.io/shipping/quote?api_key={ApiKey}", content);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<FrenetEntregaResponse>(responseJson);
        await InsertEntregaIntoDatabase(height, length, quantity, weight, width, zipCodeFrom, zipCodeTo, price, result.Price, result.DeliveryTime);

        return result;
    }

    private async Task InsertEntregaIntoDatabase(decimal height, decimal length, int quantity, decimal weight, decimal width, string zipCodeFrom, string zipCodeTo, decimal price, decimal EntregaPrice, int deliveryTime)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            INSERT INTO Entrega (Height, Length, Quantity, Weight, Width, ZipCodeFrom, ZipCodeTo, Price, EntregaPrice, DeliveryTime, CreatedAt)
            VALUES (@Height, @Length, @Quantity, @Weight, @Width, @ZipCodeFrom, @ZipCodeTo, @Price, @EntregaPrice, @DeliveryTime, GETDATE())
        ";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Height", height);
        command.Parameters.AddWithValue("@Length", length);
        command.Parameters.AddWithValue("@Quantity", quantity);
        command.Parameters.AddWithValue("@Weight", weight);
        command.Parameters.AddWithValue("@Width", width);
        command.Parameters.AddWithValue("@ZipCodeFrom", zipCodeFrom);
        command.Parameters.AddWithValue("@ZipCodeTo", zipCodeTo);
        command.Parameters.AddWithValue("@Price", price);
        command.Parameters.AddWithValue("@EntregaPrice", EntregaPrice);
        command.Parameters.AddWithValue("@DeliveryTime", deliveryTime);

        await command.ExecuteNonQueryAsync();
    }
}

public class FrenetEntregaResponse
{
    public decimal Price { get; set; }
    public int DeliveryTime { get; set; }
}

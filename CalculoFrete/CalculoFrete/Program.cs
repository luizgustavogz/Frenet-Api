public class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "DefaultConnection";
        var service = new FrenetEntregaService(connectionString);

        Console.Write("Enter height (cm): ");
        var height = decimal.Parse(Console.ReadLine());

        Console.Write("Enter length (cm): ");
        var length = decimal.Parse(Console.ReadLine());

        Console.Write("Enter quantity (unt): ");
        var quantity = int.Parse(Console.ReadLine());

        Console.Write("Enter weight (kg): ");
        var weight = decimal.Parse(Console.ReadLine());

        Console.Write("Enter width (cm): ");
        var width = decimal.Parse(Console.ReadLine());

        Console.Write("Enter sender zip code: ");
        var zipCodeFrom = Console.ReadLine();

        Console.Write("Enter recipient zip code: ");
        var zipCodeTo = Console.ReadLine();

        var shipping = await service.CalcularEntrega(height, length, quantity, weight, width, zipCodeFrom, zipCodeTo);

        Console.WriteLine($"Shipping price: {shipping.Price}");
        Console.WriteLine($"Delivery time: {shipping.DeliveryTime}");
    }
}
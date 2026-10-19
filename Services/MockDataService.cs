using GarageTrack.Models;

namespace GarageTrack.Services;

/// <summary>
/// Données fictives en mémoire (frontend uniquement, aucun backend).
/// Enregistré en Singleton : toutes les pages partagent les mêmes données pendant la session.
/// </summary>
public class MockDataService
{
    public const decimal LaborRate = 10000m;

    public string GarageName => "Garage du Wouri";
    public string GarageAddress => "Boulevard de la Liberté, Akwa · Douala";
    public string UserFullName { get; set; } = "Rodrigue Fokou";
    public string UserFirstName => UserFullName.Split(' ')[0];
    public string UserEmail { get; set; } = "rodrigue.fokou@gmail.com";
    public string UserPhone { get; set; } = "677 42 81 59";

    public List<Client> Clients { get; } = new();
    public List<Vehicle> Vehicles { get; } = new();
    public List<RepairOrder> Orders { get; } = new();

    public List<string> Mechanics { get; } = new() { "Ibrahim Moussa", "Josiane Mvondo", "Arnaud Tchakounté" };

    public List<string> TaskCatalog { get; } = new()
    {
        "Diagnostic du freinage avant",
        "Remplacement disques et plaquettes",
        "Contrôle et essai routier",
        "Contrôle des pneumatiques",
    };

    public List<QuoteLine> PartCatalog { get; } = new()
    {
        new() { Label = "Liquide de frein DOT 4", Reference = "BOS-1987479106", UnitPrice = 5000m },
        new() { Label = "Filtre à huile", Reference = "BOS-F026407157", UnitPrice = 6500m },
        new() { Label = "Balais d’essuie-glace", Reference = "BOS-3397007297", UnitPrice = 12000m },
        new() { Label = "Ampoule H7", Reference = "BOS-1987302777", UnitPrice = 3500m },
    };

    public List<QuoteLine> LaborCatalog { get; } = new()
    {
        new() { Label = "Purge du circuit de freinage", Reference = "MO-PURGE", Quantity = 0.5m, UnitPrice = LaborRate, IsLabor = true },
        new() { Label = "Diagnostic électronique", Reference = "MO-DIAG", Quantity = 1m, UnitPrice = LaborRate, IsLabor = true },
        new() { Label = "Révision constructeur", Reference = "MO-REV", Quantity = 1.5m, UnitPrice = LaborRate, IsLabor = true },
    };

    private int _nextOrder = 143;
    private int _nextQuote = 88;

    public MockDataService() => Seed();

    // ---------- Requêtes ----------
    public Client? GetClient(string? id) => Clients.FirstOrDefault(c => c.Id == id);
    public Vehicle? GetVehicle(string? id) => Vehicles.FirstOrDefault(v => v.Id == id);
    public RepairOrder? GetOrder(string? number) => Orders.FirstOrDefault(o => o.Number == number);
    public IEnumerable<Vehicle> VehiclesOf(string clientId) => Vehicles.Where(v => v.ClientId == clientId);

    public RepairOrder? CurrentOrderFor(string vehicleId) =>
        Orders.Where(o => o.Vehicle.Id == vehicleId && o.Status != OrderStatus.Done)
              .OrderByDescending(o => o.ReceivedAt)
              .FirstOrDefault();

    public IEnumerable<RepairOrder> RecentOrders => Orders.OrderByDescending(o => o.ReceivedAt);

    public int RepairsInProgress => Orders.Count(o =>
        o.Status is OrderStatus.InRepair or OrderStatus.MissingPart or OrderStatus.Received);

    public int QuotesToValidate => Orders.Count(o =>
        o.Status is OrderStatus.QuotePending or OrderStatus.QuoteToSign or OrderStatus.QuoteInProgress);

    public string PeekNextOrderNumber() => $"OR-{DateTime.Today.Year}-{_nextOrder:0000}";

    // ---------- Commandes ----------
    public RepairOrder CreateOrder(Client client, Vehicle vehicle, int mileage, string symptoms,
                                   IEnumerable<string> tasks, string mechanic)
    {
        var taskList = tasks.ToList();
        var order = new RepairOrder
        {
            Number = $"OR-{DateTime.Today.Year}-{_nextOrder++:0000}",
            Client = client,
            Vehicle = vehicle,
            Status = OrderStatus.Received,
            ReceivedAt = DateTime.Now,
            Title = taskList.Any(t => t.Contains("frein", StringComparison.OrdinalIgnoreCase)) ? "Freinage avant" : "Intervention atelier",
            Mechanic = mechanic,
            Symptoms = symptoms,
            EntryMileage = mileage,
            CheckedTasks = taskList,
        };

        vehicle.LastReadingKm = vehicle.Mileage;
        vehicle.LastReadingDate = vehicle.History.FirstOrDefault()?.Date ?? DateTime.Today;
        vehicle.Mileage = mileage;

        // Pré-chiffrage à partir de la checklist (le garagiste ajuste à l'étape 2)
        if (taskList.Contains("Remplacement disques et plaquettes"))
        {
            order.Parts.Add(new QuoteLine { Label = "Disques avant (paire)", Reference = "BOS-0986479R", Quantity = 1, UnitPrice = 75000m });
            order.Parts.Add(new QuoteLine { Label = "Plaquettes avant", Reference = "BOS-0986494K", Quantity = 1, UnitPrice = 30000m });
        }
        if (taskList.Count > 0)
        {
            order.Labor.Add(new QuoteLine
            {
                Label = order.Title == "Freinage avant" ? "Freinage + essai routier" : "Main-d’œuvre atelier",
                Reference = order.Title == "Freinage avant" ? "MO-FREIN" : "MO-ATEL",
                Mechanic = mechanic,
                Quantity = order.Title == "Freinage avant" ? 2.5m : 1m,
                UnitPrice = LaborRate,
                IsLabor = true,
            });
        }

        Orders.Add(order);
        return order;
    }

    public void PrepareQuote(RepairOrder order)
    {
        if (string.IsNullOrEmpty(order.QuoteNumber))
            order.QuoteNumber = $"D-{DateTime.Today.Year}-{_nextQuote++:0000}";
        order.Status = OrderStatus.QuoteToSign;
    }

    public void AcceptQuote(RepairOrder order)
    {
        order.Status = OrderStatus.InRepair;
        order.QuoteAcceptedAt = DateTime.Now;
        order.PlannedReturn = DateTime.Today.AddHours(17);
    }

    public Client AddClient(string fullName, string phone, string email, string address)
    {
        var client = new Client
        {
            FullName = fullName,
            Phone = phone,
            Email = email,
            AddressLine = address,
            ClientSince = DateTime.Today,
            IsVerified = false,
        };
        Clients.Add(client);
        return client;
    }

    // ---------- Jeu de données ----------
    private void Seed()
    {
        var today = DateTime.Today;

        var mireille = AddSeedClient("Mireille Ngono", new DateTime(2024, 3, 2), "699 12 34 56", "mireille.ngono@gmail.com", "Rue Joss, Bonanjo", "Douala");
        var serge = AddSeedClient("Serge Mbarga", new DateTime(2023, 9, 14), "677 45 22 81", "serge.mbarga@gmail.com", "Quartier Bastos", "Yaoundé");
        var brigitte = AddSeedClient("Brigitte Fotso", new DateTime(2025, 1, 20), "655 61 50 33", "brigitte.fotso@gmail.com", "Rue Njo-Njo, Bonapriso", "Douala");
        var jeanPaul = AddSeedClient("Jean-Paul Ekotto", new DateTime(2022, 6, 8), "670 98 14 72", "jeanpaul.ekotto@gmail.com", "Quartier Tamdja", "Bafoussam");
        var atangana = AddSeedClient("Atangana Bonaventure", new DateTime(2024, 4, 11), "694 53 26 44", "a.bonaventure@gmail.com", "16 avenue 3, Bepanda", "Douala");
        var clarisse = AddSeedClient("Clarisse Tchoumi", new DateTime(2025, 5, 3), "691 22 47 18", "clarisse.tchoumi@gmail.com", "Quartier Mvog-Mbi", "Yaoundé");
        var herve = AddSeedClient("Hervé Kamga", new DateTime(2021, 11, 27), "680 70 81 26", "herve.kamga@gmail.com", "Quartier Molyko", "Buea");
        var solange = AddSeedClient("Solange Nkolo", new DateTime(2024, 8, 19), "656 82 11 64", "solange.nkolo@gmail.com", "Carrefour Ndokoti", "Douala");
        var patrick = AddSeedClient("Patrick Essomba", new DateTime(2023, 2, 10), "675 33 90 17", "patrick.essomba@gmail.com", "Quartier Essos", "Yaoundé");
        var estelle = AddSeedClient("Estelle Manga", new DateTime(2025, 7, 1), "698 58 26 39", "estelle.manga@gmail.com", "Commercial Avenue", "Bamenda");
        var christian = AddSeedClient("Christian Njoya", new DateTime(2022, 10, 5), "651 44 61 82", "christian.njoya@gmail.com", "Bonamoussadi", "Douala");

        var history308 = new List<Intervention>
        {
            new() { Date = new DateTime(2026, 6, 12), Title = "Révision & vidange", OrderNumber = "OR-2026-0086", Mileage = 118420, Description = "Huile 5W30, filtre à huile, contrôle 20 points.", TotalTtc = 65000m },
            new() { Date = new DateTime(2025, 11, 18), Title = "Remplacement de la batterie", OrderNumber = "OR-2025-0234", Mileage = 109200, Description = "Batterie 70 Ah et contrôle du circuit de charge.", TotalTtc = 85000m },
            new() { Date = new DateTime(2024, 4, 22), Title = "Pneumatiques avant", OrderNumber = "OR-2024-0061", Mileage = 92600, Description = "2 pneus 205/55 R16, montage et équilibrage.", TotalTtc = 110000m },
        };

        var p308 = AddVehicle(mireille, "LT 123 CD", "Peugeot", "308", "1.6 BlueHDi", 2018, "Diesel", "Manuelle", 124850, "VF3LCBHZHJS123456", new DateTime(2018, 9, 18), history308);
        var corolla = AddVehicle(serge, "CE 456 JK", "Toyota", "Corolla", "1.8 VVT-i", 2015, "Essence", "Automatique", 161200, "JTDBR32E560123987", new DateTime(2015, 3, 4));
        var c3 = AddVehicle(brigitte, "LT 789 NP", "Citroën", "C3", "1.2 PureTech", 2019, "Essence", "Manuelle", 78400, "VF7SXHMZ6KT502114", new DateTime(2019, 5, 27));
        var hilux = AddVehicle(jeanPaul, "OU 234 ST", "Toyota", "Hilux", "2.5 D-4D", 2016, "Diesel", "Manuelle", 182300, "AHTFR22G106118842", new DateTime(2016, 2, 15));
        var atanganaCar = AddVehicle(atangana, "LT 704 HK", "Peugeot", "308", "1.6 BlueHDi", 2018, "Diesel", "Manuelle", 124850, "VF3LCBHZHJS778201", new DateTime(2018, 7, 2),
            new List<Intervention>(history308.Select(h => new Intervention { Date = h.Date, Title = h.Title, OrderNumber = h.OrderNumber, Mileage = h.Mileage, Description = h.Description, TotalTtc = h.TotalTtc })));
        var tucson = AddVehicle(clarisse, "CE 318 LR", "Hyundai", "Tucson", "2.0 MPi", 2019, "Essence", "Automatique", 68900, "KMHJ381ADKU241785", new DateTime(2019, 6, 10));
        var yaris = AddVehicle(herve, "SW 552 BT", "Toyota", "Yaris", "1.5 Hybride", 2022, "Hybride", "Automatique", 45210, "VNKKH3D30NA108233", new DateTime(2022, 1, 18));
        var rav4 = AddVehicle(solange, "LT 901 MC", "Toyota", "RAV4", "2.0 VVT-i", 2017, "Essence", "Automatique", 97600, "JTMBF4DV0HD554210", new DateTime(2017, 10, 3));
        var p2008 = AddVehicle(patrick, "CE 147 KP", "Peugeot", "2008", "1.2 PureTech", 2021, "Essence", "Manuelle", 52750, "VR3USHNSSMJ610498", new DateTime(2021, 4, 22));
        var swift = AddVehicle(estelle, "NW 683 ZE", "Suzuki", "Swift", "1.2 DualJet", 2020, "Essence", "Manuelle", 49400, "MA3ZC83S0LM072155", new DateTime(2020, 9, 9));
        var prado = AddVehicle(christian, "LT 275 WN", "Toyota", "Land Cruiser Prado", "3.0 D-4D", 2014, "Diesel", "Automatique", 218900, "JTEBH3FJ10K381604", new DateTime(2014, 12, 1));

        // OR-2026-0142 : celui des maquettes (freinage avant, Mireille Ngono)
        var or142 = AddOrder("OR-2026-0142", mireille, p308, OrderStatus.InRepair, today.AddHours(9).AddMinutes(20), "Freinage avant", "Ibrahim Moussa");
        or142.Symptoms = "Bruit métallique au freinage et vibrations dans le volant. Contrôler le train avant.";
        or142.EntryMileage = 124850;
        or142.CheckedTasks = TaskCatalog.Take(3).ToList();
        or142.QuoteNumber = "D-2026-0087";
        or142.QuoteAcceptedAt = today.AddHours(9).AddMinutes(35);
        or142.PlannedReturn = today.AddHours(17);
        or142.Parts.Add(new QuoteLine { Label = "Disques avant (paire)", Reference = "BOS-0986479R", Quantity = 1, UnitPrice = 75000m });
        or142.Parts.Add(new QuoteLine { Label = "Plaquettes avant", Reference = "BOS-0986494K", Quantity = 1, UnitPrice = 30000m });
        or142.Labor.Add(new QuoteLine { Label = "Freinage + essai routier", Reference = "MO-FREIN", Mechanic = "Ibrahim Moussa", Quantity = 2.5m, UnitPrice = LaborRate, IsLabor = true });

        var or141 = AddOrder("OR-2026-0141", serge, corolla, OrderStatus.QuotePending, today.AddHours(8).AddMinutes(50), "Distribution", "Josiane Mvondo");
        or141.QuoteNumber = "D-2026-0086";
        or141.Parts.Add(new QuoteLine { Label = "Kit de distribution", Reference = "BOS-1987946597", Quantity = 1, UnitPrice = 120000m });
        or141.Labor.Add(new QuoteLine { Label = "Remplacement distribution", Reference = "MO-DISTRI", Mechanic = "Josiane Mvondo", Quantity = 3m, UnitPrice = LaborRate, IsLabor = true });

        AddOrder("OR-2026-0140", brigitte, c3, OrderStatus.Done, today.AddHours(8).AddMinutes(15), "Révision", "Arnaud Tchakounté");
        AddOrder("OR-2026-0139", jeanPaul, hilux, OrderStatus.MissingPart, today.AddDays(-1).AddHours(16).AddMinutes(10), "Embrayage", "Ibrahim Moussa");
        AddOrder("OR-2026-0138", clarisse, tucson, OrderStatus.InRepair, today.AddDays(-1).AddHours(14), "Climatisation", "Josiane Mvondo");
        AddOrder("OR-2026-0137", herve, yaris, OrderStatus.InRepair, today.AddDays(-1).AddHours(11).AddMinutes(30), "Pneumatiques", "Arnaud Tchakounté");
        AddOrder("OR-2026-0136", solange, rav4, OrderStatus.Received, today.AddDays(-1).AddHours(9), "Diagnostic voyant moteur", "Ibrahim Moussa");
        AddOrder("OR-2026-0135", patrick, p2008, OrderStatus.InRepair, today.AddDays(-2).AddHours(15), "Échappement", "Arnaud Tchakounté");
        AddOrder("OR-2026-0134", estelle, swift, OrderStatus.InRepair, today.AddDays(-2).AddHours(10), "Amortisseurs arrière", "Josiane Mvondo");
        AddOrder("OR-2026-0133", christian, prado, OrderStatus.InRepair, today.AddDays(-3).AddHours(9).AddMinutes(40), "Courroie accessoires", "Ibrahim Moussa");
        AddOrder("OR-2026-0132", atangana, atanganaCar, OrderStatus.QuotePending, today.AddDays(-3).AddHours(8).AddMinutes(30), "Freinage arrière", "Arnaud Tchakounté");
        AddOrder("OR-2026-0131", brigitte, c3, OrderStatus.QuotePending, today.AddDays(-4).AddHours(17), "Rétroviseur", "Josiane Mvondo").ReceivedAt = today.AddDays(-4);
    }

    private Client AddSeedClient(string name, DateTime since, string phone, string email, string address, string city)
    {
        var women = new[] { "Mireille", "Brigitte", "Clarisse", "Solange", "Estelle" };
        var c = new Client
        {
            FullName = name, ClientSince = since, Phone = phone, Email = email, AddressLine = address, City = city,
            IsWoman = women.Contains(name.Split(' ')[0]),
        };
        Clients.Add(c);
        return c;
    }

    private Vehicle AddVehicle(Client owner, string plate, string brand, string model, string engine, int year,
                               string fuel, string gearbox, int km, string vin, DateTime firstReg,
                               List<Intervention>? history = null)
    {
        var v = new Vehicle
        {
            ClientId = owner.Id, Plate = plate, Brand = brand, Model = model, Engine = engine, Year = year,
            Fuel = fuel, Gearbox = gearbox, Mileage = km, Vin = vin, FirstRegistration = firstReg,
            History = history ?? new List<Intervention>(),
        };
        v.LastReadingKm = v.History.FirstOrDefault()?.Mileage ?? km;
        v.LastReadingDate = v.History.FirstOrDefault()?.Date ?? firstReg;
        Vehicles.Add(v);
        return v;
    }

    private RepairOrder AddOrder(string number, Client c, Vehicle v, OrderStatus status, DateTime at, string title, string mechanic)
    {
        var o = new RepairOrder
        {
            Number = number, Client = c, Vehicle = v, Status = status, ReceivedAt = at,
            Title = title, Mechanic = mechanic, EntryMileage = v.Mileage,
        };
        Orders.Add(o);
        return o;
    }
}

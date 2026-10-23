using System.Collections.Generic;
using K = Abandoned.EditorTools.StationTownBuildings.Kind;
using V = Abandoned.EditorTools.StationVehicleWear;
using C = Abandoned.EditorTools.StationVehicleKind;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The mixes the outer rings are drawn from, one per kind of place: a brick town round the stations, a medical
    /// district round the hospital, a roadside strip round the motel. Rows facing the town are its busier street fronts;
    /// rows facing out are backs of blocks (row houses, warehouses, garages); the edge is mostly houses in their yards.
    /// A design listed twice turns up twice as often. Block rows stay within StationRings.RowDepth, the edge within EdgeDepth.
    /// </summary>
    public static class StationRingPools
    {
        public static void Town(StationRingStock k, StationRingSpec s, int seed)
        {
            int row3 = k.Building(K.Rowhouse, 7f, 14f, 3, seed + 4), row2 = k.Building(K.Rowhouse, 7f, 14f, 2, seed + 6);
            Add(s.Front, k.Building(K.Shop, 10f, 18f, 2, seed + 1), k.Building(K.Shop, 8f, 17f, 3, seed + 2), k.Building(K.Shop, 12f, 18f, 1, seed + 3),
                row3, row3, k.Building(K.Rowhouse, 7f, 14f, 3, seed + 5), row2, k.Building(K.Apartment, 18f, 22f, 5, seed + 7),
                k.Building(K.Apartment, 20f, 21f, 4, seed + 8), k.Building(K.Garage, 16f, 20f, 1, seed + 9), k.Building(K.Office, 22f, 22f, 5, seed + 10));
            Add(s.Back, row3, row2, k.Building(K.Rowhouse, 7f, 14f, 3, seed + 11), k.Building(K.Apartment, 22f, 22f, 6, seed + 13),
                k.Building(K.Warehouse, 22f, 22f, 1, seed + 14), k.Building(K.Warehouse, 18f, 20f, 1, seed + 15), k.Building(K.Garage, 18f, 20f, 1, seed + 16),
                k.Building(K.Shop, 10f, 18f, 2, seed + 17));
            int h1 = k.House(seed + 21), h2 = k.House(seed + 22), h3 = k.House(seed + 23), h4 = k.House(seed + 24);
            Add(s.Edge, h1, h2, h3, h4, h1, h2, h3, h4, k.Building(K.Warehouse, 26f, 26f, 1, seed + 25), k.Building(K.Church, 16f, 24f, 1, seed + 26),
                k.Building(K.GasStation, 24f, 22f, 1, seed + 27), k.Building(K.Apartment, 24f, 24f, 4, seed + 28), row3);
            Cars(k, s, seed + 40);
        }

        public static void District(StationRingStock k, StationRingSpec s, int seed)
        {
            int row3 = k.Building(K.Rowhouse, 7f, 14f, 3, seed + 6);
            Add(s.Front, k.Building(K.Office, 24f, 22f, 6, seed + 1), k.Building(K.Office, 22f, 22f, 8, seed + 2), k.Building(K.Apartment, 20f, 22f, 6, seed + 3),
                k.Building(K.Shop, 10f, 16f, 2, seed + 4), k.Building(K.Shop, 9f, 16f, 3, seed + 5), row3, row3, k.Building(K.Apartment, 18f, 21f, 5, seed + 7),
                k.Building(K.Shop, 12f, 18f, 1, seed + 8));
            Add(s.Back, k.Building(K.Apartment, 22f, 22f, 5, seed + 11), k.Building(K.Office, 26f, 22f, 4, seed + 12), k.Building(K.Warehouse, 24f, 22f, 1, seed + 13),
                row3, k.Building(K.Rowhouse, 7f, 14f, 2, seed + 14), k.Building(K.Garage, 18f, 20f, 1, seed + 15));
            int h1 = k.House(seed + 21), h2 = k.House(seed + 22);
            Add(s.Edge, k.Building(K.ParkingDeck, 26f, 30f, 4, seed + 23), k.Building(K.Office, 26f, 26f, 7, seed + 24), k.Building(K.Apartment, 24f, 26f, 6, seed + 25),
                k.Building(K.Church, 16f, 24f, 1, seed + 26), k.Building(K.Warehouse, 26f, 26f, 1, seed + 27), k.Building(K.GasStation, 24f, 22f, 1, seed + 28),
                h1, h2, h1, h2, k.Building(K.Apartment, 20f, 24f, 4, seed + 29));
            Cars(k, s, seed + 40);
        }

        public static void Roadside(StationRingStock k, StationRingSpec s, int seed)
        {
            int h1 = k.House(seed + 1), h2 = k.House(seed + 2), h3 = k.House(seed + 3), h4 = k.House(seed + 4);
            int garage = k.Building(K.Garage, 16f, 18f, 1, seed + 5), shop = k.Building(K.Shop, 12f, 16f, 1, seed + 6);
            Add(s.Front, k.Building(K.GasStation, 24f, 22f, 1, seed + 7), shop, k.Building(K.StripMall, 30f, 18f, 1, seed + 8), garage,
                h1, h2, h3, k.Building(K.Warehouse, 24f, 22f, 1, seed + 9), k.Building(K.Shop, 10f, 16f, 2, seed + 10));
            Add(s.Back, k.Building(K.Warehouse, 26f, 22f, 1, seed + 11), k.Building(K.Garage, 18f, 20f, 1, seed + 12), h2, h4, shop, garage);
            Add(s.Edge, h1, h2, h3, h4, h1, h3, k.Building(K.Warehouse, 30f, 26f, 1, seed + 13), k.Building(K.Church, 16f, 22f, 1, seed + 14),
                k.Building(K.Garage, 20f, 22f, 1, seed + 15), k.Building(K.StripMall, 36f, 16f, 1, seed + 16));
            Cars(k, s, seed + 40);
        }

        private static void Cars(StationRingStock k, StationRingSpec s, int seed)
        {
            C[] kinds = { C.Sedan, C.Hatchback, C.Suv, C.Pickup, C.Sedan, C.Hatchback, C.Suv, C.Sedan };
            V[] wear = { V.Dusty, V.Dusty | V.FlatFront, V.Dusty | V.Smashed, V.Burnt, V.Dusty | V.DoorOpen, V.Dusty | V.FlatRear, V.Dusty, V.Dusty | V.FlatFront | V.Smashed };
            for (int i = 0; i < kinds.Length; i++) s.Cars.Add(k.Car(kinds[i], seed + i * 3, wear[i]));
        }

        private static void Add(List<int> pool, params int[] designs) => pool.AddRange(designs);
    }
}

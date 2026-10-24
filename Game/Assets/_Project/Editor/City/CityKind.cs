namespace Abandoned.EditorTools
{
    /// <summary>
    /// What a city district is made of. The kind decides a block's massing in the rough draft (footprint depth,
    /// storey height, how tall it may go) and later which facades and street furniture it gets, so a district is
    /// described once here instead of every builder guessing from where it is.
    /// </summary>
    public enum CityKind
    {
        /// <summary>Towers round the Circus: deep footprints, 80-180 m.</summary>
        Downtown,
        /// <summary>Shops and department stores: wide shallow blocks, 12-20 m.</summary>
        Retail,
        /// <summary>Civic and institutional: set back off the street, 14-28 m.</summary>
        Civic,
        /// <summary>Terraces and walk-ups filling their block, 8-14 m.</summary>
        Residential,
        /// <summary>Sheds, yards and tanks: few, large, 10-24 m.</summary>
        Industrial,
        /// <summary>Docks and warehouses on the coast, 8-18 m.</summary>
        Waterfront,
        /// <summary>Houses on lots with gaps between them, 6-10 m.</summary>
        Suburb,
        /// <summary>Open ground: no massing at all, trees and paths.</summary>
        Park,
    }
}

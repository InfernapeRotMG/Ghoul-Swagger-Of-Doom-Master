namespace wServer.realm.worlds
{
    public class ClothBazaar : World
    {
        public ClothBazaar()
        {
            Id = MARKET;
            Name = "Cloth Bazaar";
            ClientWorldName = "Cloth Bazzar";
            Background = 2;
            AllowTeleport = false;
            Difficulty = 0;
        }

        protected override void Init()
        {
            LoadMap("wServer.realm.worlds.maps.bazzar.wmap", MapType.Wmap);
        }
    }
}
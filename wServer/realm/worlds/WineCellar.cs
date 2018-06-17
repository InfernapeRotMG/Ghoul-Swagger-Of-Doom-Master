#region

using wServer.networking;

#endregion

namespace wServer.realm.worlds
{
    public class WineCellar : World
    {
        public WineCellar()
        {
            Name = "Wine Cellar";
            ClientWorldName = "Wine Cellar";
            Background = 0;
            AllowTeleport = false;
            Dungeon = true;
        }

        protected override void Init()
        {
            LoadMap("wServer.realm.worlds.maps.winecellar.wmap", MapType.Wmap);
        }

        public override World GetInstance(Client psr)
        {
            return Manager.AddWorld(new WineCellar());
        }
    }
}
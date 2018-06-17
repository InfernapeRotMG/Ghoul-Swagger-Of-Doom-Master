namespace wServer.realm.worlds
{
    public class MadLab : World
    {
        public MadLab()
        {
            Name = "Mad Lab";
            ClientWorldName = "Mad Lab";
            Background = 0;
            Difficulty = 5;
            AllowTeleport = true;
        }

        protected override void Init()
        {
            LoadMap(GeneratorCache.NextLab(Seed));
        }
    }
}

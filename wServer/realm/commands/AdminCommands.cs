using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using db;
using wServer.networking;
using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm.entities;
using wServer.realm.entities.player;
using wServer.realm.setpieces;
using wServer.realm.worlds;

namespace wServer.realm.commands
{
    internal class BanCommand : Command
    {
        public BanCommand() :
            base("ban", permLevel: 2)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            var p = player.Manager.FindPlayer(args[1]);
            if (p == null)
            {
                player.SendError("Player not found");
                return false;
            }
            player.Manager.Database.DoActionAsync(db =>
            {
                var cmd = db.CreateQuery();
                cmd.CommandText = "UPDATE accounts SET banned=1 WHERE id=@accId;";
                cmd.Parameters.AddWithValue("@accId", p.AccountId);
                cmd.ExecuteNonQuery();
            });
            player.SendInfo("{p} has successfully been banned");
            return true;
        }
    }
    
        internal class rankCommand : Command
    {
        public rankCommand() : base("rank", 3) { }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length > 2)
            {
                player.SendHelp("Usage: /rank <Player Name> <Rank Number>");
                return false;
            }
            player.Manager.Database.DoActionAsync(db =>
            {
                var cmd = db.CreateQuery();
                cmd.CommandText = "UPDATE accounts SET rank=@rank WHERE name=@name";
                cmd.Parameters.AddWithValue("@name", args[0]);
                cmd.Parameters.AddWithValue("@rank", args[1]);
                if (cmd.ExecuteNonQuery() == 0)
                {
                    player.SendError(args[0] + " could not be ranked!");
                }
                else
                {
                    player.SendInfo(args[0] + " successfully ranked");
                }
            });
            return true;
        }
    }
    
        internal class QuitCommand : Command
    {
        public QuitCommand()
            : base("quit", 3)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            player.Client.SendPacket(new TextPacket
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "@INFO",
                Text = "Server is shutting down in 1 minute. Leave the server to prevent account in use!"
            });
            player.Owner.Timers.Add(new WorldTimer(60000, (world, t) =>
            {
                Environment.Exit(0);
            }));
            return true;
        }
    }
    
        internal class VisitCommand : Command
        {
        public VisitCommand()
            : base("visit", 3)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {


            foreach (KeyValuePair<string, Client> i in player.Manager.Clients)
            {
                if (i.Value.Player.Name.EqualsIgnoreCase(args[0]))
                {
                    Packet pkt;
                    if (i.Value.Player.Owner == player.Owner)
                    {
                        player.Move(i.Value.Player.X, i.Value.Player.Y);
                        pkt = new GotoPacket
                        {
                            ObjectId = player.Id,
                            Position = new Position(i.Value.Player.X, i.Value.Player.Y)
                        };
                        i.Value.Player.UpdateCount++;
                        player.SendInfo("He is already here. ");
                        return false;
                    }
                    else
                    {
                        player.Client.Reconnect(new ReconnectPacket()
                        {
                            GameId = i.Value.Player.Owner.Id,
                            Host = "",
                            IsFromArena = false,
                            Key = Empty<byte>.Array,
                            KeyTime = -1,
                            Name = i.Value.Player.Owner.Name,
                            Port = -1,
                        });
                        player.SendInfo("You are visiting " + i.Value.Player.Owner.Id + " now");


                        return true;
                    }
                }
            }
            player.SendError(string.Format("Player '{0}' could not be found!", args));
            return false;
        }
    }
    
    internal class GodLandsCommand : Command
    {
        public GodLandsCommand()
            : base("glands", 0)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.Owner is Nexus || player.Owner is PetYard || player.Owner is ClothBazaar || player.Owner is Vault || player.Owner is GuildHall)
            {
                player.SendInfo("You cannot use this command here. Please enter the realm to use this.");
                return false;
            }
            string[] random = "950|960|970|980|990|1000|1010|1020|1030|1040|1050|1060|1070|1080|1090|1100|1100|1110|1120|1130|1140|1050".Split('|');
            int tplocation = new Random().Next(random.Length);
            string topdank = random[tplocation];
            int x, y;
            try
            {
                x = int.Parse(topdank);
                y = int.Parse(topdank);
            }
            catch
            {
                player.SendError("Invalid coordinates!");
                return false;
            }
            player.Move(x + 0.5f, y + 0.5f);
            if (player.Pet != null)
                player.Pet.Move(x + 0.5f, y + 0.5f);
            player.UpdateCount++;
            player.Owner.BroadcastPacket(new GotoPacket
            {
                ObjectId = player.Id,
                Position = new Position
                {
                    X = player.X,
                    Y = player.Y
                }
            }, null);
            player.ApplyConditionEffect(new ConditionEffect()
            {
                Effect = ConditionEffectIndex.Invulnerable,
                DurationMS = 2500,
            });
            player.ApplyConditionEffect(new ConditionEffect()
            {
                Effect = ConditionEffectIndex.Invisible,
                DurationMS = 2500,
            });
            return true;
        }
    }
    
    internal class CFameCommand : Command
    {
        public CFameCommand()
        : base("cfame", 3)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
        int newFame = Convert.ToInt32(args[0]);
        int newXP = Convert.ToInt32(newFame.ToString() + "000");
            if (args[0] == "")
            {
                player.SendHelp("Usage: /cfame <Amount>");
                return false;
            }
                if (!(player.Client.Account.Rank == 1 || player.Client.Account.Rank == 2) && newFame > 2000)
                {
                    player.SendError("Maximum fame amount is 2000!");
                    return false;
                }
                if (player.Client.Account.Rank == 3 && newFame > 2000)
                {
                    player.SendInfo("Bypass granted, increasing fame!");
                }
            try
            {
                player.Fame = newFame;
                player.Experience = newXP;
                player.SaveToCharacter();
                player.Client.Save();
                player.UpdateCount++;
                player.SendInfo("New Character Fame: " + newFame);
            }
            catch
            {
                player.SendInfo("Error Setting Fame");
                return false;
            }
            return true;
        }
    }
    
    internal class AutoTradeCommand : Command
    {
        public AutoTradeCommand() : base("autotrade", permLevel: 1)
        { }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            Player plr = player.Manager.FindPlayer(args[0]);
            if (plr != null && plr.Owner == player.Owner)
            {
                player.RequestTrade(time, new RequestTradePacket { Name = plr.Name });
                plr.RequestTrade(time, new RequestTradePacket { Name = player.Name });
                return true;
            }
            return true;
        }
    }
    
    internal class ApplyStatsCommand : Command
    {
        public ApplyStatsCommand()
            : base("applystats", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.HasConditionEffect(ConditionEffects.Invincible))
            {
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = ConditionEffectIndex.Invincible,
                    DurationMS = 0,
                });
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = ConditionEffectIndex.Berserk,
                    DurationMS = 0,
                });
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = ConditionEffectIndex.Damaging,
                    DurationMS = 0,
                });       
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = ConditionEffectIndex.Invisible,
                    DurationMS = 0,
                });                                         
                player.SendInfo("Boosted Stats Deactivated");
                return false;
            }
            else
            {
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = ConditionEffectIndex.Berserk,
                    DurationMS = -1
                });
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = ConditionEffectIndex.Damaging,
                    DurationMS = -1
                });
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = ConditionEffectIndex.Invincible,
                    DurationMS = -1
                });
                player.ApplyConditionEffect(new ConditionEffect()
                {
                    Effect = ConditionEffectIndex.Invisible,
                    DurationMS = -1,
                });                                           
                player.SendInfo("Boosted Stats Activated");
            }
            return true;
        }
    }

    internal class SpawnCommand : Command
    {
        public SpawnCommand()
            : base("spawn", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            int num;
            if (args.Length > 0 && int.TryParse(args[0], out num)) //multi
            {
                string name = string.Join(" ", args.Skip(1).ToArray());
                ushort objType;
                //creates a new case insensitive dictionary based on the XmlDatas
                Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(
                    player.Manager.GameData.IdToObjectType,
                    StringComparer.OrdinalIgnoreCase);
                if (!icdatas.TryGetValue(name, out objType) ||
                    !player.Manager.GameData.ObjectDescs.ContainsKey(objType))
                {
                    player.SendInfo("Unknown entity!");
                    return false;
                }
                int c = int.Parse(args[0]);
                if (!(player.Client.Account.Rank > 3) && c > 100)
                {
                    player.SendError("Maximum spawn count is set to 100!");
                    return false;
                }
                if (player.Client.Account.Rank == 3 && c > 100)
                {
                    player.SendInfo("Bypass granted, spawning now!");
                }
                for (int i = 0; i < num; i++)
                {
                    Entity entity = Entity.Resolve(player.Manager, objType);
                    entity.Move(player.X, player.Y);
                    player.Owner.EnterWorld(entity);
                }
                player.SendInfo("Success!");
            }
            else
            {
                string name = string.Join(" ", args);
                ushort objType;
                Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(
                    player.Manager.GameData.IdToObjectType,
                    StringComparer.OrdinalIgnoreCase);
                if (!icdatas.TryGetValue(name, out objType) ||
                    !player.Manager.GameData.ObjectDescs.ContainsKey(objType))
                {
                    player.SendHelp("Usage: /spawn <entityname>");
                    return false;
                }
                Entity entity = Entity.Resolve(player.Manager, objType);
                entity.Move(player.X, player.Y);
                player.Owner.EnterWorld(entity);
            }
            return true;
        }
    }

    internal class AddEffCommand : Command
    {
        public AddEffCommand()
            : base("addeff", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /addeff <Effect Name or Effect Number>");
                return false;
            }
            try
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim(), true),
                    DurationMS = -1
                });
                {
                    player.SendInfo("Success adding the desired effects");
                }
            }
            catch
            {
                player.SendError("Invalid effect!");
                return false;
            }
            return true;
        }
    }

    internal class RemoveEffCommand : Command
    {
        public RemoveEffCommand()
            : base("remeff", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /remeff <Effect Name or Effect Number>");
                return false;
            }
            try
            {
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), args[0].Trim(), true),
                    DurationMS = 0
                });
                player.SendInfo("Success removing the desired effects");
            }
            catch
            {
                player.SendError("Invalid effect!");
                return false;
            }
            return true;
        }
    }

    internal class GiveCommand : Command
    {
        public GiveCommand()
            : base("give", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /give <Item Name>");
                return false;
            }
            string name = string.Join(" ", args.ToArray()).Trim();
            ushort objType;
            Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(player.Manager.GameData.IdToObjectType,
                StringComparer.OrdinalIgnoreCase);
            if (!icdatas.TryGetValue(name, out objType))
            {
                player.SendError("Unknown type!");
                return false;
            }
            if (player.Client.Account.Rank < 3 &&
                (objType.Equals("Admin Sword") ||
                 objType.Equals("Admin Staff") ||
                 objType.Equals("Crown")))
            {
                player.SendError("Insufficient rank to receive item.");
                return false;
            }
            if (!player.Manager.GameData.Items[objType].Secret || player.Client.Account.Rank >= 4)
            {
                for (int i = 4; i < player.Inventory.Length; i++)
                    if (player.Inventory[i] == null)
                    {
                        player.Inventory[i] = player.Manager.GameData.Items[objType];
                        player.UpdateCount++;
                        player.SaveToCharacter();
                        player.SendInfo("Success!");
                        break;
                    }
            }
            else
            {
                player.SendError("Item cannot be given!");
                return false;
            }
            return true;
        }
    }

    class KillAll : Command
    {
        public KillAll() : base("killAll", permLevel: 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            var iterations = 0;
            var lastKilled = -1;
            var killed = 0;

            var mobName = args.Aggregate((s, a) => string.Concat(s, " ", a));
            while (killed != lastKilled)
            {
                lastKilled = killed;
                foreach (var i in player.Owner.Enemies.Values.Where(e =>
                    e.ObjectDesc?.ObjectId != null && e.ObjectDesc.ObjectId.ContainsIgnoreCase(mobName)))
                {
                    i.Death(time);
                    killed++;
                }
                if (++iterations >= 5)
                    break;
            }

            player.SendInfo($"{killed} enemies killed!");
            return true;
        }
    }

    internal class Kick : Command
    {
        public Kick()
            : base("kick", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /kick <Player Name>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        player.SendInfo("Player has been kicked");
                        i.Value.Client.Disconnect();
                    }
                }
            }
            catch
            {
                player.SendError("Could not kick that player!");
                return false;
            }
            return true;
        }
    }

    internal class Mute : Command
    {
        public Mute() : base("mute", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /mute <Player Name>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        i.Value.Muted = true;
                        i.Value.Manager.Database.DoActionAsync(db => db.MuteAccount(i.Value.AccountId));
                        player.SendInfo("Player Muted.");
                    }
                }
            }
            catch
            {
                player.SendError("Cannot mute the player!");
                return false;
            }
            return true;
        }
    }

    internal class Max : Command
    {
        public Max()
            : base("max", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                player.Stats[0] = player.ObjectDesc.MaxHitPoints;
                player.Stats[1] = player.ObjectDesc.MaxMagicPoints;
                player.Stats[2] = player.ObjectDesc.MaxAttack;
                player.Stats[3] = player.ObjectDesc.MaxDefense;
                player.Stats[4] = player.ObjectDesc.MaxSpeed;
                player.Stats[5] = player.ObjectDesc.MaxHpRegen;
                player.Stats[6] = player.ObjectDesc.MaxMpRegen;
                player.Stats[7] = player.ObjectDesc.MaxDexterity;
                player.SaveToCharacter();
                player.Client.Save();
                player.UpdateCount++;
                player.SendInfo("Successfully maxed your stats");
            }
            catch
            {
                player.SendError("Error whilst maxing stats");
                return false;
            }
            return true;
        }
    }

    internal class UnMute : Command
    {
        public UnMute()
            : base("unmute", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /unmute <playername>");
                return false;
            }
            try
            {
                foreach (KeyValuePair<int, Player> i in player.Owner.Players)
                {
                    if (i.Value.Name.ToLower() == args[0].ToLower().Trim())
                    {
                        i.Value.Muted = true;
                        i.Value.Manager.Database.DoActionAsync(db => db.UnmuteAccount(i.Value.AccountId));
                        player.SendInfo("Player Unmuted.");
                    }
                }
            }
            catch
            {
                player.SendError("Cannot unmute the player!");
                return false;
            }
            return true;
        }
    }

    internal class ServerWhoCommand : Command
    {
        public ServerWhoCommand() : base("online", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            StringBuilder sb = new StringBuilder("All players: ");

            foreach (KeyValuePair<int, World> w in player.Manager.Worlds)
            {
                World world = w.Value;
                if (w.Key != 0)
                {
                    Player[] copy = world.Players.Values.ToArray();
                    if (copy.Length != 0)
                    {
                        for (int i = 0; i < copy.Length; i++)
                        {
                            sb.Append(copy[i].Name);
                            sb.Append(", ");
                        }
                    }
                }
            }
            string fixedString = sb.ToString().TrimEnd(',', ' ');

            player.SendHelp(fixedString);
            return true;
        }
    }

    internal class Announcement : Command
    {
        public Announcement()
            : base("announce", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 0)
            {
                player.SendHelp("Usage: /announce <Text>");
                return false;
            }
            string saytext = string.Join(" ", args);

            foreach (Client i in player.Manager.Clients.Values)
            {
                i.SendPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = 70,
                    Name = "@Server Announcement",
                    Text = " " + saytext
                });
            }
            return true;
        }
    }

    internal class Summon : Command
    {
        public Summon()
            : base("summon", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.Owner is Vault || player.Owner is PetYard || player.Owner is Nexus || player.Owner is GuildHall)
            {
                player.SendError("You cannot summon in this world.");
                return false;
            }
            foreach (KeyValuePair<string, Client> i in player.Manager.Clients)
            {
                if (i.Value.Player.Name.EqualsIgnoreCase(args[0]))
                {
                    Packet pkt;
                    if (i.Value.Player.Owner == player.Owner)
                    {
                        i.Value.Player.Move(player.X, player.Y);
                        pkt = new GotoPacket
                        {
                            ObjectId = i.Value.Player.Id,
                            Position = new Position(player.X, player.Y)
                        };
                        i.Value.Player.UpdateCount++;
                        player.SendInfo("Player summoned!");
                    }
                    else
                    {
                        pkt = new ReconnectPacket
                        {
                            GameId = player.Owner.Id,
                            Host = "",
                            IsFromArena = false,
                            Key = player.Owner.PortalKey,
                            KeyTime = -1,
                            Name = "Summon",
                            Port = -1
                        };
                        player.SendInfo("Player will connect to you now!");
                    }
                    i.Value.SendPacket(pkt);
                    return true;
                }
            }
            player.SendError(string.Format("Player '{0}' could not be found!", args));
            return false;
        }
    }

    internal class QuestCommand : Command
    {
        public QuestCommand()
            : base("quest", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.Quest == null)
            {
                player.SendError("Player does not have a quest!");
                return false;
            }
            player.Move(player.Quest.X + 0.5f, player.Quest.Y + 0.5f);
            if (player.Pet != null)
            player.Pet.Move(player.Quest.X + 0.5f, player.Quest.Y + 0.5f);
            player.UpdateCount++;
            player.Owner.BroadcastPacket(new GotoPacket
            {
                ObjectId = player.Id,
                Position = new Position
                {
                    X = player.Quest.X,
                    Y = player.Quest.Y
                }
            }, null);
            player.SendInfo("Success!");
            player.ApplyConditionEffect(new ConditionEffect()
            {
                Effect = ConditionEffectIndex.Invulnerable,
                DurationMS = 2500,
            });
            player.ApplyConditionEffect(new ConditionEffect()
            {
                Effect = ConditionEffectIndex.Invisible,
                DurationMS = 2500,
            });
            player.ApplyConditionEffect(new ConditionEffect()
            {
                Effect = ConditionEffectIndex.Quiet,
                DurationMS = 2500,
            });    
            player.ApplyConditionEffect(new ConditionEffect()
            {
                Effect = ConditionEffectIndex.Stunned,
                DurationMS = 2500,
            });       
            return true;
        }
    }

    internal class LevelCommand : Command
    {
        public LevelCommand()
            : base("level", 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            try
            {
                int c = int.Parse(args[0]);
                if (!(player.Client.Account.Rank == 1 || player.Client.Account.Rank == 2) && c > 20)
                {
                    player.SendError("Maximum level is 20!");
                    return false;
                }
                if (player.Client.Account.Rank == 3 && c > 20)
                {
                    player.SendInfo("Bypass granted, increasing level!");
                }
                if (args.Length == 0)
                {
                    player.SendHelp("Use /level <ammount>");
                    return false;
                }
                if (args.Length == 1)
                {
                    player.Client.Character.Level = int.Parse(args[0]);
                    player.Client.Player.Level = int.Parse(args[0]);
                    player.UpdateCount++;
                    player.SendInfo("Success!");
                }
            }
            catch
            {
                player.SendError("Error setting level!");
                return false;
            }
            return true;
        }
    }
    
        internal class PetMaxCommand : Command
        {
        public PetMaxCommand()
            : base("petmax", 2)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (player.Pet == null) return false;
            player.Pet.Feed(new PetFoodNomNomNom());
            player.Pet.UpdateCount++;
            return true;
        }
        private class PetFoodNomNomNom : IFeedable
        {
        public int FeedPower { get; set; }
        public PetFoodNomNomNom()
         {
        FeedPower = Int32.MaxValue;
         }
     }
}

       internal class GiftCommand : Command
    {
        public GiftCommand()
            : base("gift", 3)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            if (args.Length == 1)
            {
                player.SendHelp("Usage: /gift <Player Name> <Item Name>");
                return false;
            }
            string name = string.Join(" ", args.Skip(1).ToArray()).Trim();
            var plr = player.Manager.FindPlayer(args[0]);
            ushort objType;
            Dictionary<string, ushort> icdatas = new Dictionary<string, ushort>(player.Manager.GameData.IdToObjectType,
                StringComparer.OrdinalIgnoreCase);
            if (!icdatas.TryGetValue(name, out objType))
            {
                player.SendError("Item not found");
                return false;
            }
            if (!player.Manager.GameData.Items[objType].Secret || player.Client.Account.Rank >= 3)
            {
                for (int i = 0; i < plr.Inventory.Length; i++)
                    if (plr.Inventory[i] == null)
                    {
                        plr.Inventory[i] = player.Manager.GameData.Items[objType];
                        plr.UpdateCount++;
                        plr.SaveToCharacter();
                        player.SendInfo("Success sending " + name + " to " + plr.Name);
                        plr.SendInfo("You got a " + name + " from " + player.Name);
                        break;
                    }
            }
            else
            {
                player.SendError("Item failed sending to " + plr.Name + ", make sure you spelt the command right, and their name!");
                return false;
            }
            return true;
        }
    }
    
    internal class ListCommands : Command
    {
        public ListCommands() : base("commands", permLevel: 1)
        {
        }
        protected override bool Process(Player player, RealmTime time, string[] args)
        {
            Dictionary<string, Command> cmds = new Dictionary<string, Command>();
            Type t = typeof(Command);
            foreach (Type i in t.Assembly.GetTypes())
                if (t.IsAssignableFrom(i) && i != t)
                {
                    Command instance = (Command)Activator.CreateInstance(i);
                    cmds.Add(instance.CommandName, instance);
                }
            StringBuilder sb = new StringBuilder("Commands: ");
            Command[] copy = cmds.Values.ToArray();
            for (int i = 0; i < copy.Length; i++)
            {
                if (i != 0) sb.Append(", ");
                sb.Append(copy[i].CommandName);
            }

            player.SendInfo(sb.ToString());
            return true;
        }
    }
}

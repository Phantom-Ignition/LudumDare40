using System.Collections.Generic;
using LudumDare40.PostProcessors;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace LudumDare40.Managers
{
    public class SystemManager : IUpdatableManager
    {
        //--------------------------------------------------
        // Switches & Variables

        public Dictionary<string, bool> switches;
        public Dictionary<string, int> variables;

        //--------------------------------------------------
        // Postprocessors
        
        public CinematicLetterboxPostProcessor cinematicLetterboxPostProcessor;
        public FlashPostProcessor flashPostProcessor;

        //--------------------------------------------------
        // Player

        public Entity playerEntity;

        //--------------------------------------------------
        // Map

        private TiledMap _tiledMapComponent;
        public TiledMap TiledMap => _tiledMapComponent;

        private Vector2? _spawnPosition;
        public Vector2? SpawnPosition => _spawnPosition;

        //----------------------//------------------------//

        public SystemManager()
        {
            switches = new Dictionary<string, bool>();
            variables = new Dictionary<string, int>();
        }

        public void setPlayer(Entity playerEntity)
        {
            this.playerEntity = playerEntity;
        }
        
        public void setTiledMapComponent(TiledMap map)
        {
            _tiledMapComponent = map;
        }

        public void setSpawnPosition(Vector2 position)
        {
            _spawnPosition = position;
        }

        public void setSwitch(string name, bool value)
        {
            switches[name] = value;
        }

        public bool getSwitch(string name)
        {
            return switches.ContainsKey(name) ? switches[name] : false;
        }

        public void setVariable(string name, int value)
        {
            variables[name] = value;
        }

        public int getVariable(string name)
        {
            return variables.ContainsKey(name) ? variables[name] : 0;
        }

        public void update()
        { }
    }
}

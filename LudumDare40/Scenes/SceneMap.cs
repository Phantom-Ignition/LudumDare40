using System;
using System.Collections.Generic;
using System.Linq;
using LudumDare40.Components;
using LudumDare40.Components.Map;
using LudumDare40.Components.Player;
using LudumDare40.Components.Windows;
using LudumDare40.Extensions;
using LudumDare40.Managers;
using LudumDare40.NPCs;
using LudumDare40.PostProcessors;
using LudumDare40.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Particles;
using Nez.Tiled;

namespace LudumDare40.Scenes
{
    class SceneMap : Scene
    {
        //--------------------------------------------------
        // Render Layers Constants

        public const int BACKGROUND_RENDER_LAYER = 10;
        public const int TILED_MAP_RENDER_LAYER = 9;
        public const int WATER_RENDER_LAYER = 5;
        public const int MISC_RENDER_LAYER = 4; // NPCs, Text, etc.
        public const int PLAYER_RENDER_LAYER = 3;
        public const int PARTICLES_RENDER_LAYER = 2;

        //--------------------------------------------------
        // PostProcessors

        private CinematicLetterboxPostProcessor _cinematicPostProcessor;

        //--------------------------------------------------
        // Map
        
        private TiledMap _tiledMap;

        //----------------------//------------------------//

        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            setupMap();
            setupPlayer();
            setupEntityProcessors();
            setupNpcs();
            setupParticles();
            setupLadders();
            setupWater();
            setupMapTexts();
            setupPostProcessors();
        }

        public override void onStart()
        {
            getEntityProcessor<NpcInteractionSystem>().mapStart();
        }

        private void setupMap()
        {
            var sysManager = Core.getGlobalManager<SystemManager>();
            var mapId = sysManager.MapId;
            _tiledMap = content.Load<TiledMap>(string.Format("maps/map{0}", mapId));
            sysManager.setTiledMapComponent(_tiledMap);

            var tiledEntity = createEntity("tiled-map");
            var collisionLayer = _tiledMap.properties["collisionLayer"];
            var defaultLayers = _tiledMap.properties["defaultLayers"].Split(',').Select(s => s.Trim()).ToArray();

            var tiledComp = tiledEntity.addComponent(new TiledMapComponent(_tiledMap, collisionLayer) { renderLayer = TILED_MAP_RENDER_LAYER });
            tiledComp.setLayersToRender(defaultLayers);
            
            if (_tiledMap.properties.ContainsKey("aboveWaterLayer"))
            {
                var aboveWaterLayer = _tiledMap.properties["aboveWaterLayer"];
                var tiledAboveWater = tiledEntity.addComponent(new TiledMapComponent(_tiledMap) { renderLayer = WATER_RENDER_LAYER });
                tiledAboveWater.setLayerToRender(aboveWaterLayer);
            }
        }

        private void setupPlayer()
        {
            var sysManager = Core.getGlobalManager<SystemManager>();

            var collisionLayer = _tiledMap.properties["collisionLayer"];
            Vector2? playerSpawn = null;

            if (sysManager.SpawnPosition.HasValue)
            {
                playerSpawn = sysManager.SpawnPosition;
            }
            else
            {
                playerSpawn = _tiledMap.getObjectGroup("objects").objectWithName("playerSpawn").position;
            }

            var player = createEntity("player");
            player.transform.position = playerSpawn.Value;
            player.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
            player.addComponent(new BoxCollider(-10f, -20f, 20f, 40f));
            player.addComponent(new InteractionCollider(-30f, -6, 60, 22));
            player.addComponent<PlatformerObject>();
            player.addComponent<TextWindowComponent>();
            var playerComponent = player.addComponent<PlayerComponent>();
            playerComponent.sprite.renderLayer = PLAYER_RENDER_LAYER;

            Core.getGlobalManager<SystemManager>().setPlayer(player);

            /*var inventory = createEntity("player-inventory");
            inventory.addComponent<InventoryComponent>();
            inventory.position = new Vector2(100, 20);
            inventory.getComponent<InventoryComponent>().renderLayer = 1;*/
        }

        private void setupNpcs()
        {
            var npcObjects = _tiledMap.getObjectGroup("npcs");
            if (npcObjects == null) return;

            var collisionLayer = _tiledMap.properties["collisionLayer"];
            var names = new Dictionary<string, int>();
            foreach (var npc in npcObjects.objects)
            {
                names[npc.name] = names.ContainsKey(npc.name) ? ++names[npc.name] : 0;

                var npcEntity = createEntity(string.Format("{0}:{1}", npc.name, names[npc.name]));
                var npcComponent = (NpcBase)Activator.CreateInstance(Type.GetType("LudumDare40.NPCs." + npc.type), npc.name);
                npcComponent.setRenderLayer(MISC_RENDER_LAYER);
                npcComponent.ObjectRect = new Rectangle(0, 0, npc.width, npc.height);
                npcEntity.addComponent(npcComponent);
                npcEntity.addComponent<TextWindowComponent>();
                npcEntity.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));

                if (!npcComponent.Invisible) {
                    npcEntity.position = npc.position + new Vector2(npc.width, npc.height) / 2;
                    npcEntity.addComponent<PlatformerObject>();
                }

                // Props
                if (npc.properties.ContainsKey("flipX") && npc.properties["flipX"] == "true")
                {
                    npcComponent.FlipX = true;
                }
                if (npc.properties.ContainsKey("autorun") && npc.properties["autorun"] == "true")
                {
                    getEntityProcessor<NpcInteractionSystem>().addAutorun(npcComponent);
                }
            }
        }

        private void setupParticles()
        {
            var particles = _tiledMap.getObjectGroup("particles");
            if (particles == null) return;

            var names = new Dictionary<string, int>();
            foreach (var particleObj in particles.objects)
            {
                names[particleObj.name] = names.ContainsKey(particleObj.name) ? ++names[particleObj.name] : 0;

                var particle = createEntity(string.Format("{0}:{1}", particleObj.name, names[particleObj.name]));
                var emitterConf = content.Load<ParticleEmitterConfig>("particles/" + particleObj.type);
                var emitter = particle.addComponent(new ParticleEmitter(emitterConf));
                emitter.renderLayer = PARTICLES_RENDER_LAYER;
                particle.position = particleObj.position + new Vector2(particleObj.width, particleObj.height) / 2;
            }
        }

        private void setupLadders()
        {
            var ladders = _tiledMap.getObjectGroup("ladders");
            if (ladders == null) return;

            var names = new Dictionary<string, int>();
            foreach (var ladderObj in ladders.objects)
            {
                names[ladderObj.name] = names.ContainsKey(ladderObj.name) ? ++names[ladderObj.name] : 0;

                var ladder = createEntity(string.Format("{0}:{1}", ladderObj.name, names[ladderObj.name]));
                ladder.addComponent(new LadderComponent(ladderObj));
            }
        }

        private void setupEntityProcessors()
        {
            var player = findEntity("player");
            var playerComponent = player.getComponent<PlayerComponent>();

            camera.addComponent(new CameraShake());
            var mapSize = new Vector2(_tiledMap.width * _tiledMap.tileWidth, _tiledMap.height * _tiledMap.tileHeight);
            addEntityProcessor(new CameraSystem(player) { mapLockEnabled = true, mapSize = mapSize, followLerp = 0.08f, deadzoneSize = new Vector2(20, 10) });
            addEntityProcessor(new NpcInteractionSystem(playerComponent));
            addEntityProcessor(new LadderSystem(new Matcher().all(typeof(LadderComponent)), playerComponent));
        }

        private void setupWater()
        {
            var environmentObjects = _tiledMap.getObjectGroup("environment");
            if (environmentObjects != null)
            {
                var waterObjects = environmentObjects.objects.Where(obj => obj.type == "water").ToList();
                for (var i = 0; i < waterObjects.Count; i++)
                {
                    var waterObj = waterObjects[i];
                    var entity = createEntity(string.Format("water:{0}", i));
                    var reflectionPlane = new WaterReflectionPlane(waterObj.width, waterObj.height) { renderLayer = WATER_RENDER_LAYER };
                    var material = reflectionPlane.getMaterial<WaterReflectionMaterial>();
                    material.effect.normalMap = content.Load<Texture2D>(Content.System.waterNormalMap);
                    material.effect.perspectiveCorrectionIntensity = 0.0f;
                    material.effect.normalMagnitude = 0.015f;
                    entity.addComponent(reflectionPlane);
                    entity.position = waterObj.position;
                }

                addRenderer(new RenderLayerRenderer(0, WATER_RENDER_LAYER));
            }
        }

        private void setupMapTexts()
        {
            var textObjects = _tiledMap.getObjectGroup("texts");
            if (textObjects == null) return;
            
            var names = new Dictionary<string, int>();
            foreach (var textObj in textObjects.objects)
            {
                names[textObj.name] = names.ContainsKey(textObj.name) ? ++names[textObj.name] : 0;
                var font = Graphics.instance.bitmapFont;
                var position = textObj.position.round();
                var text = textObj.properties["text"];

                if (textObj.properties.ContainsKey("shadowColor"))
                {
                    var shadowColor = textObj.properties["shadowColor"].FromHex();
                    var shadowEntity = createEntity(string.Format("[Shadow] {0}:{1}", textObj.name, names[textObj.name]));
                    shadowEntity.position = textObj.position.round() + Vector2.One;
                    var shadowTextComponent = shadowEntity.addComponent(new Text(font, text, Vector2.Zero, shadowColor));
                    shadowTextComponent.setRenderLayer(MISC_RENDER_LAYER);
                }

                var color = textObj.properties["color"].FromHex();
                var entity = createEntity(string.Format("{0}:{1}", textObj.name, names[textObj.name]));
                entity.position = textObj.position.round();
                var textComponent = entity.addComponent(new Text(font, text, Vector2.Zero, color));
                textComponent.setRenderLayer(MISC_RENDER_LAYER);
            }
        }
            
        private void setupPostProcessors()
        {
            Core.getGlobalManager<SystemManager>().cinematicLetterboxPostProcessor = addPostProcessor(new CinematicLetterboxPostProcessor(1));
            Core.getGlobalManager<SystemManager>().flashPostProcessor = addPostProcessor(new FlashPostProcessor(0));
        }

        public override void update()
        {
            base.update();

            if (Input.isKeyPressed(Keys.C))
            {
                Core.startCoroutine(
                    Core.getGlobalManager<SystemManager>().flashPostProcessor.animate(1f)
                );
            }

            // Update cinematic
            /*
            var cinematicAmount = Core.getGlobalManager<SystemManager>().cinematicAmount;
            if (cinematicAmount > 0)
            {
                _cinematicPostProcessor.enabled = true;
                _cinematicPostProcessor.letterboxSize = cinematicAmount;
            }
            else
            {
                _cinematicPostProcessor.enabled = false;
            }*/
        }
    }
}

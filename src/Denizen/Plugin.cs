using BepInEx;
using Fisobs.Core;
using Denizen.Objects;
using SlugBase;
using UnityEngine;

namespace Denizen
{
    [BepInDependency("slime-cubed.slugbase", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("oefans.denizen", "The Denizen", "0.3.5")]
    public class Plugin : BaseUnityPlugin
    {
        public static readonly PlacedObject.Type Boulder = new PlacedObject.Type("Boulder", true);

        void OnEnable()
        {
            On.Player.Update += Player_Update;
            On.PlayerGraphics.Update += Player_Graphics_Update;
            Content.Register(new BoulderFisob());
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig.Invoke(self, eu);
            
            if(SlugBaseCharacter.TryGet(self.slugcatStats.name, out SlugBaseCharacter _character))
            {
                if (_character.DisplayName == "The Denizen")
                {
                    if(self.bodyMode == Player.BodyModeIndex.Swimming)
                    {
                        Vector2 horizVector = self.SwimDir(true);
                        Vector2 swimVector = self.SwimDir(false);
                        horizVector.y = 0;
                        if (self.animation == Player.AnimationIndex.DeepSwim)
                        {
                            self.swimCycle += 0.03f * horizVector.magnitude;
                            if (self.swimCycle > 0.9f)
                            {
                                self.bodyChunks[0].vel += 0.1f * swimVector;
                            }
                            if (self.input[0].jmp && swimVector.y > 0)
                            {
                                self.swimCycle += 0.01f * swimVector.magnitude;
                                self.bodyChunks[0].vel.y += 0.6f * swimVector.y;
                            }
                        }
                        else if (self.animation == Player.AnimationIndex.SurfaceSwim)
                        {
                            self.swimCycle -= 0.04f * horizVector.magnitude;
                            self.bodyChunks[0].vel += 0.3f * swimVector.magnitude * horizVector;
                            if (self.input[0].jmp && swimVector.y > 0)
                            {
                                self.swimCycle += 0.08f * horizVector.magnitude;
                                self.bodyChunks[0].vel += 0.3f * swimVector.magnitude * horizVector;
                            }
                        }
                    }
                }
            }
        }

        private void Player_Graphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig.Invoke(self);
            Player player = (Player)(self as GraphicsModule).owner;
            if (SlugBaseCharacter.TryGet(player.slugcatStats.name, out SlugBaseCharacter _character))
            {
                if (_character.DisplayName == "The Denizen")
                {
                    self.blink = 20;
                }
            }
        }

        void OnDisable()
        {
            On.Player.Update -= Player_Update;
        }
    }
}

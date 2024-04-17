using BepInEx;
using Fisobs.Core;
using Denizen.Objects;
using SlugBase;

namespace Denizen
{
    [BepInDependency("slime-cubed.slugbase", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("oefans.Denizen", "The Denizen", "0.2.0")]
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
                    if (self.bodyMode == Player.BodyModeIndex.Swimming)
                    {
                        self.swimForce += 0.8f;
                    }
                }
            }
        }

        private void Player_Graphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig.Invoke(self);
            if (SlugBaseCharacter.TryGet(self.player.slugcatStats.name, out SlugBaseCharacter _character))
            {
                if (_character.DisplayName == "The Denizen")
                {
                    self.PlayerBlink();
                }
            }
        }

        void OnDisable()
        {
            On.Player.Update -= Player_Update;
        }
    }
}

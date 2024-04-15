using BepInEx;
using Fisobs.Core;
using SlugBase.Features;
using Denizen.Objects;

namespace Denizen
{
    [BepInDependency("slime-cubed.slugbase", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("fisobs", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Mills88CampaignJam.Denizen", "The Denizen", "0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static readonly PlayerFeature<float> SwimSpeed = FeatureTypes.PlayerFloat("swim_speed");

        void OnEnable()
        {
            On.Player.Update += Player_Update;
            Content.Register(new BoulderFisob());
        }

        void OnDisable()
        {
            On.Player.Update -= Player_Update;
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, true);
            if (SwimSpeed.TryGet(self, out var swimSpeed))
            {
                // not implemented
            }
        }
    }
}

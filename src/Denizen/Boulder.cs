using RWCustom;
using Fisobs;
using Fisobs.Items;
using Fisobs.Core;
using Fisobs.Sandbox;
using Fisobs.Properties;
using UnityEngine;



namespace Denizen.Objects
{
    // Represents a realized game object, receives updates
    sealed class Boulder : Weapon
    {
        #pragma warning disable CS8602
        public BoulderAbstract Abstr;

        public override bool HeavyWeapon
        {
            get
            {
                return true;
            }
        }

        public Boulder(BoulderAbstract abstr, Vector2 pos, Vector2 vel) : base(abstr, abstr.world)
        {
            Abstr = abstr;

            bodyChunks = new[] { new BodyChunk(this, 0, pos + vel, 4, 2f) { goThroughFloors = false } };
            bodyChunks[0].lastPos = bodyChunks[0].pos;
            bodyChunks[0].vel = vel;

            bodyChunkConnections = new BodyChunkConnection[0];
            airFriction = 0.999f;
            gravity = 1f;
            bounce = 0.2f;
            surfaceFriction = 1f;
            collisionLayer = 1;
            waterFriction = 0.92f;
            buoyancy = 0.1f;
        }

        public override void Update(bool eu)
        {
            ChangeCollisionLayer(grabbedBy.Count == 0 ? 2 : 1);

            base.Update(eu);
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj == null)
            {
                return false;
            }
            this.ChangeMode(Mode.Free);
            if (result.obj is Creature)
            {
                float stunBonus = 180f;
                (result.obj as Creature).Violence(base.firstChunk, new Vector2?(base.firstChunk.vel * base.firstChunk.mass), result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, 0.5f, stunBonus);
            }
            else if (result.chunk != null)
            {
                result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
            }
            else if (result.onAppendagePos != null)
            {
                (result.obj as PhysicalObject.IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
            }
            base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude;
            this.room.PlaySound(SoundID.Rock_Hit_Creature, base.firstChunk);
            if (result.chunk != null)
            {
                this.room.AddObject(new ExplosionSpikes(this.room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
            }
            this.SetRandomSpin();
            return true;
        }

        // from Rock class
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(this.abstractPhysicalObject.ID.RandomSeed);
            sLeaser.sprites[0] = new FSprite("Pebble1", true);
            UnityEngine.Random.state = state;
            TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[] { new TriangleMesh.Triangle(0, 1, 2) };
            TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
            sLeaser.sprites[1] = triangleMesh;
            this.AddToContainer(sLeaser, rCam, null);
        }

        // from Rock class
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
            sLeaser.sprites[0].x = vector.x - camPos.x;
            sLeaser.sprites[0].y = vector.y - camPos.y;
            Vector3 v = Vector3.Slerp(this.lastRotation, this.rotation, timeStacker);
            sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), v);
            if (base.mode == Weapon.Mode.Thrown)
            {
                sLeaser.sprites[1].isVisible = true;
                Vector2 vector2 = Vector2.Lerp(this.tailPos, base.firstChunk.lastPos, timeStacker);
                Vector2 a = Custom.PerpendicularVector((vector - vector2).normalized);
                (sLeaser.sprites[1] as TriangleMesh).MoveVertice(0, vector + a * 3f - camPos);
                (sLeaser.sprites[1] as TriangleMesh).MoveVertice(1, vector - a * 3f - camPos);
                (sLeaser.sprites[1] as TriangleMesh).MoveVertice(2, vector2 - camPos);
            }
            else
            {
                sLeaser.sprites[1].isVisible = false;
            }
            if (this.blink > 0)
            {
                if (this.blink > 1 && UnityEngine.Random.value < 0.5f)
                {
                    sLeaser.sprites[0].color = base.blinkColor;
                }
                else
                {
                    sLeaser.sprites[0].color = this.color;
                }
            }
            else if (sLeaser.sprites[0].color != this.color)
            {
                sLeaser.sprites[0].color = this.color;
            }
            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        // from Rock class 
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.color = palette.blackColor;
            sLeaser.sprites[0].color = this.color;
            sLeaser.sprites[1].color = this.color;
        }
    }

    // Represents the data of an unrealized game object, does not receive updates
    sealed class BoulderAbstract : AbstractPhysicalObject
    {
        public BoulderAbstract(World world, WorldCoordinate pos, EntityID ID) : base(world, BoulderFisob.Boulder, null, pos, ID)
        {

        }
    }

    // Represents the content to be added
    public class BoulderFisob : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType Boulder = new("Boulder", true);

        public BoulderFisob() : base(Boulder)
        {

        }

        // Read from the boulder object save data here.
        public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock? unlock)
        {
            return new BoulderAbstract(world, entitySaveData.Pos, entitySaveData.ID);
        }

        public static BoulderProperties properties = new();
    }

    // Represents the properties of new boulder objects
    public class BoulderProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
        {
            throwable = true;
        }

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            grabability = Player.ObjectGrabability.Drag;
        }
    }
}

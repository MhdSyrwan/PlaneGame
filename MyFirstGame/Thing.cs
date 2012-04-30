using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
namespace MyFirstGame
{
    namespace Physics
    {
        
        public class Thing : DrawableGameComponent
        {
    #region "Physics"
            public float Mass;

            //Rotation Property
            private float rotation;
            public float Rotation
            {
                set {
                    rotation = value;
                    if (value > 2 * Math.PI) rotation = 0; else if (value < 0) rotation =(float)( 2 * Math.PI);

                }
                get { return rotation; }
            } //end of property
            
            public List<Vector2> Forces;
            public Vector2 Acceleration;
            public Vector2 Velocity;
            public Vector2 Position;
            public float AntiPush = 0.01f;

            public delegate void CollisionEventHandler(Thing obj1, Thing obj2);
            public static event CollisionEventHandler CollisionHappend;
            public List<Thing> RelativeThings;

    #endregion

    #region "Graphics"
            public Color Tint = Color.White;
            private Texture2D Tex;
            public float Scale = 1.0f ;
            private Vector2 Half;
            public static SpriteBatch spritebatch;
            public Rectangle BoundingRect; // destroy or continue Bounds
            public bool AutoDestroy = false;
    #endregion

    #region "Controller"
        public Boolean Control=false;
        public Keys Acccelerate=Keys.Up;
        public Keys BackAccelerate=Keys.Down;
        public Keys TurnLeft=Keys.Left;
        public Keys TurnRight=Keys.Right;
        public Keys Reset=Keys.Enter;
        protected float Push = 0.1f ;
        PlayerIndex Player=PlayerIndex.One;

    #endregion

  
            public Thing(Game game,Texture2D tex,float mass, Vector2 position ,float rotation,float scale )   : base(game)
            {
                Scale = scale;
                Tex = tex;
                Position = position;
                Rotation = rotation;
                Forces = new List<Vector2>();
                Velocity = Vector2.Zero;
                Acceleration = Vector2.Zero;
                Mass = mass;
                Half = new Vector2(Tex.Width / 2, Tex.Height / 2);
                RelativeThings = new List<Thing>();
            }

            public void CheckCollisions()
            {
                
                foreach (Thing other in RelativeThings)
                {
                    if (this.IntersectsWith(other))
                    {
                        CollisionHappend(this, other);
                        //break;
                    }
                }
            }

            public bool CheckCollision(Thing other) 
            {
                //Setting Rectangles ...
                Rectangle rectangleA, rectangleB;
                rectangleA = new Rectangle((int)this.Position.X, (int)this.Position.Y, (int)this.Tex.Width, (int)this.Tex.Height);
                rectangleB = new Rectangle((int)other.Position.X, (int)other.Position.Y, (int)other.Tex.Width, (int)other.Tex.Height);
                //Setting Colors Array ...
                Color[] dataA = new Color[rectangleA.Width * rectangleA.Height];
                Color[] dataB = new Color[rectangleB.Width * rectangleB.Height];
                
                this.Tex.GetData(dataA);
                other.Tex.GetData(dataB);
                
                //Real work ..

                // Find the bounds of the rectangle intersection
                int top = Math.Max(rectangleA.Top, rectangleB.Top);
                int bottom = Math.Min(rectangleA.Bottom, rectangleB.Bottom);
                int left = Math.Max(rectangleA.Left, rectangleB.Left);
                int right = Math.Min(rectangleA.Right, rectangleB.Right);

                // Check every point within the intersection bounds
                for (int y = top; y < bottom; y++)
                {
                    for (int x = left; x < right; x++)
                    {
                        // Get the color of both pixels at this point
                        Color colorA = dataA[(x - rectangleA.Left) +
                                             (y - rectangleA.Top) * rectangleA.Width];
                        Color colorB = dataB[(x - rectangleB.Left) +
                                             (y - rectangleB.Top) * rectangleB.Width];

                        // If both pixels are not completely transparent,
                        if (colorA.ToVector3() != new Vector3(0,0,0) && colorB.ToVector3() != new Vector3(0,0,0))
                        {
                            // then an intersection has been found
                            return true;
                        }
                    }
                }
                // No intersection found
                return false;
            }
            public override void Update(GameTime gameTime)
            {
                CheckCollisions();
                // Update By Controls
                if (Control)
                {
                    Acceleration = Vector2.Zero;

                    if ((Keyboard.GetState(Player).IsKeyDown(Acccelerate)))
                    {
                        this.Acceleration = -Push * (Vector2.UnitX * (float)Math.Cos(Math.PI / 2 + this.Rotation) + Vector2.UnitY * (float)Math.Sin(Math.PI / 2 + this.Rotation));
                    }
                    if (Keyboard.GetState(Player).IsKeyDown(BackAccelerate))
                    {
                        this.Acceleration = +Push * (Vector2.UnitX * (float)Math.Cos(Math.PI / 2 + this.Rotation) + Vector2.UnitY * (float)Math.Sin(Math.PI / 2 + this.Rotation));
                    }
                    if (Keyboard.GetState(Player).IsKeyDown(TurnLeft))
                    {
                        this.Rotation -= 0.06f ;
                    }
                    if (Keyboard.GetState(Player).IsKeyDown(TurnRight))
                    {
                        this.Rotation += 0.06f  ;
                    }

                    if (Keyboard.GetState(Player).IsKeyDown(Reset))
                        this.Position = Vector2.One;

                }
                if (AutoDestroy)
                {
                    if (
                        (this.Position.X > this.BoundingRect.Right + 20.0f) ||
                        (this.Position.Y > this.BoundingRect.Bottom + 20.0f) ||
                        (this.Position.X < this.BoundingRect.Left - 20.0f) ||
                        (this.Position.Y < this.BoundingRect.Top - 20.0f)
                        ) 
                    {
                        this.Game.Components.Remove(this);
                        GC.SuppressFinalize(this);
                    }
                }
                // Finalizing 
                Velocity += Acceleration;
                Velocity += -Velocity * AntiPush;
                Position += Velocity;
                base.Update(gameTime);
            }

            public override void Draw(GameTime gameTime)
            {
                Rectangle srcrect = new Rectangle(0, 0, Tex.Width, Tex.Height);
                spritebatch.Begin(SpriteBlendMode.AlphaBlend);
                spritebatch.Draw(Tex, Position, srcrect, this.Tint, Rotation, Half, Scale, SpriteEffects.None, 0f);
                spritebatch.End();
                Tint = Color.White;
                base.Draw(gameTime);
            }

            
            public static Vector2 Dir(Vector2 Vector)
            {
                float angle = (float)Math.Atan2(Vector.Y, Vector.X);
                return VectorByAngle(angle);
            }

            public static Vector2 VectorByAngle(float Angle)
            {
                return Vector2.UnitX * (float)Math.Cos(Angle - Math.PI / 2) + Vector2.UnitY * (float)Math.Sin(Angle - Math.PI / 2);
            }

            // intersecting ... must be resolved

            public bool IntersectsWith(Thing other)
            {
                // Build the Thing's transform
                Matrix ThingTransform =
                    Matrix.CreateTranslation(new Vector3(-this.Half,0.0f)) *
                    Matrix.CreateScale(this.Scale) *
                    Matrix.CreateRotationZ(this.Rotation) *
                    Matrix.CreateTranslation(new Vector3(this.Position, 0.0f));

                // Calculate the bounding rectangle of this Thing in world space
                Rectangle ThingRectangle = CalculateBoundingRectangle(
                         new Rectangle(0, 0, this.Tex.Width, this.Tex.Height),
                         ThingTransform);

                // Build the Other's transform
                Matrix OtherTransform =
                    Matrix.CreateTranslation(new Vector3(-other.Half, 0.0f)) *
                    Matrix.CreateScale(other.Scale) *
                    Matrix.CreateRotationZ(other.Rotation) *
                    Matrix.CreateTranslation(new Vector3(other.Position, 0.0f));

                // Calculate the bounding rectangle of this Thing in world space
                Rectangle OtherRectangle = CalculateBoundingRectangle(
                         new Rectangle(0, 0, other.Tex.Width, other.Tex.Height),
                         OtherTransform);

                Color[] OtherTextureData = new Color[other.Tex.Width * other.Tex.Height];
                Color[] ThingTextureData = new Color[this.Tex.Width  *  this.Tex.Height];
                other.Tex.GetData(OtherTextureData);
                this.Tex.GetData(ThingTextureData);

                bool Hit = false;
                // The per-pixel check is expensive, so check the bounding rectangles
                // first to prevent testing pixels when collisions are impossible.
                if (OtherRectangle.Intersects(ThingRectangle))
                {
                    // Check collision with person
                    if (IntersectPixels(OtherTransform, other.Tex.Width,
                                        other.Tex.Height, OtherTextureData,
                                        ThingTransform, this.Tex.Width,
                                        this.Tex.Height, ThingTextureData))
                    {
                        Hit = true;
                    }
                }
                return Hit;
            }

            
            /// <summary>
            /// Determines if there is overlap of the non-transparent pixels between two
            /// sprites.
            /// </summary>
            /// <param name="transformA">World transform of the first sprite.</param>
            /// <param name="widthA">Width of the first sprite's texture.</param>
            /// <param name="heightA">Height of the first sprite's texture.</param>
            /// <param name="dataA">Pixel color data of the first sprite.</param>
            /// <param name="transformB">World transform of the second sprite.</param>
            /// <param name="widthB">Width of the second sprite's texture.</param>
            /// <param name="heightB">Height of the second sprite's texture.</param>
            /// <param name="dataB">Pixel color data of the second sprite.</param>
            /// <returns>True if non-transparent pixels overlap; false otherwise</returns>
            public static bool IntersectPixels(
                            Matrix transformA, int widthA, int heightA, Color[] dataA,
                            Matrix transformB, int widthB, int heightB, Color[] dataB)
            {
                // Calculate a matrix which transforms from A's local space into
                // world space and then into B's local space
                Matrix transformAToB = transformA * Matrix.Invert(transformB);

                // When a point moves in A's local space, it moves in B's local space with a
                // fixed direction and distance proportional to the movement in A.
                // This algorithm steps through A one pixel at a time along A's X and Y axes
                // Calculate the analogous steps in B:
                Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
                Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

                // Calculate the top left corner of A in B's local space
                // This variable will be reused to keep track of the start of each row
                Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

                // For each row of pixels in A
                for (int yA = 0; yA < heightA; yA++)
                {
                    // Start at the beginning of the row
                    Vector2 posInB = yPosInB;

                    // For each pixel in this row
                    for (int xA = 0; xA < widthA; xA++)
                    {
                        // Round to the nearest pixel
                        int xB = (int)Math.Round(posInB.X);
                        int yB = (int)Math.Round(posInB.Y);

                        // If the pixel lies within the bounds of B
                        if (0 <= xB && xB < widthB &&
                            0 <= yB && yB < heightB)
                        {
                            // Get the colors of the overlapping pixels
                            Color colorA = dataA[xA + yA * widthA];
                            Color colorB = dataB[xB + yB * widthB];

                            // If both pixels are not completely transparent,
                            if (colorA.A != 0 && colorB.A != 0)
                            {
                                // then an intersection has been found
                                return true;
                            }
                        }

                        // Move to the next pixel in the row
                        posInB += stepX;
                    }

                    // Move to the next row
                    yPosInB += stepY;
                }

                // No intersection found
                return false;
            }
            /// <summary>
            /// Calculates an axis aligned rectangle which fully contains an arbitrarily
            /// transformed axis aligned rectangle.
            /// </summary>
            /// <param name="rectangle">Original bounding rectangle.</param>
            /// <param name="transform">World transform of the rectangle.</param>
            /// <returns>A new rectangle which contains the trasnformed rectangle.</returns>
            public static Rectangle CalculateBoundingRectangle(Rectangle rectangle,
                                                               Matrix transform)
            {
                // Get all four corners in local space
                Vector2 leftTop     = new Vector2(rectangle.Left, rectangle.Top);
                Vector2 rightTop    = new Vector2(rectangle.Right, rectangle.Top);
                Vector2 leftBottom  = new Vector2(rectangle.Left, rectangle.Bottom);
                Vector2 rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

                // Transform all four corners into work space
                Vector2.Transform(ref leftTop    , ref transform, out leftTop    );
                Vector2.Transform(ref rightTop   , ref transform, out rightTop   );
                Vector2.Transform(ref leftBottom , ref transform, out leftBottom );
                Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

                // Find the minimum and maximum extents of the rectangle in world space
                Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                          Vector2.Min(leftBottom, rightBottom));
                Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                          Vector2.Max(leftBottom, rightBottom));

                // Return that as a rectangle
                return new Rectangle((int)min.X, (int)min.Y,
                                     (int)(max.X - min.X), (int)(max.Y - min.Y));
            }
        }
    }
}
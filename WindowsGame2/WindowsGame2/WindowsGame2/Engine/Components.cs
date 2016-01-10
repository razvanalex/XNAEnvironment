using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using WindowsGame2;
using Engine.Terrain;
using Engine.Camera;

namespace Engine
{
    public class Components : Microsoft.Xna.Framework.Game
    {
        GraphicsDevice graphicsDevice;

        public Components(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

     
        public void updateCamera(GameTime gameTime, float deltaX, float deltaY, FreeCamera camera, MouseState lastMouseState)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            deltaX += (float)lastMouseState.X - (float)mouseState.X;
            deltaY += (float)lastMouseState.Y - (float)mouseState.Y;

            ((FreeCamera)camera).Rotate(deltaX * .01f, deltaY * .01f);
            Vector3 translation = Vector3.Zero;

            if (keyState.IsKeyDown(Keys.W)) translation += Vector3.Forward;
            if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
            if (keyState.IsKeyDown(Keys.A)) translation += Vector3.Left;
            if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;
            if (keyState.IsKeyDown(Keys.LeftShift))
                translation *= 3f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            else translation *= 0.3f * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            ((FreeCamera)camera).Move(translation);

            camera.Update();
            lastMouseState = mouseState;
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
        }
    }
    public struct VertexPositionNormalTextureTangentBinormal : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;
        public Vector3 Binormal;

        static readonly VertexDeclaration MyVertexDeclaration
           = new VertexDeclaration(new VertexElement[] {
                 new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3,  VertexElementUsage.Normal, 0),
        new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector2,  VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector3,  VertexElementUsage.Tangent, 0),
        new VertexElement(sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
            });

        public VertexDeclaration VertexDeclaration
        {
            get { return MyVertexDeclaration; }
        }

        public VertexPositionNormalTextureTangentBinormal(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Vector3 tangent, Vector3 binormal)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Tangent = tangent;
            Binormal = binormal;
        }
        public static int SizeInBytes { get { return sizeof(float) * 14; } }
    }

}


/*
 * Skydome Parameters Class
 * 
 * Alex Urbano Álvarez
 * XNA Community Coordinator
 * 
 * goefuika@gmail.com
 * 
 * http://elgoe.blogspot.com
 * http://www.codeplex.com/XNACommunity
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine.Sky
{
    public class SkyDomeParameters
    {
        #region Private Properties
        private Vector4 lightDirection;
        private Vector4 lightColor;
        private Vector4 lightColorAmbient;
        private float fDensity;
        private Vector3 waveLengths;
        private Vector3 invWaveLengths;
        private Vector3 waveLengthsMie;
        private int numSamples;
        private float exposure;
        #endregion

        public Vector4 LightDirection { get { return lightDirection; } set { lightDirection = value; } }

        public Vector4 LightColor { get { return lightColor; } set { lightColor = value; } }

        public Vector4 LightColorAmbient { get { return lightColorAmbient; } set { lightColorAmbient = value; } }

        public float FogDensity { get { return fDensity; } set { fDensity = value; } }

        public Vector3 InvWaveLengths 
        {
            get { return invWaveLengths; }
        }

        public Vector3 WaveLengthsMie
        {
            get { return waveLengthsMie; }
        }

        public Vector3 WaveLengths
        {
            get { return waveLengths; }
            set 
            {
                waveLengths = value;
                setLengths();
            }
        }

        public SkyDomeParameters()
        {
            lightDirection = new Vector4(100.0f, 100.0f, 100.0f, 1.0f);
            lightColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            lightColorAmbient = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
            fDensity = 0.0003f;
            waveLengths = new Vector3(0.65f, 0.57f, 0.475f);
            numSamples = 10;
            exposure = -2.0f;

            setLengths();
        }

        public int NumSamples
        {
            get { return numSamples; }
            set { numSamples = value; }
        }

        public float Exposure
        {
            get { return exposure; }
            set { exposure = value; }
        }

        private void setLengths()
        {
            invWaveLengths.X = 1.0f / (float)Math.Pow((double)waveLengths.X, 4.0);
            invWaveLengths.Y = 1.0f / (float)Math.Pow((double)waveLengths.Y, 4.0);
            invWaveLengths.Z = 1.0f / (float)Math.Pow((double)waveLengths.Z, 4.0);

            waveLengthsMie.X = (float)Math.Pow((double)waveLengths.X, -0.84);
            waveLengthsMie.Y = (float)Math.Pow((double)waveLengths.Y, -0.84);
            waveLengthsMie.Z = (float)Math.Pow((double)waveLengths.Z, -0.84);
        }

    }
}

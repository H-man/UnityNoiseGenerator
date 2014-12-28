using System;
using System.IO;
using Assets.Helpers;
using LibNoise;
using LibNoise.Generator;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public enum GeneratorTypeEnum
    {
        Planar,
        Spherical,
        Cylindrical
    }
    public enum NoiseModeEnum
    {
        Perlin,
        Checker,
        Cylinder,
        Billow,
        RidgedMultiFractal,
        Voronoi,
    }
    public class NoiseGenerator : MonoBehaviour
    {
        [Header("General")]
        public int Seed = 1;

        public bool RandomSeed;

        public int TextureSize = 256;

        public GeneratorTypeEnum GeneratorType = GeneratorTypeEnum.Spherical;

        public Gradient TextureGradient = GradientPresets.Grayscale;
        [Space(5)]
        public bool RegenerateTextures;
        [Space(10)]
        public bool SaveTextures;

        [Header("Noise settings")]

        public NoiseModeEnum NoiseMode = NoiseModeEnum.Perlin;

        [Range(0, 20)]
        public double Frequency = 2.5f;
        [Range(0, 20)]
        public double Lacunarity = 2.5f;
        [Range(0, 5)]
        public double Persistence = 0.5f;
        [Range(0, 20)]
        public int Octaves = 5;
        public QualityMode QualityModeEnum;

        [Header("Normal map settings")]
        public bool EnableNormalMap;
        [Range(0, 20)]
        public float NormalMapStrength = 4f;
        public bool Smooth;
        [Range(0, 20)]
        public int Radius = 2;
        [Range(0, 20)]
        public int Iterations = 2;



        private bool _oldRegen;
        private bool _oldSaveText;
        // Use this for initialization
        void Start()
        {
            GenerateTexture();
        }

        // Update is called once per frame
        void Update()
        {
            if (_oldRegen != RegenerateTextures)
            {
                GenerateTexture();
                _oldRegen = RegenerateTextures;
            }
            if (_oldSaveText != SaveTextures)
            {
                var mainText = renderer.material.mainTexture as Texture2D;
                SaveTexture(mainText, "main");
                var bump = renderer.material.GetTexture("_BumpMap");
                if (bump != null)
                {
                    SaveTexture((Texture2D) bump, "bump");
                }
                _oldSaveText = SaveTextures;
            }
        }

        static void SaveTexture(Texture2D txt, string name)
        {
            var bytes = txt.EncodeToPNG();
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            var fname = @".\" + name + "_" + (int)timeSpan.TotalSeconds + ".png";
            File.WriteAllBytes(fname, bytes);
            Debug.Log("Saved texture " + fname);
        }

        void GenerateTexture()
        {
            var noise = GetNoise();
            var builder = new Noise2D(TextureSize, noise);
            switch (GeneratorType)
            {
                case GeneratorTypeEnum.Cylindrical:
                    builder.GenerateCylindrical(0, 360, 0, 5);
                    break;
                case GeneratorTypeEnum.Spherical:
                    builder.GenerateSpherical(-90, 90, -180, 180);
                    break;
                case GeneratorTypeEnum.Planar:
                    builder.GeneratePlanar(0, 5, 0, 5);
                    break;

            }
            var mainTex = builder.GetTexture(TextureGradient);
            mainTex.anisoLevel = 5;
            mainTex.Apply(true);
            renderer.material.mainTexture = mainTex;

            if (EnableNormalMap)
            {
                var bump = mainTex;
                // get grayscale texture for normal map
                if (TextureGradient != GradientPresets.Grayscale)
                {
                    bump = builder.GetTexture();
                }
                
                if (Smooth)
                {
                    var blur = new Blur();
                    bump = blur.FastBlur(mainTex, Radius, Iterations);
                }

                bump = bump.NormalMap(NormalMapStrength);
                bump.anisoLevel = 5;
                bump.Apply(true);
                renderer.material.SetTexture("_BumpMap", bump);
            }
            else
            {
                renderer.material.SetTexture("_BumpMap", null);
            }
        }

        ModuleBase GetNoise()
        {
            switch (NoiseMode)
            {
                case NoiseModeEnum.Billow:
                    return GetBillowNoise();
                case NoiseModeEnum.Perlin:
                    return GetPerlinNoise();
                case NoiseModeEnum.Checker:
                    return GetCheckerNoise();
                case NoiseModeEnum.Cylinder:
                    return GetCylindersNoise();
                case NoiseModeEnum.RidgedMultiFractal:
                    return GetRidgedMultifractalNoise();
                case NoiseModeEnum.Voronoi:
                    return GetVoronoiNoise();
                default:
                    throw new Exception("unknown noise mode: " + NoiseMode);
            }
        }

        int GetSeed()
        {
            if (!RandomSeed) return Seed;
            var seed = Random.Range(0, int.MaxValue);
            Debug.Log("Using seed: " + seed);
            return seed;
        }

        Perlin GetPerlinNoise()
        {
            return new Perlin(Frequency, Lacunarity, Persistence, Octaves, GetSeed(), QualityModeEnum);
        }

        Voronoi GetVoronoiNoise()
        {
            return new Voronoi(Frequency, Lacunarity, GetSeed(), Persistence > 1);
        }

        Billow GetBillowNoise()
        {
            return new Billow(Frequency, Lacunarity, Persistence, Octaves, GetSeed(), QualityModeEnum);
        }

        RidgedMultifractal GetRidgedMultifractalNoise()
        {
            return new RidgedMultifractal(Frequency, Lacunarity, Octaves, GetSeed(), QualityModeEnum);
        }

        Cylinders GetCylindersNoise()
        {
            return new Cylinders(Frequency);
        }

        Checker GetCheckerNoise()
        {
            return new Checker();
        }
    }
}

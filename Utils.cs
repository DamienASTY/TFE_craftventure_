using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Utils {
    
    // Hauteur maximale pour la génération de pierres
    static int maxHeight = 126;
    
    // Facteur de lissage pour la génération de hauteur
    static float smooth = 0.01f;
    
    // Nombre d'octaves utilisées pour la génération de hauteur
    static int octaves = 5;
    
    // Facteur de persistance pour la génération de hauteur
    static float persistence = 0.5f;

    // Graine pour la génération du bruit Perlin
    static int seed;
    
    public static void genSeed()
    {
        const ushort max = 5;
        string seed = "";
        for (int i = 0; i < max; i++)
        {
            int randomNumber = Random.Range(1, 9);
            seed += $"{randomNumber}";
        }
        Debug.Log(seed);
        int seedInt = int.Parse(seed);
        Utils.seed = seedInt;
    }
    
    // Génère la hauteur pour une pierre à la position (x, z)
    public static int GenerateStoneHeight(float x, float z)
    {
        // Génère une hauteur en utilisant la fonction fBM avec des paramètres spécifiques
        float height = Map(0, maxHeight - 5, 0, 1, fBM(x * smooth * 2, z * smooth * 2, octaves + 1, persistence, seed));
        return (int)height;
    }
    
    // Génère la hauteur pour la position (x, z)
    public static int GenerateHeight(float x, float z)
    {
        // Génère une hauteur en utilisant la fonction fBM avec des paramètres spécifiques
        float height = Map(0, maxHeight, 0, 1, fBM(x * smooth, z * smooth, octaves, persistence, seed));
        return (int)height;
    }
    
    // Génère une valeur de bruit fractal 3D
    public static float fBM3D(float x, float y, float z, float sm, int oct)
    {
        // Calcule les valeurs de bruit 2D pour chaque paire de coordonnées (x, y), (y, z) et (x, z)
        float XY = fBM(x * sm, y * sm, oct, 0.5f, seed);
        float YZ = fBM(y * sm, z * sm, oct, 0.5f, seed);
        float XZ = fBM(x * sm, z * sm, oct, 0.5f, seed);

        // Calcule les valeurs de bruit 2D pour chaque paire de coordonnées (y, x), (z, y) et (z, x)
        float YX = fBM(y * sm, x * sm, oct, 0.5f, seed);
        float ZY = fBM(z * sm, y * sm, oct, 0.5f, seed);
        float ZX = fBM(z * sm, x * sm, oct, 0.5f, seed);

        // Calcule la moyenne des valeurs de bruit pour obtenir une valeur 3D
        return (XY + YZ + XZ + YX + ZY + ZX) / 6.0f;
    }
    
    // Fonction de mappage linéaire pour convertir une valeur d'une plage à une autre
    static float Map(float newmin, float newmax, float origmin, float origmax, float value)
    {
        return Mathf.Lerp(newmin, newmax, Mathf.InverseLerp(origmin, origmax, value));
    }
    
    // Fonction de bruit fractal (Fractional Brownian Motion)
    static float fBM(float x, float z, int oct, float pers, int seed)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        
        // Boucle pour générer les différentes octaves de bruit
        for (int i = 0; i < oct; i++)
        {
            // Ajoute la valeur de bruit perlin à la somme totale
            total += Mathf.PerlinNoise((x + seed) * frequency, (z + seed) * frequency) * amplitude;

            // Accumule les valeurs d'amplitude pour le facteur de persistance
            maxValue += amplitude;

            // Réduit l'amplitude pour l'octave suivante
            amplitude *= pers;
            
            // Double la fréquence pour l'octave suivante
            frequency *= 2;
        }

        // Normalise la valeur totale en divisant par la valeur maximale
        return total / maxValue;
    }
}